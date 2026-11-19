using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LevelManager
{
    //负责关卡的胜利与否的判定
    private static LevelManager Inst;
    //[Tooltip("当前所进行的关卡")]
    //public static int CurrentLevel;
    //[Tooltip("最远玩到的关卡")]
    //public static int TheFarthestLevel;
    

    #region 以下数据由Touched传输获得

    int TwoStarStep;//达成二星所要求不高于的步数
    int ThreeStarStep;//达成三星所要求不高于的步数
    [Tooltip("每关的答案，由Touched提供（节点，边）")]
     List<int> Answear;


    #endregion
    public int GetMaxUseCountForChargeChangeItem;
    [Tooltip("一定为最边上的元素的编号，遍历开始的节点，进入游戏后获取名为StartCharge的电荷并将其赋值给StartKey")]
    public Charge StartKey;

    List<KeyValuePair<int, List<int>>> playerFinalPolmer;

    [SerializeField]
    [Tooltip("当前场景中的所有基团/电荷")]
    List<Charge> charges;

    public Touched touched;
    public static LevelManager GetInst()
    {
        if (Inst == null)
        {
            Debug.LogError("未找到LevelManager实例");
            return Inst;
        }
        return Inst;
    }

    public LevelManager()//开始就会执行的函数
    {
        Inst = this;
        playerFinalPolmer = new List<KeyValuePair<int, List<int>>>();
        charges = new List<Charge>();
    }


    /// <summary>
    ///进入游戏场景后被调用
    /// </summary>
    public void FindCharge()
    {
        if (!touched)
        {
            Debug.LogWarning("touched还没被找到");
            touched = GameObject.FindObjectOfType<Touched>().gameObject.GetComponent<Touched>();
        }
            
        TwoStarStep = touched.TwoStarStep;
        ThreeStarStep = touched.ThreeStarStep;
        Answear = touched.Answear;
        charges.Clear();
        GameObject[] chargeGameobjects = GameObject.FindGameObjectsWithTag("Charge");
        foreach (GameObject charge in chargeGameobjects)
        {
            if (charge.TryGetComponent<Charge>(out Charge charge1))
            {
                charges.Add(charge1);
                if (charge.name == "StartKey")
                    StartKey = charge1;
            }                          
        }           
    }
    public void VictoryCheck()//每当电荷连接时调用
    {
        int connectedValue=0;
        foreach(Charge charge in charges)//获取场景中所有电荷
        {
            if (charge.CurrntConnectNum==charge.MaxConnectNum)//如果某个基团的键连满了
                connectedValue += 1;
        }
        // 示例逻辑：场景中只有一个粒子仍有剩余连接位，其余全部满了
        int canStillConnectCount = charges.Count(charge => charge.MaxConnectNum - charge.CurrntConnectNum > 0);
        if (canStillConnectCount == 1)
        {
            TriggerDefeat("无剩余共价键，已无可能再完成聚合");
            return;
        }

        //全部电荷都链满
        if (connectedValue == charges.Count)//解决思路可以每个基团为一个集合，当和其他基团结合时，两个集合合二为一，当场景中只剩一个集合时就是全部结合了，或者所有电荷的键连满？
            checkDFS();
    }
    #region 执行DFS的函数
    private void checkDFS()//判定胜负
    {
        Debug.Log("开始胜利判定");        
        playerFinalPolmer.Clear();// 确保是空的
      
        //List<int> visitedOrder = DFS();//获取DFS算法函数的返回值     
        //Debug.Log("遍历顺序: " + string.Join(", ", visitedOrder));// 这一步是获取遍历顺序        
        if (CheckAnswer())// 检查该返回值是否与答案匹配
        {
            //结算动画，显示胜利UI，星数显示并更新最远进行关卡
            Debug.Log("赢了");
            
            SaveManager.GetInst().PassLevel();//根据当前关卡和通关星数给玩家金币奖励


            if (starNumJudge() == 3 && !touched.inAssistMode)
            {
                PRTS.GetInst().OnVictoryCheckOrPrompt(SaveManager.CurrentLevel);
            }
            //touched.SmoothTimeStop();
            touched.enabled = false;
            AudioManager.Inst.Updatelist(3);//播放胜利BGM
            SaveManager.GetInst().UpdateLevelStar(starNumJudge());

        }
                  
        else
        {
            TriggerDefeat("基团链接顺序错误");
        }
                   
    }
    public void TriggerDefeat(string reason)
    {
        if (UIManager.IsPause)
            return;
        Debug.LogWarning("任务失败：" + reason);
        //touched.SmoothTimeStop();
        touched.enabled = false;
        DefeatedPanel defeatedPanel = new DefeatedPanel();
        defeatedPanel.DefeatedMessage = reason;
        UIManager.GetInst().Push(defeatedPanel);
        AudioManager.Inst.Updatelist(4);//播放失败BGM
        
    }

    private void DFS(Charge current, List<Charge> result, HashSet<Charge> visited)
    {
        visited.Add(current);
        result.Add(current);

        List<Charge> neighbors = new List<Charge>(current.neighbour);

        // 主链优先逻辑
        int nextIndex = result.Count;
        if (nextIndex < Answear.Count)
        {
            int nextTargetValue = Answear[nextIndex];

            foreach (Charge neighbor in neighbors)
            {
                if (!visited.Contains(neighbor) && neighbor.Value == nextTargetValue)
                {
                    DFS(neighbor, result, visited); // ✅ 主链继续走
                }
            }
        }

        // 遍历剩下的非目标邻居（支链）
        foreach (Charge neighbor in neighbors)
        {
            if (!visited.Contains(neighbor))
            {
                DFS(neighbor, result, visited); // ✅ 支链补充走
            }
        }
    }





    private bool CheckAnswer()
    {
        List<Charge> playerAnswer = new List<Charge>();
        DFS(StartKey, playerAnswer, new HashSet<Charge>());

        Debug.Log("遍历顺序: " + string.Join(", ", playerAnswer.Select(c => c.Value)));

        if (playerAnswer.Count != Answear.Count)
            return false;

        for (int i = 0; i < playerAnswer.Count; i++)
        {
            if (playerAnswer[i].Value != Answear[i])
                return false;
        }

        return true;
    }
/*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="matchingCharge">当前正在匹配的基团</param>
    /// <param name="answerIndex">当前匹配的基团编号在Answear中的索引</param>
    /// <param name="visited">在一次递归中已经走过的基团集合</param>
    /// <returns></returns>
    private bool MatchDFSRecursive(Charge current, int answerIndex, HashSet<Charge> visited, List<int> path)
    {
        if (current == null || answerIndex >= Answear.Count)
        {
            path.Add(current != null ? current.Value : -1); // 加上异常节点
            Debug.Log("❌ 遍历失败（无效节点或超出范围），路径: " + string.Join(", ", path));
            path.RemoveAt(path.Count - 1);
            return false;
        }

        if (current.Value != Answear[answerIndex])
        {
            path.Add(current.Value);
            Debug.Log("❌ 值不匹配，路径: " + string.Join(", ", path));
            path.RemoveAt(path.Count - 1);
            return false;
        }

        visited.Add(current);
        path.Add(current.Value);

        if (answerIndex == Answear.Count - 1)
        {
            Debug.Log("✅ 匹配成功！遍历顺序: " + string.Join(", ", path));
            return true;
        }

        foreach (Charge neighbor in current.neighbour)//从Charge的邻居里挨着遍历
        {
            if (!visited.Contains(neighbor) && neighbor.Value == Answear[answerIndex + 1])
            {
                if (MatchDFSRecursive(neighbor, answerIndex + 1, visited, path))
                    return true;
            }
        }


        // 回溯
        visited.Remove(current);
        path.RemoveAt(path.Count - 1);
        Debug.Log("❌ 路径回退，当前路径: " + string.Join(", ", path));
        return false;
    }

    */

    #endregion
    public int starNumJudge()//判断当前是几星通关
    {
        Debug.Log("通关第"+SaveManager.CurrentLevel+"关");
        if (touched.Step<=ThreeStarStep)
        {
            //LevelUIManager.levelUIInst.VectorySettlement(2);
            return 3;
        }
        else if(touched.Step>ThreeStarStep&& touched.Step<=TwoStarStep)
        {
            //LevelUIManager.levelUIInst.VectorySettlement(1);
            return 2;
        }
        else
        {
            return 1;
            //LevelUIManager.levelUIInst.VectorySettlement(0);
        }
        
    }
}
