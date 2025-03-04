using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    #region 生成其他管理器实例    
    private UIManager uiManager;
    public UIManager UIManager_Root { get => uiManager; }

    private SceneControl sceneControl;
    public SceneControl SceneControl_Root { get => sceneControl; }

    private SaveManager saveManager;
    public SaveManager SaveManager_Root { get => saveManager; }

    private LevelManager levelManager;
    public LevelManager LevelManager_Root { get => levelManager; }

    private MissionManager missionManager;
    public MissionManager MissionManager_Root { get => missionManager; }

    private TechTreeManager techTreeManager;
    public TechTreeManager TechTreeManager_Root { get => techTreeManager; }
    private AchievementManager achievementManager;
    public AchievementManager AchievementManager_Root { get => achievementManager; }
    #endregion

    #region 生成方法类实例
    private GetTouchedVector getTouchedVector;
    public GetTouchedVector GetTouchedVector_Root { get => getTouchedVector; }

    private ConnectionManager connectionManager;
    public ConnectionManager ConnectionManager_Root { get => connectionManager; }

    #endregion
    private static GameRoot Inst;
    public static GameRoot GetInst()
    {
        if (Inst == null)
            Debug.LogError("未找到GameRoot实例");
        return Inst;
    }
    private void HandleEnterScene()
    {
        if (connectionManager != null)
        {
            // 清空已有连接数据而不是重新实例化
            connectionManager.Reset();
            Debug.Log("ConnectionManager已重置！");
        }
        else
        {
            // 如果 connectionManager 尚未实例化，进行实例化
            connectionManager = new ConnectionManager();
            Debug.Log("ConnectionManager实例化成功！");
        }
    }
    private void OnDestroy()
    {
        // 取消订阅进入关卡事件
        BaseScene.OnEnterScene -= HandleEnterScene;
    }
    private void Awake()
    {
        BaseScene.OnEnterScene += HandleEnterScene;
        if (Inst == null)
            Inst = this;
        else
            Destroy(gameObject);             
        uiManager = new UIManager();
        sceneControl = new SceneControl();
        getTouchedVector = new GetTouchedVector();
        levelManager = new LevelManager();
        saveManager = new SaveManager();
        missionManager = new MissionManager();
        techTreeManager = new TechTreeManager();
        achievementManager = new AchievementManager();
        //读取存档
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        UIManager_Root.CurrentSceneCanvas = UIFunction.Inst.FindCanvas();
        StartScene startScene = new StartScene();
        SceneControl_Root.Scene_Dict.Add(startScene.SceneName, startScene);
        #region 推入第一个界面
        UIManager_Root.Push(new StartPanel());
        #endregion
    }
}
