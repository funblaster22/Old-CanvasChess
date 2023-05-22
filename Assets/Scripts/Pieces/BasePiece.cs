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
    /// <summary>Highlighted cells that the player can move to in 2 turns</summary>
    public List<Cell> previewHighlightedCells = new List<Cell>();
    /// <summary>Highighted cells that the player can move to</summary>
    public List<Cell> highlightedCells = null;
    abstract public int Value { get; }
    public bool IsAlive => gameObject.activeInHierarchy;

    private GameObject ghostPiece;

    // TODO: document why there are 4 different Cells
    /// <summary>Cell that piece belongs in at the start of a new game</summary>
    protected Cell mOriginalCell = null;
    protected Cell mCurrentCell = null;
    protected Cell cellBeforeDrag = null;
    /// <summary>Cell that player is dragging over</summary>
    protected Cell mTargetCell = null;

    protected RectTransform mRectTransform = null;
    protected PieceManager mPieceManager;

    protected BasePiece temporarlyCaptured = null;

    protected Vector3Int mMovement = Vector3Int.one;

    // TODO: merge these definitions with their protected fields { get; protected set; }
    public Cell CurrentCell => mCurrentCell;

    public Cell OriginalCell => mOriginalCell;

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

    public void Jail() {
        Transform jail = isBlack ? mPieceManager.blackJail : mPieceManager.whiteJail;
        GameObject jailedPiece = Instantiate(gameObject, jail);
        jailedPiece.SetActive(true);
        jailedPiece.tag = "Untagged";
        jailedPiece.transform.localEulerAngles = Vector3.zero;
        jailedPiece.GetComponent<BasePiece>().enabled = false;
    }

    public bool HasMove()
    {
        CheckPathing();

        // If no moves
        if (previewHighlightedCells.Count == 0)
            return false;

        // If moves available
        return true;
    }

    public void ComputerMove()
    {
        // Get random cell
        int i = Random.Range(0, previewHighlightedCells.Count);
        mTargetCell = previewHighlightedCells[i];

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
                previewHighlightedCells.Add(firstHit);
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
            if (cellState == CellState.Friendly) {
                var cell = mCurrentCell.mBoard.mAllCells[currentX, currentY];
                if (cell.mCurrentPiece is not King)
                    mPieceManager.allDefendedCells.Add(cell);
            }

            // If the cell is not free, break
            if (cellState != CellState.Free)
                break;

            // Add to list
            previewHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
        }
    }

    public virtual void CheckPathing()
    {
        previewHighlightedCells.Clear();

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
                    previewHighlightedCells.Add(mTargetCell);  // Use mHeilighted or actualHighlighted?
                    Cell.SetBackgroundAll(highlightedCells, isBlack ? Globals.red : Globals.blue, 200);
                    cellBeforeDrag.SetBackground(isBlack ? Globals.red : Globals.blue, 200);
                }
            }

            return true;
        }
        return false;
    }

    public void StartTurn() {
        if (ghostPiece != null)
            Destroy(ghostPiece);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        // Remember which cell the piece started in
        cellBeforeDrag = mCurrentCell;
        highlightedCells = new List<Cell>(previewHighlightedCells);
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
            Cell.SetBackgroundAll(highlightedCells, isBlack ? Globals.red : Globals.blue, 200);
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
        foreach (Cell cell in highlightedCells)
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
        Cell.ClearBackgroundAll(highlightedCells);
        cellBeforeDrag.background.enabled = false;
        highlightedCells = previewHighlightedCells;

        // Rescale HUD
        ResetMobileHUD();

        // Return to original position
        if (!mTargetCell || mTargetCell == cellBeforeDrag || mPieceManager.KingInDanger)
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
        if (temporarlyCaptured)
            temporarlyCaptured.Jail();

        // End turn
        SaveSystem.SaveGame(!isBlack, mPieceManager.isTwoPlayer, mPieceManager.AllPieces);
        mPieceManager.SwitchSides(mColor);
    }
    #endregion
}
