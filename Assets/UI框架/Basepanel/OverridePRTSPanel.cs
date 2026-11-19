using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class OverridePRTSPanel : BasePanel
{
    private static string name = "OverridePRTSPanel";
    private static string path = "Panel/OverridePRTSPanel";
    private int targetLevel;
    private List<PosForceDate> pendingSteps;
    public static readonly UIType uitype = new UIType(path, name);
    public OverridePRTSPanel() : base(uitype)
    {

    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }
    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Confirm").onClick.AddListener(OnConfirm);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Cancel").onClick.AddListener(Back);
    }
    public void Setup(int level, List<PosForceDate> steps)
    {
        targetLevel = level;
        pendingSteps = new List<PosForceDate>(steps);
        Debug.Log("数据已初始化，当前关卡"+level+"当前步数"+steps.Count);
    }

    public void OnConfirm()
    {
        PRTS.GetInst().OverwriteLevelSteps(targetLevel, pendingSteps);
        UIManager.GetInst().Pop(false);
    }

    public void Back()
    {
        UIManager.GetInst().Pop(false);
    }
}
