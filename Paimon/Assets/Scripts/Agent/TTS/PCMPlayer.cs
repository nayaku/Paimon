using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;


public class PCMPlayer
{
    private List<byte> audioBuffer = new();
    private AudioSource audioSource;
    private int audioBufferSize;
    private uint audioClipCount = 0;
    private bool firstPlay = true;

    public PCMPlayer(AudioSource audioSource, int audioBufferSize = 44100 * 2)
    {
        this.audioSource = audioSource;
        this.audioBufferSize = audioBufferSize;
    }

    public async UniTask WriteAsync(byte[] data)
    {
        audioBuffer.AddRange(data);
        if (audioBuffer.Count >= audioBufferSize)
        {
            await PlayAsync();
        }
    }

    public async UniTask FlushAsync()
    {
        await PlayAsync();
    }

    private async UniTask PlayAsync()
    {
        if (firstPlay)
        {
            TimeUtil.LogDebugTimestamp("首次开始播放");
            firstPlay = false;
        }

        var byteConsume = audioBuffer.Count / 2 * 2;

        if (byteConsume <= 2)
            return;

        var audioClip = ConvertPcmToAudioClip(audioBuffer.ToArray(), byteConsume);
        audioSource.clip = audioClip;
        audioSource.PlayOneShot(audioClip);
        await Awaitable.WaitForSecondsAsync(audioClip.length);
        if (byteConsume != audioBuffer.Count)
        {
            byte last = audioBuffer[byteConsume];
            audioBuffer.Clear();
            audioBuffer.Add(last);
        }
        else
        {
            audioBuffer.Clear();
        }
    }

    /// <summary>
    /// 转换PCM格式为AudioClip
    /// </summary>
    /// <param name="pcmData"></param>
    /// <returns></returns>
    private AudioClip ConvertPcmToAudioClip(byte[] pcmData, int length)
    {
        int sampleCount = length / 2; // 每个样本为 2 字节
        float[] floatSamples = new float[sampleCount];
        // 转换
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            floatSamples[i] = sample / (float)short.MaxValue;
        }

        // 创建AudioClip
        var audioClip = AudioClip.Create($"ttsSound_{audioClipCount}", sampleCount, 1, 44100, false);
        audioClip.SetData(floatSamples, 0);
        //#if DEBUG
        //        var bytes = AudioClipConverter.AudioClipToWav(audioClip);
        //        // 保存到文件
        //        string filePath = Path.Join(Application.persistentDataPath, $"tts_{audioClipCount}.wav");
        //        Log.Debug("生成的TTS音频保存路径：" + filePath);
        //        using (FileStream fs = File.OpenWrite(filePath))
        //        {
        //            fs.Write(bytes, 0, bytes.Length);
        //        }
        //#endif
        audioClipCount++;
        return audioClip;
    }

}

