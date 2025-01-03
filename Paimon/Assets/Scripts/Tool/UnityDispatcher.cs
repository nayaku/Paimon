using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

/// <summary>
/// Unity主线程调度器
/// </summary>
public class UnityDispatcher : MonoBehaviour
{
    public static int MainThreadId { get; private set; } = -1;

    private static ConcurrentQueue<TaskAction> queue = new();

    public static bool IsMainThread() => MainThreadId == Thread.CurrentThread.ManagedThreadId;

    void Start()
    {
        MainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    void Update()
    {
        while (!queue.IsEmpty)
        {
            var result = queue.TryDequeue(out var invokeAction);
            if (!result)
                break;
            invokeAction.Action();
            invokeAction.DoneCallBack?.Invoke();
        }
    }

    public static void Invoke(Action action, Action doneCallBack = null)
    {
        // 如果请求是主线程，就直接调度
        if (IsMainThread())
        {
            action();
            doneCallBack?.Invoke();
            return;
        }

        // 插入任务队列
        var taskAction = new TaskAction(action, doneCallBack);
        queue.Enqueue(taskAction);
    }

    private class TaskAction
    {
        public Action Action { get; }
        public Action DoneCallBack { get; }

        public TaskAction(Action action, Action doneCallBack)
        {
            Action = action;
            DoneCallBack = doneCallBack;
        }
    }
}
