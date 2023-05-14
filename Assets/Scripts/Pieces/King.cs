using System;
using UnityEngine;
using UnityEngine.UI;

public class King : BasePiece
{
    private Rook mLeftRook = null;
    private Rook mRightRook = null;

    public override int Value {
        get { return 10; }
    }

    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // King stuff
        mMovement = new Vector3Int(1, 1, 1);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_King");
    }

    public override void Kill()
    {
        base.Kill();

        mPieceManager.mIsKingAlive = false;
    }

    public override void CheckPathing()
    {
        // Normal pathing
        base.CheckPathing();

        // Right
        mRightRook = GetRook(3);

        // Left
        mLeftRook = GetRook(-4);
    }

    protected override void Move(bool preview = false)
    {
        if (!preview)
        {
            // Left rook
            if (CanCastle(mLeftRook))
                mLeftRook.Castle();

            // Right rook
            if (CanCastle(mRightRook))
                mRightRook.Castle();
        }

        // Base move (run this last b/c it sets mIsFirstMove to false, which will prevent castling)
        base.Move(preview);
    }

    private bool CanCastle(Rook rook)
    {
        // For rook
        if (rook == null)
            return false;

        // Do the cells match?
        if (rook.mCastleTriggerCell != mCurrentCell)
            return false;

        // Check if same team, and hasn't moved
        if (rook.mColor != mColor || !rook.mIsFirstMove)
            return false;

        return true;
    }

    private Rook GetRook(int count)
    {
        // Has the king moved?
        if (!mIsFirstMove)
            return null;

        // Numbers and stuff
        var checkCell = cellBeforeDrag != null ? cellBeforeDrag : mCurrentCell;
        int currentX = checkCell.mBoardPosition.x;
        int currentY = checkCell.mBoardPosition.y;

        // Ensure all cells in between are empty. Skip 0 b/c that's where king is
        for (int i = 1; i < Math.Abs(count); i++)
        {
            int offsetX = currentX + (i * Math.Sign(count));
            CellState cellState = mCurrentCell.mBoard.ValidateCell(offsetX, currentY, this);

            // Check if king b/c the preview king piece might be there
            if (cellState != CellState.Free && mCurrentCell.mBoard.mAllCells[offsetX, currentY].mCurrentPiece is not King)
                return null;
        }

        // Try and get rook
        Cell rookCell = mCurrentCell.mBoard.mAllCells[currentX + count, currentY];
        Rook rook = null;

        // Check for cast
        if (rookCell.mCurrentPiece is Rook)
            rook = (Rook)rookCell.mCurrentPiece;

        // Add target cell to highlighed cells
        if (rook != null)
            mHighlightedCells.Add(rook.mCastleTriggerCell);

        return rook;
    }
}