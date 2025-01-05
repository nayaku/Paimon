using UnityEngine;
/// <summary>
/// 流程
/// 调用顺序：
/// PreExitProcedure    previous procedure
/// PreEnterProcedure   latest procedure
/// ExitProcedure       previous procedure
/// EnterProcedure      latest procedure
/// </summary>
public class ProcedureBase : MonoBehaviour
{
    public virtual void PreEnterProcedure() { }
    public virtual void EnterProcedure() { }
    public virtual void PreExitProcedure() { }
    public virtual void ExitProcedure() { }
}

