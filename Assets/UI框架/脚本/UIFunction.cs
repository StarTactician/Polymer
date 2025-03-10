using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 由于需要淡入淡出的协程，故该脚本需要继承自MonoBehaviour，必须挂载到GameManager的游戏物体上
/// </summary>
public class UIFunction : MonoBehaviour
{

    public static UIFunction Inst;
    private BasePanel panel;
    public Dictionary<int, string> RomanMap = new Dictionary<int, string>()
        {
        {10, "X"},
        {9, "IX"},
        {8, "Ⅷ"},
        {7, "Ⅶ"},
        {6, "Ⅵ"},
        {5, "V"},
        {4, "IV"},
        {3, "Ⅲ"},
        {2, "Ⅱ"},
        {1, "I"},
        {0, ""}
    };
    //public float FadeTime;
    /*public static UIFunction GetInst()
    {
        if (Inst == null)
            Inst = new UIFunction();

        return Inst;
    }*/
    private void Awake()
    {
        if (Inst == null)
            Inst = this;

        else if (Inst != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(BasePanel basePanel)
    {
        panel = basePanel;
    }
    /// <summary>
    /// 需要执行UI界面整体淡入淡出操作时调用
    /// </summary>
    /// <param name="duration">淡入淡出的时间</param>
    /// <param name="basepanel">执行操作的具体界面</param>
    /// <param name="isFadingIn">淡入true淡出false</param>
    public void StartFade(float duration, BasePanel basepanel,bool isFadingIn, float waitTime = 0)
    {
        StartCoroutine(FadeCoroutine(duration, basepanel, isFadingIn, waitTime));
    }
    /// <summary>
    /// 需要执行某个RawImage对象整体淡入淡出操作时调用
    /// </summary>
    /// <param name="duration">淡入淡出的时间</param>
    /// <param name="image">执行操作的具体 RawImage 对象</param>
    /// <param name="isFadingIn">淡入 true，淡出 false</param>
    public void StartFade(float duration, Image image, bool isFadingIn, float waitTime = 0)
    {
        StartCoroutine(FadeCoroutine(duration, image, isFadingIn, waitTime));
    }
    /// <summary>
    /// 需要执行某个Image对象整体淡入淡出操作时调用
    /// </summary>
    /// <param name="duration">淡入淡出的时间</param>
    /// <param name="rawImage">执行操作的具体 Image 对象</param>
    /// <param name="isFadingIn">淡入 true，淡出 false</param>
    public void StartFade(float duration, RawImage rawImage, bool isFadingIn, float waitTime = 0)
    {
        StartCoroutine(FadeCoroutine(duration, rawImage, isFadingIn, waitTime));
    }
    /// <summary>
    /// 需要执行UI元素放大缩小操作时调用
    /// </summary>
    /// <param name="duration">放大缩小过程的时间</param>
    /// <param name="rectTransform">执行操作的具体 RectTransform 对象</param>
    /// <param name="targetScale">目标缩放比例</param>
    /// <param name="waitTime">执行前等待的时间</param>
    public void StartScale(float duration, RectTransform rectTransform, Vector3 targetScale,float waitTime=0)
    {
        StartCoroutine(ScaleCoroutine(duration, rectTransform, targetScale,waitTime));
    }
    /// <summary>
    /// 需要滑动屏幕时的操作（仅限在scrollRect上滑动）
    /// </summary>
    /// <param name="scrollRect"></param>
    /// <param name="targetX"></param>
    public void StartSmoothScroll(RectTransform content, float targetX)
    {
        StartCoroutine(SmoothScroll(content, targetX));
    }
    /// <summary>
    /// 右上角生成数字图片或者new的方法
    /// </summary>
    /// <param name="targetImage">指定的图片</param>
    /// <param name="number">显示的数字</param>
    /// <param name="isNum">控制类型（数字 or New）</param>
    public GameObject AddBadgeToImage(Image targetImage, int? number = null,float size=40,float x=35,float y=0,bool isNum=true)
    {
        if (targetImage == null)
        {
            Debug.LogWarning("目标 Image 为空，无法生成徽章！");
            return null;
        }

        // 创建一个新的 Image 作为徽章
        //GameObject badgeObj = new GameObject("Badge");//不对，应该是生成一个带有textpro文字的预制体才对
        
        // 根据参数选择对应的资源路径
        string resourcePath = "";
        
        if (!isNum)
        {
            resourcePath = "IMG/Badges/New"; // 你的 "New" 标志图片路径
        }
        else if (isNum && number.HasValue)
        {
            resourcePath = $"IMG/Badges/number"; // 你的数字图片路径，如 "1", "2", "3"
        }
        else
        {
            Debug.LogWarning("未知的徽章类型或数字为空！");
            return null;
        }
        GameObject badgeObj = Instantiate<GameObject>(Resources.Load<GameObject>(resourcePath), targetImage.transform);       
        //Sprite badgeSprite = Resources.Load<Sprite>(resourcePath);
        if (badgeObj == null)
        {
            Debug.LogWarning($"未找到资源: {resourcePath}");
            return null;
        }
        if(number.HasValue)
            Inst.GetCompoentInObjsChild<TextMeshProUGUI>(badgeObj, "Num").text = number.ToString();
        //badgeObj.GetComponent<TextMeshProUGUI>().text = number.ToString();
        // 设定角标的位置
        RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
        badgeRect.sizeDelta = new Vector2(size, size); // 设置徽章大小（可调整）
        badgeRect.anchorMin = new Vector2(1, 1); // 右上角
        badgeRect.anchorMax = new Vector2(1, 1);
        badgeRect.pivot = new Vector2(1, 1);
        badgeRect.anchoredPosition = new Vector2(x, y); // 适当调整位置
        return badgeObj;
    }

    /// <summary>
    /// 从场景中获取画布物体
    /// </summary>
    /// <returns></returns>
    public GameObject FindCanvas()
    {
        GameObject canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
        if(canvas==null)        
            Debug.LogError("没有找到画布物体");       
        return canvas;
    }
    /// <summary>
    /// 从画布物体中获得它的某个子物体
    /// </summary>
    /// <param name="panel">画布物体</param>
    /// <param name="childName">所寻找的子物体的名称</param>
    /// <returns></returns>
    public GameObject GetChild(GameObject panel,string childName)
    {
        Transform[] childen = panel.GetComponentsInChildren<Transform>();
        foreach(Transform child in childen)
        {
            if(child.gameObject.name==childName)            
                return child.gameObject;            
        }
        Debug.LogWarning("没有在"+panel.name+"中找到"+childName );
        return null;
    }
    /// <summary>
    /// 在物体上获取某个组件
    /// </summary>
    /// <typeparam name="T">所寻找的组件</typeparam>
    /// <param name="gameObject">画布物体</param>
    /// <returns></returns>
    public T GetComponentInObj<T>(GameObject gameObject)
    {
        if(gameObject == null)
        {
            Debug.LogError("传入的 GameObject 为空！");
            return default(T);
        }
        if (gameObject.TryGetComponent<T>(out T component))        
            return component;        
        else
        {
            Debug.LogWarning("在 " + gameObject.name + " 上未找到组件 " + typeof(T).Name);
            return default(T);
        }
    }
    /// <summary>
    /// 在画布物体的指定子物体上获取某个组件
    /// </summary>
    /// <typeparam name="T">所寻找的组件</typeparam>
    /// <param name="gameObject">画布物体</param>
    /// <param name="childName">所寻找的子物体的名称</param>
    /// <returns></returns>
    public T GetCompoentInObjsChild<T>(GameObject gameObject,string childName)
    {
        if(gameObject == null)
        {
            Debug.LogError("传入的 GameObject 为空！");
            return default(T);
        }
        Transform[] childen = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in childen)
        {
            if (child.gameObject.name == childName)
            {
                if (child.gameObject.TryGetComponent<T>(out T component))
                    return component;
                else
                {
                    Debug.LogWarning("在 " + child.gameObject.name + " 上未找到组件 " + typeof(T).Name);
                    return default(T);
                }
            }                                       
        }
        Debug.LogWarning("没有在" + gameObject.name + "中找到" + childName);
        return default(T);
    }
    /// <summary>
    /// 在某物体的指定子物体上获取某个组件组
    /// </summary>
    /// <typeparam name="T">所寻找的组件</typeparam>
    /// <param name="gameObject">画布物体</param>
    /// <param name="childName">从这个子物体上的子物体获取组件</param>
    /// <returns></returns>
    public List<T> GetComponentsInObjsChild<T>(GameObject gameObject, string childName)
    {
        if (gameObject == null)
        {
            Debug.LogError("传入的 GameObject 为空！");
            return null;
        }

        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.name == childName)
            {
                List<T> components = new List<T>();
                Transform[] subChildren = child.GetComponentsInChildren<Transform>();
                foreach (Transform subChild in subChildren)
                {
                    if (subChild.gameObject.TryGetComponent<T>(out T component))
                    {
                        components.Add(component);
                    }
                }

                if (components.Count == 0)
                {
                    Debug.LogWarning("在 " + child.gameObject.name + " 的子物体中未找到组件 " + typeof(T).Name);
                    return null;
                }
                return components;
            }
        }

        Debug.LogWarning("没有在 " + gameObject.name + " 中找到 " + childName);
        return null;
    }
    /// <summary>
    /// 在某物体上获取指定组件组
    /// </summary>
    /// <typeparam name="T">所寻找的组件</typeparam>
    /// <param name="gameObject">指定物体</param>
    /// <returns></returns>
    public List<T> GetComponentsInObj<T>(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("传入的 GameObject 为空！");
            return null;
        }

        List<T> components = new List<T>(); // 在循环外部声明组件列表

        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            // 过滤掉自身，只处理子物体
            if (child == gameObject.transform)
            {
                continue;
            }
            if (child.gameObject.TryGetComponent<T>(out T component))
            {
                components.Add(component);
            }
        }

        // 如果没有找到任何组件，输出警告并返回 null
        if (components.Count == 0)
        {
            Debug.LogWarning("在 " + gameObject.name + " 的子物体中未找到组件 " + typeof(T).Name);
            return null;
        }

        return components;
    }
    /// <summary>
    /// 取代SetActive的方法，用来设置UI物体的可见性与交互性
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="visible"></param>
    public void SetCanvasGroupVise(GameObject gameObject, bool visible)
    {
        CanvasGroup canvasGroup ;
        if (GetComponentInObj<CanvasGroup>(gameObject) == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        else
            canvasGroup = GetComponentInObj<CanvasGroup>(gameObject);

        if (visible)
        {
            canvasGroup.alpha = 1;          // 设置完全可见
            canvasGroup.blocksRaycasts = true;  // 启用交互
            canvasGroup.interactable = true;    // 启用交互
        }
        else
        {
            canvasGroup.alpha = 0;          // 设置完全不可见
            canvasGroup.blocksRaycasts = false; // 禁用交互
            canvasGroup.interactable = false;   // 禁用交互
        }
    }
    /// <summary>
    /// 移动某个物体向某个方向时调用
    /// </summary>
    /// <param name="obj">要移动的物体</param>
    /// <param name="direction">要移动的方向</param>
    /// <param name="distance">要移动的距离</param>
    public Coroutine MoveObject(GameObject obj, Vector2 direction, float distance, float duration,BasePanel basePanel)
    {
        if (!basePanel.IsMoveing)
            return StartCoroutine(MoveToTargetCoroutine(obj, direction * distance, duration, basePanel));
        else return null;
    }
    // 平滑移动到目标位置的协程
    private IEnumerator MoveToTargetCoroutine(GameObject obj, Vector2 targetOffset, float duration, BasePanel basePanel)
    {
        if (obj == null)
        {
            Debug.LogWarning("要移动的游戏物体为空！");
            yield break;
        }
        basePanel.IsMoveing = true;
        Vector2 startPosition = obj.transform.localPosition;
        Vector2 targetPosition = startPosition + targetOffset;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            obj.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.localPosition = targetPosition;
        basePanel.IsMoveing = false;
    }
    /// <summary>
    /// 需要每隔多久执行一次什么函数
    /// </summary>
    /// <param name="method">需要执行的函数</param>
    /// <param name="interval">函数执行的间隔</param>
    /// <returns></returns>
    public Coroutine StartCoroutineWithInterval(System.Action method, float interval)
    {
        return StartCoroutine(IntervalCoroutine(method, interval));
    }

    private IEnumerator IntervalCoroutine(System.Action method, float interval)
    {
        while (true)
        {
            method.Invoke();
            yield return new WaitForSecondsRealtime(interval);
        }
    }
    /// <summary>
    /// 需要停止被循环执行的函数时调用
    /// </summary>
    /// <param name="coroutine"></param>
    public void StopTheCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }
    /// <summary>
    /// 需要延迟执行函数方法的协程
    /// </summary>
    /// <param name="method"></param>
    /// <param name="interval"></param>
    public Coroutine StartDelayMethod(System.Action method, float interval)
    {
        return StartCoroutine(DelayTimeCoroutine(method, interval));
    }

    private IEnumerator DelayTimeCoroutine(System.Action method, float interval)
    {
        yield return new WaitForSeconds(interval);
        method.Invoke();
    }
    #region 处理淡入淡出的具体协程方法
    private IEnumerator FadeCoroutine(float duration, BasePanel basepanel, bool isFadingIn, float waitTime = 0)
    {
        if (basepanel == null)
        {
            yield break; // 如果对象在中途被销毁，退出协程
        }
        yield return new WaitForSeconds(waitTime);
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = basepanel.PanelObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = basepanel.PanelObj.AddComponent<CanvasGroup>();
        }

        // 如果是淡入，起始透明度为0，淡出则起始透明度为1
        float startAlpha = isFadingIn ? 0f : 1f;
        float endAlpha = isFadingIn ? 1f : 0f;

        while (elapsedTime < duration)
        {
            if (basepanel == null)
            {
                yield break; // 如果对象在中途被销毁，退出协程
            }
            elapsedTime += Time.unscaledDeltaTime;
            // 计算当前alpha值
            if(canvasGroup)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / duration));
            yield return null;
        }
        // 确保动画结束时，alpha值准确设置为目标值
        canvasGroup.alpha = endAlpha;
    }
    
    /// <summary>
    /// 需要执行UI Image对象整体淡入淡出操作时调用
    /// </summary>
    /// <param name="duration">淡入淡出的时间</param>
    /// <param name="image">执行操作的具体 Image 对象</param>
    /// <param name="isFadingIn">淡入 true，淡出 false</param>
    private IEnumerator FadeCoroutine(float duration, Image image, bool isFadingIn, float waitTime = 0)
    {
        if (image == null)
        {
            yield break; // 如果对象在中途被销毁，退出协程
        }
        yield return new WaitForSeconds(waitTime);
        float elapsedTime = 0f;

        // 如果是淡入，起始透明度为0，淡出则起始透明度为1
        float startAlpha = isFadingIn ? 0f : 1f;
        float endAlpha = isFadingIn ? 1f : 0f;

        Color currentColor = image.color;
        currentColor.a = startAlpha;
        image.color = currentColor;

        while (elapsedTime < duration)
        {
            if (image == null)
            {
                yield break; // 如果对象在中途被销毁，退出协程
            }
            elapsedTime += Time.unscaledDeltaTime;
            // 计算当前alpha值
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / duration));
            currentColor.a = newAlpha;
            image.color = currentColor;
            yield return null;
        }
        // 确保动画结束时，alpha值准确设置为目标值
        currentColor.a = endAlpha;
        image.color = currentColor;
    }
    private IEnumerator FadeCoroutine(float duration, RawImage rawImage, bool isFadingIn, float waitTime = 0)
    {
        if (rawImage == null)
        {
            yield break; // 如果对象在中途被销毁，退出协程
        }
        yield return new WaitForSeconds(waitTime);
        float elapsedTime = 0f;

        // 如果是淡入，起始透明度为0，淡出则起始透明度为1
        float startAlpha = isFadingIn ? 0f : 1f;
        float endAlpha = isFadingIn ? 1f : 0f;

        Color currentColor = rawImage.color;
        currentColor.a = startAlpha;
        rawImage.color = currentColor;

        while (elapsedTime < duration)
        {
            if (rawImage == null)
            {
                yield break; // 如果对象在中途被销毁，退出协程
            }
            elapsedTime += Time.unscaledDeltaTime;
            // 计算当前alpha值
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / duration));
            currentColor.a = newAlpha;
            rawImage.color = currentColor;
            yield return null;
        }
        // 确保动画结束时，alpha值准确设置为目标值
        currentColor.a = endAlpha;
        rawImage.color = currentColor;
    }
    #endregion
    private IEnumerator SmoothScroll(RectTransform content, float targetX)
    {
        float duration = 0.3f; // 滚动时间
        float elapsedTime = 0;
        Vector2 startPos = content.anchoredPosition;
        Vector2 targetPos = new Vector2(targetX, startPos.y);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        content.anchoredPosition = targetPos; // 确保到达目标位置
    }
    #region 处理UI元素放大缩小的具体协程方法
    private IEnumerator ScaleCoroutine(float duration, RectTransform rectTransform, Vector3 targetScale,float waitTime=0)
    {
        if (rectTransform == null)
        {
            yield break; // 如果对象在中途被销毁，退出协程
        }
        yield return new WaitForSeconds(waitTime);
        float elapsedTime = 0f;
        Vector3 initialScale = rectTransform.localScale;

        while (elapsedTime < duration)
        {
            if (rectTransform == null)
            {
                yield break; // 如果对象在中途被销毁，退出协程
            }
            elapsedTime += Time.unscaledDeltaTime;
            // 计算当前缩放值
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, Mathf.Clamp01(elapsedTime / duration));
            yield return null;
        }
        // 确保动画结束时，缩放值准确设置为目标值
        if (rectTransform == null)
        {
            yield break; // 如果对象在中途被销毁，退出协程
        }
        rectTransform.localScale = targetScale;
    }
    #endregion
}
