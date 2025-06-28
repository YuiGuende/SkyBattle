using UnityEngine;
using Mirror;

public enum PlaneSkillType
{
    FighterBoost,
    JammingField,
    AreaBomb,
    RadarScan,
    TrapDeploy
}

public class PlaneSkill : NetworkBehaviour
{
    public PlaneSkillType skillType;
    public Sprite skillIcon;
    public int energyCost = 1;
    public int cooldownTurns = 3;

    [SyncVar] public int currentCooldown = 0;
    [SyncVar] public bool isOnCooldown = false;

    public bool isSilenced = false;

    public void ActivateSkill()
    {
        if (isSilenced)
        {
            Debug.Log("🔇 Bạn đang bị câm lặng! Không thể dùng kỹ năng!");
            return;
        }

        NetworkPlayer localPlayer = TurnManager.Instance.localPlayer;
        if (localPlayer == null || !localPlayer.isLocalPlayer)
        {
            Debug.Log("❌ Không tìm thấy localPlayer.");
            return;
        }

        if (isOnCooldown)
        {
            Debug.Log("⏳ Kỹ năng đang cooldown.");
            return;
        }

        if (localPlayer.manaNotification.mana < energyCost)
        {
            Debug.Log($"⚠️ Không đủ mana. Cần {energyCost}, đang có {localPlayer.manaNotification.mana}");
            return;
        }

        // Trừ mana
        localPlayer.CmdConsumeMana(energyCost);

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
                SkillTargeting.Instance.StartTargeting(this, 2);
                break;
            case PlaneSkillType.TrapDeploy:
                Debug.Log("🪤 Kích hoạt: Đặt bẫy!");
                break;
        }

        currentCooldown = cooldownTurns;
        isOnCooldown = true;
        Debug.Log($"✅ Dùng kỹ năng thành công. Tiêu hao {energyCost} mana.");
    }

    public void ReduceCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown--;
            if (currentCooldown == 0)
            {
                isOnCooldown = false;
                Debug.Log("🎉 Kỹ năng đã sẵn sàng!");
            }
        }
    }
}
