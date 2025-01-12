using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenAI.Chat;
using System.Collections.Generic;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
public class RAGRequestDataExtraBody
{
    public int TopK { get; set; } = 3;
    public double ScoreThreshold { get; set; } = 2.0;
    public bool ReturnDirect { get; set; } = true;
}
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
public class RAGRequestData
{
    public string Model { get; set; } = "";
    [JsonProperty(ItemConverterType = typeof(ChatMessageJsonConverter))]
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public bool Stream { get; set; } = true;
    public double Temperature { get; set; } = 0.7;
    public RAGRequestDataExtraBody ExtraBody { get; set; } = new();
    public string PromptName { get; set; } = "default";
    public string Stop { get; set; } = "\n";
    public int MaxTokens { get; set; } = 240;
}
