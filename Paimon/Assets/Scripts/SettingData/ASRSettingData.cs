public class ASRSettingData
{
    /// <summary>
    /// ASR地址
    /// </summary>
    public string ASRServerUrl { get; set; } = "http://127.0.0.1:8000";
    /// <summary>
    /// 最小触发声音
    /// </summary>
    public float MinTriggerVolume { get; set; } = 0.03f;
    /// <summary>
    /// 语音停顿时间
    /// </summary>
    public float SoundWaitTime { get; set; } = 2f;

    /// <summary>
    /// 识别间隔
    /// </summary>
    public float RecognizeInterval { get; set; } = 0.8f;

    ///// <summary>
    ///// 不间断聊天
    ///// </summary>
    //public bool Uninterrupted { get; set; } = false;
}
