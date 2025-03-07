using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DescribeWindow : BasePanel
{
    private static string name = "DescribeWindow";
    private static string path = "Panel/DescribeWindow";
    public static readonly UIType uitype = new UIType(path, name);
    public bool IsLocked;
    public string TechDescription;
    //public string UnlockWay;
    public string TechName;
    public int techLevel;
    public Sprite TechSprite= Resources.Load<Sprite>("IMG/TechImag/Locked");//Ä¬ÈÏÍ¼Æ¬
    public DescribeWindow() : base(uitype)
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
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "DescribeText").text= TechDescription;
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Title").text = TechName;
        UIFunction.Inst.SetCanvasGroupVise(UIFunction.Inst.GetChild(PanelObj, "LockState").gameObject,IsLocked);
        UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image").sprite = TechSprite;
        switch (techLevel)
        {
            case 1:
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgeWhite");
                break;
            case 2:
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgeBlue");
                break;
            case 3:
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgePurple");
                break;
            case 4:
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgeGloden");
                break;
            default:
                break;
        }
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Rank").text = UIFunction.Inst.RomanMap[techLevel];
    }
    void Back()
    {
        UIManager.GetInst().PopWindow(this, true);//false
    }
}
