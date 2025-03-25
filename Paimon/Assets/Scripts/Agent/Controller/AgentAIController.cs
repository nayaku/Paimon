using UnityEngine;
using Object = UnityEngine.Object;


public class AgentAIController : MonoBehaviour
{
    protected AudioPlayer audioPlayer;

    protected virtual void Awake()
    {
        audioPlayer = gameObject.AddComponent<AudioPlayer>();
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="commandString"></param>
    /// <returns></returns>
    public virtual void DoCommand(Object source, AIMessage aiMessage)
    {
    }
}

