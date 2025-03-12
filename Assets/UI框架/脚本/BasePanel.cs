using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 添加一个UI界面的步骤，完成一个UI预制体拖进Res，并写完对应的BasePanel脚本
/// </summary>
public class BasePanel 
{
    //UI面板脚本的父类，存放UI界面的操作函数
    public UIType uiType;
    /// <summary>
    /// 在场景中对应的UI或者窗口的Gameobject
    /// </summary>
    public GameObject PanelObj;

    //public GameObject PanelObj;//对应的窗口物体
    public bool IsMoveing;//是否在被协程所移动
    public BasePanel(UIType uitype)
    {
        uiType = uitype;
    }
    public virtual void OnStart()//游戏开始时
    {
        UIFunction.Inst.GetComponentInObj<CanvasGroup>(PanelObj).interactable = true;
        //UIFunction.GetInst().StartFadeIn(0.2f);
    }
    public virtual void OnEnable()//被开启时
    {
        UIFunction.Inst.GetComponentInObj<CanvasGroup>(PanelObj).interactable = true;
    }
    public virtual void OnDisable()//被关闭时
    {
        UIFunction.Inst.GetComponentInObj<CanvasGroup>(PanelObj).interactable = false;
        //CorrespondPanelObj.SetActive(false);
        
    }
    public virtual void OnDestroy()//切换场景时
    {
        UIFunction.Inst.GetComponentInObj<CanvasGroup>(PanelObj).interactable = false;
    }
}
