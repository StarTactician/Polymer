using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class BuildingManager
{
    private static BuildingManager Inst;
    private string buildingDataFilePath = Application.persistentDataPath + "/BuildingData.json";
    private BuildingList buildingList;
    public List<BuildingData> unlockFactory=new List<BuildingData>();
    public static BuildingManager GetInst()
    {
        if (Inst == null)
        {
            Inst = new BuildingManager();
        }
        return Inst;
    }
    public BuildingManager()
    {
        Inst = this;
        LoadBuildingData();
        GenerateBuildingsOnLoad();
    }

    public void LoadBuildingData()
    {
        if (File.Exists(buildingDataFilePath))
        {
            string json = File.ReadAllText(buildingDataFilePath);
            buildingList = JsonUtility.FromJson<BuildingList>(json);
            if (buildingList == null || buildingList.buildings == null)
            {
                Debug.LogError("建筑数据为空，请检查 JSON 文件。");
                return;
            }
            Debug.Log("建筑数据加载成功！");
        }
        else
        {
            Debug.LogWarning("建筑数据文件不存在，加载默认建筑数据...");
            TextAsset resourcesBuildingData = Resources.Load<TextAsset>("DefaultBuildingData");
            if (resourcesBuildingData != null)
            {
                buildingList = JsonUtility.FromJson<BuildingList>(resourcesBuildingData.text);
                SaveBuildingData();
                Debug.Log("默认建筑数据加载成功！");
            }
            else
            {
                Debug.LogError("无法加载默认建筑数据！");
                buildingList = new BuildingList();
            }
        }
    }

    public void SaveBuildingData()
    {
        string json = JsonUtility.ToJson(buildingList, true);
        File.WriteAllText(buildingDataFilePath, json);
        Debug.Log("建筑数据已保存！");
    }
    public BuildingData GetFactoryByName(string factoryName)
    {
        foreach (BuildingData factory in unlockFactory)
        {
            if (factory.name == factoryName)
            {
                return factory;
            }
        }
        return null;
    }

    /// <summary>
    /// 在初始化时，根据建筑的已解锁情况，在主界面的月球背景安放对应的建筑
    /// </summary>
    public void GenerateBuildingsOnLoad()
    {
        GetFactory(); // 确保 unlockFactory 已更新

        foreach (BuildingData building in buildingList.buildings)
        {
            if (!building.isUnlocked || string.IsNullOrEmpty(building.name))
                continue;

            // 如果该建筑已被更高级版本替代，就跳过生成
            if (IsReplacedByUpgrade(building, buildingList.buildings.FindAll(b => b.isUnlocked)))
                continue;

            GameObject buildingPrefab = Resources.Load<GameObject>($"Prefabs/Buildings/{building.name}");
            if (buildingPrefab != null)
            {
                GameObject buildingObj = GameObject.Instantiate(buildingPrefab);
                buildingObj.transform.position = new Vector3(building.position.x, building.position.y, building.position.z);
                buildingObj.transform.localScale = new Vector3(building.scale.x, building.scale.y, building.scale.z);
                buildingObj.transform.rotation = Quaternion.Euler(building.rotation.x, building.rotation.y, building.rotation.z);
                Debug.Log($"生成建筑: {building.name} at {building.position}");
            }
            else
            {
                Debug.LogWarning($"未找到建筑预制体: {building.name}");
            }
        }
    }
    /// <summary>
    /// 资源领取方法
    /// </summary>
    /// <param name="factoryName"></param>
    public void CollectBuildingResources(string factoryName)
    {
        UpdateBuildingProduction(); // 确保是最新数据

        BuildingData factory = buildingList.buildings.Find(b => b.name == factoryName && b.isUnlocked);
        if (factory == null)
        {
            Debug.LogWarning($"未找到建筑：{factoryName}");
            return;
        }

        if (factory.currentReserves <= 0)
        {
            Debug.Log($"建筑 {factoryName} 当前储量为 0，无法领取。");
            return;
        }

        // 增加玩家资源
        SaveManager.GetInst().ResourcesChange(factory.currentReserves, factory.incomeType);

        
        GetCoinPanel rewardPanel = new GetCoinPanel();
        if (factory.incomeType == "PU")
        {
            rewardPanel.rewardPuShown = true;
            rewardPanel.Name1 = factory.currentReserves.ToString();
        }
            
        else if (factory.incomeType == "EU")
        {
            rewardPanel.rewardCoinShown = true;
            rewardPanel.CoinNumber = factory.currentReserves;
        }
        // 清空储量（但不改生产时间）
        factory.currentReserves = 0;
        UIManager.GetInst().Push(rewardPanel);
        SaveBuildingData();

        Debug.Log($"✅ 成功领取 {factoryName} 的资源，当前储量已清空");
    }

    /// <summary>
    /// 资源生成方法,12点时，建筑面板进入时，以及并在进入建筑面板后每分钟执行一次
    /// </summary>
    public void UpdateBuildingProduction()
    {
        DateTime now = DateTime.Now;
        GetFactory();

        foreach (var building in unlockFactory)
        {
            //if (!building.isUnlocked || building.type != 1) continue; // 只处理工厂类

            if (string.IsNullOrEmpty(building.nextGenerateTime))
            {
                // 第一次设定 nextGenerateTime
                building.nextGenerateTime = now.AddSeconds(building.producInterval).ToString("yyyy-MM-dd HH:mm:ss");
                continue;
            }

            DateTime nextTime = DateTime.Parse(building.nextGenerateTime);

            // 当前时间未达生产时间
            if (now < nextTime || building.currentReserves >= building.maxReserves)
                continue;
                            
            // 计算产出次数
            TimeSpan elapsed = now - nextTime;
            int intervalCount = 1 + Mathf.FloorToInt((float)elapsed.TotalSeconds / building.producInterval);
            int actualAdd = intervalCount * building.income;

            // 不能超过最大储量
            int spaceLeft = building.maxReserves - building.currentReserves;
            int finalAdd = Mathf.Min(actualAdd, spaceLeft);
            building.currentReserves += finalAdd;

            // 更新下次生产时间
            DateTime newNextTime = nextTime.AddSeconds(intervalCount * building.producInterval);
            building.nextGenerateTime = newNextTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        SaveBuildingData();
    }

    /// <summary>
    /// 解锁指定建筑时调用
    /// </summary>
    /// <param name="buildingName"></param>
    public void UnlockBuilding(string buildingName)
    {
        BuildingData building = buildingList.buildings.Find(b => b.name == buildingName);
        if (building != null && !building.isUnlocked)
        {
            building.isUnlocked = true;
            GetFactory();
            SaveBuildingData();
            Debug.Log($"建筑 {buildingName} 已解锁！");
        }
    }
    /// <summary>
    /// 某个建筑是否被同类更高级建筑替代（upgradeBy 相同）
    public bool IsReplacedByUpgrade(BuildingData building, List<BuildingData> unlockedList)
    {
        foreach (var b in unlockedList)
        {
            if (b.isUnlocked && !string.IsNullOrEmpty(b.upgradeBy) && b.upgradeBy == building.name)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取已经解锁的工厂类建筑
    /// </summary>
    private void GetFactory()
    {
        foreach(BuildingData factory in buildingList.buildings)
        {
            if (factory.type == 1 && factory.isUnlocked&&!unlockFactory.Contains(factory))
                unlockFactory.Add(factory);               
        }
        Debug.Log("当前解锁工厂数量"+unlockFactory.Count);
        if (unlockFactory.Count == 0)
            Debug.Log("工厂数目为0，请检查JSON是否正确被读取");
    }
}


