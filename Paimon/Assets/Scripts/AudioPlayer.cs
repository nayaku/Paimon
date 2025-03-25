using System;
using System.Collections.Generic;
using UnityEngine;


public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private List<float> audioBuffer = new();
    private uint audioClipCount = 0;

    private Action playFinishAction;

    public void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (audioSource.isPlaying)
            return;
        lock (audioBuffer)
        {
            if (audioBuffer.Count == 0)
            {
                if (playFinishAction == null) return;
                playFinishAction();
                playFinishAction = null;
                return;
            }
        }

        PlayClip();
    }

    public void Write(float[] data)
    {
        lock (audioBuffer)
        {
            audioBuffer.AddRange(data);
        }
    }

    public void SetPlayFinishAction(Action action)
    {
        playFinishAction = action;
    }

    private void PlayClip()
    {
        float[] audioData;
        lock (audioBuffer)
        {
            audioData = audioBuffer.ToArray();
            audioBuffer.Clear();
        }
        var audioClip = AudioClip.Create($"ttsSound_{audioClipCount++}", audioData.Length, 1, 44100, false);
        audioClip.SetData(audioData, 0);
        audioSource.clip = audioClip;
        audioSource.PlayOneShot(audioClip);
    }
}
