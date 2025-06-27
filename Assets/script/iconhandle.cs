using UnityEngine;

public class iconhandle : MonoBehaviour
{
    public PlaneSkill skillLinked;

    private void OnMouseDown()
    {
        if (skillLinked != null)
        {
            Debug.Log("📦 Kích hoạt kỹ năng từ icon: " + skillLinked.skillType);
            skillLinked.ActivateSkill();
        }
    }
}