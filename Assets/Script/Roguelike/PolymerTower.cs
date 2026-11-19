using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolymerTower : MonoBehaviour
{
    public int ID;
    public int Level;
    public List<int> PassiveSkillIDs;


    private float finalDamage;
    private float damageMultiplier;
    private Transform target;
    private float fireCooldown;

    void Awake()
    {
        // 根据 Level 和技能动态计算 baseDamage, damageMultiplier
        finalDamage = (20 + (Level - 1) * 5)*damageMultiplier;
        //damageMultiplier = SkillConfig.GetMultiplier(PassiveSkillIDs);
    }

    void Update()
    {
        AcquireTarget();
        if (target != null)
        {
            AimAt(target.position);
            TryFire();
        }
    }

    void AcquireTarget()
    {
        // 简单示例：找最近的 MonomerBall
        //target = BallManager.Inst.GetNearest(transform.position);
    }

    void AimAt(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0, 0, angle),
            Time.deltaTime * 5f
        );
    }

    void TryFire()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / GetFireRate();
        }
    }

    void Fire()
    {
        //int bulletValue = Mathf.RoundToInt(finalDamage * damageMultiplier);
        // Instantiate projectile,设置参数...
    }

    float GetFireRate()
    {
        // 可根据技能、Level 计算每秒射击次数
        return 1f + PassiveSkillIDs.Count * 0.1f;
    }
}

