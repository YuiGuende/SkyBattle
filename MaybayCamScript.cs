using UnityEngine;

public class MaybayCamScript : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;

    public GridManager gridManager;

    private void Start()
    {
        Debug.Log("plane has been clicked");
        gridManager =GameObject.FindGameObjectWithTag("GridTag").GetComponent<GridManager>();
    }

    void OnMouseDown()
    {
        Debug.Log("plane has been clicked");
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, 0);
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
            Vector3 newPos = mouseWorld + offset;
            newPos.z = 0;
            transform.position = newPos;
        }
    }


    void OnMouseUp()
    {
        isDragging = false;

        Vector3 localPos = transform.position - gridManager.transform.position;

        int x = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int y = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        if (x >= 0 && x < 15 && y >= 0 && y < 15)
        {
            GameObject cell = gridManager.GetCell(x, y);
            if (cell)
            {
                transform.position = cell.transform.position;
            }
        }
    }
}
