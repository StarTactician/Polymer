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
    private List<Button> achiveButtons;
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
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.StartFade(0.2f, this, true);
        //改成for循环才能实现每个隔一点时间缩放出来，（包括回弹）
        for (int i = 0; i < achiveButtons.Count; i++)
        {
            Button button = achiveButtons[i];
            if (!AchievementManager.IsTechnologyUnlocked(i+1)) // 未解锁，设置黑白颜色 + 上锁图标
            {
                Image LockedImage = button.GetComponent<Image>();//黑白需要调饱和度，不好实现，还需要别的办法
                UIFunction.Inst.StartFade(0.1f, UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "techImage"), true);//锁图标初始透明度为0
                // 锁定图标可以加上 sprite
                // UIFunction.Inst.GetCompoentInObjsChild<Image>(button.gameObject, "techImage").sprite = Resources.Load<Sprite>("IMG/TechImag/");
            }

            // 计算延迟时间（第 1 个按钮立即执行，第 2 个按钮延迟 0.1s，第 3 个按钮延迟 0.2s 依次类推）
            float waitTime = i * 0.1f;

            // **执行放大动画**
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            UIFunction.Inst.StartScale(0.1f, rectTransform, new Vector3(1.2f, 1.2f, 1), waitTime); // 放大
            UIFunction.Inst.StartScale(0.05f, rectTransform, new Vector3(1.0f, 1.0f, 1), waitTime + 0.1f); // 回弹

            button.onClick.AddListener(Select);
        }

    }
    void Select()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            UIManager.GetInst().PopAllWindow();
            int buttonName = int.Parse(clickedButton.name);
            Achievement selectedAchiev = AchievementManager.GetInst().GetAchiev(buttonName);//根据id获取
            if (selectedAchiev.hasCheck != 0)
            {
                selectedAchiev.hasCheck = 0;
                AchievementManager.GetInst().SaveAchievementLibrary();
            }
            TechDescribeWindow techWindow = new TechDescribeWindow();
            if (TechTreeManager.IsTechnologyUnlocked(buttonName))//只有已解锁的才会传递成就描述，否则显示未解锁以及解锁方式
            {
                techWindow.IsLocked = false;
                techWindow.TechName = selectedAchiev.name;
                techWindow.TechDescription = selectedAchiev.description;
                techWindow.TechSprite = Resources.Load<Sprite>(loadPath + buttonName);
            }
            else
            {
                techWindow.IsLocked = true;
                techWindow.TechName = "???";
                techWindow.TechDescription = selectedAchiev.lockedInfo;
                techWindow.TechSprite = Resources.Load<Sprite>("IMG/LittleUI/UILock");
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
        ScrollRect scrollRect = UIFunction.Inst.GetCompoentInObjsChild<ScrollRect>(PanelObj, "Scroll");
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
    void ShowOut()
        //让成就一个接一个冒出来
    {

    }
}
