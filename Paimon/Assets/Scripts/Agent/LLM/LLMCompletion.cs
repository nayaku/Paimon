using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;


public class LLMCompletion
{
    [SerializeField]
    private List<ChatMessage> messages = new();

    public async UniTask<ChatMessage> CompleteChatAsync(ChatMessage chatMessage)
    {
        messages.Add(chatMessage);

        var llmSettingData = UserSettingData.Instance.LLMSettingData;
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri($"{llmSettingData.Url}/{llmSettingData.Mode}/{llmSettingData.SampleName}"),
            //NetworkTimeout = TimeSpan.FromSeconds(10),
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
        var clientResult = await client.CompleteChatAsync(content);
        var response = clientResult.GetRawResponse();
        using var jsonDocument = JsonDocument.Parse(response.Content);
        var element = jsonDocument.RootElement;

        // 解析
        var completion = BinaryData.FromString(element.GetString());
        var chatCompletion = ModelReaderWriter.Read<ChatCompletion>(completion);
        var resultChatMessage = new AssistantChatMessage(chatCompletion);
        messages.Add(resultChatMessage);
        return resultChatMessage;
    }
}

