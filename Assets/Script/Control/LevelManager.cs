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
        //全部电荷都链满
        if (connectedValue == charges.Count)//解决思路可以每个基团为一个集合，当和其他基团结合时，两个集合合二为一，当场景中只剩一个集合时就是全部结合了，或者所有电荷的键连满？
            checkDFS();
    }
    #region 执行DFS的函数
    private void checkDFS()//判定胜负
    {
        Debug.Log("开始胜利判定");        
        playerFinalPolmer.Clear();// 确保是空的
        /*foreach (Charge charge in charges)
        {
            playerFinalPolmer.Add(new KeyValuePair<int, List<int>>(charge.Value, charge.neighbour));//在列表中添加元素编号和其邻居编号      
        }*/           
        List<int> visitedOrder = DFS();//获取DFS算法函数的返回值     
        Debug.Log("遍历顺序: " + string.Join(", ", visitedOrder));// 这一步是获取遍历顺序        
        if (CheckAnswer(visitedOrder))// 检查该返回值是否与答案匹配
        {
            //结算动画，显示胜利UI，星数显示并更新最远进行关卡
            Debug.Log("赢了");
            SaveManager.GetInst().PassLevel();//根据当前关卡和通关星数给玩家金币奖励
            touched.enabled = false;
            AudioManager.Inst.Updatelist(3);//播放胜利BGM
            
        }
                  
        else
        {
            UIManager.GetInst().Push(new DefeatedPanel());//显示失败UI
            touched.enabled = false;
            Debug.Log("寄了");
        }
                   
    }
    private List<int> DFS()
    {
        HashSet<Charge> visited = new HashSet<Charge>(); // 记录已访问的电荷
        Stack<Charge> stack = new Stack<Charge>(); // 栈用于DFS遍历
        List<int> playerAnswer = new List<int>(); 
        stack.Push(StartKey); 
        while (stack.Count > 0)
        {
            Charge currentCharge = stack.Pop(); // 从栈中取出一个电荷
            if (!visited.Contains(currentCharge))
            {
                visited.Add(currentCharge); // 标记当前电荷为已访问
                playerAnswer.Add(currentCharge.Value); // 将当前电荷的值加入遍历顺序

                foreach (Charge neighbor in currentCharge.neighbour)
                {
                    Debug.Log("遍历邻居节点了");
                    if (!visited.Contains(neighbor))
                    {
                        stack.Push(neighbor); // 将未访问的邻居节点压入栈
                        Debug.Log("邻居节点压入栈了");
                    }
                }
            }
            Debug.Log("执行了");
        }

        return playerAnswer; // 返回遍历顺序
    }

    private bool CheckAnswer(List<int> visitedOrder)
    {    
        List<int> answerOrder = new List<int>(Answear); // 获取答案的遍历顺序
        //answerOrder.Sort(); // 事实上答案只有一种，不需要从小到大排列
        //visitedOrder.Sort(); // 

        return visitedOrder.SequenceEqual(answerOrder); // 比较两个顺序是否相同
    }
    #endregion
    public int starNumJudge()//判断当前是几星通关
    {
        if(touched.Step<=ThreeStarStep)
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
