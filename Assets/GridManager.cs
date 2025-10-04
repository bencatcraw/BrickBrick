using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Grid Settings")]
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab; // Should be a UI prefab with an Image + GridCell script
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform gridRectTransform;
    public Canvas canvas;

    private GridCell[,] grid;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        grid = new GridCell[width, height];
        GenerateGrid();
        GameManager.instance.SpawnBlocks();
    }

    void GenerateGrid()
    {
        for (int y = 0; y < height; y++) // Y first to match grid rows
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellGO = Instantiate(cellPrefab, gridLayoutGroup.transform);
                GridCell cell = cellGO.GetComponent<GridCell>();
                cell.gridPosition = new Vector2Int(x, y);
                grid[x, y] = cell;
            }
        }
    }

    public Vector2Int GetCellIndexAtPosition(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRectTransform,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        Vector2 cellSize = gridLayoutGroup.cellSize;
        Vector2 spacing = gridLayoutGroup.spacing;

        float totalCellWidth = cellSize.x + spacing.x;
        float totalCellHeight = cellSize.y + spacing.y;

        float originX = -gridRectTransform.rect.width * gridRectTransform.pivot.x;
        float originY = gridRectTransform.rect.height * (1f - gridRectTransform.pivot.y);

        float x = (localPoint.x - originX) / totalCellWidth;
        float y = (originY - localPoint.y) / totalCellHeight;

        return new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    public GridCell GetCellAtIndex(Vector2Int index)
    {
        if (index.x >= 0 && index.x < width && index.y >= 0 && index.y < height)
        {
            return grid[index.x, index.y];
        }
        return null;
    }

    public bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (!grid[x, y].isOccupied)
                return false;
        }
        return true;
    }

    public bool IsColumnFull(int x)
    {
        for (int y = 0; y < height; y++)
        {
            if (!grid[x, y].isOccupied)
                return false;
        }
        return true;
    }

    public void ClearRow(int y)
    {
        for (int x = 0; x < width; x++)
        {
            grid[x, y].ToggleBlock(false, Color.white);
            GameManager.instance.addScore(1);
        }
    }

    public void ClearColumn(int x)
    {
        for (int y = 0; y < height; y++)
        {
            grid[x, y].ToggleBlock(false, Color.white);
            GameManager.instance.addScore(1);
        }
    }

    public bool CanPieceFit(Piece piece)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool valid = true;

                foreach (var unit in piece.blockUnits)
                {
                    Vector2Int rotatedOffset = piece.RotateOffset(unit.offset, piece.rotation);
                    int targetX = x + rotatedOffset.x;
                    int targetY = y - rotatedOffset.y; // Flip Y if needed

                    GridCell cell = GetCellAtIndex(new Vector2Int(targetX, targetY));
                    if (cell == null || cell.isOccupied)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    return true; // Found a valid spot!
            }
        }

        return false; // No valid spot found
    }

    public bool WouldRowBeFull(int row, List<GridCell> hoveredCells)
    {
        for (int x = 0; x < width; x++)
        {
            GridCell cell = GetCellAtIndex(new Vector2Int(x, row));
            if (cell == null) continue;

            bool willBeOccupied = cell.isOccupied || hoveredCells.Contains(cell);
            if (!willBeOccupied)
                return false; // There’s still a gap
        }
        return true;
    }

    public bool WouldColumnBeFull(int col, List<GridCell> hoveredCells)
    {
        for (int y = 0; y < height; y++)
        {
            GridCell cell = GetCellAtIndex(new Vector2Int(col, y));
            if (cell == null) continue;

            bool willBeOccupied = cell.isOccupied || hoveredCells.Contains(cell);
            if (!willBeOccupied)
                return false;
        }
        return true;
    }

    public List<GridCell> GetRowCells(int row)
    {
        List<GridCell> rowCells = new List<GridCell>();
        for (int x = 0; x < width; x++)
        {
            GridCell cell = GetCellAtIndex(new Vector2Int(x, row));
            if (cell != null)
                rowCells.Add(cell);
        }
        return rowCells;
    }

    public List<GridCell> GetColumnCells(int col)
    {
        List<GridCell> colCells = new List<GridCell>();
        for (int y = 0; y < height; y++)
        {
            GridCell cell = GetCellAtIndex(new Vector2Int(col, y));
            if (cell != null)
                colCells.Add(cell);
        }
        return colCells;
    }
}