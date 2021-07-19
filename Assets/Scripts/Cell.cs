using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public Image mOutlineImage;

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

    public static void setOutlineAll(IEnumerable<Cell> cells, string color)
    {
        foreach (Cell cell in cells)
            cell.setOutline(color);
    }

    public static void clearOutlineAll(IEnumerable<Cell> cells)
    {
        setOutlineAll(cells, "clear");
    }

    public void setOutline(string color)
    {  // TODO
        if (color.Equals("clear"))
            this.mOutlineImage.enabled = false;
        else
            this.mOutlineImage.enabled = true;
    }

    public void setOverlay()
    {

    }
}
