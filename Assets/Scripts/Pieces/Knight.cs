using UnityEngine;
using UnityEngine.UI;

public class Knight : BasePiece
{
    public override int Value {
        get { return 3; }
    }

    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // Knight stuff
        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_Knight");
    }

    private void CreateCellPath(int flipper)
    {
        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Left
        MatchesState(currentX - 2, currentY + (1 * flipper));

        // Upper left
        MatchesState(currentX - 1, currentY + (2 * flipper));

        // Upper right
        MatchesState(currentX + 1, currentY + (2 * flipper));

        // Right
        MatchesState(currentX + 2, currentY + (1 * flipper));
    }

    // New
    public override void CheckPathing()
    {
        mHighlightedCells.Clear();

        // Draw top half
        CreateCellPath(1);

        // Draw bottom half
        CreateCellPath(-1);
    }

    // New
    private void MatchesState(int targetX, int targetY)
    {
        CellState cellState = CellState.None;
        cellState = mCurrentCell.mBoard.ValidateCell(targetX, targetY, this);
        if (cellState != CellState.OutOfBounds)
        {
            Cell cell = mCurrentCell.mBoard.mAllCells[targetX, targetY];

            if (cellState == CellState.Friendly)  // Is defended
                mPieceManager.allDefendedCells.Add(cell);
            else  // Is a legal move
            {
                if (cellState == CellState.Enemy)  // Is attacking
                    (mColor == Color.white ? mPieceManager.blackAttackedCells : mPieceManager.whiteAttackedCells).Add(cell);
                mHighlightedCells.Add(cell);
            }
        }
    }
}
