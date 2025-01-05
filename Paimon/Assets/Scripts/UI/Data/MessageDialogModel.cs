using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class MessageDialogModel : MonoBehaviour
{
    #region 数据绑定
    [CreateProperty]
    public string Title { get; set; } = "弹窗标题";

    [CreateProperty]
    public string Message { get; set; } = "弹窗消息内容！";

    [CreateProperty]
    public string ConfirmButtonText { get; set; } = "确认";

    [CreateProperty]
    public string CancelButtonText { get; set; } = "取消";

    [CreateProperty]
    public DisplayStyle CancelButtonDisplayMode { get; set; } = DisplayStyle.None;

    [CreateProperty]
    public DisplayStyle DisplayStyle { get; set; } = DisplayStyle.None;

    [CreateProperty]
    public float Opacity { get; set; } = 0.0f;
    #endregion

    public Action ConfirmAction { get; set; }

    public Action CancelAction { get; set; }

    private static MessageDialogModel messageDialogModel = null;
    private UIDocument uiDocument;

    private enum ButtonTypeEnum { Confirm, Cancel }

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        uiDocument.rootVisualElement.dataSource = this;
        messageDialogModel = this;
    }

    void OnEnable()
    {
        // 绑定事件
        var rootElement = uiDocument.rootVisualElement;
        var confirmButton = rootElement.Q<Button>("ConfirmButton");
        confirmButton.RegisterCallback<PointerDownEvent, ButtonTypeEnum>(OnButtonDown, ButtonTypeEnum.Confirm);
        var cancelButton = rootElement.Q<Button>("CancelButton");
        cancelButton.RegisterCallback<PointerDownEvent, ButtonTypeEnum>(OnButtonDown, ButtonTypeEnum.Cancel);
    }

    //void OnDisable()
    //{
    //    // 解绑事件
    //    var rootElement = uiDocument.rootVisualElement;
    //    var confirmButton = rootElement.Q<Button>("ConfirmButton");
    //    confirmButton.UnregisterCallback<PointerDownEvent, ButtonTypeEnum>(OnButtonDown);
    //    var cancelButton = rootElement.Q<Button>("CancelButton");
    //    cancelButton.UnregisterCallback<PointerDownEvent, ButtonTypeEnum>(OnButtonDown);
    //}

    //IEnumerator Start()
    //{
    //    yield return new WaitForSeconds(5);
    //    MessageDialogModel.Show("Hi", "Content!");
    //}

    /// <summary>
    /// 弹出消息框
    /// </summary>
    /// <param name="title">消息标题</param>
    /// <param name="message">消息内容</param>
    /// <param name="confirmAction">确认按钮的操作</param>
    /// <param name="confirmButtonText">确认按钮文字</param>
    /// <param name="cancelAction">取消按钮的操作</param>
    /// <param name="cancelButtonText">取消按钮的文字</param>
    /// <param name="showCancel">是否显示取消按钮</param>
    public static void Show(string title, string message, Action<GameObject> confirmAction = null, string confirmButtonText = "确认",
        Action<GameObject> cancelAction = null, string cancelButtonText = "取消", bool showCancel = false)
    {
        UnityDispatcher.Invoke(() =>
        messageDialogModel.ShowInner(title, message, confirmAction, confirmButtonText, cancelAction, cancelButtonText, showCancel));
    }

    private void ShowInner(string title, string message, Action<GameObject> confirmAction = null, string confirmButtonText = "确认",
    Action<GameObject> cancelAction = null, string cancelButtonText = "取消", bool showCancel = false)
    {
        // 当存在取消操作的时候，显示取消按钮
        if (cancelAction != null)
            showCancel = true;

        // 绑定数据
        Title = title;
        Message = message;
        ConfirmButtonText = confirmButtonText;
        CancelButtonText = cancelButtonText;
        if (showCancel)
            CancelButtonDisplayMode = DisplayStyle.Flex;
        else
            CancelButtonDisplayMode = DisplayStyle.None;

        // 显示
        DisplayStyle = DisplayStyle.Flex;
        Opacity = 1.0f;
    }

    private void OnButtonDown(PointerDownEvent e, ButtonTypeEnum buttonType)
    {
        Opacity = 0.0f;
        DisplayStyle = DisplayStyle.None; // TODO 这里有点小问题

        if (buttonType == ButtonTypeEnum.Confirm)
        {
            ConfirmAction?.Invoke();
        }
        else if (buttonType == ButtonTypeEnum.Cancel)
        {
            CancelAction?.Invoke();
        }
    }
}
