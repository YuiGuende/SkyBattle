using System.Collections.Generic;
using UnityEngine;

public class maybay2 : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private PlaneShape planeShape;

    [Header("References")]
    public GridManager2 gridManager;

    [Header("Placement Settings")]
    public LayerMask gridLayerMask = -1;

    void Start()
    {

        planeShape = GetComponent<PlaneShape>();
        if (planeShape == null)
        {
            planeShape = gameObject.AddComponent<PlaneShape>();
        }
    }

    void Awake()
    {
    }

    void Update()
    {

         if (gridManager != null && gridManager.isGameStarted)
        return; // Lock rotation during game
        // Nhấn R để xoay máy bay
        if (Input.GetKeyDown(KeyCode.R) && isDragging)
        {
            planeShape.Rotate();
        }
    }

    void OnMouseDown()
    {
        Debug.Log("maybaycs đang nhận mouse down");

        if (gridManager != null && gridManager.isGameStarted) { 
            return; // Lock rotation during game
    }
            
       
        
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, 0);
        isDragging = true;

        // Highlight các ô mà máy bay đang chiếm
        HighlightOccupiedCells(true);
    }

    void OnMouseDrag()
    {
        
        if (isDragging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = mouseWorld + offset;
            newPos.z = 0;
            transform.position = newPos;

            // Kiểm tra xem có thể đặt ở vị trí này không
            CheckPlacementValidity();
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        HighlightOccupiedCells(false);

        // Tìm vị trí grid gần nhất
        Vector3 localPos = transform.position - gridManager.transform.position;
        int centerX = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int centerY = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        // Kiểm tra xem toàn bộ máy bay có fit không
        if (CanPlaceAt(centerX, centerY))
        {
            // Đặt máy bay tại vị trí hợp lệ
            PlaceAt(centerX, centerY);

        }
        else
        {
            // Trả về vị trí ban đầu nếu không thể đặt
            Debug.Log("Cannot place plane at this position!");
            // Bạn có thể thêm logic để trả về vị trí ban đầu ở đây
        }
    }

    bool CanPlaceAt(int centerX, int centerY)
    {
        List<PlaneCell> shape = planeShape.GetRotatedShape();

        foreach (PlaneCell cell in shape)
        {
            int gridX = centerX + cell.x;
            int gridY = centerY + cell.y;

            // Kiểm tra boundaries
            if (gridX < 0 || gridX >= 15 || gridY < 0 || gridY >= 15)
            {
                return false;
            }

            // Kiểm tra xem ô có bị chiếm bởi máy bay khác không
            if (gridManager.IsCellOccupied(gridX, gridY, this.gameObject))
            {
                return false;
            }
        }

        return true;
    }

    void PlaceAt(int centerX, int centerY)
    {
        GameObject centerCell = gridManager.GetCell(centerX, centerY);
        if (centerCell != null)
        {
            // Đặt máy bay lên cell, nhưng đẩy lên trục Z một tí để không bị grid đè
            transform.position = centerCell.transform.position + new Vector3(0, 0, -0.1f);

            gridManager.SetPlaneOccupation(centerX, centerY, planeShape.GetRotatedShape(), this.gameObject);
        }
    }


    void CheckPlacementValidity()
    {
        Vector3 localPos = transform.position - gridManager.transform.position;
        int centerX = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int centerY = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        bool canPlace = CanPlaceAt(centerX, centerY);

        // Thay đổi màu sắc để hiển thị trạng thái
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = canPlace ? Color.white : Color.red;
        }
    }

    void HighlightOccupiedCells(bool highlight)
    {
        Vector3 localPos = transform.position - gridManager.transform.position;
        int centerX = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int centerY = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        List<PlaneCell> shape = planeShape.GetRotatedShape();

        foreach (PlaneCell cell in shape)
        {
            int gridX = centerX + cell.x;
            int gridY = centerY + cell.y;

            if (gridX >= 0 && gridX < 15 && gridY >= 0 && gridY < 15)
            {
                gridManager.HighlightCell(gridX, gridY, highlight);
            }
        }
    }
}
