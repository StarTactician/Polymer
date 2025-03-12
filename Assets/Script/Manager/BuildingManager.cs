using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildingManager
{
    private static BuildingManager Inst;
    private string buildingDataFilePath = Application.persistentDataPath + "/BuildingData.json";
    private BuildingList buildingList;
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
    /// <summary>
    /// 在初始化时，根据建筑的已解锁情况，在主界面的月球背景安放对应的建筑
    /// </summary>
    public void GenerateBuildingsOnLoad()
    {
        foreach (BuildingData building in buildingList.buildings)
        {
            if (building.isUnlocked && !string.IsNullOrEmpty(building.name))
            {
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
    }

    public void UnlockBuilding(string buildingName)
    {
        BuildingData building = buildingList.buildings.Find(b => b.name == buildingName);
        if (building != null && !building.isUnlocked)
        {
            building.isUnlocked = true;
            SaveBuildingData();
            Debug.Log($"建筑 {buildingName} 已解锁！");
        }
    }
}


