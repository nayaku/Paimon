using UnityEngine;

public class Global : MonoBehaviour
{
    public GameObject MessageDialogGO;
    public GameObject DialogueGO;
    public GameObject AgentAIManagerGO;
    public GameObject AudioInputListenerGO;

    public static Global Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// …Ë÷√ƒ¨»œ÷°¬ 
    /// </summary>
    private void SetDefaultFrameRate()
    {
        Application.targetFrameRate = 60;
    }
}
