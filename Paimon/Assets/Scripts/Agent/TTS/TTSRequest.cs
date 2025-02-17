using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
public class ReferenceAudio
{
    public byte[] Audio { get; set; }
    public string Text { get; set; }
}
public enum TTSRequestFormatEnum { Wav, Pcm, Mp3 }
public enum UseMemoryCacheEnum { On, Off }
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
public class TTSRequest
{
    [Required]
    public string Text { get; set; }
    [Range(100, 300)]
    public int ChunkLength { get; set; } = 200;
    [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
    public TTSRequestFormatEnum Format { get; set; } = TTSRequestFormatEnum.Wav;
    public ReferenceAudio[] References { get; set; } = new ReferenceAudio[0];
    public string ReferenceId { get; set; } = null;
    public int? Seed { get; set; } = null;
    [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
    public UseMemoryCacheEnum UseMemoryCache { get; set; } = UseMemoryCacheEnum.Off;
    public bool Normalize { get; set; } = true;
    public bool Streaming { get; set; } = false;
    public int MaxNewTokens { get; set; } = 1024;
    [Range(0.1, 1.0)]
    public double TopP { get; set; } = 0.7;
    [Range(0.9, 2.0)]
    public double RepetitionPenalty { get; set; } = 1.2;
    [Range(0.1, 1.0)]
    public double Temperature { get; set; } = 0.7;
}
