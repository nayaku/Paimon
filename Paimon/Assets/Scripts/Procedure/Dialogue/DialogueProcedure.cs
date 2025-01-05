public class DialogueProcedure : ProcedureBase
{
    private AgentAIManager agentAIManager;
    private PaimonController paimonController;

    public override void EnterProcedure()
    {
        base.EnterProcedure();
        agentAIManager = Global.Instance.AgentAIManagerGO.GetComponent<AgentAIManager>();
        paimonController = agentAIManager.CreateAgent<PaimonController>();
    }

    public override void ExitProcedure()
    {
        base.ExitProcedure();
        agentAIManager.DestroyAgent(paimonController);
    }

}

