using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    [HideInInspector]
    public bool mIsKingAlive = true;

    public GameObject mPiecePrefab;

    private List<BasePiece> mWhitePieces = null;
    private List<BasePiece> mBlackPieces = null;
    private List<BasePiece> mPromotedPieces = new List<BasePiece>();
    private Cell[,] cells;

    [HideInInspector]
    public HashSet<Cell> allPossibleMoves = new HashSet<Cell>();  // Only applies to current player
    public HashSet<Cell> allDefendedCells = new HashSet<Cell>();
    public HashSet<Cell> whiteAttackedCells = new HashSet<Cell>();
    public HashSet<Cell> blackAttackedCells = new HashSet<Cell>();
    public HashSet<Cell> allPinnedCells = new HashSet<Cell>();

    private string[] mPieceOrder = new string[16]
    {
        "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "Q", "K", "B", "KN", "R"
    };

    private Dictionary<string, Type> mPieceLibrary = new Dictionary<string, Type>()
    {
        {"P",  typeof(Pawn)},
        {"R",  typeof(Rook)},
        {"KN", typeof(Knight)},
        {"B",  typeof(Bishop)},
        {"K",  typeof(King)},
        {"Q",  typeof(Queen)}
    };

    public void Setup(Board board)
    {
        cells = board.mAllCells;

        // Create white pieces
        mWhitePieces = CreatePieces(Color.white, new Color32(80, 124, 159, 255));

        // Create place pieces
        mBlackPieces = CreatePieces(Color.black, new Color32(210, 95, 64, 255));

        // Place pieces
        PlacePieces(1, 0, mWhitePieces, board);
        PlacePieces(6, 7, mBlackPieces, board);

        // White goes first
        SwitchSides(Color.black);
    }

    private List<BasePiece> CreatePieces(Color teamColor, Color32 spriteColor)
    {
        List<BasePiece> newPieces = new List<BasePiece>();

        for (int i = 0; i < mPieceOrder.Length; i++)
        {
            // Get the type
            string key = mPieceOrder[i];
            Type pieceType = mPieceLibrary[key];

            // Create
            BasePiece newPiece = CreatePiece(pieceType);
            newPieces.Add(newPiece);

            // Setup
            newPiece.Setup(teamColor, spriteColor, this);
        }

        return newPieces;
    }

    private BasePiece CreatePiece(Type pieceType)
    {
        // Create new object
        GameObject newPieceObject = Instantiate(mPiecePrefab);
        newPieceObject.transform.SetParent(transform);

        // Set scale and position
        newPieceObject.transform.localScale = new Vector3(1, 1, 1);
        newPieceObject.transform.localRotation = Quaternion.identity;

        // Store new piece
        BasePiece newPiece = (BasePiece)newPieceObject.AddComponent(pieceType);

        return newPiece;
    }

    private void PlacePieces(int pawnRow, int royaltyRow, List<BasePiece> pieces, Board board)
    {
        for (int i = 0; i < 8; i++)
        {
            // Place pawns    
            pieces[i].Place(board.mAllCells[i, pawnRow]);

            // Place royalty
            pieces[i + 8].Place(board.mAllCells[i, royaltyRow]);
        }
    }

    private void SetInteractive(List<BasePiece> allPieces, bool value)
    {
        foreach (BasePiece piece in allPieces)
            piece.enabled = value;
    }

    /*
    private void MoveRandomPiece()
    {
        BasePiece finalPiece = null;

        while (!finalPiece)
        {
            // Get piece
            int i = UnityEngine.Random.Range(0, mBlackPieces.Count);
            BasePiece newPiece = mBlackPieces[i];

            // Does this piece have any moves?
            if (!newPiece.HasMove())
                continue;

            // Is piece active?
            if (newPiece.gameObject.activeInHierarchy)
                finalPiece = newPiece;
        }

        finalPiece.ComputerMove();
    }
    */

    public void SwitchSides(Color color)
    {
        if (!mIsKingAlive)
        {
            // Reset pieces
            ResetPieces();

            // King has risen from the dead
            mIsKingAlive = true;

            // Change color to black, so white can go first again
            color = Color.black;
        }

        bool isBlackTurn = color == Color.white ? true : false;

        // Set team interactivity
        SetInteractive(mWhitePieces, !isBlackTurn);

        // Disable this so player can't move pieces
        SetInteractive(mBlackPieces, isBlackTurn);

        // Show assist overlay
        ShowAssist(isBlackTurn);

        // Set promoted interactivity
        foreach (BasePiece piece in mPromotedPieces)
        {
            bool isBlackPiece = piece.mColor != Color.white ? true : false;
            bool isPartOfTeam = isBlackPiece == true ? isBlackTurn : !isBlackTurn;

            piece.enabled = isPartOfTeam;
        }

        // ADDED: Move random piece
        /*
        if (isBlackTurn)
            MoveRandomPiece();
        */
    }

    private void ShowAssist(bool isBlackTurn)
    {
        Cell.clearOutlineAll(allPossibleMoves);
        Cell.clearOutlineAll(allDefendedCells);
        Cell.clearOutlineAll(whiteAttackedCells);
        Cell.clearOutlineAll(blackAttackedCells);
        Cell.clearOutlineAll(allPinnedCells);
        allPossibleMoves.Clear();
        allDefendedCells.Clear();
        whiteAttackedCells.Clear();
        blackAttackedCells.Clear();
        allPinnedCells.Clear();

        foreach (BasePiece piece in mWhitePieces)
            piece.OnTurnStart();
        foreach (BasePiece piece in mBlackPieces)
            piece.OnTurnStart();
        foreach (BasePiece piece in (isBlackTurn ? mBlackPieces : mWhitePieces))  // Find all possible moves
            allPossibleMoves.UnionWith(piece.mHighlightedCells);

        Cell.setOutlineAll(isBlackTurn ? blackAttackedCells : whiteAttackedCells, "red");
    }

    public void ResetPieces()
    {
        foreach (BasePiece piece in mPromotedPieces)
        {
            piece.Kill();
            Destroy(piece.gameObject);
        }

        mPromotedPieces.Clear();

        foreach (BasePiece piece in mWhitePieces)
            piece.Reset();

        foreach (BasePiece piece in mBlackPieces)
            piece.Reset();
    }

    public void PromotePiece(Pawn pawn, Cell cell, Color teamColor, Color spriteColor)
    {
        // Kill Pawn
        pawn.Kill();

        // Create
        BasePiece promotedPiece = CreatePiece(typeof(Queen));
        promotedPiece.Setup(teamColor, spriteColor, this);

        // Place piece
        promotedPiece.Place(cell);

        // Add
        mPromotedPieces.Add(promotedPiece);
    }
}
