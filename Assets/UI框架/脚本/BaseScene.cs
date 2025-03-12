using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseScene 
{
    [Tooltip("当前场景的类型，主界面0，主线关1，无尽关2，胜利结算3，失败结算4，该字段用于音频播放")]
    public static int CurrentType;
    public static event System.Action OnEnterScene;
    public abstract void EnterScene();
    public abstract void ExitScene();
    protected void InvokeEnterScene()
    {
        OnEnterScene?.Invoke();
    }
}
