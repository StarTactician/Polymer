using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildInfoWindow : BasePanel
{
    private static string name = "BuildInfoWindow";
    private static string path = "Panel/BuildInfoWindow";
    public static readonly UIType uitype = new UIType(path, name);

    public Slider slider;
    public Button claimButton;
    public Image claimButtonInactiveIcon;

    public int CurrentNum;
    public int MaxNum;
    public string FactoryCNName;
    public string FactoryName;
    public string type; // 收入类型（为空则为非工厂类）
    bool isFactory;

    public BuildInfoWindow() : base(uitype) { }

    public override void OnEnable()
    {
        base.OnEnable();
        updateDate();
    }

    public override void OnStart()
    {
        base.OnStart();

        slider = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "Slider");
        claimButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Claim");
        claimButtonInactiveIcon = UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "claim0");

        
        if (!string.IsNullOrEmpty(type))
        {
            isFactory = true;
            claimButton.onClick.AddListener(CollectResources);
        }
        
        updateDate();
    }

    void updateDate()
    {
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "FactoryName").text = FactoryCNName;

        // 判断是否为工厂类（type 非空）
        //bool isFactory = !string.IsNullOrEmpty(type);

        // 控制 Progress 和 Slider 显示
        UIFunction.Inst.SetCanvasGroupVise(UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Progress").gameObject,isFactory);
        UIFunction.Inst.SetCanvasGroupVise(slider.gameObject, isFactory);

        if (isFactory)
        {
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Progress").text = $"{CurrentNum} / {MaxNum}";
            slider.maxValue = MaxNum;
            slider.value = CurrentNum;

            // 领取按钮显示逻辑
            if (CurrentNum > 0)
            {
                UIFunction.Inst.SetCanvasGroupVise(claimButton.gameObject, true);
            }
            else
            {
                UIFunction.Inst.SetCanvasGroupVise(claimButton.gameObject, false);
            }

            //UIFunction.Inst.SetImageVise(claimButtonInactiveIcon, false);
        }
        else // 非工厂类处理逻辑
        {
            // 关闭领取按钮和失活图标
            UIFunction.Inst.SetCanvasGroupVise(claimButton.gameObject, false);
            UIFunction.Inst.SetImageVise(claimButtonInactiveIcon, false);
        }
    }

    /// <summary>
    /// 领取资源并更新数据
    /// </summary>
    public static void HideInactiveWindows(Dictionary<int, BuildInfoWindow> windows, BuildingManager buildings)
    {
        foreach (var entry in windows)
        {
            if (!buildings.IsActiveBuilding(buildings.GetBuildingByID(entry.Key)))
                entry.Value.PanelObj.SetActive(false);
        }
    }

    private void CollectResources()
    {
        if (CurrentNum > 0 && BuildingManager.GetInst().CollectBuildingResources(FactoryName))
        {
            CurrentNum = 0;
            updateDate();
        }
    }
}
