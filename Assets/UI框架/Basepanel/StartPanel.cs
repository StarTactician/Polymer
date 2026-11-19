using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartPanel : BasePanel
{
    private static string name = "StartPanel";
    private static string path = "Panel/StartPanel";
    /*Button LabButton;
    Button HUDButton;
    Button ExitButton;
    GameObject HUDBadge;*/
    public Slider slider;
    public int CurrentProgress;
    public TextMeshProUGUI percent;
    public TextMeshProUGUI ratio;
    public TextMeshProUGUI progress;
    private TextMeshProUGUI coinValue;
    private TextMeshProUGUI PUValue;
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
        ResetText();
        if (GameRoot.GetInst().SecondaryCamera == null)
            GameRoot.GetInst().FindCamera();
        if (GameRoot.GetInst().SecondaryCamera.activeSelf)
            GameRoot.GetInst().SecondaryCamera.SetActive(false);
        //AudioManager.Inst.Play();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        ResetText();
        if(GameRoot.GetInst().SecondaryCamera==null)
            GameRoot.GetInst().FindCamera();
        if (GameRoot.GetInst().SecondaryCamera.activeSelf)
            GameRoot.GetInst().SecondaryCamera.SetActive(false);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "EnterLab").onClick.AddListener(EnterLab);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "EnterHUD").onClick.AddListener(CheckHUD);
        UIFunction.Inst.StartFade(0.2f, this, true);
        AudioManager.Inst.Play();
    }
    
    void EnterLab()
    {
        //推入实验室面板，应当包含主线，肉鸽按钮
        UIManager.GetInst().Push(new LabPanel());
    }
    void CheckHUD()
    {
        //查看基地详情面板，应当包含任务按钮，成就按钮，设置按钮，建筑详情查看按钮
        UIManager.GetInst().Push(new HUDPanel());
    }
    
    void ResetText()
    {
        SaveManager.GetInst().UpdateProgress();
        coinValue = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue");
        PUValue = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "PUValue");

        slider = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "Slider");
        percent = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "%");
        ratio = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "/");
        progress = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "level");
        slider.value = SaveManager.GetInst().PlayerProgress;
        percent.text = SaveManager.GetInst().PlayerProgress / 100 + "%";
        ratio.text = SaveManager.GetInst().PlayerProgress + "/" + 10000;
        if (SaveManager.GetInst().PlayerProgress >= 3000)
            progress.text = "第二阶段";
        if (SaveManager.GetInst().PlayerProgress >= 6000)
            progress.text = "第三阶段";
        if (SaveManager.GetInst().PlayerProgress >= 10000)
            progress.text = "初步开发已完成";

        coinValue.text = SaveManager.CurrentCoin.ToString();
        PUValue.text = SaveManager.CurrentPU.ToString();
    }
    /*  void EnterTestScene()
      {
          UIManager.GetInst().Push(new ConfirmCoinCostPanel());
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
    void CheckBuliding()
    {
        UIManager.GetInst().Push(new BuildCheckPanel());
    }
    */
}
