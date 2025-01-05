using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;

public class AudioInputListener : MonoBehaviour
{
    private AgentAIManager llmAIManager;

    // 最小检测时间段
    private const int volumeLength = 128;

    private string device;
    private int count = 0;
    private AudioClip audioClip;
    private bool isStart;
    private int startPos;
    private int endPos;
    private float prevTime;
    private bool isSending = false;
    private bool enableListen = false;

    void Awake()
    {
        // 获取麦克风
        string[] myDevice = Microphone.devices;
        if (myDevice.Length == 0)
        {
            var msg = "没有麦克风设备！";
            MessageDialogModel.Show("错误", msg);
            Log.Error(msg);
            return;
        }
        device = myDevice[0];
        Log.Debug("使用麦克风设备：" + device);
    }

    void Start()
    {
        llmAIManager = Global.Instance.AgentAIManagerGO.GetComponent<AgentAIManager>();
    }

    void Update()
    {
        if (enableListen)
            Listening();
    }

    void OnDestroy()
    {
        StopRecord();
    }

    public void EnableListen(bool enable)
    {
        if (enable == enableListen)
            return;
        enableListen = enable;
        if (!enableListen)
        {
            StopRecord();
        }
        else
        {
            StartRecord();
        }
    }

    private void Listening()
    {
        if (!Microphone.IsRecording(device))
        {
            // 空转了一小时，则重新开始监听
            StopRecord();
            StartRecord();
            return;
        }

        float volume = GetMaxVolume();
        var asrSettingData = UserSettingData.Instance.ASRSettingData;
        var minVolume = asrSettingData.MinTriggerVolume;
        var soundWaitTime = asrSettingData.SoundWaitTime;
        var recognizeInterval = asrSettingData.RecognizeInterval;

        if (!isStart)
        {
            if (volume > minVolume)
            {
                isStart = true;
                startPos = Math.Max(0, Microphone.GetPosition(device) - 2 * volumeLength);
                Log.Debug($"当前音量为{volume}>{minVolume}，开始录音！Pos:{startPos}");

                var aiMessage = new ASRMessage(ASRMessageStateEnum.Starting, "", "");
                llmAIManager.SendAIMessage(this, aiMessage);
            }
        }
        else
        {
            if (volume > minVolume)
            {
                prevTime = Time.time;
                var curPos = Microphone.GetPosition(device);
                // 每隔一段时间就识别一下，模拟流式识别，反正识别速度很快
                if (curPos - endPos >= 44100 * recognizeInterval)
                {
                    endPos = curPos;
                    Recognize(startPos, endPos, false);
                }
            }
            else
            {
                var waitTime = Time.time - prevTime;
                if (waitTime > soundWaitTime)
                {
                    // 停止录音
                    Debug.Log($"停顿时间{waitTime}>{soundWaitTime}，停止录音，开始正式识别。");
                    Recognize(startPos, Microphone.GetPosition(device), true);
                    StopRecord();
                    enableListen = false;
                }
            }
        }
    }

    /// <summary>
    /// 结束监听麦克风
    /// </summary>
    private void StopRecord()
    {
        Log.Debug("结束监听麦克风");
        Microphone.End(device);
        isStart = false;
    }

    /// <summary>
    /// 开始监听麦克风
    /// </summary>
    private void StartRecord()
    {
        if (Microphone.IsRecording(device))
            StopRecord();
        Log.Debug("开始监听麦克风");
        audioClip = Microphone.Start(device, true, 3000, 44100);
    }

    private void Recognize(int start, int end, bool isFinish)
    {
        count++;
        // 准备音频数据
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            byte[] audioData = AudioClipToByte(start, end);
            WriteHeader(ms, audioClip, end - start);
            ms.Write(audioData, 0, audioData.Length);
            ms.Seek(0, SeekOrigin.Begin);
            bytes = ms.ToArray();
        }
        //#if DEBUG
        //        // Debug模型下保存录音数据
        //        string filePath = Path.Join(Application.persistentDataPath, $"audio_{count}.wav");
        //        Debug.Log("录音保存路径：" + filePath);
        //        using (FileStream fs = File.OpenWrite(filePath))
        //        {
        //            fs.Write(bytes, 0, bytes.Length);
        //        }
        //#endif
        // 准备发送到服务器解析
        if (isSending && !isFinish)
        {
            // 丢弃来不及处理的中间过程
            Log.Debug("上一个识别的还没出结果，抛弃中间临时识别过程！");
            return;
        }
        StartCoroutine(Send(bytes, isFinish));
    }

    private float GetMaxVolume()
    {
        // 获取数值
        float[] volumeData = new float[volumeLength];
        int offset = Microphone.GetPosition(device) - (volumeLength + 1);
        if (offset < 0)
            return 0f;
        audioClip.GetData(volumeData, offset);
        float maxVolume = Mathf.Max(volumeData.Max(), 0);

        // 设置可视化
        return maxVolume;
    }

    private byte[] AudioClipToByte(int start, int end)
    {
        float[] data = new float[end - start];
        audioClip.GetData(data, start);
        byte[] outData = new byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            short tempShort = (short)(data[i] * short.MaxValue);
            byte[] tempData = BitConverter.GetBytes(tempShort);
            outData[i * 2] = tempData[0];
            outData[i * 2 + 1] = tempData[1];
        }
        return outData;
    }

    /// <summary>
    /// 写WAV文件头
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="clip"></param>
    private static void WriteHeader(Stream stream, AudioClip clip, int sampleCount)
    {
        int byteRate = clip.frequency * clip.channels * 2; // 16-bit audio
        int blockAlign = clip.channels * 2;

        var writer = new BinaryWriter(stream);
        writer.Write("RIFF".ToCharArray());
        writer.Write(36 + sampleCount * 2); // 文件大小
        writer.Write("WAVE".ToCharArray());
        writer.Write("fmt ".ToCharArray());
        writer.Write(16); // fmt chunk size
        writer.Write((short)1); // PCM format
        writer.Write((short)clip.channels);
        writer.Write(clip.frequency);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)16); // bits per sample
        writer.Write("data".ToCharArray());
        writer.Write(sampleCount * 2); // data chunk size
    }

    /// <summary>
    /// 发送到服务器解析
    /// </summary>
    /// <param name="audioData"></param>
    /// <returns></returns>
    private IEnumerator Send(byte[] audioData, bool isFinish)
    {
        yield return new WaitUntil(() => isSending == false);
        isSending = true;

        var asrSettingData = UserSettingData.Instance.ASRSettingData;
        var asrServerUrl = asrSettingData.ASRServerUrl;

        var form = new WWWForm();
        form.AddBinaryData("file", audioData, "file.wav", "audio/wav");

        var url = asrServerUrl + "/api/v1/asr";
        var webRequest = UnityWebRequest.Post(url, form);
        yield return webRequest.SendWebRequest();
        //异常处理
        if (webRequest.error != null)
        {
            //如果错误，打印服务器错误信息
            Log.Error(webRequest.error);
            MessageDialogModel.Show("错误", webRequest.error);
        }
        else
        {
            var resp = webRequest.downloadHandler.text;
            Log.Debug("服务器返回值" + resp);//正确打印服务器返回值

            var receiveData = JObject.Parse(resp);
            var text = (string)receiveData["text"];
            var rawText = (string)receiveData["raw_text"];
            var messageState = isFinish ? ASRMessageStateEnum.Finish : ASRMessageStateEnum.Recognizing;
            var aiMessage = new ASRMessage(messageState, text, rawText);
            llmAIManager.SendAIMessage(this, aiMessage);
        }
        isSending = false;

    }
}
