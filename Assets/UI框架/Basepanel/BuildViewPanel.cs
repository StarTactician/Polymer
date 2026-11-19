using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildViewPanel : BasePanel
{
    private static string name = "BuildViewPanel";
    private static string path = "Panel/BuildViewPanel";
    public static readonly UIType uitype = new UIType(path, name);
    private bool showFactory = true;
    private Transform factoryContainer;
    private Transform unfactoryContainer;
    private Button factorySwitchBtn;
    private Button unfactorySwitchBtn;
    private Button In;
    private Button Out;
    private bool spreadOut=false;
    private Transform basementControl;
    private Coroutine moveCoroutine;
    //private GameObject buildInfoWindow; // 生产建筑信息窗口预制体
    private Dictionary<int, BuildInfoWindow> activeBuildWindows = new Dictionary<int, BuildInfoWindow>(); // 存储已生成工厂类的窗口
    private Dictionary<int, BuildInfoWindow> activeUnFactoryWindows = new Dictionary<int, BuildInfoWindow>();

    //private List<BuildingData> unlockFactories = new List<BuildingData>(); // 已解锁的生产建筑列表
    Coroutine progressCoroutine;

    public BuildViewPanel() : base(uitype) { }
    public override void OnDisable()
    {
        base.OnDisable();
        if (progressCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(progressCoroutine);
        }
        //UIFunction.Inst.StartFade(0.2f, this, false);
        GameRoot.GetInst().MainCamera.SetActive(true);
        GameRoot.GetInst().SecondaryCamera.SetActive(false);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        activeBuildWindows.Clear();
        if (progressCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(progressCoroutine);
        }
        GameRoot.GetInst().MainCamera.SetActive(true);
        GameRoot.GetInst().SecondaryCamera.SetActive(false);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        updateGenerateProgress();
        progressCoroutine = UIFunction.Inst.StartCoroutineWithInterval(updateGenerateProgress, 60f);
        GameRoot.GetInst().MainCamera.SetActive(false);
        GameRoot.GetInst().SecondaryCamera.SetActive(true);

    }

    public override void OnStart()
    {
        base.OnStart();
        factoryContainer = UIFunction.Inst.GetCompoentInObjsChild<Transform>(PanelObj, "FactoryContainer");
        unfactoryContainer = UIFunction.Inst.GetCompoentInObjsChild<Transform>(PanelObj, "UnfactoryContainer");
        basementControl = UIFunction.Inst.GetCompoentInObjsChild<Transform>(PanelObj, "BasementControl");
        factorySwitchBtn = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Factory");
        unfactorySwitchBtn = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "UnFactory");
        In = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "In");In.onClick.AddListener(ViewsSpread);
        Out= UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Out"); Out.onClick.AddListener(ViewsSpread);
        UIFunction.Inst.SetCanvasGroupVise(In.gameObject, false);

        factorySwitchBtn.onClick.AddListener(SwitchFactoryDisplay);
        unfactorySwitchBtn.onClick.AddListener(SwitchFactoryDisplay);

        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        updateGenerateProgress();
        UpdateTypeDisplay();
        progressCoroutine = UIFunction.Inst.StartCoroutineWithInterval(updateGenerateProgress, 60f);
        DialogueManager.GetInst().TryTriggerDialogue("BuildCheckPanelIntro");
        GameRoot.GetInst().MainCamera.SetActive(false);
        GameRoot.GetInst().SecondaryCamera.SetActive(true);
    }

    private void ViewsSpread()
    {
        spreadOut = !spreadOut;
        UIFunction.Inst.SetCanvasGroupVise(In.gameObject, spreadOut);
        UIFunction.Inst.SetCanvasGroupVise(Out.gameObject, !spreadOut);
        //false收回，true展开
        Vector2 DIR = spreadOut ? Vector2.left : Vector2.right;
        moveCoroutine = UIFunction.Inst.MoveObject(basementControl.gameObject, DIR, 1000, 0.33f, this);
    }

    private void SwitchFactoryDisplay()
    {
        showFactory = !showFactory;
        UpdateTypeDisplay();
    }

    private void UpdateTypeDisplay()
    {
        // 按钮显示控制：只显示另一个按钮
        UIFunction.Inst.SetCanvasGroupVise(factorySwitchBtn.gameObject, !showFactory);
        UIFunction.Inst.SetCanvasGroupVise(unfactorySwitchBtn.gameObject, showFactory);

        // 面板显示控制
        UIFunction.Inst.SetCanvasGroupVise(factoryContainer.gameObject, showFactory);
        UIFunction.Inst.SetCanvasGroupVise(unfactoryContainer.gameObject, !showFactory);
        if (showFactory)
            UpdateFactoryUI();
        else
            UpdateUnfactoryUI();
    }

    /// <summary>
    /// 生成/更新工厂信息窗口
    /// </summary>
    private void UpdateFactoryUI()
    {
        var unlockedFactories = BuildingManager.GetInst().unlockBuilding;

        foreach (BuildingData factory in unlockedFactories)
        {
            // 如果该建筑已被更高级版本替代，就跳过
            if (BuildingManager.GetInst().IsReplacedByUpgrade(factory, unlockedFactories))
                continue;
            // 只处理工厂类建筑（非 type == 1）
            if (factory.type != 1)
                continue;
            if (activeBuildWindows.ContainsKey(factory.id))//如果有，则是更新
            {
                BuildInfoWindow existingWindow = activeBuildWindows[factory.id];
                existingWindow.FactoryCNName = factory.textName;
                existingWindow.FactoryName = factory.name;
                existingWindow.CurrentNum = factory.currentReserves;
                existingWindow.MaxNum = factory.maxReserves;
                existingWindow.OnEnable();
            }
            else//如果没有，则是创建
            {
                BuildInfoWindow windowScript = new BuildInfoWindow();
                
                windowScript.type = factory.incomeType;
                windowScript.FactoryCNName = factory.textName;
                windowScript.FactoryName = factory.name;
                windowScript.CurrentNum = factory.currentReserves;
                windowScript.MaxNum = factory.maxReserves;
                activeBuildWindows.Add(factory.id, windowScript);
                GameObject windowObj = UIManager.GetInst().PushChild(windowScript, factoryContainer);
                Button checkButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(windowObj, "checkButton");
                int buildingId = factory.id;
                checkButton.onClick.AddListener(() =>
                {
                    Camera cam = GameObject.Find("SecondaryCamera").GetComponent<Camera>();
                    Transform camPos = BuildingManager.GetInst().GetCameraPos(buildingId);
                    Debug.Log("建筑查看按钮被点击");
                    BuildingDescribeWindow describeWindow = new BuildingDescribeWindow();
                    describeWindow.Description = factory.description;
                    UIManager.GetInst().Push(describeWindow, true);
                    if (cam != null && camPos != null)
                    {
                        // ✨ 开始插值移动摄像机
                        UIFunction.Inst.MoveCameraSmooth(cam, camPos.position, camPos.rotation, 1.2f);
                    }
                    else
                    {
                        Debug.LogWarning($"找不到摄像机或建筑 {buildingId} 的 CameraPos");
                    }
                });
                //查看建筑详情
                // windowScript.button.onClick.AddListener(() => ShowFactoryDetails(factory));
            }
        }
    }
    /// <summary>
    /// 生成/更新非工厂建筑信息窗口
    /// </summary>
    private void UpdateUnfactoryUI()
    {
        var unlockedBuildings = BuildingManager.GetInst().unlockBuilding;

        foreach (BuildingData building in unlockedBuildings)
        {
            // 跳过被高级版本替代的建筑
            if (BuildingManager.GetInst().IsReplacedByUpgrade(building, unlockedBuildings))
                continue;

            // 只处理非工厂类建筑（非 type == 1）
            if (building.type == 1)
                continue;

            if (activeUnFactoryWindows.ContainsKey(building.id)) // 已存在则更新
            {
                BuildInfoWindow existingWindow = activeUnFactoryWindows[building.id];
                existingWindow.FactoryCNName = building.textName;
                existingWindow.FactoryName = building.name;
                existingWindow.CurrentNum = building.currentReserves;
                existingWindow.MaxNum = building.maxReserves;
                existingWindow.OnEnable();
            }
            else // 不存在则创建
            {
                BuildInfoWindow windowScript = new BuildInfoWindow();
                
                windowScript.type = building.incomeType;
                windowScript.FactoryCNName = building.textName;
                windowScript.FactoryName = building.name;
                windowScript.CurrentNum = building.currentReserves;
                windowScript.MaxNum = building.maxReserves;
                GameObject windowObj = UIManager.GetInst().PushChild(windowScript, unfactoryContainer);
                activeUnFactoryWindows.Add(building.id, windowScript);

                // 摄像头按钮绑定
                Button checkButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(windowObj, "checkButton");
                int buildingId = building.id;
                checkButton.onClick.AddListener(() =>
                {
                    Camera cam = GameObject.Find("SecondaryCamera").GetComponent<Camera>();
                    Transform camPos = BuildingManager.GetInst().GetCameraPos(buildingId);
                    Debug.Log("建筑查看按钮被点击");
                    BuildingDescribeWindow describeWindow = new BuildingDescribeWindow();
                    describeWindow.Description = building.description;
                    UIManager.GetInst().Push(describeWindow, true);
                    if (cam != null && camPos != null)
                    {
                        UIFunction.Inst.MoveCameraSmooth(cam, camPos.position, camPos.rotation, 1.2f);
                    }
                    else
                    {
                        Debug.LogWarning($"找不到摄像机或建筑 {buildingId} 的 CameraPos");
                    }
                });
            }
        }
    }

    void updateGenerateProgress()
    {
        BuildingManager.GetInst().UpdateBuildingProduction();
        UpdateFactoryUI();
    }

    void Back()
    {
        UIManager.GetInst().Pop(false);
    }

}
