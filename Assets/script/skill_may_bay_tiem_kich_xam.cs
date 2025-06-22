using UnityEngine;

public class skill_may_bay_tiem_kich_xam : MonoBehaviour, ISkill
{
    public GridManager ownerGrid;         // GridManager sở hữu skill này
    public int manaCost = 5;              // Mana để dùng
    public int cooldownTurns = 6;         // Cooldown tổng
    private int currentCooldown = 0;      // Còn lại bao nhiêu lượt

    public string skillName = "Skill Xám 2";

    void OnMouseDown()
    {
        if (ownerGrid == null || !ownerGrid.isGameStarted)
            return;

        if (TurnManager.Instance.GetCurrentTurn() != ownerGrid.gridOwner)
            return;

        if (currentCooldown > 0)
        {
            ownerGrid.notification.text = $"⏳ {skillName} đang hồi! Còn {currentCooldown} lượt.";
            return;
        }

        if (ownerGrid.mana < manaCost)
        {
            ownerGrid.notification.text = $"⚠️ Không đủ năng lượng để dùng {skillName}!";
            return;
        }

        ActivateSkill();
    }

    void ActivateSkill()
    {
        // Tùy theo skill, bạn có thể gắn hàm riêng tại đây
        ownerGrid.shotsRemaining += 2;
        ownerGrid.mana -= manaCost;
        currentCooldown = cooldownTurns;

        ownerGrid.notification.text = $"🛩️ {skillName} kích hoạt! +2 lượt bắn.\nCooldown: {cooldownTurns} lượt.";
    }

    // Gọi từ GridManager mỗi lượt
    public void TickCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }

    public int GetCooldownRemaining() => currentCooldown;
}
