using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tiktoken;
using Tiktoken.Encodings;
using Unity.Logging;
using UnityEngine;


public class LLMCompletion
{
    [SerializeField]
    [ReadOnly]
    private List<ChatMessage> messages = new();

    [SerializeField]
    [ReadOnly]
    private List<int> messageTokenCount = new();

    /// <summary>
    /// 添加对话内容
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(ChatMessage message)
    {
        messages.Add(message);
        var encoder = new Encoder(new O200KBase());
        var numberOfTokens = encoder.CountTokens(message.Content[0].Text);
        Log.Debug($"Token {numberOfTokens}");
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
        AddMessage(chatMessage);
        RemoveOldMessage();

        var llmSettingData = UserSettingData.Instance.LLMSettingData;
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri($"{llmSettingData.Url}/{llmSettingData.Mode}/{llmSettingData.SampleName}"),
            NetworkTimeout = TimeSpan.FromSeconds(300),
            RetryPolicy = new ClientRetryPolicy(4),
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
                TopK = 6,
                ScoreThreshold = 0.6,
                ReturnDirect = false,
            },
            PromptName = "paimon",
        };
        var requestDataJson = JsonConvert.SerializeObject(requestData);
        var requestDataJsonBytes = BinaryData.FromString(requestDataJson);
        using var content = BinaryContent.Create(requestDataJsonBytes);

        // 请求
        //TimeUtil.LogDebugTimestamp("发送前线程ID:" + Thread.CurrentThread.ManagedThreadId);
        var clientResult = await client.CompleteChatAsync(content);
        //TimeUtil.LogDebugTimestamp("发送后线程ID" + Thread.CurrentThread.ManagedThreadId);
        var response = clientResult.GetRawResponse();
        using var jsonDocument = JsonDocument.Parse(response.Content);
        var element = jsonDocument.RootElement;

        // 解析
        var completion = BinaryData.FromString(element.GetString());
        var chatCompletion = ModelReaderWriter.Read<ChatCompletion>(completion);
        var resultChatMessage = new AssistantChatMessage(chatCompletion);
        //messages.Add(resultChatMessage);
        AddMessage(resultChatMessage);
        return resultChatMessage;
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

