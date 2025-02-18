using System;

public class LLMSettingData
{
    /// <summary>
    /// LLM地址
    /// </summary>
    public Uri Url { get; set; } = new("http://127.0.0.1:7861/knowledge_base");

    public string Mode { get; set; } = "local_kb";
    public string SampleName { get; set; } = "Genshin_small";

    /// <summary>
    /// 认证Token
    /// </summary>
    public string Authorization { get; set; } = "ABCDEFG";

    /// <summary>
    /// 模型名
    /// </summary>
    public string Model { get; set; } = "glm4-chat";

    /// <summary>
    /// 最大Token数
    /// </summary>
    public int MaxTokenNum { get; set; } = 512;
}

