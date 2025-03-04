using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TechTreePanel : BasePanel
{
    private static string name = "TechTreePanel";
    private static string path = "Panel/TechTreePanel";
    public static readonly UIType uitype = new UIType(path, name);
    private string loadPath = "IMG/TechImg/";
    TechDescribeWindow techWindow;
    private List<Button> techButtons;
    public Dictionary<int,GameObject> ButtonBadges_Dict = new Dictionary<int,GameObject>();//将按钮与科技ID一一对应
    ScrollRect scrollRect;
    public TechTreePanel() : base(uitype)
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        UIManager.GetInst().PopWindow(techWindow, true);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);
        UIManager.GetInst().PopWindow(techWindow, true);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }
    public override void OnStart()
    {
        base.OnStart();
        ButtonBadges_Dict.Clear();
        UIFunction.Inst.StartFade(0.2f, this, true);
        techButtons = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "Techs");
        scrollRect = UIFunction.Inst.GetCompoentInObjsChild<ScrollRect>(PanelObj, "Scroll");
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        foreach (Button button in techButtons)//根据按钮的名字来确定其展示的科技ID，并根据科技的解锁情况来判定是否出现指定科技图片
        {
            //先判断其名字对应的科技是否解锁           
            int buttonName = int.Parse(button.name);
            if (TechTreeManager.IsTechnologyUnlocked(buttonName))//只有已解锁的才显示图片,如果已解锁并且hasCheck=1，就显示NEW图标,并且NEW的会显示解锁动画
            {
                // 设置按钮的图片           
                //解锁动画buttonSprite逐渐透明，子物体techImage图片变为指定图片
                Sprite techSprite = Resources.Load<Sprite>(loadPath + buttonName);
                Image techImage= UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "techImage");
                if (TechTreeManager.GetInst().GetTech(buttonName).hasCheck==1)
                {
                    ButtonBadges_Dict.Add(buttonName, UIFunction.Inst.AddBadgeToImage(techImage, null, 200, 80, 70, false));
                }
                //Image LockedImage = button.GetComponent<Image>();
                if (techSprite != null)
                {
                    techImage.sprite = techSprite;
                    //UIFunction.Inst.StartFade(0.8f, LockedImage, false);
                }
                else
                {
                    Debug.LogWarning("未能找到与按钮名称匹配的图片: " + buttonName);
                }
            }               
            button.onClick.AddListener(Select);
        }
    }
    void Select()//点击后(先关闭现有窗口)出现介绍窗口，并将选定的科技滑动到面板左边,并将hascheck设置为0
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            UIManager.GetInst().PopAllWindow();
            int buttonName = int.Parse(clickedButton.name);
            Technology selectedTech=TechTreeManager.GetInst().GetTech(buttonName);//根据id获取
            if(selectedTech.hasCheck != 0)
            {
                selectedTech.hasCheck = 0;              
                GameObject.Destroy(ButtonBadges_Dict[buttonName]);
                ButtonBadges_Dict.Remove(buttonName);
                TechTreeManager.GetInst().SaveTechTree();
            }
            
            techWindow = new TechDescribeWindow();
            techWindow.techLevel = selectedTech.TechLevel;
            if (TechTreeManager.IsTechnologyUnlocked(buttonName))//只有已解锁的才会传递成就描述，否则显示未解锁以及解锁方式
            {
                techWindow.IsLocked = false;
                techWindow.TechName = selectedTech.TechName;
                techWindow.TechDescription = selectedTech.TechCNName+":"+selectedTech.Description;
                techWindow.TechSprite= Resources.Load<Sprite>(loadPath + buttonName);
            }
            else
            {
                techWindow.IsLocked = true;
                techWindow.TechName = "???";
                techWindow.TechDescription = selectedTech.UnlockWay;
            }
            Debug.Log(clickedButton.name);
            ScrollToButton(buttonName);
            UIManager.GetInst().Push(techWindow, true);
        }
        else
        {
            Debug.LogWarning("无法找到被点击的按钮");
        }
    }
    void ScrollToButton(int index)
    {

        if (scrollRect == null)
        {
            Debug.LogWarning("未找到 ScrollRect 组件！");
            return;
        }

        RectTransform contentRect = scrollRect.content; // 获取 ScrollRect 的内容区域

        // x = (index - 1) * 480
        float targetX = -(index - 2) * 480;//我也不知道为什么减一不行减二行
        // 限制范围，防止滑动超出边界
        float minX = -524;//协程没搞懂，数据全靠试
        float maxX = contentRect.rect.width - scrollRect.viewport.rect.width;
        targetX = Mathf.Clamp(targetX, -maxX, -minX);

        // 启动平滑滚动协程
        UIFunction.Inst.StartSmoothScroll(contentRect, targetX);
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
}
