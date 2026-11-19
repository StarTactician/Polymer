using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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

    private BuildingManager buildingManager;
    public BuildingManager BuildingManager_Root { get => buildingManager; }

    private MissionManager missionManager;
    public MissionManager MissionManager_Root { get => missionManager; }

    private TechTreeManager techTreeManager;
    public TechTreeManager TechTreeManager_Root { get => techTreeManager; }
    private AchievementManager achievementManager;
    public AchievementManager AchievementManager_Root { get => achievementManager; }
    private DialogueManager dialogueManager;
    public DialogueManager DialogueManager_Root { get => dialogueManager; }

    #endregion

    #region 生成方法类实例
    private GetTouchedVector getTouchedVector;
    public GetTouchedVector GetTouchedVector_Root { get => getTouchedVector; }
    private PRTS prts;
    public PRTS PRTS_Root { get => prts; }

    private ConnectionManager connectionManager;
    public ConnectionManager ConnectionManager_Root { get => connectionManager; }

    #endregion
    public GameObject globalLoadingCanvas;
    public GameObject MainCamera;
    public GameObject SecondaryCamera;
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
        {
            Destroy(gameObject);
            return;
        }
                        
        uiManager = new UIManager();
        sceneControl = new SceneControl();
        getTouchedVector = new GetTouchedVector();
        prts = new PRTS();
        levelManager = new LevelManager();
        saveManager = new SaveManager();
        
        buildingManager = new BuildingManager();
        
        
        techTreeManager = new TechTreeManager();
        missionManager = new MissionManager();
        achievementManager = new AchievementManager();
        dialogueManager = new DialogueManager();
        //读取存档
        
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        UIManager_Root.CurrentSceneCanvas = UIFunction.Inst.FindCanvas();
        StartScene startScene = new StartScene();
        SceneControl_Root.Scene_Dict.Add(startScene.SceneName, startScene);
        // GameRoot 或 UIManager 初始化时调用
        globalLoadingCanvas = Instantiate(Resources.Load<GameObject>("Panel/LoadingCanvas"));
        DontDestroyOnLoad(globalLoadingCanvas);
        SceneControl_Root.loadingPanel = globalLoadingCanvas;//每次进场景时都要做一遍
        SceneControl_Root.progressBar = UIFunction.Inst.GetCompoentInObjsChild<Slider>(globalLoadingCanvas, "Slider");
        SceneControl_Root.percentageText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(globalLoadingCanvas, "%");
        SceneControl_Root.tipText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(globalLoadingCanvas, "Tips");
        SceneControl_Root.loadingBackground = UIFunction.Inst.GetCompoentInObjsChild<Image>(globalLoadingCanvas, "Panel");
        UIFunction.Inst.SetCanvasGroupVise(globalLoadingCanvas.gameObject, false);
        //globalLoadingCanvas.gameObject.SetActive(false);
        TechTreeManager_Root.OnStart();        // 可以最先初始化（自己独立）
        BuildingManager_Root.OnStart();        // 必须在 SaveManager 之前准备好数据
        SaveManager_Root.OnStart();            // 最后执行，进度读取时会用到上面两个的内容
        //tips:我的框架真的漏洞百出啊
        FindCamera();
        #region 推入第一个界面
        if (GameRoot.GetInst().SecondaryCamera.activeSelf)
            SecondaryCamera.SetActive(false);
        if (SaveManager.IsFirstEnter)
            UIManager_Root.Push(new CGPanel());
        else
            UIManager_Root.Push(new StartPanel());
        #endregion
        AudioManager.Inst.SetMusicVolume(saveManager.TheSetMusicVolum) ;
        AudioManager.Inst.SetEffectVolume(saveManager.TheSetEffectVolum) ;
    }
    public void ReLoad()
    {
        TechTreeManager_Root.OnStart(); //returm前inst行，root不行
        BuildingManager_Root.OnStart(); 
        SaveManager_Root.OnStart();           
    }
    public void FindCamera()
    {
        if (!MainCamera)
            MainCamera = GameObject.Find("Main Camera");
        if (!SecondaryCamera)
            SecondaryCamera = GameObject.Find("SecondaryCamera");
    }
    public void SetLoadingCanvas(GameObject globalLoadingCanvas)
    {
        
    }
}
