//using System;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UIElements;

///// <summary>
///// 消息悬浮框
///// 需要挂载MessageDialog到Global空物体下
///// </summary>
//public class MessageDialog
//{
//    private static uiDocument messageDialogDocument;

//    /// <summary>
//    /// 加载
//    /// </summary>
//    private static void Load()
//    {
//        if (messageDialogDocument != null)
//            return;

//        // 查找消息框组件
//        var messageDialogGO = GameObject.Find("/Global").GetComponent<Global>().MessageDialogGO;
//        messageDialogDocument = messageDialogGO.GetComponent<uiDocument>();
//    }

//    /// <summary>
//    /// 弹出消息框
//    /// </summary>
//    /// <param name="message">消息内容</param>
//    /// <param name="okAction">确认按钮的操作</param>
//    /// <param name="okButtonText">确认按钮文字</param>
//    /// <param name="cancelAction">取消按钮的操作</param>
//    /// <param name="cancelButtonText">取消按钮的文字</param>
//    /// <param name="showCancel">是否显示取消按钮</param>
//    public static void Show(string message, string title, Action<GameObject> okAction = null, string okButtonText = "确认",
//        Action<GameObject> cancelAction = null, string cancelButtonText = "取消", bool showCancel = false)
//    {
//        UnityDispatcher.Invoke(() =>
//        ShowInner(message, title, okAction, okButtonText, cancelAction, cancelButtonText, showCancel));
//    }

//    private static void ShowInner(string message, string title, Action<GameObject> okAction = null, string okButtonText = "确认",
//    Action<GameObject> cancelAction = null, string cancelButtonText = "取消", bool showCancel = false)
//    {
//        Load();
//        // 当存在取消操作的时候，显示取消按钮
//        if (cancelAction != null)
//            showCancel = true;

//        var root = messageDialogDocument.rootVisualElement;
//        root.Q("TitleLabel")

//        messageBox.transform.Find("MessageText").GetComponent<TMP_Text>().text = message;

//        var canvasGroup = messageBox.GetComponent<CanvasGroup>();
//        canvasGroup.interactable = true;
//        canvasGroup.blocksRaycasts = true;
//        canvasGroup.DOFade(1.0f, 0.2f);
//        // 关闭消息框组件
//        void closeAction(Action<GameObject> buttonAction)
//        {
//            canvasGroup.interactable = false;
//            canvasGroup.blocksRaycasts = false;
//            canvasGroup.DOFade(0, 0.2f).OnComplete(() =>
//            {
//                buttonAction?.Invoke(messageBox);
//            });
//        }
//        // 是否显示取消按钮
//        if (showCancel)
//        {
//            messageBox.transform.Find("OkButtonSingle").gameObject.SetActive(false);
//            messageBox.transform.Find("OkButton").gameObject.SetActive(true);
//            messageBox.transform.Find("CancelButton").gameObject.SetActive(true);

//            var okButton = messageBox.transform.Find("OKButton");
//            okButton.Find("OkButtonText").GetComponent<TMP_Text>().text = okButtonText;
//            okButton.GetComponent<Button>().onClick.AddListener(() =>
//            {
//                closeAction(okAction);
//            });

//            var cancelButton = messageBox.transform.Find("CancelButton");
//            cancelButton.Find("CancelButtonText").GetComponent<TMP_Text>().text = cancelButtonText;
//            cancelButton.GetComponent<Button>().onClick.AddListener(() =>
//            {
//                closeAction(cancelAction);
//            });
//        }
//        else
//        {
//            messageBox.transform.Find("OkButtonSingle").gameObject.SetActive(true);
//            messageBox.transform.Find("OkButton").gameObject.SetActive(false);
//            messageBox.transform.Find("CancelButton").gameObject.SetActive(false);

//            var okButton = messageBox.transform.Find("OkButtonSingle");
//            okButton.Find("OkButtonSingleText").GetComponent<TMP_Text>().text = okButtonText;
//            okButton.GetComponent<Button>().onClick.AddListener(() =>
//            {
//                closeAction(okAction);
//            });
//        }
//    }
//}
