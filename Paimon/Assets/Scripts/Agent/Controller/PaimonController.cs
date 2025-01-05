using System.Collections;
using UnityEngine;



public class PaimonController : AgentAIController
{
    private DialogueModel dialogueModel;

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
    public override IEnumerator DoCommand(Object source, AIMessage aiMessage)
    {
        yield return base.DoCommand(source, aiMessage);
        if (aiMessage is ASRMessage asrMessage)
        {
            DoListening(asrMessage);
            yield return new WaitForSeconds(2);
            if (asrMessage.ASRMessageState == ASRMessageStateEnum.Finish)
            {
                var audioInputListener = Global.Instance.AudioInputListenerGO.GetComponent<AudioInputListener>();
                audioInputListener.EnableListen(true);
            }
        }
    }

    private void DoListening(ASRMessage asrMessage)
    {
        dialogueModel.UserContent = asrMessage.Text;
    }

}

