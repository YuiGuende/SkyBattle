using UnityEngine;

public class SkillTargeting : MonoBehaviour
{
    public static SkillTargeting Instance;

    private PlaneSkill activeSkill;
    private int areaRadius = 0;
    private bool isTargeting = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartTargeting(PlaneSkill skill, int radius)
    {
        activeSkill = skill;
        areaRadius = radius;
        isTargeting = true;

        Debug.Log("🎯 Hãy chọn ô trung tâm để thả skill: " + skill.skillType);
    }

    // Hàm này sẽ được gọi từ GridCell khi người chơi chọn ô
    public void OnCellSelected(int x, int y)
    {
        if (!isTargeting || activeSkill == null) return;

        Debug.Log($"📌 Kích hoạt skill {activeSkill.skillType} tại ({x},{y})");

        NetworkPlayer localPlayer = TurnManager.Instance?.localPlayer;
        if (localPlayer == null)
        {
            Debug.LogError("❌ Không tìm thấy localPlayer!");
            return;
        }

        switch (activeSkill.skillType)
        {
            case PlaneSkillType.AreaBomb:
                // Gửi tọa độ về server để xử lý
                localPlayer.CmdRequestBOMBDUYMOM(x, y);
                break;

                // Thêm các skill khác tại đây
                // case PlaneSkillType.JammingField: ...
                // case PlaneSkillType.TrapDeploy: ...
        }

        // Reset trạng thái chọn
        isTargeting = false;
        activeSkill.currentCooldown = activeSkill.cooldownTurns;
        activeSkill.isOnCooldown = true;
    }
}
