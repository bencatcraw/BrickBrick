using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Piece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private Transform parentAfterDrag;

    [HideInInspector] public List<BlockUnit> blockUnits = new List<BlockUnit>();

    private HashSet<GridCell> lastHoveredCells = new HashSet<GridCell>();
    private HashSet<GridCell> lastGlowCells = new HashSet<GridCell>();

    public int rotation = 0; // Rotation in degrees (0, 90, 180, 270)
    public Vector3 offset = Vector3.zero;
    public Vector3 grabOffset;
    public Color[] colors;
    private Color color;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            BlockUnit unit = child.GetComponent<BlockUnit>();
            if (unit != null)
                blockUnits.Add(unit);
        }
    }

    void Start()
    {
        transform.localScale = new Vector3(0.5f, 0.5f);
        Vector3 rOffset = Quaternion.Euler(0, 0, -rotation) * offset / 2;
        GetComponent<RectTransform>().localPosition += rOffset;
        startPosition = transform.position;
    }

    public Vector2Int RotateOffset(Vector2Int offset, int rotationDegrees)
    {
        switch (rotationDegrees % 360)
        {
            case 90:
                return new Vector2Int(offset.y, -offset.x);
            case 180:
                return new Vector2Int(-offset.x, -offset.y);
            case 270:
                return new Vector2Int(-offset.y, offset.x);
            default:
                return offset;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1, 1);
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        if (GameManager.instance.graboffset)
        {
            transform.position = Input.mousePosition + grabOffset;
        }
        else
        {
            transform.position = Input.mousePosition;
        }
        Vector3 rOffset = Quaternion.Euler(0, 0, -rotation) * offset;
        GetComponent<RectTransform>().localPosition += rOffset;

        Vector2Int originIndex = GridManager.instance.GetCellIndexAtPosition(transform.position);


        // Clear previous highlights
        foreach (var cell in lastHoveredCells)
            cell?.ResetHighlight();
        lastHoveredCells.Clear();

        List<GridCell> hoveredCells = new List<GridCell>();
        HashSet<int> rows = new HashSet<int>();
        HashSet<int> cols = new HashSet<int>();

        bool valid = true;
        foreach (var unit in blockUnits)
        {
            Vector2Int rotatedOffset = RotateOffset(unit.offset, rotation);
            Vector2Int cellIndex = new Vector2Int(
                originIndex.x + rotatedOffset.x,
                originIndex.y - rotatedOffset.y
            );

            GridCell cell = GridManager.instance.GetCellAtIndex(cellIndex);
            if (cell == null || cell.isOccupied)
            {
                valid = false;
                break;
            }

            hoveredCells.Add(cell);
            rows.Add(cellIndex.y);
            cols.Add(cellIndex.x);
        }

        HashSet<GridCell> newGlowCells = new HashSet<GridCell>();

        if (valid)
        {
            foreach (var cell in hoveredCells)
            {
                cell.Highlight(color);
                lastHoveredCells.Add(cell);
            }

            foreach (int row in rows)
            {
                if (GridManager.instance.WouldRowBeFull(row, hoveredCells))
                {
                    foreach (var cell in GridManager.instance.GetRowCells(row))
                    {
                        if (!hoveredCells.Contains(cell)) // skip the placing piece cells
                            newGlowCells.Add(cell);
                    }
                }
            }

            foreach (int col in cols)
            {
                if (GridManager.instance.WouldColumnBeFull(col, hoveredCells))
                {
                    foreach (var cell in GridManager.instance.GetColumnCells(col))
                    {
                        if (!hoveredCells.Contains(cell)) // skip the placing piece cells
                            newGlowCells.Add(cell);
                    }
                }
            }
        }

        // If ANY difference between newGlowCells and lastGlowCells, reset ALL glows and reapply
        bool glowChanged = newGlowCells.Count != lastGlowCells.Count || !newGlowCells.SetEquals(lastGlowCells);
        if (glowChanged)
        {
            // Reset all previously glowing cells
            foreach (var cell in lastGlowCells)
                cell.GlowToggle(false, color);

            // Apply glow to the new set
            foreach (var cell in newGlowCells)
                cell.GlowToggle(true, color);

            // Update the lastGlowCells set
            lastGlowCells = newGlowCells;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localScale = new Vector3(0.5f, 0.5f);

        Vector2Int originIndex = GridManager.instance.GetCellIndexAtPosition(transform.position);

        foreach (var cell in lastHoveredCells)
            cell?.ResetHighlight();
        lastHoveredCells.Clear();

        foreach (var cell in lastGlowCells)
            cell?.GlowToggle(false, color);
        lastGlowCells.Clear();

        bool valid = true;
        List<GridCell> targetCells = new List<GridCell>();
        HashSet<int> rows = new HashSet<int>();
        HashSet<int> cols = new HashSet<int>();

        foreach (var unit in blockUnits)
        {
            Vector2Int rotatedOffset = RotateOffset(unit.offset, rotation);
            Vector2Int cellIndex = new Vector2Int(
                originIndex.x + rotatedOffset.x,
                originIndex.y - rotatedOffset.y
            );

            GridCell cell = GridManager.instance.GetCellAtIndex(cellIndex);

            if (cell == null || cell.isOccupied)
            {
                valid = false;
                break;
            }

            targetCells.Add(cell);
            rows.Add(cellIndex.y);
            cols.Add(cellIndex.x);
        }

        if (valid)
        {
            foreach (var cell in targetCells)
                cell.ToggleBlock(true, color);

            List<int> rowsToClear = new List<int>();
            List<int> colsToClear = new List<int>();

            foreach (int row in rows)
                if (GridManager.instance.IsRowFull(row))
                    rowsToClear.Add(row);

            foreach (int col in cols)
                if (GridManager.instance.IsColumnFull(col))
                    colsToClear.Add(col);

            foreach (int row in rowsToClear)
                GridManager.instance.ClearRow(row);

            foreach (int col in colsToClear)
                GridManager.instance.ClearColumn(col);

            GameManager.instance.CheckBlocks(this);
            Destroy(gameObject);
        }
        else
        {
            transform.position = startPosition;
            transform.SetParent(parentAfterDrag);
        }
    }

    public void SetRandomColor()
    {
        int i = Random.Range(0, colors.Length);
        color = colors[i];
        foreach (var unit in blockUnits)
            unit.setColor(color);
    }

    public void SetRandomRotation()
    {
        int[] rotations = { 0, 90, 180, 270 };
        rotation = rotations[Random.Range(0, rotations.Length)];
        ApplyVisualRotation();
    }

    private void ApplyVisualRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, -rotation);
    }
}
