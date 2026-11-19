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
                return new Level2();
            case 3:
                return new Level3(); 
            case 4:
                return new Level4(); 
            case 5:
                return new Level5(); 
            case 6:
                return new Level6(); 
            case 7:
                return new Level7();
            case 8:
                return new Level8(); 
            case 9:
                return new Level9(); 
            case 10:
                return new Level10(); 
            case 11:
                return new Level10(); 
            // 添加更多关卡的实例创建
            default:
                Debug.LogError("无效的关卡编号: " + level);
                return null;
        }
    }
}
