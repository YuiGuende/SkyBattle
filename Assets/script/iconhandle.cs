using UnityEngine;

public class iconhandle : MonoBehaviour
{
    public PlaneSkill skillLinked;

    //private void OnMouseDown()
    //{
    //    if (skillLinked != null)
    //    {
    //        Debug.Log("📦 Kích hoạt kỹ năng từ icon: " + skillLinked.skillType);
    //        skillLinked.ActivateSkill();
    //    }
    //}
    private void OnMouseDown()
    {
        if (skillLinked != null)
        {
            Debug.Log("📦 Chọn kỹ năng: " + skillLinked.skillType);
            var player = TurnManager.Instance.localPlayer;
            if (player != null)
            {
                player.selectedSkill = skillLinked;
            }
        }
    }

}