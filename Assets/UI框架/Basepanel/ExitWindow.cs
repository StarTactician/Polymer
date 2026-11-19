using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExitWindow : BasePanel
{
    private static string name = "ExitWindow";
    private static string path = "Panel/ExitWindow";
    public static readonly UIType uitype = new UIType(path, name);
    public ExitWindow() : base(uitype)
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
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Cancel").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Confirm").onClick.AddListener(Confirm);

    }
    void Back()
    {
        UIManager.GetInst().PopOnceOnlyUsePanel();
    }
    void Confirm()
    {
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
