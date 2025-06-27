using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class maybay : NetworkBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private PlaneShape planeShape;

    [Header("References")]
    public GridManager gridManager;

    [SyncVar(hook = nameof(OnGridAssigned))]
    public NetworkIdentity gridIdentity;

    [Header("Placement Settings")]
    public LayerMask gridLayerMask = -1;

    void Awake()
    {
        planeShape = GetComponent<PlaneShape>();
        if (planeShape == null)
            planeShape = gameObject.AddComponent<PlaneShape>();
    }

    void Start()
    {
        // Ẩn máy bay nếu không sở hữu
        //if (!isOwned)
        //    gameObject.SetActive(false);
        //if (!isOwned)
        //{
        //    var sr = GetComponentInChildren<SpriteRenderer>();
        //    if (sr != null) sr.enabled = false;
        //}


    }

    public override void OnStartAuthority()
    {
        gameObject.SetActive(true);
    }

    void OnGridAssigned(NetworkIdentity oldGrid, NetworkIdentity newGrid)
    {
        if (newGrid != null)
        {
            gridManager = newGrid.GetComponent<GridManager>();
        }
    }

    void Update()
    {
        if (gridManager != null && gridManager.isGameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.R) && isDragging)
        {
            planeShape.Rotate();
        }
    }

    void OnMouseDown()
    {
        if (!isOwned || gridManager == null || gridManager.isGameStarted) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, 0);
        isDragging = true;

        HighlightOccupiedCells(true);
    }

    void OnMouseDrag()
    {
        if (!isDragging || gridManager == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPos = mouseWorld + offset;
        newPos.z = 0;
        transform.position = newPos;

        CheckPlacementValidity();
    }

    void OnMouseUp()
    {
        if (gridManager == null) return;

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
            Debug.Log("❌ Cannot place plane at this position!");
        }
    }

    bool CanPlaceAt(int centerX, int centerY)
    {
        if (gridManager == null) return false;

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
            transform.position = centerCell.transform.position + new Vector3(0, 0, -0.1f);
            gridManager.SetPlaneOccupation(centerX, centerY, planeShape.GetRotatedShape(), this.gameObject);
        }
    }

    void CheckPlacementValidity()
    {
        if (gridManager == null) return;

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
        if (gridManager == null) return;

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

    [Command]
    public void CmdSetFinalPlanePosition(Vector3 pos)
    {
        if (gridManager == null) return;

        Vector3 localPos = pos - gridManager.transform.position;
        int centerX = Mathf.RoundToInt((localPos.x - gridManager.gridOrigin.x) / gridManager.cellSpacing);
        int centerY = Mathf.RoundToInt((localPos.y - gridManager.gridOrigin.y) / gridManager.cellSpacing);

        List<PlaneCell> shape = planeShape.GetRotatedShape();

        gridManager.SetPlaneOccupation(centerX, centerY, shape, this.gameObject);
        Debug.Log($"✅ [Server] CmdSetFinalPlanePosition for plane at ({centerX}, {centerY})");
    }

}
