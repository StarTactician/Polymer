using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LabPanel : BasePanel
{
    private static string name = "LabPanel";
    private static string path = "Panel/LabPanel";

    //GameObject buildingBadge;
    public static readonly UIType uitype = new UIType(path, name);
    public LabPanel() : base(uitype)
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
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Start").onClick.AddListener(Start);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);



    }

    void Start()
    {
        UIManager.GetInst().Push(new LevelChoosePanel());
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
}
