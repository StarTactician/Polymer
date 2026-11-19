using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovementConfig
{
    public string objectName;         // 物体名称
    public List<Vector3> pathPoints;  // 移动路径点
    public float speed;               // 移动速度
    public string startTime;          // 开始移动的具体时间 (可选)
    public float delay;               // 场景加载后延迟开始的时间 (如果startTime为空则使用这个)
    public float randomOffset;        // 随机上下浮动范围
    public string activateObjectName; // 开始移动时激活的游戏物体名称 (可选)
    public int hideAtPathIndex = -1;  // 在哪个路径点隐藏自己，默认-1不隐藏
    public string buildingNameForTime; // 基于哪个建筑刷新时间来移动（可选）
}

[Serializable]
public class MovementConfigList
{
    public List<MovementConfig> configs;
}

public class MovementManager : MonoBehaviour
{
    private static MovementManager inst;
    public static MovementManager Inst
    {
        get
        {
            if (inst == null)
            {
                inst = FindObjectOfType<MovementManager>();
                if (inst == null)
                {
                    GameObject obj = new GameObject("MovementManager");
                    inst = obj.AddComponent<MovementManager>();
                }
            }
            return inst;
        }
    }

    public Dictionary<string, MovementConfig> movementConfigs = new Dictionary<string, MovementConfig>();

    void Awake()
    {
        if (inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadMovementConfigs(TextAsset json)
    {
        MovementConfigList configList = JsonUtility.FromJson<MovementConfigList>(json.text);
        foreach (var config in configList.configs)
        {
            movementConfigs[config.objectName] = config;
        }
    }

    public void StartMovements()
    {
        foreach (var config in movementConfigs.Values)
        {
            GameObject obj = GameObject.Find(config.objectName);
            if (obj != null)
            {
                float delay = config.delay + UnityEngine.Random.Range(-config.randomOffset, config.randomOffset);

                if (!string.IsNullOrEmpty(config.startTime))
                {
                    DateTime startDateTime = DateTime.Parse(config.startTime);
                    double waitSeconds = (startDateTime - DateTime.Now).TotalSeconds;
                    delay = (float)Math.Max(waitSeconds, 0);
                }
                else if (!string.IsNullOrEmpty(config.buildingNameForTime))
                {
                    BuildingData building = BuildingManager.GetInst().GetBuildingByName(config.buildingNameForTime);
                    if (building != null && !string.IsNullOrEmpty(building.nextGenerateTime))
                    {
                        DateTime nextTime = DateTime.Parse(building.nextGenerateTime);
                        delay = (float)Math.Max((nextTime - DateTime.Now).TotalSeconds, 0);
                    }
                }

                StartCoroutine(MoveObjectAlongPath(obj, config));
            }
        }
    }

    public void MoveInstantiatedObject(GameObject obj, MovementConfig config)
    {
        float delay = config.delay + UnityEngine.Random.Range(-config.randomOffset, config.randomOffset);
        StartCoroutine(MoveObjectAlongPath(obj, config, delay));
    }

    private IEnumerator MoveObjectAlongPath(GameObject obj, MovementConfig config, float overrideDelay = -1)
    {
        float delay = overrideDelay >= 0 ? overrideDelay : config.delay;
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(config.activateObjectName))
        {
            GameObject activateObj = GameObject.Find(config.activateObjectName);
            if (activateObj != null)
                activateObj.SetActive(true);
        }

        for (int i = 0; i < config.pathPoints.Count; i++)
        {
            Vector3 startPoint = obj.transform.position;
            Vector3 endPoint = config.pathPoints[i];
            float journey = 0f;
            float distance = Vector3.Distance(startPoint, endPoint);

            while (journey < distance)
            {
                journey += config.speed * Time.deltaTime;
                obj.transform.position = Vector3.Lerp(startPoint, endPoint, journey / distance);
                yield return null;
            }

            obj.transform.position = endPoint;

            if (i == config.hideAtPathIndex)
                obj.SetActive(false);
        }
    }
}
