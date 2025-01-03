using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueModel : MonoBehaviour
{
    #region 数据绑定
    [CreateProperty]
    public string Name { get; set; } = "派蒙";

    [CreateProperty]
    public string Content { get; set; } = "你好啊，我是派蒙~";

    [CreateProperty]
    public DisplayStyle NextIconDisplayStyle { get; set; } = DisplayStyle.None;
    #endregion
    private UIDocument uiDocument;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        uiDocument.rootVisualElement.dataSource = this;
    }

    void OnEnable()
    {
        var rootElement = uiDocument.rootVisualElement;
        var nextIcon = rootElement.Q<VisualElement>("NextIcon");
        nextIcon.RegisterCallback<TransitionEndEvent>(evt => { nextIcon.ToggleInClassList("next-button-ani-down"); });
        rootElement.schedule.Execute(() => nextIcon.ToggleInClassList("next-button-ani-down")).StartingIn(1000);
    }

}
