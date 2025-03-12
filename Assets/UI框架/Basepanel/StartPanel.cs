using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartPanel : BasePanel
{
    private static string name = "StartPanel";
    private static string path = "Panel/StartPanel";
    private TextMeshProUGUI coinValue;
    Button missionButton;
    Button achiveButton;
    GameObject missonBadge;
    GameObject achiveBadge;
    public static readonly UIType uitype = new UIType(path, name);
    public StartPanel() : base(uitype)
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
        //UIFunction.Inst.GetCompoentInObjsChild<Text>(PanelObj, "FinishNum").text = MissionManager.GetInst().LoadTaskQueue().ToString();
        if (missonBadge != null)
            GameObject.Destroy(missonBadge);
        else if (MissionManager.GetInst().LoadTaskQueue() > 0)
            missonBadge=UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(missionButton.gameObject), MissionManager.GetInst().LoadTaskQueue());
        if(achiveBadge!=null)
            GameObject.Destroy(achiveBadge);
        else if(TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount() > 0)
            achiveBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(achiveButton.gameObject), (TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount()));
        coinValue = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue");
        coinValue.text = SaveManager.CurrentCoin.ToString();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();                 
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Start").onClick.AddListener(Start);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Setting").onClick.AddListener(Setting);

        missionButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Mission");
        missionButton.onClick.AddListener(Mission);
        if(MissionManager.GetInst().LoadTaskQueue()>0&& missonBadge == null)
            missonBadge=UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(missionButton.gameObject), MissionManager.GetInst().LoadTaskQueue());
        //UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(missionButton.gameObject), MissionManager.GetInst().LoadTaskQueue());
        //UIFunction.Inst.GetCompoentInObjsChild<Text>(PanelObj, "FinishNum").text = MissionManager.GetInst().LoadTaskQueue().ToString();
        achiveButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Achivement");
        achiveButton.onClick.AddListener(Achivement);        
        if(achiveBadge==null&&TechTreeManager.GetInst().AmountUncheckedCount()+AchievementManager.GetInst().AmountUncheckedCount()>0)        
            achiveBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(achiveButton.gameObject), (TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount()));
        
        coinValue = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue");
        coinValue.text = SaveManager.CurrentCoin.ToString();
        UIFunction.Inst.StartFade(0.2f, this, true);
        //OnEnable();
    }
    void Start()
    {
        UIManager.GetInst().Push(new LevelChoosePanel());
    }
    void Setting()
    {
        UIManager.GetInst().Push(new SettingPanel());
    }
    void Mission()
    {
        UIManager.GetInst().Push(new MissionViewPanel());
    }
    void Achivement()
    {
        UIManager.GetInst().Push(new AchiveTechPanel());
    }
    void EnterTestScene()
    {
        UIManager.GetInst().Push(new ConfirmCoinCostPanel());
    }
}
