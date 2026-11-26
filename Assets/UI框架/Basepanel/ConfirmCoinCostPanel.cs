using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmCoinCostPanel : BasePanel
{
    private static string name = "ConfirmCoinCostPanel";
    private static string path = "Panel/ConfirmCoinCostPanel";
    public static readonly UIType uitype = new UIType(path, name);
    private TextMeshProUGUI coinNum;
    private bool closed;
    public int Num;
    public int Level;
    public ConfirmCoinCostPanel() : base(uitype)
    {

    }


    public override void OnDestroy()
    {
        closed = true;
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
        coinNum = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "costNum");
        CoinNum(Num);
    }
    void Back()
    {
        if (closed || SceneControl.GetInst().IsLevelEntryPending) return;
        closed = true;
        UIManager.GetInst().PopOnceOnlyUsePanel();
    }

    void Confirm()
    {
        if (closed || PanelObj == null || !PanelObj.activeInHierarchy) return;
        SceneControl.LevelEntryStatus result = SceneControl.GetInst().TryBeginLevelLoad(Level, Num);
        if (result == SceneControl.LevelEntryStatus.Started || result == SceneControl.LevelEntryStatus.Busy) return;

        closed = true;
        UIManager.GetInst().PopOnceOnlyUsePanel();
        TipsJumpWindow tip = new TipsJumpWindow
        {
            Text = result == SceneControl.LevelEntryStatus.InsufficientPU
                ? "\u57fa\u5143\u4e0d\u8db3\uff0c\u65e0\u6cd5\u5f00\u59cb\u7814\u53d1\u3002"
                : "\u8be5\u5173\u5361\u76ee\u524d\u65e0\u6cd5\u8fdb\u5165\u3002"
        };
        UIManager.GetInst().PushTips(tip, 2.5f);
    }

    void CoinNum(int num)
    {
        coinNum.text = "进入关卡将花费 "+num.ToString();
    }
}
