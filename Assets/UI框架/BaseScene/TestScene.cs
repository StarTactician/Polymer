using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : BaseScene
{
    public readonly string SceneName = "TestScene";
    public override void EnterScene()
    {
        CurrentType = 1;
        InvokeEnterScene();
        if (LevelManager.GetInst() != null)
            LevelManager.GetInst().FindCharge();
        else
            Debug.LogWarning("LevelManager未被正确实例化！");
        UIManager.GetInst().Push(new PauseButtonPanel());
    }

    public override void ExitScene()
    {
        
    }
}
