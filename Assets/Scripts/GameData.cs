using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public bool isBlackTurn;
    public List<PieceData> pieces;
    public bool isTwoPlayer;
    public Settings whiteSettings = Settings.white;
    public Settings blackSettings = Settings.black;

    public GameData(bool isBlackTurn, List<BasePiece> pieces)
    {
        this.isBlackTurn = isBlackTurn;
        this.pieces = new List<PieceData>();
        foreach (BasePiece piece in pieces)
            this.pieces.Add(new PieceData(piece));
    }
}

[System.Serializable]
public class PieceData
{
    public Coordinate originalPosition;
    public Coordinate position;
    public bool isDefeated;

    public PieceData(BasePiece piece)
    {
        isDefeated = !piece.gameObject.activeSelf;
        position = new Coordinate(piece.CurrentCell.mBoardPosition);
        originalPosition = new Coordinate(piece.OriginalCell.mBoardPosition);
    }
}

[System.Serializable]
public class Coordinate : IEquatable<Coordinate>
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

    // Adapted from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type
    public override int GetHashCode() => (x, y).GetHashCode();

    public override bool Equals(object obj) => this.Equals(obj as Coordinate);

    public bool Equals(Coordinate coord) {
        if (coord is null)
            return false;

        // Optimization for a common success case.
        if (System.Object.ReferenceEquals(this, coord))
            return true;

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != coord.GetType())
            return false;

        return coord.x == x && coord.y == y;
    }

    public static bool operator == (Coordinate lhs, Coordinate rhs) {
        if (lhs is null) {
            if (rhs is null) {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Coordinate lhs, Coordinate rhs) => !(lhs == rhs);
}