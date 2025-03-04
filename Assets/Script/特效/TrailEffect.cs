using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffect : MonoBehaviour
{
    // 拖尾和粒子效果
    public TrailRenderer trailEffect;
    public ParticleSystem particleEffect;

    private GetTouchedVector getTouchedVector;

    void Start()
    {
        getTouchedVector = GameRoot.GetInst().GetTouchedVector_Root;
    }

    void Update()
    {
        if (getTouchedVector != null)
        {
            Vector3 position = getTouchedVector.GetTouchedPosition(Input.mousePosition);
            ApplyEffects(position);
        }
    }

    void ApplyEffects(Vector3 position)
    {
        // 更新拖尾效果的位置
        if (trailEffect != null)
        {
            trailEffect.transform.position = position;
        }

        // 播放粒子效果
        if (particleEffect != null)
        {
            particleEffect.transform.position = position;
            if (!particleEffect.isPlaying)
            {
                particleEffect.Play();
            }
        }
    }
}

// 需要在编辑器中进行的设置：
// 1. 将 TrailRenderer 和 ParticleSystem 组件附加到场景中的某个对象上，然后将其引用拖动到脚本中的 trailEffect 和 particleEffect 变量。
// 2. 确保 GetTouchedVector 已正确挂载在场景中的某个游戏对象上。
// 3. 将此脚本挂载到负责应用特效的对象上，例如一个空的 GameObject。
