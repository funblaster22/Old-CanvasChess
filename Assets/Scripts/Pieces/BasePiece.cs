using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public abstract class BasePiece : EventTrigger
{
    [HideInInspector]
    public Color mColor = Color.clear;
    private bool isBlack;
    public bool mIsFirstMove = true;
    public List<Cell> mHighlightedCells = new List<Cell>();  // Highlighted cells that the player can move to in 2 turns // TODO: rename to 'previewHighlightedCells'
    public List<Cell> actualHighlightedCells = null;  // Highighted cells that the player can move to
    abstract public int Value { get; }
    public bool IsAlive => gameObject.activeInHierarchy;

    private GameObject ghostPiece;

    protected Cell mOriginalCell = null;  // Cell that piece belongs in at the start of a new game
    protected Cell mCurrentCell = null;
    protected Cell cellBeforeDrag = null;

    protected RectTransform mRectTransform = null;
    protected PieceManager mPieceManager;

    protected Cell mTargetCell = null;  // Cell that player is dragging over
    protected BasePiece temporarlyCaptured = null;

    protected Vector3Int mMovement = Vector3Int.one;

    public Cell CurrentCell
     {
        get
        {
            return mCurrentCell;
        }
    }

    public Cell OriginalCell {
        get {
            return mOriginalCell;
        }
    }

    public virtual void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        mPieceManager = newPieceManager;

        mColor = newTeamColor;
        isBlack = mColor == Color.black;
        GetComponent<Image>().color = newSpriteColor;
        mRectTransform = GetComponent<RectTransform>();
    }

    public virtual void Place(Cell newCell, bool overrideOriginal=true)
    {
        // Cell stuff
        mCurrentCell = newCell;
        if (overrideOriginal)
            mOriginalCell = newCell;
        mCurrentCell.mCurrentPiece = this;

        // Object stuff
        transform.position = newCell.transform.position;
        gameObject.SetActive(true);
    }

    public void Reset()
    {
        Kill();

        mIsFirstMove = true;

        Place(mOriginalCell);
    }

    public virtual void Kill()
    {
        // Clear current cell
        mCurrentCell.mCurrentPiece = null;

        // Remove piece
        gameObject.SetActive(false);
    }

    public bool HasMove()
    {
        CheckPathing();

        // If no moves
        if (mHighlightedCells.Count == 0)
            return false;

        // If moves available
        return true;
    }

    public void ComputerMove()
    {
        // Get random cell
        int i = Random.Range(0, mHighlightedCells.Count);
        mTargetCell = mHighlightedCells[i];

        // Move to new cell
        Move();

        // End turn
        mPieceManager.SwitchSides(mColor);
    }

    #region Movement
    private void CreateCellPath(int xDirection, int yDirection, int movement)
    {
        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Check each cell
        for (int i = 1; i <= movement; i++)
        {
            currentX += xDirection;
            currentY += yDirection;

            // Get the state of the target cell
            CellState cellState = CellState.None;
            cellState = mCurrentCell.mBoard.ValidateCell(currentX, currentY, this);

            // If enemy, add to list, break
            if (cellState == CellState.Enemy)
            {
                Cell firstHit = mCurrentCell.mBoard.mAllCells[currentX, currentY];
                mHighlightedCells.Add(firstHit);
                (mColor == Color.white ? mPieceManager.blackAttackedCells : mPieceManager.whiteAttackedCells)
                    .Add(firstHit);
                // Check for pieces that are pinned behind this one
                while (i <= movement)
                {
                    currentX += xDirection;
                    currentY += yDirection;
                    cellState = mCurrentCell.mBoard.ValidateCell(currentX, currentY, this);
                    if (cellState == CellState.OutOfBounds) break;
                    if (cellState == CellState.Enemy)
                    {
                        Cell cell = mCurrentCell.mBoard.mAllCells[currentX, currentY];
                        if (cell.mCurrentPiece.Value > 1)  // Only mark as pinned if 2nd piece is not a pawn
                        {
                            // TODO: only mark as pinned if 2nd piece is undefended
                            mPieceManager.allPinnedCells.Add(firstHit);
                            mPieceManager.allPinnedCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
                        }
                        break;
                    }
                    i++;
                }
                break;
            }

            // If friendly, add to list, break
            if (cellState == CellState.Friendly)
                mPieceManager.allDefendedCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);

            // If the cell is not free, break
            if (cellState != CellState.Free)
                break;

            // Add to list
            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
        }
    }

    public virtual void CheckPathing()
    {
        mHighlightedCells.Clear();

        // Horizontal
        CreateCellPath(1, 0, mMovement.x);
        CreateCellPath(-1, 0, mMovement.x);

        // Vertical 
        CreateCellPath(0, 1, mMovement.y);
        CreateCellPath(0, -1, mMovement.y);

        // Upper diagonal
        CreateCellPath(1, 1, mMovement.z);
        CreateCellPath(-1, 1, mMovement.z);

        // Lower diagonal
        CreateCellPath(-1, -1, mMovement.z);
        CreateCellPath(1, -1, mMovement.z);
    }

    /*public static List<Cell> CheckPathing(Color color, int currentX, int currentY)
    {

    }TODO*/

    protected virtual void Move(bool preview=false)  // TODO: don't actually move piece if not preview, b/c it's been done already
    {
        // If there is an enemy piece, remove it
        if (mTargetCell.mCurrentPiece != null && mTargetCell.mCurrentPiece.mColor != mColor)
        {
            if (preview)
                temporarlyCaptured = mTargetCell.mCurrentPiece;
            mTargetCell.RemovePiece();
        }

        // Clear current
        mCurrentCell.mCurrentPiece = null;

        // Switch cells
        mCurrentCell = mTargetCell;
        mCurrentCell.mCurrentPiece = this;
        
        if (!preview)
        {
            // First move switch
            if (mTargetCell != cellBeforeDrag)
                mIsFirstMove = false;

            // Move on board
            transform.position = mCurrentCell.transform.position;
            mTargetCell = null;
        }
    }

    protected virtual void UndoCapture()
    {
        if (temporarlyCaptured && mTargetCell != temporarlyCaptured.mCurrentCell)
        {
            temporarlyCaptured.gameObject.SetActive(true);
            temporarlyCaptured.mCurrentCell.mCurrentPiece = temporarlyCaptured;
            temporarlyCaptured = null;
        }
    }
    #endregion

    #region Events
    private void ResetMobileHUD() {
        if (Input.touchCount >= 1) {
            var cell = mTargetCell != null ? mTargetCell : cellBeforeDrag;
            var hud = cell.gameObject.transform.GetChild(1);
            hud.localScale = Vector3.one;
            // As goofy as this assignment looks, it is nessisary. localPosition places in center of cell
            hud.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
    }

    private bool CheckEntry(Cell cell)
    {
        var prevCell = mTargetCell;
        if (RectTransformUtility.RectangleContainsScreenPoint(cell.mRectTransform, Input.mousePosition))
            //TODO: account for begining cell RectTransformUtility.RectangleContainsScreenPoint(cellBeforeDrag.mRectTransform, Input.mousePosition)
            {
            // If the target cell changed, recalculate what board will look like if player were to place
            if (mTargetCell != cell) {
                // If the mouse is within a valid cell, get it, and break.
                mTargetCell = cell;
                // Set the state of the new cell
                Move(true);

                if (Input.touchCount >= 1 && mTargetCell != prevCell) {
                    var targetHud = mTargetCell.gameObject.transform.GetChild(1);
                    targetHud.localScale = Vector3.one * 1.5f;
                    targetHud.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.up * 100;
                    if (prevCell != null) {
                        var prevHud = prevCell.gameObject.transform.GetChild(1);
                        prevHud.localScale = Vector3.one;
                        prevHud.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    }
                }

                // Restore the state of the cell you're leaving
                UndoCapture();

                // Re-render overlays
                mPieceManager.HideAssist();  // Prevents actualHighlightedCells from being cleared when erasing previous outlines
                bool isBlack = mColor == Color.black;
                Settings mySettings = Settings.GetPlayer(isBlack);
                mPieceManager.ShowAssist();
                if (mySettings.showCurrentMove) {
                    mHighlightedCells.Add(mTargetCell);  // Use mHeilighted or actualHighlighted?
                    Cell.SetBackgroundAll(actualHighlightedCells, isBlack ? Globals.red : Globals.blue, 200);
                    cellBeforeDrag.SetBackground(isBlack ? Globals.red : Globals.blue, 200);
                }
            }

            return true;
        }
        return false;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        // Remember which cell the piece started in
        cellBeforeDrag = mCurrentCell;
        actualHighlightedCells = new List<Cell>(mHighlightedCells);
        temporarlyCaptured = null;

        // Ghost piece
        if (ghostPiece != null)
            Destroy(ghostPiece);
        ghostPiece = Instantiate(gameObject, transform.parent);
        ghostPiece.transform.localPosition = transform.localPosition;
        ghostPiece.GetComponent<BasePiece>().enabled = false;
        var img = ghostPiece.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);

        // Show valid cells
        if (Settings.GetPlayer(mColor == Color.black).showCurrentMove)
        {  // TODO: reduce redundancy with CheckEntry
            Cell.SetBackgroundAll(actualHighlightedCells, isBlack ? Globals.red : Globals.blue, 200);
            cellBeforeDrag.SetBackground(isBlack ? Globals.red : Globals.blue, 200);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        // Follow pointer
        transform.position += (Vector3)eventData.delta;

        // Check for overlapping available squares
        if (CheckEntry(cellBeforeDrag)) return;
        foreach (Cell cell in actualHighlightedCells)
        {
            if (CheckEntry(cell))
                return;
        }

        // If the mouse is not within any highlighted cell, we don't have a valid move.

        // Rescale HUD
        ResetMobileHUD();

        mTargetCell = null;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        // Hide
        Cell.ClearBackgroundAll(actualHighlightedCells);
        cellBeforeDrag.background.enabled = false;
        actualHighlightedCells = mHighlightedCells;

        // Rescale HUD
        ResetMobileHUD();

        // Return to original position
        if (!mTargetCell || mTargetCell == cellBeforeDrag)
        {
            transform.position = mCurrentCell.gameObject.transform.position;
            mTargetCell = cellBeforeDrag;
            Move();
            UndoCapture();
            mPieceManager.ShowAssist();
            if (ghostPiece != null)
                Destroy(ghostPiece);
            return;
        }

        // Move to new cell
        Move();

        // End turn
        SaveSystem.SaveGame(!isBlack, mPieceManager.AllPieces);
        mPieceManager.SwitchSides(mColor);
    }
    #endregion
}
