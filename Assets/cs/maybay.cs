using System.Collections.Generic;
using UnityEngine;

public class maybay : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private PlaneShape planeShape;

    [Header("References")]
    public GridManager gridManager;

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
        gridManager = FindObjectOfType<GridManager>();
    }

    void Update()
    {
        // Cho phép xoay khi đang kéo
        if (Input.GetKeyDown(KeyCode.R) && isDragging)
        {
            planeShape.Rotate();
        }
    }

    void OnMouseDown()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, 0);
        isDragging = true;

        // Highlight các ô đang chiếm
        HighlightOccupiedCells(true);
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = mouseWorld + offset;
            newPos.z = -3; // Đảm bảo hiển thị đúng mặt
            transform.position = newPos;

            CheckPlacementValidity();
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        HighlightOccupiedCells(false);

        Vector3 localPos = transform.position - gridManager.transform.position;
        int centerX = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int centerY = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        if (CanPlaceAt(centerX, centerY))
        {
            PlaceAt(centerX, centerY);
        }
        else
        {
            Debug.Log("Cannot place plane at this position!");
        }
    }

    bool CanPlaceAt(int centerX, int centerY)
    {
        List<PlaneCell> shape = planeShape.GetRotatedShape();

        foreach (PlaneCell cell in shape)
        {
            int gridX = centerX + cell.x;
            int gridY = centerY + cell.y;

            if (gridX < 0 || gridX >= 15 || gridY < 0 || gridY >= 15)
                return false;

            if (gridManager.IsCellOccupied(gridX, gridY, this.gameObject))
                return false;
        }

        return true;
    }

    void PlaceAt(int centerX, int centerY)
    {
        GameObject centerCell = gridManager.GetCell(centerX, centerY);
        if (centerCell != null)
        {
            Vector3 pos = centerCell.transform.position;
            pos.z = 0;
            transform.position = pos;

            gridManager.SetPlaneOccupation(centerX, centerY, planeShape.GetRotatedShape(), this.gameObject);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Aircraft"; // thay đổi nếu cần
                sr.sortingOrder = 10;
            }
        }
    }

    void CheckPlacementValidity()
    {
        Vector3 localPos = transform.position - gridManager.transform.position;
        int centerX = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int centerY = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        bool canPlace = CanPlaceAt(centerX, centerY);

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
