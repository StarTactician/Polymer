using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDPanel : BasePanel
{
    private static string name = "HUDPanel";
    private static string path = "Panel/HUDPanel";
    Button missionButton;
    Button achiveButton;
    Button buildingButton;
    GameObject missonBadge;
    GameObject achiveBadge;
    //GameObject buildingBadge;
    public static readonly UIType uitype = new UIType(path, name);
    public HUDPanel() : base(uitype)
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
        //UIFunction.Inst.GetCompoentInObjsChild<Text>(PanelObj, "FinishNum").text = MissionManager.GetInst().LoadTaskQueue().ToString();
        missionButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Mission");
        achiveButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Achivement");
        if (missonBadge != null)
            GameObject.Destroy(missonBadge);
        else if (MissionManager.GetInst().LoadTaskQueue() > 0)
            missonBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(missionButton.gameObject), MissionManager.GetInst().LoadTaskQueue());
        if (achiveBadge != null)
            GameObject.Destroy(achiveBadge);
        else if (TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount() > 0)
            achiveBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(achiveButton.gameObject), (TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount()));
        //coinValue = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue");
        //UIFunction.Inst.StartFade(0.2f, this, true);

        // **重置初始状态**
        RectTransform panelRect = PanelObj.GetComponent<RectTransform>();
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -Screen.height / 2);
        panelRect.localScale = Vector3.one * 0.5f;

        // **再次执行动画**
        UIFunction.Inst.StartScale(0.3f, panelRect, Vector3.one, 0f);
        UIFunction.Inst.MoveObject(PanelObj, Vector2.up, Screen.height / 2, 0.3f, this);
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Setting").onClick.AddListener(Setting);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Exit").onClick.AddListener(Exit);
        missionButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Mission");
        missionButton.onClick.AddListener(Mission);
        if (MissionManager.GetInst().LoadTaskQueue() > 0 && missonBadge == null)
            missonBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(missionButton.gameObject), MissionManager.GetInst().LoadTaskQueue());

        achiveButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Achivement");
        achiveButton.onClick.AddListener(Achivement);
        if (achiveBadge == null && TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount() > 0)
            achiveBadge = UIFunction.Inst.AddBadgeToImage(UIFunction.Inst.GetComponentInObj<Image>(achiveButton.gameObject), (TechTreeManager.GetInst().AmountUncheckedCount() + AchievementManager.GetInst().AmountUncheckedCount()));

        buildingButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "BulidingCheck");
        buildingButton.onClick.AddListener(CheckBuliding);
        //UIFunction.Inst.StartFade(0.2f, this, true);
        //OnEnable();

        RectTransform panelRect = PanelObj.GetComponent<RectTransform>();

        // **初始位置（屏幕下方）**
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -Screen.height / 2);

        // **初始缩放（缩小状态）**
        panelRect.localScale = Vector3.one * 0.5f;

        // **启动动画**
        UIFunction.Inst.StartScale(0.3f, panelRect, Vector3.one, 0f);  // **放大动画**
        UIFunction.Inst.MoveObject(PanelObj, Vector2.up, Screen.height / 2, 0.3f, this);  // **上移动画**

        
    }


    void Exit()
    {
        //推入是否返回地球的窗口

        UIManager.GetInst().Push(new ExitWindow());
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
    void CheckBuliding()
    {
        UIManager.GetInst().Push(new BuildCheckPanel());
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
}
