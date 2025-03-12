using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLevel : BaseScene
{
    public readonly string SceneName = "1";

    public override void EnterScene()
    {
        CurrentType = 1;

        // 发布进入场景的事件，音乐管理器将自动更新歌单,GameRoot也会实例化需要的脚本等
        InvokeEnterScene();

        if (LevelManager.GetInst() != null)
            LevelManager.GetInst().FindCharge();
        else
            Debug.LogWarning("LevelManager未被正确实例化！");

        UIManager.GetInst().Push(new PauseButtonPanel());
    }

    public override void ExitScene()
    {
        // 可以发布退出场景的事件来处理退出逻辑
    }
}
