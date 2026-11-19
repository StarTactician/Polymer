using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingDescribeWindow : BasePanel
{
    private static string name = "BuildingDescribeWindow";
    private static string path = "Panel/BuildingDescribeWindow";
    public static readonly UIType uitype = new UIType(path, name);
    public string Description;
    public BuildingDescribeWindow() : base(uitype)
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();

    }

    public override void OnDisable()
    {
        base.OnDisable();


    }

    public override void OnEnable()
    {
        base.OnEnable();
        //UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartScale(0.2f, PanelObj.GetComponent<RectTransform>(), new Vector3(1f, 1f, 0.2f));
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "DescribeText").text = Description;
    }
    void Back()
    {
        UIManager.GetInst().PopWindow(this, true);//false
    }
}
