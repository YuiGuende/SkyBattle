//using UnityEngine;

//public class start_game : MonoBehaviour
//{
//    // Start is called once before the first execution of Update after the MonoBehaviour is created

//    public GridManager grid1;
//    public GridManager grid2;

//    void OnMouseDown()
//    {
//        if (grid1 != null) grid1.StartGame();
//        if (grid2 != null) grid2.StartGame();

//        gameObject.SetActive(false); // Ẩn nút start
//    }
//}
using UnityEngine;

public class start_game : MonoBehaviour
{
    public GridManager grid1;
    public GridManager2 grid2;

    public GameObject gridPlayer1; // cha của các máy bay player 1
    public GameObject gridPlayer2; // cha của các máy bay player 2

    void OnMouseDown()
    {
        if (grid1 != null) grid1.StartGame();
        if (grid2 != null) grid2.StartGame();

        DisableAllPlaneColliders(); // 👈 thêm dòng này

        gameObject.SetActive(false); // Ẩn nút start
    }

    void DisableAllPlaneColliders()
    {
        DisablePlaneCollidersIn(gridPlayer1);
        DisablePlaneCollidersIn(gridPlayer2);
    }

    void DisablePlaneCollidersIn(GameObject gridRoot)
    {
        if (gridRoot == null) return;

        // Nếu bạn dùng tag "Plane", có thể lọc dễ hơn
        Collider2D[] colliders = gridRoot.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            // Nếu collider nằm trên máy bay thì tắt
            if ( col.gameObject.CompareTag("Plane"))
            {
                col.enabled = false;
            }
        }
    }
}
