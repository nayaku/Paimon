using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Logging;
using UnityEngine;


public class LLMCompletion
{
    [SerializeField][ReadOnly] private List<ChatMessage> messages = new();

    [SerializeField][ReadOnly] private List<int> messageTokenCount = new();

    public LLMCompletion(string systemPrompt = "You are a helpful assistant.")
    {
        var systemPromptMessage = new SystemChatMessage(systemPrompt);
        AddMessage(systemPromptMessage);
    }

    /// <summary>
    /// 添加对话内容
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(ChatMessage message)
    {
        messages.Add(message);
        // var encoder = new Encoder(new O200KBase());
        // var numberOfTokens = encoder.CountTokens(message.Content[0].Text);
        var numberOfTokens = message.Content[0].Text.Length; // 改为超简易的计算方式
        messageTokenCount.Add(numberOfTokens);
        RemoveOldMessage();
    }

    /// <summary>
    /// 补全对话
    /// </summary>
    /// <param name="chatMessage"></param>
    /// <returns></returns>
    public async Task<ChatMessage> CompleteChatAsync(ChatMessage chatMessage)
    {
        //var timeWatch = new DebugStopWatcher("CompleteChatAsync");
        AddMessage(chatMessage);
        //timeWatch.Interrupt("添加和删除旧消息");

        var llmSettingData = UserSettingData.Instance.LLMSettingData;
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri($"{llmSettingData.Url}/{llmSettingData.Mode}/{llmSettingData.SampleName}"),
            NetworkTimeout = TimeSpan.FromSeconds(300),
            RetryPolicy = new ClientRetryPolicy(1),
            Transport = new HttpClientPipelineTransport(new System.Net.Http.HttpClient(new LoggingHandler()))
        };
        var apiKey = new ApiKeyCredential(llmSettingData.Authorization);
        var client = new ChatClient(llmSettingData.Model, apiKey, options);

        var requestData = new RAGRequestData
        {
            Model = llmSettingData.Model,
            Messages = messages,
            Stream = false,
            Temperature = 0.7,
            ExtraBody = new RAGRequestDataExtraBody
            {
                TopK = 3,
                ScoreThreshold = 0.6,
                ReturnDirect = false,
            },
            PromptName = "paimon",
            MaxTokens = 120
        };
        var requestDataJson = JsonConvert.SerializeObject(requestData);
        var requestDataJsonBytes = BinaryData.FromString(requestDataJson);
        using var content = BinaryContent.Create(requestDataJsonBytes);

        // 请求
        //timeWatch.Interrupt("准备请求");
        var clientResult = await client.CompleteChatAsync(content);
        //timeWatch.Interrupt("收到请求");
        var response = clientResult.GetRawResponse();
        using var jsonDocument = JsonDocument.Parse(response.Content);
        var element = jsonDocument.RootElement;

        // 解析
        var completion = BinaryData.FromString(element.GetString());
        var chatCompletion = ModelReaderWriter.Read<ChatCompletion>(completion);
        var resultChatMessage = new AssistantChatMessage(chatCompletion);
        //timeWatch.Interrupt("解析请求");
        //messages.Add(resultChatMessage);
        AddMessage(resultChatMessage);
        //timeWatch.Interrupt("返回");
        return resultChatMessage;
    }

    /// <summary>
    /// 补全对话
    /// </summary>
    /// <param name="chatMessage"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteChatStreamAsync(ChatMessage chatMessage)
    {
        var timeWatch = new DebugStopWatcher("CompleteChatAsync");
        AddMessage(chatMessage);
        timeWatch.Interrupt("添加和删除旧消息");


        var llmSettingData = UserSettingData.Instance.LLMSettingData;
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri($"{llmSettingData.Url}/{llmSettingData.Mode}/{llmSettingData.SampleName}"),
            NetworkTimeout = TimeSpan.FromSeconds(300),
            RetryPolicy = new ClientRetryPolicy(1),
            //Transport = new HttpClientPipelineTransport(new System.Net.Http.HttpClient(new LoggingHandler()))
        };
        var apiKey = new ApiKeyCredential(llmSettingData.Authorization);
        var client = new ChatClient(llmSettingData.Model, apiKey, options);

        var extraBody = new RAGRequestDataExtraBody
        {
            TopK = 6,
            ScoreThreshold = 2.0,
            ReturnDirect = false,
            PromptName = "paimon"
        };
        var extraBodyJson = JsonConvert.SerializeObject(extraBody);

        var cco = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = 120,
            Temperature = 0.7f,
            FrequencyPenalty = 1.5f,
        };
        cco = ((IJsonModel<ChatCompletionOptions>)cco).Create(new BinaryData(extraBodyJson),
            ModelReaderWriterOptions.Json);
        timeWatch.Interrupt("准备数据");
        var completionUpdates = client.CompleteChatStreamingAsync(messages, cco);
        var texts = new List<string>();
        var first = true;
        await foreach (var completionUpdate in completionUpdates)
        {
            if (first)
            {
                first = false;
                timeWatch.Interrupt("首次接收消息：" + completionUpdate.ContentUpdate[0].Text);
            }

            var serializedDataProperty = completionUpdate.GetType().GetProperty("SerializedAdditionalRawData",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (serializedDataProperty != null)
            {
                var serializedData = (IDictionary<string, BinaryData>)serializedDataProperty.GetValue(completionUpdate);
                if (serializedData.TryGetValue("docs", out var binaryData))
                {
                    var docsList = binaryData.ToObjectFromJson<List<string>>();
                    Log.Debug(string.Join("\n\n", docsList));
                }
            }

            if (completionUpdate.ContentUpdate.Count > 0)
            {
                texts.Add(completionUpdate.ContentUpdate[0].Text);
            }

            yield return completionUpdate;
        }

        timeWatch.Interrupt("接收消息完成");
        var messageString = string.Join("", texts);
        Log.Debug("Assistant: " + messageString);
        var resultChatMessage = new AssistantChatMessage(messageString);
        AddMessage(resultChatMessage);
        timeWatch.Interrupt("添加消息完成");
    }

    private void RemoveOldMessage()
    {
        var llmSettingData = UserSettingData.Instance.LLMSettingData;
        var totalTokenNum = messageTokenCount.Sum();
        int delNum = 0;
        while (totalTokenNum > llmSettingData.MaxTokenNum)
        {
            totalTokenNum -= messageTokenCount[delNum];
            delNum++;
        }

        if (delNum > 0)
        {
            Log.Debug($"超过最大Token数，删除{delNum}个历史对话。");
            messages.RemoveRange(0, delNum);
            messageTokenCount.RemoveRange(0, delNum);
        }

        Debug.Assert(messages.Count == messageTokenCount.Count);
    }
}
