using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSceneFactory
{
    public static BaseScene CreateScene(int level)
    {
        switch (level)
        {
            case 1:
                return new SimpleLevel(); 
            case 2:
                return new SimpleLevel(); 
            // 添加更多关卡的实例创建
            default:
                Debug.LogError("无效的关卡编号: " + level);
                return null;
        }
    }
}
