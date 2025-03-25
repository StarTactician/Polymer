using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildCheckPanel : BasePanel
{
    private static string name = "BuildCheckPanel";
    private static string path = "Panel/BuildCheckPanel";
    public static readonly UIType uitype = new UIType(path, name);

    private Transform Container; // 滚动区域的Content
    //private GameObject buildInfoWindow; // 生产建筑信息窗口预制体
    private Dictionary<int, BuildInfoWindow> activeBuildWindows = new Dictionary<int, BuildInfoWindow>(); // 存储已生成的窗口
    //private List<BuildingData> unlockFactories = new List<BuildingData>(); // 已解锁的生产建筑列表
    Coroutine progressCoroutine;

    public BuildCheckPanel() : base(uitype) { }
    public override void OnDisable()
    {
        base.OnDisable();
        if (progressCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(progressCoroutine);
        }
        //UIFunction.Inst.StartFade(0.2f, this, false);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        activeBuildWindows.Clear();
        if (progressCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(progressCoroutine);
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
        updateProgress();
        progressCoroutine = UIFunction.Inst.StartCoroutineWithInterval(updateProgress, 60f);
    }

    public override void OnStart()
    {
        base.OnStart();
        Container = UIFunction.Inst.GetCompoentInObjsChild<Transform>(PanelObj, "Container");
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        updateProgress();
        progressCoroutine = UIFunction.Inst.StartCoroutineWithInterval(updateProgress, 60f);

    }



    /// <summary>
    /// 生成/更新工厂信息窗口
    /// </summary>
    private void UpdateFactoryUI()
    {
        var unlockedFactories = BuildingManager.GetInst().unlockFactory;

        foreach (BuildingData factory in unlockedFactories)
        {
            // 如果该建筑已被更高级版本替代，就跳过
            if (BuildingManager.GetInst().IsReplacedByUpgrade(factory, unlockedFactories))
                continue;

            if (activeBuildWindows.ContainsKey(factory.id))
            {
                BuildInfoWindow existingWindow = activeBuildWindows[factory.id];
                existingWindow.FactoryCNName = factory.textName;
                existingWindow.FactoryName = factory.name;
                existingWindow.CurrentNum = factory.currentReserves;
                existingWindow.MaxNum = factory.maxReserves;
                existingWindow.OnEnable();
            }
            else
            {
                BuildInfoWindow windowScript = new BuildInfoWindow();
                GameObject windowObj = UIManager.GetInst().PushChild(windowScript, Container);
                windowScript.type = factory.incomeType;
                windowScript.FactoryCNName = factory.textName;
                windowScript.FactoryName = factory.name;
                windowScript.CurrentNum = factory.currentReserves;
                windowScript.MaxNum = factory.maxReserves;
                activeBuildWindows.Add(factory.id, windowScript);
                //查看建筑详情
                // windowScript.button.onClick.AddListener(() => ShowFactoryDetails(factory));
            }
        }
    }

    void updateProgress()
    {
        BuildingManager.GetInst().UpdateBuildingProduction();
        UpdateFactoryUI();
    }


    /// <summary>
    /// 点击工厂窗口后，显示详情，并将摄像机移动到该工厂
    /// </summary>
    /// <param name="factory">对应工厂</param>
    private void ShowFactoryDetails(BuildingData factory)
    {
        Debug.Log($"查看工厂: {factory.textName}");

        // 移动副摄像头到建筑位置
        Camera secondaryCamera = GameObject.Find("SecondaryCamera").GetComponent<Camera>();
        if (secondaryCamera != null)
        {
            secondaryCamera.transform.position = new Vector3(factory.position.x, factory.position.y + 10, factory.position.z - 10);
            secondaryCamera.transform.LookAt(new Vector3(factory.position.x, factory.position.y, factory.position.z));
        }

        // 推入详细窗口
        //FactoryDetailWindow detailWindow = new FactoryDetailWindow(factory);
        //UIManager.GetInst().Push(detailWindow, true);
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
    
}
