using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public enum HitType
{
    Miss,
    BodyHit,
    HeadHit,
    Shooted
}

public class GridManager : NetworkBehaviour
{
    [SerializeField] private int gridSizeX = 15;
    [SerializeField] private int gridSizeY = 15;
    [SerializeField] public float cellSpacing = 1.1366348f;
    [SerializeField] private Sprite cellSprite;
    [SerializeField] private Material cellMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] public Vector2 gridOrigin = Vector2.zero;

    [Header("Hit Colors")]
    [SerializeField] private Material missHitMaterial;
    [SerializeField] private Material bodyHitMaterial;
    [SerializeField] private Material headHitMaterial;

    [Header("UI")]
    public TextMeshProUGUI notification;

    [Header("Effect")]
    public GameObject missilePrefab;

    public bool isGameStarted = false;

    private GameObject[,] gridCells;
    private GameObject[,] occupiedBy;
    private GameObject[,] occupiedHead;
    private Material[,] originalMaterials;
    private bool[,] hasBeenShot;

    void Start()
    {
        gridCells = new GameObject[gridSizeX, gridSizeY];
        occupiedBy = new GameObject[gridSizeX, gridSizeY];
        occupiedHead = new GameObject[gridSizeX, gridSizeY];
        originalMaterials = new Material[gridSizeX, gridSizeY];
        hasBeenShot = new bool[gridSizeX, gridSizeY];
        if (!isOwned){ 
            //gameObject.SetActive(false);//nếu tôi không dùng cái này thì lúc vào game grid vẫn bị hiện
            //SetGridVisible(false);
        }
        if (notification == null)
        {
            var foundText = GameObject.Find("TurnStatusText");
            if (foundText != null)
            {
                notification = foundText.GetComponent<TextMeshProUGUI>();
                Debug.Log("✅ Gán Notification thành công");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy TurnStatusText");
            }
        }
        GenerateGrid();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isOwned)
        {
            Debug.Log("[GridManager] Not owned => hiding grid");
            SetGridVisible(false);
        }
    }


    void GenerateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.parent = transform;
                cell.transform.localPosition = new Vector3(
                    gridOrigin.x + x * cellSpacing,
                    gridOrigin.y + y * cellSpacing,
                    0
                );

                SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
                renderer.sprite = cellSprite;
                renderer.material = cellMaterial;
                renderer.sortingOrder = 0;

                BoxCollider2D collider = cell.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one * cellSpacing;

                GridCell cellScript = cell.AddComponent<GridCell>();
                cellScript.Initialize(x, y, this);

                gridCells[x, y] = cell;
                originalMaterials[x, y] = cellMaterial;
            }
        }
    }

    public GameObject GetCell(int x, int y)
    {
        if (!InBounds(x, y))
        {
            Debug.LogError($"❌ GetCell: ({x},{y}) out of bounds");
            return null;
        }

        if (gridCells[x, y] == null)
        {
            Debug.LogError($"❌ GetCell: gridCells[{x},{y}] is null");
        }

        return gridCells[x, y];
    }


    public bool IsCellOccupied(int x, int y, GameObject exclude = null)
    {
        if (!InBounds(x, y)) return true;

        GameObject occupier = occupiedBy[x, y];
        if (occupier == null || occupier == exclude || !occupier.activeInHierarchy)
            return false;

        return true;
    }

    public void SetPlaneOccupation(int centerX, int centerY, List<PlaneCell> shape, GameObject plane)
    {
        ClearPlaneOccupation(plane);

        foreach (PlaneCell cell in shape)
        {
            int x = centerX + cell.x;
            int y = centerY + cell.y;
            if (InBounds(x, y))
            {
                occupiedBy[x, y] = plane;
                if (cell.isHead)
                    occupiedHead[x, y] = plane;
            }
        }
    }

    public void ClearPlaneOccupation(GameObject plane)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (occupiedBy[x, y] == plane)
                    occupiedBy[x, y] = null;
                if (occupiedHead[x, y] == plane)
                    occupiedHead[x, y] = null;
            }
        }
    }

    public void HighlightCell(int x, int y, bool highlight)
    {
        if (!InBounds(x, y)) return;

        SpriteRenderer renderer = gridCells[x, y].GetComponent<SpriteRenderer>();
        if (highlight)
            renderer.material = highlightMaterial;
        else
            renderer.material = originalMaterials[x, y];
    }

    public HitType ShootAt(int x, int y)
        
    {
        Debug.Log("shootat is called at x="+x+",Y="+y);
        if (!isGameStarted || !InBounds(x, y))
            return HitType.Miss;

        if (hasBeenShot[x, y])
        {
            notification.text = $"🔁 Cell ({x},{y}) đã bị bắn!";
            return HitType.Shooted;
        }

        hasBeenShot[x, y] = true;

        if (missilePrefab)
        {
            Vector3 target = GetCell(x, y).transform.position;
            if (Camera.main == null)
            {
                Debug.LogError("❌ Camera.main is NULL! Cannot shoot missile.");
                return HitType.Miss;
            }

            Vector3 start = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 0));
            start.z = 0;

            GameObject missile = Instantiate(missilePrefab, start, Quaternion.identity);
            missile.GetComponent<missile_animation>().targetPosition = target;//đây là dòng 185, tại sao lại null khi tôi đã gán missle như hình rồi mà, hay là null cái khác
        }

        HitType result = CheckHit(x, y);
        ApplyHitVisual(x, y, result);
        return result;
    }

    HitType CheckHit(int x, int y)
    {
        if (occupiedHead[x, y] != null)
        {
            notification.text = $"🔥 HEAD HIT at ({x},{y})";
            return HitType.HeadHit;
        }

        if (occupiedBy[x, y] != null)
        {
            notification.text = $"💥 BODY HIT at ({x},{y})";
            return HitType.BodyHit;
        }

        notification.text = $"💨 MISS at ({x},{y})";
        return HitType.Miss;
    }

    public void ApplyHitVisual(int x, int y, HitType type)
    {
        SpriteRenderer renderer = gridCells[x, y].GetComponent<SpriteRenderer>();

        switch (type)
        {
            case HitType.Miss:
                renderer.material = missHitMaterial;
                break;
            case HitType.BodyHit:
                renderer.material = bodyHitMaterial;
                break;
            case HitType.HeadHit:
                renderer.material = headHitMaterial;
                break;
        }
    }

    public bool HasBeenShot(int x, int y)
    {
        return InBounds(x, y) && hasBeenShot[x, y];
    }

    public void StartGame()
    {
        isGameStarted = true;
        Debug.Log($"[GridManager] Game started on: {gameObject.name} | isServer={NetworkServer.active} | isClient={NetworkClient.active}");
    }

    public void ResetGrid()
    {
        Debug.Log("reset grid is called");
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                hasBeenShot[x, y] = false;

                SpriteRenderer renderer = gridCells[x, y].GetComponent<SpriteRenderer>();
                renderer.material = originalMaterials[x, y];
            }
        }

        Debug.Log("🔄 Grid reset");
    }
    public void SetGridVisible(bool visible)
    {
        if (gridCells == null)
        {
            Debug.Log("SetGridVisible, gridcells is null!");
            return;
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (gridCells[x, y] == null) continue;
                Debug.Log("SetGridVisible, process gridcell [" + gridCells[x, y].name + "]");
                foreach (var sr in gridCells[x, y].GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.enabled = visible;
                }

                var collider = gridCells[x, y].GetComponent<Collider2D>();
                if (collider != null)
                    collider.enabled = visible;

                // Hoặc: gridCells[x, y].SetActive(visible); nếu bạn muốn ẩn hẳn
            }
        }

        if (notification != null)
            notification.enabled = visible;

        Debug.Log($"SetGridVisible completed, visible = {visible}");
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;
    }

    public bool AreAllPlanesDestroyed()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Nếu có 1 ô thuộc máy bay (dù là body hay head) mà chưa bị bắn → chưa thua
                if (occupiedBy[x, y] != null && !hasBeenShot[x, y])
                {
                    return false;
                }
            }
        }
        return true; // tất cả ô máy bay đã bị bắn
    }


}
