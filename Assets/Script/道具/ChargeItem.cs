using UnityEngine;

public class ChargeItem : Item
{
    [Tooltip("电荷变化量：正数=加电荷，负数=减电荷")]
    public int deltaCharge = 1;

    public override bool TryApplyToCharge(Charge charge)
    {
        if (charge == null) return false;
        if (!CanUse()) return false;

        // 改电荷数值
        charge.ChargeNum += deltaCharge;

        // 同步颜色/光效（Charge自带方法）
        charge.changeColor();

        // 消耗次数
        ConsumeOnce();
        return true;
    }
}
