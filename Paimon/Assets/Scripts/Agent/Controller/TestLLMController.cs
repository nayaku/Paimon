using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Unity.Logging;

public class TestLLMController:AgentAIController
{
    void Start()
    {
        TestLLMAsync();
    }

    private async Task TestLLMAsync()
    {
        Log.Debug("TestLLMAsync");
        var llm = new LLMCompletion();
        var totalTime = 0.0;
        var totalCount = 0;
        var questions = new List<string>()
            {
                "璃月美食",
                "蒙德美食",
                "璃月美食",
                "小岛",
                "空中",
                "璃月美食",
                "想吃什么",
                "想吃什么",
                "神里绫华",
                "饿了吗",
                "饿了吗",
                "饿了吗",
                "饿了吗",
                "口渴了吗",
            };
        foreach (var input in questions)
        {
            var startTime = DateTime.Now;
            var result = await llm.CompleteChatAsync(input);
            var endTime = DateTime.Now;
            totalTime += (endTime - startTime).TotalSeconds;
            totalCount++;
            Unity.Logging.Log.Debug(result.Content[0].Text.Trim());
            Unity.Logging.Log.Debug($"耗时：{(endTime - startTime).TotalSeconds}");
        }
        Unity.Logging.Log.Debug($"对话{totalCount}次");
        Unity.Logging.Log.Debug($"每次耗时：{totalTime / totalCount}");
    }
}
