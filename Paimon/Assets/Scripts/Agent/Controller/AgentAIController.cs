using System.Collections;
using UnityEngine;


public class AgentAIController : MonoBehaviour
{
    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="commandString"></param>
    /// <returns></returns>
    public virtual IEnumerator DoCommand(Object source, AIMessage aiMessage)
    {
        yield break;
    }



}

