using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlaneCell
{
    //public int x;
    //public int y;
    //public bool isHead;

    //public PlaneCell(int x, int y)
    //{
    //    this.x = x;
    //    this.y = y;
    //}

    //public override bool Equals(object obj)
    //{
    //    if (obj is PlaneCell other)
    //    {
    //        return x == other.x && y == other.y;
    //    }
    //    return false;
    //}

    //public override int GetHashCode()
    //{
    //    return x.GetHashCode() ^ y.GetHashCode();
    //}
    public int x;
    public int y;
    public bool isHead = false; // Thêm attribute để đánh dấu đầu máy bay

    public PlaneCell(int x, int y, bool isHead = false)
    {
        this.x = x;
        this.y = y;
        this.isHead = isHead;
    }

    public override bool Equals(object obj)
    {
        if (obj is PlaneCell other)
        {
            return x == other.x && y == other.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }
}

public class PlaneShape : MonoBehaviour
{
    //[HideInInspector] public string cellPrefix = "Cell_";
    //[HideInInspector] public int currentRotation = 0;

    //[SerializeField, HideInInspector] private bool showDebugGizmos = true;
    //[SerializeField, HideInInspector] private Color debugColor = Color.red;

    //private List<PlaneCell> cachedShape = new List<PlaneCell>();
    //private bool shapeCached = false;

    //void Start()
    //{
    //    DetectShapeFromChildren();
    //    //SetCellsVisible(false); // Ẩn tất cả cells khi bắt đầu
    //}

    //[ContextMenu("Detect Shape From Children")]
    //public void DetectShapeFromChildren()
    //{
    //    cachedShape.Clear();

    //    // Tìm tất cả child objects có tên bắt đầu với cellPrefix
    //    foreach (Transform child in transform)
    //    {
    //        if (child.name.StartsWith(cellPrefix))
    //        {
    //            // Lấy vị trí local của child object
    //            Vector3 localPos = child.localPosition;

    //            // Chuyển đổi thành grid coordinates
    //            int x = Mathf.RoundToInt(localPos.x);
    //            int y = Mathf.RoundToInt(localPos.y);

    //            cachedShape.Add(new PlaneCell(x, y));
    //        }
    //    }

    //    shapeCached = true;
    //    //SetCellsVisible(false); // Ẩn tất cả cells sau khi detect
    //    Debug.Log($"Shape detected with {cachedShape.Count} cells");
    //}

    //public List<PlaneCell> GetRotatedShape()
    //{
    //    if (!shapeCached)
    //    {
    //        DetectShapeFromChildren();
    //    }

    //    List<PlaneCell> rotatedCells = new List<PlaneCell>();

    //    foreach (PlaneCell cell in cachedShape)
    //    {
    //        PlaneCell rotatedCell = RotateCell(cell, currentRotation);
    //        rotatedCells.Add(rotatedCell);
    //    }

    //    return rotatedCells;
    //}

    //PlaneCell RotateCell(PlaneCell cell, int rotation)
    //{
    //    int newX = cell.x;
    //    int newY = cell.y;

    //    switch (rotation)
    //    {
    //        case 90:
    //            newX = -cell.y;
    //            newY = cell.x;
    //            break;
    //        case 180:
    //            newX = -cell.x;
    //            newY = -cell.y;
    //            break;
    //        case 270:
    //            newX = cell.y;
    //            newY = -cell.x;
    //            break;
    //    }

    //    return new PlaneCell(newX, newY);
    //}

    //public void Rotate()
    //{
    //    currentRotation = (currentRotation + 90) % 360;
    //    transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    //}

    //void OnDrawGizmos()
    //{
    //    if (!showDebugGizmos) return;

    //    Gizmos.color = debugColor;
    //    List<PlaneCell> currentShape = GetRotatedShape();

    //    foreach (PlaneCell cell in currentShape)
    //    {
    //        Vector3 cellWorldPos = transform.position + new Vector3(cell.x * 1.1366348f, cell.y * 1.1366348f, 0);
    //        Gizmos.DrawWireCube(cellWorldPos, Vector3.one * 1.1366348f);
    //    }
    //}

    //// Ẩn/hiện tất cả cell objects
    //public void SetCellsVisible(bool visible)
    //{
    //    foreach (Transform child in transform)
    //    {
    //        if (child.name.StartsWith(cellPrefix))
    //        {
    //            child.gameObject.SetActive(visible);
    //        }
    //    }
    //}

    //// Method để kiểm tra xem một vị trí có thuộc shape không
    //public bool ContainsPosition(int x, int y)
    //{
    //    List<PlaneCell> shape = GetRotatedShape();
    //    foreach (PlaneCell cell in shape)
    //    {
    //        if (cell.x == x && cell.y == y)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    [HideInInspector] public string cellPrefix = "Cell_";
    [HideInInspector] public string headPrefix = "Head_"; // Prefix cho đầu máy bay
    [HideInInspector] public int currentRotation = 0;

    [SerializeField, HideInInspector] private bool showDebugGizmos = true;
    [SerializeField, HideInInspector] private Color debugColor = Color.red;
    [SerializeField, HideInInspector] private Color headColor = Color.yellow;

    private List<PlaneCell> cachedShape = new List<PlaneCell>();
    private bool shapeCached = false;

    void Start()
    {
        DetectShapeFromChildren();
        SetCellsVisible(false);
    }

    [ContextMenu("Detect Shape From Children")]
    public void DetectShapeFromChildren()
    {
        cachedShape.Clear();

        // Tìm tất cả child objects
        foreach (Transform child in transform)
        {
            bool isCell = child.name.StartsWith(cellPrefix);
            bool isHead = child.name.StartsWith(headPrefix);

            if (isCell || isHead)
            {
                Vector3 localPos = child.localPosition;
                int x = Mathf.RoundToInt(localPos.x);
                int y = Mathf.RoundToInt(localPos.y);

                cachedShape.Add(new PlaneCell(x, y, isHead));

                Debug.Log($"Detected {(isHead ? "HEAD" : "BODY")} at ({x}, {y}) from child: {child.name}");
            }
        }

        shapeCached = true;
        SetCellsVisible(false);
        Debug.Log($"Shape detected with {cachedShape.Count} cells");
    }

    public List<PlaneCell> GetRotatedShape()
    {
        if (!shapeCached)
        {
            DetectShapeFromChildren();
        }

        List<PlaneCell> rotatedCells = new List<PlaneCell>();

        foreach (PlaneCell cell in cachedShape)
        {
            PlaneCell rotatedCell = RotateCell(cell, currentRotation);
            rotatedCells.Add(rotatedCell);
        }

        return rotatedCells;
    }

    PlaneCell RotateCell(PlaneCell cell, int rotation)
    {
        int newX = cell.x;
        int newY = cell.y;

        switch (rotation)
        {
            case 90:
                newX = -cell.y;
                newY = cell.x;
                break;
            case 180:
                newX = -cell.x;
                newY = -cell.y;
                break;
            case 270:
                newX = cell.y;
                newY = -cell.x;
                break;
        }

        return new PlaneCell(newX, newY, cell.isHead); // Giữ nguyên isHead
    }

    public void Rotate()
    {
        currentRotation = (currentRotation + 90) % 360;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        List<PlaneCell> currentShape = GetRotatedShape();

        foreach (PlaneCell cell in currentShape)
        {
            // Màu khác nhau cho head và body
            Gizmos.color = cell.isHead ? headColor : debugColor;
            Vector3 cellWorldPos = transform.position + new Vector3(cell.x * 1.1366348f, cell.y * 1.1366348f, 0);
            Gizmos.DrawWireCube(cellWorldPos, Vector3.one * 1.1366348f);
        }
    }

    public void SetCellsVisible(bool visible)
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith(cellPrefix) || child.name.StartsWith(headPrefix))
            {
                child.gameObject.SetActive(visible);
            }
        }
    }

    public bool ContainsPosition(int x, int y)
    {
        List<PlaneCell> shape = GetRotatedShape();
        foreach (PlaneCell cell in shape)
        {
            if (cell.x == x && cell.y == y)
            {
                return true;
            }
        }
        return false;
    }

    // Method để kiểm tra xem vị trí có phải là head không
    public bool IsHeadPosition(int x, int y)
    {
        List<PlaneCell> shape = GetRotatedShape();
        foreach (PlaneCell cell in shape)
        {
            if (cell.x == x && cell.y == y && cell.isHead)
            {
                return true;
            }
        }
        return false;
    }
}