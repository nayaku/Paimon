using Newtonsoft.Json;
using OpenAI.Chat;
using System;
using System.ClientModel.Primitives;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

public class ChatMessageJsonConverter : JsonConverter<ChatMessage>
{
    public override ChatMessage ReadJson(JsonReader reader, Type objectType, ChatMessage existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, ChatMessage value, Newtonsoft.Json.JsonSerializer serializer)
    {
        var message = value as IJsonModel<ChatMessage>;

        using var stream = new MemoryStream();
        // 不输出\u
        var jsonOptions = new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        using var utf8JsonWriter = new Utf8JsonWriter(stream, jsonOptions);


        message.Write(utf8JsonWriter, ModelReaderWriterOptions.Json);
        utf8JsonWriter.Flush();

        // 这里再把UTF-8转换为String
        // TODO 可能存在性能问题。这里把UTF-8的JSON转换为String，过后再发送给服务器的时候，又会再转换一次
        var resultString = Encoding.UTF8.GetString(stream.ToArray());

        writer.WriteRawValue(resultString);
    }
}

