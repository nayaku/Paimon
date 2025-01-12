using System.Collections.Generic;
using UnityEngine;


public class AgentAIManager : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private List<AgentAIController> agentAIControllerList = new();


    public T CreateAgent<T>() where T : AgentAIController
    {
        var controller = gameObject.AddComponent<T>();
        agentAIControllerList.Add(controller);
        return controller;
    }

    public void DestroyAgent(AgentAIController controller)
    {
        agentAIControllerList.Remove(controller);
        Destroy(controller);
    }

    public void SendAIMessage(Object source, AIMessage message)
    {
        agentAIControllerList.ForEach((controller) =>
        {
            controller.DoCommand(source, message);
        });
    }
}
