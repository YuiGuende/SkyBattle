using UnityEngine;

public class start_game : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
public GridManager gridManager;

    void OnMouseDown()
    {
        if (gridManager != null)
        {
            gridManager.StartGame();
            gameObject.SetActive(false);
        }
    }
}
