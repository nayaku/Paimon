using System;
using Unity.Logging;
using UnityEngine;

public class ProcedureManager : MonoBehaviour
{
    public string BoostProcedureTypeString;

    public static ProcedureBase CurrentProcedure { get; private set; } = null;
    public static bool IsChange { get; private set; } = false;

    private static ProcedureManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var type = Type.GetType(BoostProcedureTypeString);
        ChangeProcedure(type);
    }

    /// <summary>
    /// 更变流程
    /// </summary>
    /// <param name="latestProcedure"></param>
    public static void ChangeProcedure<T>() where T : ProcedureBase
    {
        ChangeProcedure(typeof(T));
    }
    private static void ChangeProcedure(Type ProcedureType)
    {
        if (IsChange)
        {
            Log.Error("当前正在切换流程，无法执行切换流程操作！");
            return;
        }
        IsChange = true;

        var latestProcedure = (ProcedureBase)instance.gameObject.AddComponent(ProcedureType);
        // 处理切换
        var previousProcess = CurrentProcedure;
        if (previousProcess != null)
            previousProcess.PreExitProcedure();
        latestProcedure.PreEnterProcedure();
        if (CurrentProcedure != null)
            previousProcess.ExitProcedure();
        latestProcedure.EnterProcedure();
        CurrentProcedure = latestProcedure;
        Destroy(previousProcess);

        IsChange = false;
    }
}
