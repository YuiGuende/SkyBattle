
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GridManager2 : MonoBehaviour
{
    [SerializeField] private int gridSizeX = 15;
    [SerializeField] private int gridSizeY = 15;
    [SerializeField] public float cellSpacing = 1.1366348f;
    [SerializeField] private Sprite cellSprite;
    [SerializeField] private Material cellMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] public Vector2 gridOrigin = Vector2.zero;

    [Header("Hit Colors")]
    [SerializeField] private Material missHitMaterial; // Màu xám cho miss
    [SerializeField] private Material bodyHitMaterial; // Màu cam cho body hit
    [SerializeField] private Material headHitMaterial; // Màu đỏ cho head hit4


    [Header("UI")]
    public TextMeshProUGUI notification;//thong bao cua game

    [Header("EFFECT")]
    public GameObject missilePrefab;

    public TurnManager.PlayerTurn gridOwner;

    public bool isGameStarted = false;//check game da bat dau hay chua

    private GameObject[,] gridCells;
    private GameObject[,] occupiedBy; // Theo dõi máy bay nào đang chiếm ô nào
    private GameObject[,] occupiedHead; // Theo dõi ô nào là head
    private Material[,] originalMaterials; // Lưu material gốc
    private bool[,] hasBeenShot; // Theo dõi ô nào đã bị bắn

    void Start()
    {
        gridCells = new GameObject[gridSizeX, gridSizeY];
        occupiedBy = new GameObject[gridSizeX, gridSizeY];
        occupiedHead = new GameObject[gridSizeX, gridSizeY];
        originalMaterials = new Material[gridSizeX, gridSizeY];
        hasBeenShot = new bool[gridSizeX, gridSizeY];

        GenerateGrid();
    }

    void GenerateGrid()
    {
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                try
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
                    renderer.sortingLayerName = "Default";
                    renderer.sortingOrder = 0;

                    // Thêm Collider để có thể click
                    BoxCollider2D collider = cell.AddComponent<BoxCollider2D>();
                    collider.size = Vector2.one * cellSpacing;

                    // Thêm GridCell component để handle click
                    GridCell2 gridCellComponent = cell.AddComponent<GridCell2>();
                    gridCellComponent.Initialize(x, y, this);

                    gridCells[x, y] = cell;
                    originalMaterials[x, y] = cellMaterial;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Lỗi khi tạo cell ({x}, {y}): {e.Message}");
                    return;
                }
            }
        }

        Debug.Log($"Grid đã được tạo thành công với {gridSizeX}x{gridSizeY} = {gridSizeX * gridSizeY} cells");
    }

    public GameObject GetCell(int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && gridCells != null)
        {
            return gridCells[x, y];
        }
        return null;
    }

    public bool IsCellOccupied(int x, int y, GameObject excludePlane = null)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || occupiedBy == null)
            return true;

        GameObject occupier = occupiedBy[x, y];
        return occupier != null && occupier != excludePlane;
    }

    public void SetPlaneOccupation(int centerX, int centerY, List<PlaneCell> shape, GameObject plane)
    {
        if (occupiedBy == null) return;

        ClearPlaneOccupation(plane);

        foreach (PlaneCell cell in shape)
        {
            int gridX = centerX + cell.x;
            int gridY = centerY + cell.y;

            if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
            {
                occupiedBy[gridX, gridY] = plane;

                // Nếu là head cell thì thêm vào occupiedHead
                if (cell.isHead)
                {
                    occupiedHead[gridX, gridY] = plane;
                }
            }
        }
    }

    public void ClearPlaneOccupation(GameObject plane)
    {
        if (occupiedBy == null) return;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (occupiedBy[x, y] == plane)
                {
                    occupiedBy[x, y] = null;
                }
                if (occupiedHead[x, y] == plane)
                {
                    occupiedHead[x, y] = null;
                }
            }
        }
    }

    public void HighlightCell(int x, int y, bool highlight)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || gridCells == null)
            return;

        GameObject cell = gridCells[x, y];
        if (cell == null) return;

        SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (highlight && highlightMaterial != null)
            {
                renderer.material = highlightMaterial;
            }
            else if (originalMaterials != null && originalMaterials[x, y] != null)
            {
                renderer.material = originalMaterials[x, y];
            }
        }
    }

    // Method chính để xử lý bắn
    public HitType ShootAt(int x, int y)
    {
        if (!isGameStarted)
        {
            return HitType.Miss;
        }
        Vector3 targetworldPos = GetCell(x, y).transform.position;

        // Instantiate shooting effect
        if (missilePrefab != null)
        {

            Vector3 startPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 0));
            startPos.z = 0f; // Make sure it's on the correct Z layer

            GameObject missile = Instantiate(missilePrefab, startPos, Quaternion.identity);

            missile.GetComponent<missile_animation>().targetPosition = targetworldPos;
            // Destroy(effect, 2f); // Clean up after 2 seconds
        }



        // Kiểm tra bounds
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return HitType.Miss;

        // Kiểm tra xem đã bắn chưa
        if (hasBeenShot[x, y])
        {
            notification.text = $"Cell ({x}, {y}) đã được bắn rồi!";
            Debug.Log($"Cell ({x}, {y}) đã được bắn rồi!");
            return HitType.Miss;
        }

        hasBeenShot[x, y] = true;

        HitType hitType = CheckHit(x, y);
        ApplyHitVisual(x, y, hitType);

        return hitType;
    }

    public HitType CheckHit(int x, int y)
    {
        // Kiểm tra head hit trước
        if (occupiedHead[x, y] != null)
        {
            GameObject hitPlane = occupiedHead[x, y];
            notification.text = $"HEAD HIT! Plane: {hitPlane.name} at ({x}, {y})";
            Debug.Log($"HEAD HIT! Plane: {hitPlane.name} at ({x}, {y})");
            return HitType.HeadHit;
        }

        // Kiểm tra body hit
        if (occupiedBy[x, y] != null)
        {
            GameObject hitPlane = occupiedBy[x, y];
            notification.text = $"BODY HIT! Plane: {hitPlane.name} at ({x}, {y})";
            Debug.Log($"BODY HIT! Plane: {hitPlane.name} at ({x}, {y})");
            return HitType.BodyHit;
        }

        // Miss
        notification.text = $"MISS at ({x}, {y})";
        Debug.Log($"MISS at ({x}, {y})");
        return HitType.Miss;
    }

    void ApplyHitVisual(int x, int y, HitType hitType)
    {
        GameObject cell = gridCells[x, y];
        if (cell == null) return;

        SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        switch (hitType)
        {
            case HitType.HeadHit:
                if (headHitMaterial != null)
                    renderer.material = headHitMaterial;
                break;
            case HitType.BodyHit:
                if (bodyHitMaterial != null)
                    renderer.material = bodyHitMaterial;
                break;
            case HitType.Miss:
                if (missHitMaterial != null)
                    renderer.material = missHitMaterial;
                break;
        }
    }

    // Method để kiểm tra xem ô đã bị bắn chưa
    public bool HasBeenShot(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return true;
        return hasBeenShot[x, y];
    }

    //game start
    public void StartGame()
    {
        isGameStarted = true;
        Debug.Log("Game started!");
    }



    // Method để reset game
    public void ResetGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                hasBeenShot[x, y] = false;

                GameObject cell = gridCells[x, y];
                if (cell != null)
                {
                    SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = originalMaterials[x, y];
                    }
                }
            }
        }

        Debug.Log("Grid reset!");
    }
}