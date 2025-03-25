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
    private TextMeshProUGUI text;
    
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
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue").text = SaveManager.CurrentCoin.ToString();
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);
        levelButtons = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "Levels");
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinValue").text = SaveManager.CurrentCoin.ToString();
        foreach (Button button in levelButtons)
        {
            // 设置按钮的图片
            string buttonName = button.name;
            Sprite buttonSprite = Resources.Load<Sprite>("IMG/PolymerImg/" + buttonName);
            if (buttonSprite != null)
            {
                button.GetComponent<Image>().sprite = buttonSprite;
            }
            else
            {
                Debug.LogWarning("未能找到与按钮名称匹配的图片: " + buttonName);
            }
            button.onClick.AddListener(Select);
            for (int i = 0; i <= SaveManager.LastesLevel; i++)//i等于的值为解锁的关卡值,在窗口中的levelButtons[i].gameObject是全部关闭的状态
            {
                levelButtons[i].gameObject.SetActive(true);
            }
        }
    }
    
    void Select()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            Debug.Log(clickedButton.name);
            SaveManager.CurrentLevel= int.Parse(clickedButton.name);
            //SceneControl.GetInst().LoadLevel(SaveManager.CurrentLevel);
            ConfirmCoinCostPanel coinCostPanel = new ConfirmCoinCostPanel();
            coinCostPanel.Num = SaveManager.CountLevelCost();
            UIManager.GetInst().Push(coinCostPanel);
        }
        else
        {
            Debug.LogWarning("无法找到被点击的按钮");
        }
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
}
