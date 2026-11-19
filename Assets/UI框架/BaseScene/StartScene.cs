using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScene : BaseScene
{    
    public readonly string SceneName = "StartScene";   
    public override void EnterScene()
    {
        CurrentType = 0;
        AudioManager.Inst.Updatelist(CurrentType);//ÇÐ¸è

        UIManager.GetInst().Push(new StartPanel());
        GameRoot.GetInst().ReLoad();

    }

    public override void ExitScene()
    {
        
    }
}
