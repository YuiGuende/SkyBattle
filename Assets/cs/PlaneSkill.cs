using UnityEngine;

public enum PlaneSkillType
{
    None,
    FighterBoost,
    JammingField,
    AreaBomb,
    RadarScan,
    TrapDeploy
}

public class PlaneSkill : MonoBehaviour
{
    public PlaneSkillType skillType;
    public Sprite skillIcon;
    public int cooldownTurns;
    public int energyCost;

    [HideInInspector] public bool isOnCooldown = false;
    [HideInInspector] public int currentCooldown = 0;

    public void ActivateSkill()
    {
        if (isOnCooldown || !HasEnoughEnergy())
        {
            Debug.Log("❌ Không thể dùng kỹ năng (cooldown hoặc thiếu năng lượng)");
            return;
        }

        switch (skillType)
        {
            case PlaneSkillType.FighterBoost:
                Debug.Log("🛩️ Kích hoạt: Tiêm kích +2 lượt bắn!");
                break;

            case PlaneSkillType.JammingField:
                Debug.Log("📡 Kích hoạt: Nhiễu sóng vùng 6x6!");
                break;

            case PlaneSkillType.AreaBomb:
                Debug.Log("💣 Kích hoạt: Bom vùng 5x5!");
                break;

            case PlaneSkillType.RadarScan:
                Debug.Log("📍 Kích hoạt: Rada quét vùng 2x2!");
                SkillTargeting.Instance.StartTargeting(this, 2); // 2 là bán kính (2 ô => vùng 5x5)
                break;

            case PlaneSkillType.TrapDeploy:
                Debug.Log("🪤 Kích hoạt: Đặt bẫy!");
                break;
        }

        currentCooldown = cooldownTurns;
        isOnCooldown = true;

        Debug.Log($"Đã dùng kỹ năng, tiêu {energyCost} năng lượng.");
    }

    public void ReduceCooldown()
    {
        if (isOnCooldown)
        {
            currentCooldown--;
            if (currentCooldown <= 0)
            {
                isOnCooldown = false;
                Debug.Log("✅ Kỹ năng đã hồi!");
            }
        }
    }

    private bool HasEnoughEnergy()
    {
        // TODO: Gắn hệ thống năng lượng thật
        return true;
    }
}
