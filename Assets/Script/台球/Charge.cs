using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Charge : Ball
{
    Charge self;
    SpriteRenderer colorCompoent;
    GameObject colorObject;

    protected virtual void Start()
    {
        self = gameObject.GetComponent<Charge>();
        collison = gameObject.GetComponent<CircleCollider2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        elcLight = gameObject.GetComponent<Light>();
        creatMaterial();
        canConnect = true;
        
        colorObject= Instantiate<GameObject>(Resources.Load<GameObject>("Prefeb/ColorObject"), transform);
        colorCompoent = UIFunction.Inst.GetCompoentInObjsChild<SpriteRenderer>(colorObject,"color");
        changeColor();
    }
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canConnect) return;

        if (collision.gameObject.TryGetComponent<Charge>(out Charge otherCharge))
        {
            ConnectionManager.GetInst().HandleCollision(self, otherCharge);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    public void changeColor()
    {
        if (elcLight == null)
        {
            Debug.LogWarning(self.gameObject.name + "上没有找到Light组件");
        }
        Color color = ChargeNum > 0 ? Color.red : Color.blue;
        if (ChargeNum == 0)
            color = new Color(0, 0, 0, 0);
        colorCompoent.color = color;

        float scale = Mathf.Abs(ChargeNum);
        colorObject.transform.localScale = new Vector3(scale, scale, colorObject.transform.localScale.z);

        elcLight.intensity = 2 * Mathf.Abs(ChargeNum);//根据电荷大小设置强度
        elcLight.range = 2 * Mathf.Abs(ChargeNum);//根据电荷大小设置光照区域
        elcLight.color = ChargeNum > 0 ? Color.red : Color.blue;//根据电荷正负设置颜色        
    }
    public void ChangeCharge(int amount)
    {
        ChargeNum += amount;
        // 更新小球的颜色和其他电荷相关的属性
        changeColor();
    }

}
