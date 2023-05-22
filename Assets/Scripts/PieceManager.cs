using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.UI;

public class PieceManager : MonoBehaviour
{
    [HideInInspector]
    public bool mIsKingAlive = true;
    public bool KingInDanger => (isBlackTurn ? blackAttackedCells : whiteAttackedCells).Contains(
        (isBlackTurn ? mBlackPieces : mWhitePieces).Find(piece => piece is King).CurrentCell
    );

    public GameObject mPiecePrefab;
    public Text whiteScore;
    public Text blackScore;

    private List<BasePiece> mWhitePieces = null;
    private List<BasePiece> mBlackPieces = null;
    public List<BasePiece> AllPieces
    {
        get { return mWhitePieces.Concat(mBlackPieces).ToList(); }
    }
    private List<BasePiece> mPromotedPieces = new List<BasePiece>();
    private Cell[,] cells;
    private bool isBlackTurn = false;
    public bool isTwoPlayer;

    [HideInInspector]
    public List<Cell> allDefendedCells = new();  // Is a list instead of HashSet b/c need to know how well defended each piece is
    [HideInInspector]
    public List<Cell> whiteAttackedCells = new();
    [HideInInspector]
    public List<Cell> blackAttackedCells = new();
    public HashSet<Cell> allPossibleMoves = new HashSet<Cell>();  // Only applies to current player
    public HashSet<Cell> allPinnedCells = new HashSet<Cell>();

    private readonly string[] mPieceOrder = new string[16]
    {
        "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "Q", "K", "B", "KN", "R"
    };

    private readonly Dictionary<string, Type> mPieceLibrary = new Dictionary<string, Type>()
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

        GameData game = SaveSystem.LoadGame();
        if (game != null)
        {
            foreach (PieceData piece in game.pieces)
            {
                // Query piece
                Cell originalCell = board.mAllCells[piece.originalPosition.x, piece.originalPosition.y];
                BasePiece gamePiece = originalCell.mCurrentPiece;
                // Kill
                gamePiece.Kill();
                // Set firstmove to false if moved
                if (piece.originalPosition != piece.position)  // TODO: will cause issues with references?
                    gamePiece.mIsFirstMove = false;
                // Place
                if (!piece.isDefeated)
                    gamePiece.Place(board.mAllCells[piece.position.x, piece.position.y], false);
            }
            mIsKingAlive = true;
            isTwoPlayer = game.isTwoPlayer;
            SwitchSides(game.isBlackTurn ? Color.white : Color.black);
        } else
        {
            // White goes first
            SwitchSides(Color.black);
        }
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

        // Calculate Score
        static int summer(BasePiece piece) => piece.IsAlive ? piece.Value : 0;
        whiteScore.text = mWhitePieces.Sum(summer).ToString();
        blackScore.text = mBlackPieces.Sum(summer).ToString();

        isBlackTurn = color == Color.white;

        // Rotate board
        foreach (var rotatable in GameObject.FindGameObjectsWithTag("Rotatable")) {
            rotatable.transform.localEulerAngles = new Vector3(0, 0, isBlackTurn ? 180 : 0);
        }

        // Set team interactivity
        SetInteractive(mWhitePieces, !isBlackTurn);

        // Disable this so player can't move pieces
        SetInteractive(mBlackPieces, isBlackTurn);

        // Show assist overlay
        ShowAssist();

        // Set promoted interactivity
        foreach (BasePiece piece in mPromotedPieces)
        {
            bool isBlackPiece = piece.mColor != Color.white ? true : false;
            bool isPartOfTeam = isBlackPiece == true ? isBlackTurn : !isBlackTurn;

            piece.enabled = isPartOfTeam;
        }

        (isBlackTurn ? mBlackPieces : mWhitePieces).ForEach(piece => piece.StartTurn());

        if (isBlackTurn && !isTwoPlayer)
            MoveRandomPiece();
    }

    public void HideAssist()
    {
        IEnumerable<Cell> allCells = Enumerable.Cast<Cell>(cells);
        Cell.ClearBackgroundAll(allCells);
        Cell.ClearOutlineAll(allCells);
        Cell.ClearOverlayAll(allCells);
        allPossibleMoves.Clear();
        allDefendedCells.Clear();
        whiteAttackedCells.Clear();
        blackAttackedCells.Clear();
        allPinnedCells.Clear();
    }

    public void ShowAssist()
    {
        HideAssist();
        var attackedAndDefended = new List<Cell>();

        foreach (BasePiece piece in mWhitePieces)
            if (piece.gameObject.activeSelf)  // Make sure piece not defeated
                piece.CheckPathing();
        foreach (BasePiece piece in mBlackPieces)
            if (piece.gameObject.activeSelf)
                piece.CheckPathing();
        foreach (BasePiece piece in (isBlackTurn ? mBlackPieces : mWhitePieces))  // Find all possible moves
            if (piece.gameObject.activeSelf) {
                allPossibleMoves.Add(piece.CurrentCell);
                allPossibleMoves.UnionWith(piece.mHighlightedCells);  // TODO: integrate within prev. loops to reduce redundancy
            }

        if (Settings.GetPlayer(isBlackTurn).showCaptures)
            Cell.SetOutlineAll(isBlackTurn ? whiteAttackedCells : blackAttackedCells, OutlineState.Capture);
        if (Settings.GetPlayer(isBlackTurn).showAllMoves)
            Cell.SetBackgroundAll(allPossibleMoves, isBlackTurn ? Globals.red : Globals.blue, 100);
        if (Settings.GetPlayer(isBlackTurn).showDanger) {
            // Known issue: only works properly when all assist options enabled
            // TODO: move this logic to compute before assists rendered. Split attackedAndDefended into "whiteAttackedAndDefended" & "blackAttackedAndDefended"
            // However, when I tried, PartialShield stopped working :(
            var allAttackedCells = whiteAttackedCells.Concat(blackAttackedCells).ToList();
            for (int i=allDefendedCells.Count() - 1; i>=0; i--) {
                var cell = allDefendedCells[i];
                if (allAttackedCells.Contains(cell)) {
                    attackedAndDefended.Add(cell);
                    // TODO: will there be consequences mutating this var?
                    allDefendedCells.Remove(cell);
                    allAttackedCells.Remove(cell);
                    whiteAttackedCells.Remove(cell);
                    blackAttackedCells.Remove(cell);
                }
            }

            Cell.SetOutlineAll(isBlackTurn ? blackAttackedCells : whiteAttackedCells, OutlineState.Danger);
            Cell.SetOverlayAll(allAttackedCells, OverlayType.Sword);
        }
        if (Settings.GetPlayer(isBlackTurn).showDefended) {
            Cell.SetOverlayAll(attackedAndDefended, OverlayType.PartialShield);
            Cell.SetOverlayAll(allDefendedCells, OverlayType.Shield);
        } else if (Settings.GetPlayer(isBlackTurn).showDanger) {
            Cell.SetOverlayAll(attackedAndDefended, OverlayType.Sword);
        }
        if (Settings.GetPlayer(isBlackTurn).showPinned)
            Cell.SetOverlayAll(allPinnedCells, OverlayType.Pin);
    }

    public void ResetPieces()
    {
        print("Good Game!");
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

        File.Delete(SaveSystem.savePath);
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
