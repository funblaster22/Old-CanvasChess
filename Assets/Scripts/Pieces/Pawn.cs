using UnityEngine;
using UnityEngine.UI;

public class Pawn : BasePiece
{
    public override int Value {
        get { return 1; }
    }

    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // Pawn Stuff
        mMovement = mColor == Color.white ? new Vector3Int(0, 1, 1) : new Vector3Int(0, -1, -1);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_Pawn");
    }

    protected override void Move()
    {
        base.Move();

        CheckForPromotion();
    }

    private bool MatchesState(int targetX, int targetY, CellState targetState)
    {
        CellState cellState = CellState.None;
        cellState = mCurrentCell.mBoard.ValidateCell(targetX, targetY, this);
        if (cellState != CellState.OutOfBounds)
        {
            Cell cell = mCurrentCell.mBoard.mAllCells[targetX, targetY];

            if (targetState == CellState.Enemy) {  // When checking corners
                if (cellState == CellState.Friendly)  // Is defended
                    mPieceManager.allDefendedCells.Add(cell);
                else if (cellState == CellState.Enemy)  // Is attacking
                    (mColor == Color.white ? mPieceManager.blackAttackedCells : mPieceManager.whiteAttackedCells).Add(cell);
            }
            if (cellState == targetState) {
                mHighlightedCells.Add(cell);
                return true;
            }
        }

        return false;
    }

    private void CheckForPromotion()
    {
        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Check if pawn has reached the end of the board
        CellState cellState = mCurrentCell.mBoard.ValidateCell(currentX, currentY + mMovement.y, this);

        if (cellState == CellState.OutOfBounds)
        {
            Color spriteColor = GetComponent<Image>().color;
            mPieceManager.PromotePiece(this, mCurrentCell, mColor, spriteColor);
        }
    }

    public override void CheckPathing()
    {
        mHighlightedCells.Clear();

        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Top left
        MatchesState(currentX - mMovement.z, currentY + mMovement.z, CellState.Enemy);

        // Forward
        if (MatchesState(currentX, currentY + mMovement.y, CellState.Free))
        {
            // If the first forward cell is free, and first move, check for next
            if (mIsFirstMove)
            {
                MatchesState(currentX, currentY + (mMovement.y * 2), CellState.Free);
            }
        }

        // Top right
        MatchesState(currentX + mMovement.z, currentY + mMovement.z, CellState.Enemy);
    }
}
