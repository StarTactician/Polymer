using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneControl
{
    public enum LevelEntryStatus { Started, Unavailable, InsufficientPU, Busy }

    private sealed class LevelEntry
    {
        public readonly int Level;
        public readonly int Cost;
        public readonly int BuildIndex;
        public readonly string Path;
        public bool ActivationRequested;
        public bool SceneObserved;
        public bool Settled;

        public LevelEntry(int level, int cost, int buildIndex, string path)
        {
            Level = level;
            Cost = cost;
            BuildIndex = buildIndex;
            Path = path;
        }
    }

    private static SceneControl Inst;
    private LevelEntry pendingLevelEntry;
    public bool IsLevelEntryPending => pendingLevelEntry != null;
    public Dictionary<string, BaseScene> Scene_Dict;

    public GameObject loadingPanel;
    public Slider progressBar;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI tipText;
    public Image loadingBackground;
    private string[] loadingTips = {
    "正在部署月球太阳能板...",
    "正在运送聚合基元...",
    "连接月球主控 AI 中...",
    "初始化基团合成模块..."
};

    public static SceneControl GetInst()
    {        
        if (Inst == null)
        {           
            Debug.LogError("未找到SceneManager实例");
            return Inst;
        }
        //Debug.Log("SceneManager被实例化！");
        return Inst;
    }
    public SceneControl()
    {
        Inst = this;
        Scene_Dict = new Dictionary<string, BaseScene>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    /// <summary>
    /// 加载指定场景时调用，一个类型的人可以有很多，同一个类型的人总会有一个共同点
    /// </summary>
    /// <param name="scene_name">目标场景的名称（相当于具体人名）</param>
    /// <param name="baseScene">目标场景的BaseScene脚本（相当于一个类型） </param>
    public void LoadScene(string scene_name, BaseScene baseScene)
    {
        if (IsLevelEntryPending) return;

        if (!Scene_Dict.ContainsKey(scene_name))
         Scene_Dict.Add(scene_name, baseScene);
        // 调用协程（因为 SceneControl 不是 MonoBehaviour，需要由 GameRoot 发起）
        PRTS.GetInst().ResetSession();

        GameRoot.GetInst().StartCoroutine(LoadSceneAsync(scene_name));
    }
/*public void LoadScene(string scene_name,BaseScene baseScene)
{
    if(!Scene_Dict.ContainsKey(scene_name))
    {
        Scene_Dict.Add(scene_name, baseScene);
    }
    //在加载指定场景前先退出当前的场景
    if (Scene_Dict.ContainsKey(SceneManager.GetActiveScene().name))
    {
        Scene_Dict[SceneManager.GetActiveScene().name].ExitScene();
    }
    else
        Debug.LogWarning("Scene_Dict字典中不包含"+ SceneManager.GetActiveScene().name);
    UIManager.GetInst().Pop(true);
    SceneManager.LoadScene(scene_name);
}*/
/// <summary>
/// 重开当前关卡时调用
/// </summary>
public void LoadScene()
    {
        if (IsLevelEntryPending) return;

        UIManager.GetInst().Pop(true);
        PRTS.GetInst().ResetSession();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /// <summary>
    /// 当场景加载完成后调用，确保只有在场景加载完全后再调用EnterScene方法，防止出现空引用
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LevelEntry entry = pendingLevelEntry;
        if (entry != null && mode == LoadSceneMode.Single &&
            scene.buildIndex == entry.BuildIndex && scene.path == entry.Path)
        {
            if (entry.ActivationRequested) EnterPaidLevel(scene, entry);
            return;
        }

        //UIManager.GetInst().Pop(true);//确保完全
        if (Scene_Dict.ContainsKey(scene.name))
        {
            Scene_Dict[scene.name].EnterScene();
        }
        else
        {
            Debug.LogWarning("Scene_Dict字典中不包含" + scene.name);
        }
    }
    /// <summary>
    ///加载对应关卡，选关确认后，以及过关后确定进入下一关的时候被调用
    /// </summary>
    /// /// <param name="level">要加载的指定关卡</param>
    private static string LevelPath(int level) => $"Assets/Scenes/Level/{level}.unity";

    private static int GetLoadableBuildIndex(string path)
    {
        int index = SceneUtility.GetBuildIndexByScenePath(path);
        return index >= 0 && Application.CanStreamedLevelBeLoaded(index) ? index : -1;
    }

    public bool CanLoadLevel(int level)
    {
        return level > 0 && GetLoadableBuildIndex(LevelPath(level)) >= 0 &&
            LevelSceneFactory.CreateScene(level) != null;
    }

    public LevelEntryStatus TryBeginLevelLoad(int level, int cost)
    {
        if (IsLevelEntryPending) return LevelEntryStatus.Busy;
        if (level <= 0 || cost < 0) return LevelEntryStatus.Unavailable;

        string path = LevelPath(level);
        int index = GetLoadableBuildIndex(path);
        BaseScene levelController = index >= 0 ? LevelSceneFactory.CreateScene(level) : null;
        if (levelController == null) return LevelEntryStatus.Unavailable;
        if (SaveManager.CurrentPU < cost) return LevelEntryStatus.InsufficientPU;

        GameRoot root = GameRoot.GetInst();
        if (root == null || !root.isActiveAndEnabled) return LevelEntryStatus.Unavailable;

        AsyncOperation operation;
        try
        {
            operation = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        }
        catch (System.Exception error)
        {
            Debug.LogException(error);
            return LevelEntryStatus.Unavailable;
        }
        if (operation == null) return LevelEntryStatus.Unavailable;

        operation.allowSceneActivation = false;
        string sceneName = level.ToString();
        if (!Scene_Dict.ContainsKey(sceneName)) Scene_Dict.Add(sceneName, levelController);
        LevelEntry entry = new LevelEntry(level, cost, index, path);
        pendingLevelEntry = entry;
        root.StartCoroutine(LoadSceneAsync(sceneName, operation, entry));
        return LevelEntryStatus.Started;
    }

    private void EnterPaidLevel(Scene scene, LevelEntry entry)
    {
        if (entry.SceneObserved) return;
        entry.SceneObserved = true;

        int previousLevel = SaveManager.CurrentLevel;
        SaveManager.CurrentLevel = entry.Level;
        try
        {
            Scene_Dict[scene.name].EnterScene();
        }
        catch (System.Exception error)
        {
            SaveManager.CurrentLevel = previousLevel;
            Debug.LogException(error);
            return;
        }

        if (SaveManager.CurrentPU < entry.Cost)
        {
            Debug.LogError("PU balance changed during level loading.");
            return;
        }

        SaveManager.GetInst().ResourcesChange(-entry.Cost, "PU");
        entry.Settled = true;
    }

    public void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    /// <summary>
    /// 异步加载所需
    /// </summary>
    /// <param name="scene_name"></param>
    /// <returns></returns>
    private void ExitCurrentScene()
    {
        if (Scene_Dict.TryGetValue(SceneManager.GetActiveScene().name, out BaseScene previousScene))
            previousScene.ExitScene();
        UIManager.GetInst().Pop(true);
    }

    private IEnumerator LoadSceneAsync(string scene_name, AsyncOperation op = null, LevelEntry entry = null)
    {
        try
        {
            if (!loadingPanel)
                loadingPanel=GameRoot.GetInst().globalLoadingCanvas;
            if (entry == null) ExitCurrentScene();

            // 激活加载面板
            //loadingPanel.SetActive(true);
            if (loadingPanel) UIFunction.Inst.SetCanvasGroupVise(loadingPanel, true);

            // 设置随机背景图
            Sprite[] backgrounds = Resources.LoadAll<Sprite>("LoadingBG");
            if (backgrounds.Length > 0 && loadingBackground != null)
            {
                loadingBackground.sprite = backgrounds[Random.Range(0, backgrounds.Length)];
            }

            // 设置随机 tip 文本
            if (tipText != null && loadingTips.Length > 0)
            {
                tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            // 初始化进度条和文本
            if (progressBar != null) progressBar.value = 0;
            if (percentageText != null) percentageText.text = "0%";

            // 开始异步加载
            if (op == null)
            {
                op = SceneManager.LoadSceneAsync(scene_name);
                if (op == null) yield break;
                op.allowSceneActivation = false;
            }

            while (!op.isDone && op.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);

                if (progressBar != null) progressBar.value = progress * 100;
                if (percentageText != null) percentageText.text = Mathf.RoundToInt(progress * 100) + "%";

                yield return null;
            }

            // 显示完成状态
            if (op.isDone) yield break;

            if (progressBar != null) progressBar.value = 100;
            if (percentageText != null) percentageText.text = "100%";
            if (tipText != null) tipText.text = "即将进入新场景...";

            if (entry == null) yield return new WaitForSeconds(0.5f);
            else yield return new WaitForSecondsRealtime(0.5f);
            if (op.isDone) yield break;
            if (entry != null)
            {
                PRTS.GetInst().ResetSession();
                ExitCurrentScene();
                entry.ActivationRequested = true;
            }
            op.allowSceneActivation = true;
            if (entry != null) yield return op;
        }
        finally
        {
            if (entry != null)
            {
                pendingLevelEntry = null;
                if (!entry.Settled) Debug.LogWarning($"Level {entry.Level} did not finish entering.");
                if (!op.isDone) op.allowSceneActivation = true;
            }
            if (loadingPanel) UIFunction.Inst.SetCanvasGroupVise(loadingPanel, false);
        }
    }

}

