using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 在点击确定进入关卡后，异步加载场景时调用
/// </summary>
public class LoadPanelFactory
{
    public static BasePanel CreatePanel(int Picture)
    {
        switch (Picture)
        {
            case 1:
                return new SettingPanel();
            case 2:
                return new SettingPanel();
            // 添加更多加载画面的实例创建
            default:
                Debug.LogError("无效的加载画面编号: " + Picture);
                return null;
        }
    }
}
