using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TipsJumpWindow : BasePanel
{
    private static string name = "TipsJumpWindow";
    private static string path = "Panel/TipsJumpWindow";
    public string Text;
    public static readonly UIType uitype = new UIType(path, name);
    public TipsJumpWindow() : base(uitype)
    {

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UIFunction.Inst.StartFade(0.3f, this, false);
    }
    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Text").text=Text;
        UIFunction.Inst.StartFade(0.15f, this, true);
    }
}
