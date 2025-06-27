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

        Debug.Log("🎯 Hãy chọn ô trung tâm để thả bom!");
    }

    // Giả sử GridCell có sự kiện OnMouseDown
    public void OnCellSelected(int x, int y)
    {
        if (!isTargeting || activeSkill == null) return;

        Debug.Log($"💥 Bom sẽ rơi vào vùng quanh ({x}, {y})");

        for (int dx = -areaRadius; dx <= areaRadius; dx++)
        {
            for (int dy = -areaRadius; dy <= areaRadius; dy++)
            {
                int tx = x + dx;
                int ty = y + dy;

                // Gây ảnh hưởng tại ô tx, ty (nếu trong lưới)
                GridManager grid = FindObjectOfType<GridManager>();
                if (grid != null && tx >= 0 && ty >= 0 && tx < 15 && ty < 15)
                {
                    // Thử bắn luôn
                    grid.ShootAt(tx, ty);
                }
            }
        }

        // Reset targeting
        isTargeting = false;
        activeSkill.currentCooldown = activeSkill.cooldownTurns;
        activeSkill.isOnCooldown = true;
    }
}
