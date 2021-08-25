using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public bool isBlackTurn;
    public List<PieceData> pieces;
    public Settings whiteSettings = Settings.white;
    public Settings blackSettings = Settings.black;

    public GameData(bool isBlackTurn, List<BasePiece> pieces)
    {
        this.isBlackTurn = isBlackTurn;
        this.pieces = new List<PieceData>();
        foreach (BasePiece piece in pieces)
            if (piece.gameObject.activeSelf == true)
            this.pieces.Add(new PieceData(piece));
    }
}

[System.Serializable]
public class PieceData
{
    public Coordinate originalPosition;
    public Coordinate position;

    public PieceData(BasePiece piece)
    {
        position = new Coordinate(piece.CurrentCell.mBoardPosition);
        originalPosition = new Coordinate(piece.OriginalCell.mBoardPosition);
    }
}

[System.Serializable]
public class Coordinate
{
    public int x;
    public int y;

    public Coordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Coordinate(Vector2Int pos)
    {
        x = pos.x;
        y = pos.y;
    }
}