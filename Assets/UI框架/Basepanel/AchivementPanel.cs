using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchivementPanel : BasePanel
{
    private static string name = "AchivementPanel";
    private static string path = "Panel/AchivementPanel";
    public static readonly UIType uitype = new UIType(path, name);
    private string loadPath = "IMG/AchiveImg/";
    int lastButtonName;
    DescribeWindow describeWindow = null;
    private List<Button> achiveButtons;
    ScrollRect scrollRect;
    private List<Button> switchButtons;
    public GameObject part1, part2, part3;
    public Dictionary<int, GameObject> ButtonBadges_Dict = new Dictionary<int, GameObject>();//将按钮标号与New标识的物体一一对应
    public AchivementPanel() : base(uitype)
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
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        achiveButtons = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "Achive");
        scrollRect = UIFunction.Inst.GetCompoentInObjsChild<ScrollRect>(PanelObj, "Scroll");

        part1 = UIFunction.Inst.GetChild(PanelObj, "part1");
        part2 = UIFunction.Inst.GetChild(PanelObj, "part2");
        part3 = UIFunction.Inst.GetChild(PanelObj, "part3");
        switchButtons = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "switchButtons");
        InitSwitchButtons();

        //改成for循环才能实现每个隔一点时间缩放出来，
        for (int i = 0; i < achiveButtons.Count; i++)//AchievementManager.GetInst().GetAchivesNum()
        {
            Button button = achiveButtons[i];
            int buttonName = int.Parse(achiveButtons[i].name);
            if (!AchievementManager.IsAchiveUnlocked(i+1)) // 未解锁，设置黑白颜色 + 上锁图标
            {
                //Image LockedImage = button.GetComponent<Image>();//黑白需要调饱和度，不好实现，还需要别的办法
                
                // 锁定图标可以加上 sprite
                // UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "techImage").sprite = Resources.Load<Sprite>("IMG/TechImag/");
            }
            else
            {
                Sprite techSprite = Resources.Load<Sprite>(loadPath + buttonName);
                //Image achiveImage = UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "techImage");
                Image achiveImage = UIFunction.Inst.GetComponentInObj<Image>(button.gameObject);
                //UIFunction.Inst.StartFade(0.1f, UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "techImage"), true);//锁图标初始透明度为0
                if (AchievementManager.GetInst().GetAchiev(buttonName).hasCheck == 1)
                {
                    ButtonBadges_Dict.Add(buttonName, UIFunction.Inst.AddBadgeToImage(achiveImage, null, 200, 80, 70, false));
                    Debug.Log("在编号" + buttonName + "上添加new图标成功");
                }
                if (techSprite == null)
                {
                    techSprite = Resources.Load<Sprite>("IMG/LittleUI/NotFound");
                    //UIFunction.Inst.StartFade(0.8f, LockedImage, false);
                }
                achiveImage.sprite = techSprite;
            }
            // 计算延迟时间
            float waitTime = i * 0.25f;

            // **执行放大动画**
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            UIFunction.Inst.StartScale(0.2f, rectTransform, new Vector3(1.2f, 1.2f, 1), waitTime); // 放大
            UIFunction.Inst.StartScale(0.05f, rectTransform, new Vector3(1.0f, 1.0f, 1), waitTime + 0.2f); // 回弹

            button.onClick.AddListener(Select);
        }

    }
    void Select()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            
            int buttonName = int.Parse(clickedButton.name);
            if (buttonName != lastButtonName)
            {
                UIManager.GetInst().PopAllWindow();
            }
            else
                return;

            lastButtonName = buttonName; // 更新上一个按钮记录
            Achievement selectedAchiev = AchievementManager.GetInst().GetAchiev(buttonName);//根据id获取
            if (selectedAchiev == null)
                return;
            if (selectedAchiev.hasCheck != 0)//领取奖励也在这里实现
            {
                selectedAchiev.hasCheck = 0;
                if (ButtonBadges_Dict.ContainsKey(buttonName))
                {
                    GameObject.Destroy(ButtonBadges_Dict[buttonName]);
                    ButtonBadges_Dict.Remove(buttonName);
                }
                GetCoinPanel coinPanel = new GetCoinPanel();               
                SaveManager.GetInst().ResourcesChange(selectedAchiev.RewardType.targetValue, selectedAchiev.RewardType.type);
                switch(selectedAchiev.RewardType.type)
                {
                    case ("PU"):
                        coinPanel.PUNumber = selectedAchiev.RewardType.targetValue;
                        break;

                    case ("coin"):
                        coinPanel.CoinNumber = selectedAchiev.RewardType.targetValue;
                        break;                    
                    default:
                        break;
                }
                UIManager.GetInst().Push(coinPanel);
                AchievementManager.GetInst().SaveAchievementLibrary();
            }
            describeWindow = new DescribeWindow();
            //describeWindow.techLevel = selectedTech.TechLevel;
            if (AchievementManager.IsAchiveUnlocked(buttonName))//只有已解锁的才会传递成就描述，否则显示未解锁以及解锁方式
            {
                describeWindow.IsLocked = false;
                describeWindow.TechName = selectedAchiev.AchieveName;
                describeWindow.TechDescription = selectedAchiev.Description;
                Sprite sprite= Resources.Load<Sprite>(loadPath + buttonName);
                if(sprite!=null)
                    describeWindow.TechSprite = sprite;
                else describeWindow.TechSprite = Resources.Load<Sprite>("IMG/LittleUI/NotFound");


            }
            else
            {
                describeWindow.IsLocked = true;
                describeWindow.TechName = "???";
                describeWindow.TechDescription = selectedAchiev.lockedInfo;
                describeWindow.TechSprite = Resources.Load<Sprite>("IMG/LittleUI/NotFound");
            }
            Debug.Log(clickedButton.name);
            ScrollToButton(buttonName);
            UIManager.GetInst().Push(describeWindow, true);
        }
        else
        {
            Debug.LogWarning("无法找到被点击的按钮");
        }
    }
    void ScrollToButton(int index)
    {
        //ScrollRect scrollRect = UIFunction.Inst.GetCompoentInObjsChild<ScrollRect>(PanelObj, "Scroll");
        if (scrollRect == null)
        {
            Debug.LogWarning("未找到 ScrollRect 组件！");
            return;
        }

        RectTransform contentRect = scrollRect.content; // 获取 ScrollRect 的内容区域

        // x = (index - 1) * 480
        float targetX = -(index - 1) * 580;//我也不知道为什么减一不行减二行
        // 限制范围，防止滑动超出边界
        float minX = -580;//数据全靠试
        float maxX = contentRect.rect.width - scrollRect.viewport.rect.width;
        targetX = Mathf.Clamp(targetX, -maxX, -minX);

        // 启动平滑滚动协程
        UIFunction.Inst.StartSmoothScroll(contentRect, targetX);
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
        lastButtonName = 0;
        if (describeWindow != null)
        {
            // 确保对象仍然存在
            if (describeWindow.PanelObj != null && UIManager.GetInst().Windows_Dict.ContainsKey(describeWindow.PanelObj.name))
            {
                UIManager.GetInst().PopWindow(describeWindow, true);
            }
            describeWindow = null; // 置空，避免后续访问已销毁的对象
        }
        UIManager.GetInst().Pop(false);
    }

}
