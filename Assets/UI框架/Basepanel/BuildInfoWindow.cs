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
    public int CurrentNum;
    public int MaxNum;
    public string FactoryCNName;
    public string FactoryName;
    public string type;
    //public string Progress;

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
        claimButton.onClick.AddListener(CollectResources);
        updateDate();
    }
    void updateDate()
    {
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "FactoryName").text = FactoryCNName;
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Progress").text = $"{CurrentNum} / {MaxNum}";
        slider.maxValue = MaxNum;
        slider.value = CurrentNum;
        if (CurrentNum > 0)
            UIFunction.Inst.SetCanvasGroupVise(claimButton.gameObject, true);
        else
            UIFunction.Inst.SetCanvasGroupVise(claimButton.gameObject, false);
    }
    /// <summary>
    /// 领取资源并更新数据
    /// </summary>
    private void CollectResources()
    {
        //updateDate();
        if (CurrentNum > 0)
        {
            BuildingManager.GetInst().CollectBuildingResources(FactoryName);

            // 更新 UI 表现
            CurrentNum = 0;
            updateDate();
        }
    }


}
