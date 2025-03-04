using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AchiveTechPanel : BasePanel
{
    private static string name = "AchiveTechPanel";
    private static string path = "Panel/AchiveTechPanel";
    public static readonly UIType uitype = new UIType(path, name);
    Button techButton;
    Button achiveButton;
    GameObject techBadge;
    GameObject achiveBadge;
    public AchiveTechPanel() : base(uitype)
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
        if (techBadge != null)
            GameObject.Destroy(techBadge);
        else if (TechTreeManager.GetInst().AmountUncheckedCount() > 0)
            techBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(techButton.gameObject), TechTreeManager.GetInst().AmountUncheckedCount(), 65, 10, 10);
        if (achiveBadge != null)
            GameObject.Destroy(achiveBadge);
        else if (AchievementManager.GetInst().AmountUncheckedCount() > 0)
            achiveBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(achiveButton.gameObject), TechTreeManager.GetInst().AmountUncheckedCount(), 100, 10, 10);

        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        achiveButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "AchivementButton");
        techButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "TechTreeButton");

        achiveButton.onClick.AddListener(Achivement);
        techButton.onClick.AddListener(TechTree);

        if (techBadge == null && TechTreeManager.GetInst().AmountUncheckedCount() > 0)
            techBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(techButton.gameObject), TechTreeManager.GetInst().AmountUncheckedCount(), 65, 10, 10);
        if (achiveBadge == null &&  AchievementManager.GetInst().AmountUncheckedCount() > 0)
            achiveBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(achiveButton.gameObject),  AchievementManager.GetInst().AmountUncheckedCount(),100,10,10);


        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.StartFade(0.2f, this, true);
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
    void Achivement()
    {
        UIManager.GetInst().Push(new TechTreePanel());
    }
    void TechTree()
    {
        UIManager.GetInst().Push(new TechTreePanel());
    }
}
