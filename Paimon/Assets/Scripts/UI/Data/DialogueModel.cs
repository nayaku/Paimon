using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueModel : MonoBehaviour
{
    #region 数据绑定
    /// <summary>
    /// 对话框名字
    /// </summary>
    [CreateProperty]
    public string Name { get; set; } = "派蒙";

    /// <summary>
    /// 对话框内容
    /// </summary>
    [CreateProperty]
    public string Content { get; set; } = "你好啊，我是派蒙~";

    /// <summary>
    /// 用户的输入内容
    /// </summary>
    [CreateProperty]
    public string UserContent { get; set; } = "你好派蒙";

    /// <summary>
    /// 下一句图标显示
    /// </summary>
    [CreateProperty]
    public DisplayStyle NextIconDisplayStyle { get; set; } = DisplayStyle.None;

    public EventCallback<PointerDownEvent> OnHistoryButtonPointerDown { get; set; }
    public EventCallback<PointerDownEvent> OnConfigureButtonDown { get; set; }

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

        // 设置下一句图标动画
        var nextIcon = rootElement.Q<VisualElement>("NextIcon");
        nextIcon.RegisterCallback<TransitionEndEvent>(evt => { nextIcon.ToggleInClassList("next-button-ani-down"); });
        rootElement.schedule.Execute(() => nextIcon.ToggleInClassList("next-button-ani-down")).StartingIn(1000);

        // 绑定按钮
        var historyButton = rootElement.Q<Button>("HistoryButton");
        historyButton.RegisterCallback<PointerDownEvent>(evt => OnHistoryButtonPointerDown?.Invoke(evt));
        var configureButton = rootElement.Q<Button>("ConfigureButton");
        configureButton.RegisterCallback<PointerDownEvent>(evt => OnConfigureButtonDown?.Invoke(evt));
    }

}
