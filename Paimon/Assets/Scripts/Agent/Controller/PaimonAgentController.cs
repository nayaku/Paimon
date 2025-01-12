using Cysharp.Threading.Tasks;
using OpenAI.Chat;
using Unity.Logging;
using UnityEngine;


public enum PaimonAgentState
{
    Idle,
    Listening,
    Thinking,
    Speaking
}

public class PaimonAgentController : AgentAIController
{
    private DialogueModel dialogueModel;
    private LLMCompletion lLMCompletion = new();
    [ReadOnly]
    [SerializeField]
    private PaimonAgentState paimonAgentState = PaimonAgentState.Idle;

    void Start()
    {
        dialogueModel = Global.Instance.DialogueGO.GetComponent<DialogueModel>();
        dialogueModel.UserContent = "";
        dialogueModel.Content = "";
        dialogueModel.NextIconDisplayStyle = UnityEngine.UIElements.DisplayStyle.None;

        var audioInputListener = Global.Instance.AudioInputListenerGO.GetComponent<AudioInputListener>();
        audioInputListener.EnableListen(true);
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
                DoThinking(asrMessage.Text);
                //var audioInputListener = Global.Instance.AudioInputListenerGO.GetComponent<AudioInputListener>();
                //audioInputListener.EnableListen(true);
            }
        }
    }

    private void DoListening(ASRMessage asrMessage)
    {
        dialogueModel.UserContent = asrMessage.Text;
    }

    private async UniTask DoThinking(string asrMessage)
    {
        paimonAgentState = PaimonAgentState.Thinking;
        var asrChatMessage = new SystemChatMessage("以下内容来自来自用户语音的结果，其中可能包含语音识别错误，并且没有重建标准符号，你需要结合语境分析：" + asrMessage);
        var resultChatMessage = await lLMCompletion.CompleteChatAsync(asrChatMessage);
        Log.Debug(resultChatMessage);
        var dialogueModel = Global.Instance.DialogueGO.GetComponent<DialogueModel>();
        dialogueModel.Content = resultChatMessage.Content[0].Text.Trim();
    }

}

