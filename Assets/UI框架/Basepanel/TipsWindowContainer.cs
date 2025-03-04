using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipsWindowContainer : BasePanel
{
    private static string name = "TipsWindowContainer";
    private static string path = "Panel/TipsWindowContainer";
    public static readonly UIType uitype = new UIType(path, name);
    public TipsWindowContainer() : base(uitype)
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
        UIFunction.Inst.StartFade(0.15f, this, true);
    }
}
