using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected CircleCollider2D collison;
    [SerializeField]
    public bool canConnect;
    protected Light elcLight;
    //以下为电荷专有部分
    [Tooltip("电荷量,影响被电场弹开的力度以及带电特效明显程度")]
    public int ChargeNum;
    [Tooltip("最大能够连接的数量")]
    public int MaxConnectNum;
    [Tooltip("当前连接的数量")]
    public int CurrntConnectNum;
    [Tooltip("电荷种类（不唯一）")]
    public int Value;
    [Tooltip("邻居集合")]
    public List<Charge>  neighbour;
    [Tooltip("电荷在胜利判定时的自身编号（唯一）")]
    public int Code;
    /*[Tooltip("在自身电场影响范围内的电荷")]
    public List<int>  EffectCharges;*/
    [Tooltip("是否已连接")]
    public bool IsConnected;
    
    protected virtual void creatMaterial()// 创建并设置 PhysicsMaterial2D
    {      
        PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D();
        bouncyMaterial.bounciness = 1.0f;
        bouncyMaterial.friction = 0.0f;
        // 将 PhysicsMaterial2D 应用于 Collider2D
        collison.sharedMaterial = bouncyMaterial;
    }
    protected virtual void FixedUpdate()//吸引附近的异种电荷,排斥同种电荷
    {
        if (ChargeNum == 0)
            return;
        else
        {
            Collider2D[] effectCharges = Physics2D.OverlapCircleAll(transform.position, Mathf.Abs(ChargeNum)*2);//超出ChargeNum*2.5f这个范围视为无穷远处
            foreach (Collider2D hitCollider in effectCharges)
            {
                //Debug.Log(hitCollider.gameObject.name);
                if (hitCollider.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigiball) && hitCollider.TryGetComponent<Ball>(out Ball ball))
                {
                    Vector3 direction = (ball.transform.position - transform.position).normalized;
                    float dis = Mathf.Abs(Vector3.Distance(transform.position, ball.transform.position));
                    if(dis!=0)
                    {
                        float force = (ChargeNum * ball.ChargeNum) / (dis * dis); // 库伦定理，k值待定
                        rigiball.AddForce(direction * force);
                    }                    
                }
            }
        }
    }
}
