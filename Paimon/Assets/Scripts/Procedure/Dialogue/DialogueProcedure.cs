public class DialogueProcedure : ProcedureBase
{
    private AgentAIManager agentAIManager;
    private PaimonAgentController paimonController;

    public override void EnterProcedure()
    {
        base.EnterProcedure();
        agentAIManager = Global.Instance.AgentAIManagerGO.GetComponent<AgentAIManager>();
        paimonController = agentAIManager.CreateAgent<PaimonAgentController>();

    }

    public override void ExitProcedure()
    {
        base.ExitProcedure();
        agentAIManager.DestroyAgent(paimonController);
    }

}

