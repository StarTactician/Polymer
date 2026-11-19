using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class StartupLoader : MonoBehaviour
{
    public Slider progressBar;       
    public TextMeshProUGUI progressText;        
    public TextMeshProUGUI percent;        
    public string nextSceneName = "StartScene"; 

    void Start()
    {
        StartCoroutine(LoadMainScene());
    }

    IEnumerator LoadMainScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(nextSceneName);
        async.allowSceneActivation = false;  // 控制何时跳转

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            progressBar.value = progress;
            percent.text = Mathf.RoundToInt(progress * 100).ToString()+"%";
            progressText.text = $"正在加载 {Mathf.RoundToInt(progress * 100)}%";

            // 加载完成但未跳转
            if (async.progress >= 0.9f)
            {
                progressText.text = "加载完成，正在进入游戏...";
                yield return new WaitForSeconds(0.5f);
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
