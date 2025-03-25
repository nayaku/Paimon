using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Unity.Logging;

public class TextToSpeech
{
    private Action onPlayFinishAction;
    private AudioPlayer audioPlayer;

    private CancellationTokenSource cancellationTokenSource;
    private List<string> needProcessText = new();
    private bool isEndText = false;
    private AsyncAutoResetEvent workWaitHandle = new(false);
    private AsyncAutoResetEvent workDoneWaitHandle = new(false);
    private Task loopTask = null;

    public TextToSpeech(AudioPlayer audioPlayer, Action onPlayFinishAction = null)
    {
        this.audioPlayer = audioPlayer;
        this.onPlayFinishAction = onPlayFinishAction;
    }

    public async void StartTTSAsync()
    {
        await CancelAsync();
        cancellationTokenSource = new();
        isEndText = false;
        loopTask = Task.Run(LoopAsync);
    }

    public void AddText(string text)
    {
        Log.Debug("添加文本：" + text);
        lock (needProcessText)
        {
            needProcessText.Add(text);
        }
        workWaitHandle.Set();
    }

    public async Task EndTTSAsync()
    {
        isEndText = true;
        workWaitHandle.Set();
        await workDoneWaitHandle.WaitAsync();
    }

    public async Task CancelAsync()
    {
        if (loopTask != null)
        {
            cancellationTokenSource?.Cancel();
            workDoneWaitHandle.Set();
            await EndTTSAsync();
        }
    }

    /// <summary>
    /// 循环处理
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async void LoopAsync()
    {
        while (true)
        {
            await workWaitHandle.WaitAsync();
            if (cancellationTokenSource.Token.IsCancellationRequested)
                break;
            string text;
            lock (needProcessText)
            {
                if (needProcessText.Count == 0)
                    continue;
                text = string.Join("", needProcessText);
                needProcessText.Clear();
            }
            await _TTSAsync(text, audioPlayer, needProcessText.Count == 0 && isEndText, cancellationTokenSource.Token);
            if (needProcessText.Count == 0 && isEndText)
                break;
        }
        workDoneWaitHandle.Set();
    }

    private async Task _TTSAsync(string text, AudioPlayer audioPlayer, bool isFinish,
        CancellationToken cancellationToken = default)
    {
        Log.Debug("发送文本：" + text);
        // 准备
        using var client = new HttpClient();
        using var request = CreateRequest(text);

        // 发送请求并获取响应流
        TimeUtil.LogDebugTimestamp("请求发送");
        await using var stream = await SendAsync(client, request, cancellationToken);
        if (stream == null)
            return;
        TimeUtil.LogDebugTimestamp("开始读取Stream");
        await ReadStreamAsync(stream, audioPlayer, isFinish, cancellationToken);
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
            Temperature = 0.6,
            ChunkLength = ttsSettingData.ChunkLength,
        };
        // 转换为Json
        var json = JsonConvert.SerializeObject(ttsRequest);
        var request = new HttpRequestMessage(HttpMethod.Post, ttsSettingData.Url) { Content = new StringContent(json) };
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
    private async UniTask<Stream> SendAsync(HttpClient client, HttpRequestMessage requestMessage,
        CancellationToken cancellationToken = default)
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
    /// 转换PCM格式为AudioData
    /// </summary>
    /// <param name="pcmData"></param>
    /// <returns></returns>
    private float[] ConvertPcmToAudioData(byte[] pcmData, int length)
    {
        int sampleCount = length / 2; // 每个样本为 2 字节
        float[] floatSamples = new float[sampleCount];
        // 转换
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            floatSamples[i] = sample / (float)short.MaxValue;
        }

        return floatSamples;
    }

    /// <summary>
    /// 从Stream中读取数据
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="pipeWriter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ReadStreamAsync(Stream stream, AudioPlayer audioPlayer,
        bool isFinish, CancellationToken cancellationToken = default)
    {
        const int bufferSize = 44100 * 10;
        var offset = 0;
        var buff = new byte[bufferSize];
        try
        {
            while (true)
            {
                var count = await stream.ReadAsync(buff, offset, bufferSize - offset, cancellationToken);
                if (count == 0)
                    break;
                //Debug.Assert(count % 2 != 0); // 应该所有数据均为short（2的倍数）
                var audioData = ConvertPcmToAudioData(buff, offset + count);
                offset = (offset + count) - audioData.Length * 2;
                //Debug.Assert(offset == 0 || offset == 1);
                audioPlayer.Write(audioData);
            }
            if (isFinish)
            {
                if (onPlayFinishAction != null)
                    audioPlayer.SetPlayFinishAction(onPlayFinishAction);
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

        TimeUtil.LogDebugTimestamp("全部Stream流读取完毕");
    }
}
