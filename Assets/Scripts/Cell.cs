using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OutlineState
{
    None,
    Danger,
    Warning,
    Legal,
    Capture,
    Preview
}

public enum OverlayType
{
    PartialShield,
    Shield,
    Sword,
    Pin
}

public class Cell : MonoBehaviour
{
    public Image mOutlineImage;
    public Transform overlayContainer;
    public Image background;

    [HideInInspector]
    public Vector2Int mBoardPosition = Vector2Int.zero;
    [HideInInspector]
    public Board mBoard = null;
    [HideInInspector]
    public RectTransform mRectTransform = null;

    [HideInInspector]
    public BasePiece mCurrentPiece = null;

    public void Setup(Vector2Int newBoardPosition, Board newBoard)
    {
        mBoardPosition = newBoardPosition;
        mBoard = newBoard;

        mRectTransform = GetComponent<RectTransform>();
    }

    public void RemovePiece()
    {
        if (mCurrentPiece != null)
        {
            mCurrentPiece.Kill();
        }
    }

    public static void SetOutlineAll(IEnumerable<Cell> cells, OutlineState color)
    {
        foreach (Cell cell in cells)
            cell.SetOutline(color);
    }

    public static void ClearOutlineAll(IEnumerable<Cell> cells)
    {
        SetOutlineAll(cells, OutlineState.None);
    }

    public void SetOutline(OutlineState color)
    {
        mOutlineImage.enabled = true;
        switch (color)
        {
            case OutlineState.None:
                mOutlineImage.enabled = false;
                break;
            case OutlineState.Legal:
                mOutlineImage.color = Color.black;
                break;
            case OutlineState.Danger:
                mOutlineImage.color = Color.red;
                break;
            case OutlineState.Preview:
                mOutlineImage.color = Color.grey;
                break;
            case OutlineState.Capture:
                mOutlineImage.color = Color.green;
                break;
            case OutlineState.Warning:
                mOutlineImage.color = Color.yellow;  // TODO: should be orange
                break;
        }
    }

    public void SetOverlay(OverlayType sprite)
    {
        GameObject obj = new GameObject("Item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        //obj.transform.parent = overlayContainer.transform;
        obj.transform.SetParent(overlayContainer, false);
        Image img = obj.GetComponent<Image>();
        img.preserveAspect = true;
        switch (sprite)
        {
            case OverlayType.Shield:
                img.color = Globals.brown;
                img.sprite = Resources.Load<Sprite>("T_Shield");
                break;
            case OverlayType.PartialShield:
                img.color = Globals.lightbrown;
                img.sprite = Resources.Load<Sprite>("T_Shield");
                break;
            case OverlayType.Sword:
                img.color = Globals.brown;
                img.sprite = Resources.Load<Sprite>("T_Sword");
                break;
            case OverlayType.Pin:
                img.color = Globals.purple;
                img.sprite = Resources.Load<Sprite>("T_Pin");
                break;
        }
        
    }

    public static void SetOverlayAll(IEnumerable<Cell> cells, OverlayType sprite)
    {
        foreach (Cell cell in cells)
            cell.SetOverlay(sprite);
    }

    public void ClearOverlay()
    {
        foreach (Transform child in overlayContainer)
            GameObject.Destroy(child.gameObject);
    }

    public static void ClearOverlayAll(IEnumerable<Cell> cells)
    {
        foreach (Cell cell in cells)
            cell.ClearOverlay();
    }

    public static void SetBackgroundAll(IEnumerable<Cell> cells, Color color, byte transparency)
    {
        foreach (Cell cell in cells)
            cell.SetBackground(color, transparency);
    }

    public void SetBackground(Color color, byte transparency)
    {
        Color newColor = new Color(color.r, color.g, color.b, (float)transparency / 255);
        background.enabled = true;
        background.color = newColor;
    }

    public static void ClearBackgroundAll(IEnumerable<Cell> cells)
    {
        foreach (Cell cell in cells)
            cell.background.enabled = false;
    }
}
