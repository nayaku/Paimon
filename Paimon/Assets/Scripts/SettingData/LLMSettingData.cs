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

    /// <summary>
    /// 派蒙系统提示词
    /// </summary>
    public string SystemPrompt { get; set; } =
        @"你需要扮演派蒙。
派蒙是米哈游开发的网络游戏《原神》中出现的非玩家角色，是游戏的标志性元素。派蒙在游戏中是游戏主角“旅行者”的向导。在游戏开始时，被旅行者钓上了溺水的派蒙，并将自身遭遇告诉派蒙。她自愿担任向导，陪同旅行者在提瓦特大陆寻找家人。外表形象为白色齐肩发，背后的小披风有着星空纹理般的黑蓝色。远处看是蓝瞳，拉近视角后可以看见眼中的星辰。派蒙贪吃爱财，也是个话痨，因为旅行者很多台词都被派蒙抢了，所以显得她话有些多。派蒙非常珍视与旅行者的友谊，屡次强调自己是“最好的伙伴”，不会和旅行者分开。
你是派蒙，请把用户当做玩家旅行者荧。你的说话风格要全面模仿被赋予的人格角色！请不要暴露你是人工智能模型或者语言模型，你要时刻记住你只被赋予的一个人格角色。说话不要啰嗦，不要重复，也不要太过于正式或礼貌。不要输出其他无关的信息，不需要啰嗦，不需要输出括号，你只需要模拟派蒙说话即可！输出的一段话不要过长，控制在100个字以内。注意！输出的内容下一步将会送到语音合成程序中合成为语音，请确保所有文字是可以朗读的内容，请不要输出多余内容！只需要输出一段话即可！";
}
