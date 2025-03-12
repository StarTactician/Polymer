using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Charge : Ball
{
    Charge self;
    
    void Start()
    {
        self = GetComponent<Charge>();
        collison = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        elcLight = GetComponent<Light>();
        creatMaterial();
        canConnect = true;
        changeColor();
    }
    protected void OnCollisionEnter2D(Collision2D collision)//电荷链接的逻辑
    {

        //执行结束时，如果当前链接数等于最大链接数（大于等于）canConnected = false        
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
        elcLight.intensity = 2 * Mathf.Abs(ChargeNum);//根据电荷大小设置强度
        elcLight.range = 2* Mathf.Abs(ChargeNum);//根据电荷大小设置光照区域
        elcLight.color = ChargeNum > 0 ? Color.red : Color.blue;//根据电荷正负设置颜色        
    }
}
