using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unity.Logging;
using UnityEngine;


public class TextToSpeech
{
    public async Awaitable TTSAsync(string text, AudioSource audioSource, CancellationToken cancellationToken)
    {
        // 准备
        using var client = CreateHttpClient();
        using var request = CreateRequest(text);

        // 发送请求并获取响应流
        TimeUtil.LogDebugTimestamp("请求发送");
        using var stream = await SendAsync(client, request, cancellationToken);
        if (stream != null)
        {
            TimeUtil.LogDebugTimestamp("开始读取Stream");
            var pipeOptions = new PipeOptions(pauseWriterThreshold: 0); // 不暂停，一直可写
            var pipe = new Pipe(pipeOptions);
            // 持续从流读数据
            var readTask = ReadStreamAsync(stream, pipe.Writer, cancellationToken);
            // 边下边播
            var playTask = PlayAudioAsync(audioSource, pipe.Reader, cancellationToken);
            await (playTask, readTask);
        }
    }

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    /// <returns></returns>
    private HttpClient CreateHttpClient()
    {
        // 初始化Http
        var httpClientHandler = new HttpClientHandler();
        var client = new HttpClient(httpClientHandler);
        return client;
    }

    /// <summary>
    /// 生成请求内容
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private HttpRequestMessage CreateRequest(string text)
    {
        // 准备参数
        var userSettingData = UserSettingData.Instance;
        var ttsSettingData = userSettingData.TTSSettingData;

        // 准备填装
        var ttsRequest = new TTSRequest
        {
            Text = text,
            Format = TTSRequestFormatEnum.Wav,
            UseMemoryCache = UseMemoryCacheEnum.On,
            Streaming = true,
            Temperature = 0.6
        };
        // 转换为Json
        var json = JsonConvert.SerializeObject(ttsRequest);
        var request = new HttpRequestMessage(HttpMethod.Post, ttsSettingData.Url)
        {
            Content = new StringContent(json)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return request;
    }

    /// <summary>
    /// 发送
    /// </summary>
    /// <param name="client"></param>
    /// <param name="requestMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async UniTask<Stream> SendAsync(HttpClient client, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        try
        {
            var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            TimeUtil.LogDebugTimestamp("获取响应");
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                // 输出错误
                var content = await responseMessage.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(content);
                if (jsonObject.ContainsKey("status") && jsonObject.ContainsKey("message"))
                {
                    var status = (int)jsonObject["status"];
                    var message = (string)jsonObject["message"];
                    MessageDialogModel.Show("文本转音频服务错误", $"请求文本转音频服务时出错：\n错误代码：{status}\n错误内容：{message}");
                }
                Log.Error(content);
                return null;
            }
            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return stream;
        }
        catch (HttpRequestException ex)
        {
            Log.Error($"请求异常: {ex.Message}\n{ex.InnerException}");
            MessageDialogModel.Show("文本转音频服务错误", $"请求文本转音频服务时异常:\n{ex.Message}\n{ex.InnerException}");
            return null;
        }
        catch (OperationCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
                Log.Debug("请求取消");
            else
            {
                Log.Error($"请求超时: {ex.Message}\n{ex.InnerException}");
            }
            return null;
        }
    }

    /// <summary>
    /// 从Stream中读取数据
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="pipeWriter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async UniTask ReadStreamAsync(Stream stream, PipeWriter pipeWriter, CancellationToken cancellationToken)
    {
        await Awaitable.BackgroundThreadAsync();
        const int bufferSize = 10240;
        try
        {
            while (true)
            {
                var memory = pipeWriter.GetMemory(bufferSize);
                //TimeUtil.LogDebugTimestamp($"等待stream。。。");
                var count = await stream.ReadAsync(memory, cancellationToken);
                if (count == 0)
                    break;
                //TimeUtil.LogDebugTimestamp($"从steam中获得{count}字节。");
                pipeWriter.Advance(count);
                var result = await pipeWriter.FlushAsync(); // 这里应该不会堵塞
                if (result.IsCompleted)
                    break;
            }
        }
        catch (TaskCanceledException ex)
        {
            if (ex.CancellationToken.IsCancellationRequested)
                Log.Debug("写入取消");
            else
            {
                Log.Error($"写入流被取消: {ex.Message}\n{ex.InnerException}");
            }
        }
        await pipeWriter.CompleteAsync();
        TimeUtil.LogDebugTimestamp("全部Stream流读取完毕");
    }

    /// <summary>
    /// 播放音频
    /// </summary>
    /// <param name="pipeReader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async UniTask PlayAudioAsync(AudioSource audioSource, PipeReader pipeReader, CancellationToken cancellationToken)
    {
        var pcmPlayer = new PCMPlayer(audioSource);
        while (true)
        {
            // 获取缓冲
            var result = await pipeReader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;
            if (cancellationToken.IsCancellationRequested)
            {
                TimeUtil.LogDebugTimestamp("播放取消");
                break;
            }
            var bytes = buffer.ToArray();
            await pcmPlayer.WriteAsync(bytes);
            pipeReader.AdvanceTo(buffer.GetPosition(bytes.Length));
            // 读取完成
            if (result.IsCompleted)
                break;
        }
        await pipeReader.CompleteAsync();
        await pcmPlayer.FlushAsync();
        TimeUtil.LogDebugTimestamp("播放结束");
    }
}
