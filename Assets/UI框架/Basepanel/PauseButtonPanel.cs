using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ×óÉÏ½ÇµÄÔÝÍ£°´Å¥
/// </summary>
public class PauseButtonPanel : BasePanel
{
    private static string name = "PauseButtonPanel";
    private static string path = "Panel/PauseButtonPanel";
    public static readonly UIType uitype = new UIType(path, name);
    public PauseButtonPanel() : base(uitype)
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
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Pause").onClick.AddListener(Pause);
    }

    void Pause()
    {
        UIManager.Pause(true);
        UIManager.GetInst().Push(new PausePanel());
    }

}
