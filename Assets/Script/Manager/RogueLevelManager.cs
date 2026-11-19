using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RogueLevelManager : MonoBehaviour
{
    // 根据 RogueGlobalManager.CurrentLevel 以及总分子量需求，计算本关“目标分子量”

    //生成并管理所有基团（MonomerBall），包括它们的血量、移动、销毁

    //追踪并记录玩家在本关的喷口的数据：每座喷口的等级、所选专精技能

    //本关胜利／失败检测：所有基团销毁或时间耗尽
    public int currentLevel;

    private int currentTotalNum;//这一关所需分子量总数
    private int currentPolmerNum;//这一关的基团个数，前期2-3个，最多8个
    //进入场景时初始化关卡
    public void StartLoad()
    {

    }
}
