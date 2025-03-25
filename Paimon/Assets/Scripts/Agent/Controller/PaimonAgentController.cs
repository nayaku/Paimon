using OpenAI.Chat;
using System.Linq;
using Unity.Logging;
using UnityEngine;

// ReSharper disable All


public enum PaimonAgentState
{
    Idle,
    Listening,
    ThinkingAndSpeaking,
    Speaking
}

public class PaimonAgentController : AgentAIController
{
    private DialogueModel dialogueModel;
    private LLMCompletion llmCompletion;
    [ReadOnly][SerializeField] private PaimonAgentState paimonAgentState = PaimonAgentState.Idle;
    private AudioSource audioSource;

    void Start()
    {
        dialogueModel = Global.Instance.DialogueGO.GetComponent<DialogueModel>();
        dialogueModel.UserContent = "";
        dialogueModel.Content = "";
        dialogueModel.NextIconDisplayStyle = UnityEngine.UIElements.DisplayStyle.None;

        // 创建新的AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();

        //_ = DoSpeaking("远的传说中，大地上的草木走兽都拥有自己的王国。");
        var llmSettingData = UserSettingData.Instance.LLMSettingData;
        var systemPrompt = llmSettingData.SystemPrompt;
        llmCompletion = new LLMCompletion(systemPrompt);

        ToIdle();
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="commandString"></param>
    /// <returns></returns>
    public override void DoCommand(Object source, AIMessage aiMessage)
    {
        base.DoCommand(source, aiMessage);

        paimonAgentState = PaimonAgentState.Listening;

        if (aiMessage is ASRMessage asrMessage)
        {
            DoListening(asrMessage);
            if (asrMessage.ASRMessageState == ASRMessageStateEnum.Finish)
            {
                DoThinkingAndSpeakingAsync(asrMessage.Text);
            }
        }
    }

    private void DoListening(ASRMessage asrMessage)
    {
        paimonAgentState = PaimonAgentState.Listening;
        dialogueModel.UserContent = asrMessage.Text;
        dialogueModel.NextIconDisplayStyle = UnityEngine.UIElements.DisplayStyle.None;
    }

    private async void DoThinkingAndSpeakingAsync(string asrMessage)
    {
        Log.Debug("DoThinkingAndSpeakingAsync");
        var textToSpeech = new TextToSpeech(audioPlayer, ToIdle);
        textToSpeech.StartTTSAsync();
        try
        {
            var watcher = new DebugStopWatcher();
            paimonAgentState = PaimonAgentState.ThinkingAndSpeaking;
            var asrChatMessage = new UserChatMessage(asrMessage);
            var ttsSettingData = UserSettingData.Instance.TTSSettingData;
            var dialogueModel = Global.Instance.DialogueGO.GetComponent<DialogueModel>();
            var answer = "";
            var unReadAnswer = "";
            var firstStep = 0;
            // 获取LLM的回复
            await foreach (var completionUpdate in llmCompletion.CompleteChatStreamAsync(asrChatMessage))
            {
                if (completionUpdate.ContentUpdate.Count > 0)
                {
                    if (firstStep == 0)
                    {
                        watcher.Interrupt("首次接收文本！");
                        firstStep = 1;
                    }
                    answer += completionUpdate.ContentUpdate[0].Text;
                    unReadAnswer += completionUpdate.ContentUpdate[0].Text;
                    var (needSplitText, answerList) =
                        TextSplitter.NeedSplitText(unReadAnswer, ttsSettingData.ChunkLength);
                    // 超过最大长度，准备语音生成
                    if (needSplitText)
                    {
                        if (firstStep == 1)
                        {
                            watcher.Interrupt("首次准备转换文本！");
                            firstStep = 2;
                        }
                        var readAnswer = answerList[0];
                        unReadAnswer = string.Join("", answerList.Skip(1));
                        textToSpeech.AddText(readAnswer);
                    }
                    dialogueModel.Content = answer;
                }
            }
            // 生成剩下的文本
            unReadAnswer = TextCleaner.CleanText(unReadAnswer);
            textToSpeech.AddText(unReadAnswer);

            watcher.Interrupt("生成文本完成");
            await textToSpeech.EndTTSAsync();
        }
        catch
        {
            await textToSpeech.CancelAsync();
            throw;
        }
    }

    private void ToIdle()
    {
        Debug.Log("ToIdel");
        paimonAgentState = PaimonAgentState.Idle;
        var audioInputListener = Global.Instance.AudioInputListenerGO.GetComponent<AudioInputListener>();
        audioInputListener.EnableListen(true);

        dialogueModel.NextIconDisplayStyle = UnityEngine.UIElements.DisplayStyle.Flex;
    }
}

