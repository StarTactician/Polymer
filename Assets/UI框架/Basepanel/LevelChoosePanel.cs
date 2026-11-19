using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelChoosePanel : BasePanel
{
    private static string name = "LevelChoosePanel";
    private static string path = "Panel/LevelChoosePanel";
    public static readonly UIType uitype = new UIType(path, name);
    private List<Button> levelButtons;
    private List<Button> switchButtons;
    public GameObject part1, part2, part3;
    /*private Button common;
    private Button engineering;
    private Button special;*/

    public LevelChoosePanel() : base(uitype)
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
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue").text = SaveManager.CurrentPU.ToString();
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);
        levelButtons = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "Levels");
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue").text = SaveManager.CurrentPU.ToString();

        part1 = UIFunction.Inst.GetChild(PanelObj, "part1");
        part2 = UIFunction.Inst.GetChild(PanelObj, "part2");
        part3 = UIFunction.Inst.GetChild(PanelObj, "part3");
        switchButtons = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "switchButtons");
        InitSwitchButtons();

        foreach (Button button in levelButtons)
        {
            string buttonName = button.name;

            // 加载与按钮名称对应的图片（需放在 Resources/IMG/PolymerImg/ 下）
            Sprite buttonSprite = Resources.Load<Sprite>("IMG/PolymerImg/" + buttonName);
            
            if (buttonSprite != null)
            {
                UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "target").sprite = buttonSprite;
                //button.GetComponent<Image>().sprite = buttonSprite;
            }
            
            else
            {
                button.GetComponent<Image>().sprite = Resources.Load<Sprite>("IMG/LittleUI/NotFound");
                UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "target").sprite = Resources.Load<Sprite>("IMG/LittleUI/NotFound");
                Debug.LogWarning("未能找到与按钮名称匹配的图片: " + buttonName);
            }
            int levelNum = int.Parse(button.name);
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(button.gameObject, "Text").text = "第" + buttonName + "关:" + TechTreeManager.GetInst().GetTech(levelNum).TechCNName;
            
            if(SaveManager.LastesLevel==0)
            {

            }
            else if (levelNum > SaveManager.LastesLevel)
                continue;

            int starCount = SaveManager.GetInst().GetLevelStar(levelNum);
            GameObject starContainer = UIFunction.Inst.GetChild(button.gameObject, "StarEmpty");

            if (starContainer != null)
            {
                List<Image> stars = UIFunction.Inst.GetComponentsInObj<Image>(starContainer);
                for (int i = 0; i < stars.Count; i++)
                {
                    UIFunction.Inst.SetImageVise(stars[i], i < starCount);
                }
            }

            // 添加点击事件
            button.onClick.AddListener(Select);
        }

        // 只激活已解锁的关卡按钮
        for (int i = 0; i < levelButtons.Count; i++)
        {
            // 只显示已构建的关卡按钮（小于 MaxLevel）
            if (i < SaveManager.GetInst().MaxLevel && i < SaveManager.LastesLevel)
            {
                UIFunction.Inst.SetCanvasGroupVise(levelButtons[i].gameObject, true);
            }
            else
            {
                UIFunction.Inst.SetCanvasGroupVise(levelButtons[i].gameObject, false);
            }
        }




        DialogueManager.GetInst().TryTriggerDialogue("LaboratoryPanelIntro");
    }

    void Select()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            Debug.Log(clickedButton.name);
            //SaveManager.CurrentLevel= int.Parse(clickedButton.name);
            //SceneControl.GetInst().LoadLevel(SaveManager.CurrentLevel);
            ConfirmCoinCostPanel coinCostPanel = new ConfirmCoinCostPanel();
            coinCostPanel.Num = SaveManager.CountLevelCost(int.Parse(clickedButton.name));
            coinCostPanel.Level= int.Parse(clickedButton.name);
            UIManager.GetInst().Push(coinCostPanel);
        }
        else
        {
            Debug.LogWarning("无法找到被点击的按钮");
        }
    }
    void InitSwitchButtons()
    {
        // 添加监听器
        for (int i = 0; i < switchButtons.Count; i++)
        {
            int index = i; // 闭包变量
            switchButtons[i].onClick.AddListener(() => OnSwitchButtonClick(index));
        }

        int lastLevel = SaveManager.LastesLevel;

        // 默认显示 part1（按钮0）
        OnSwitchButtonClick(0);

        Debug.Log("成功初始化关卡界面");
    }

    void OnSwitchButtonClick(int index)
    {
        Debug.Log("关卡界面切换成功");
        for (int i = 0; i < switchButtons.Count; i++)
        {
            UIFunction.Inst.SetCanvasGroupVise(switchButtons[i].gameObject, i != index);
        }

        UIFunction.Inst.SetCanvasGroupVise(part1, index == 0);
        UIFunction.Inst.SetCanvasGroupVise(part2, index == 1);
        UIFunction.Inst.SetCanvasGroupVise(part3, index == 2);
    }


    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
}
