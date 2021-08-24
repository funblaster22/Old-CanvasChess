using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    public static readonly Settings white = new Settings(false);
    public static readonly Settings black = new Settings(true);

    public bool isBlack;
    public bool showAssist = true;
    public bool showDefended = true;
    public bool showPinned = true;
    public bool showDanger = true;
    public bool showCaptures = true;

    // TODO: not implemented yet
    public bool showAllMoves = true;

    public Settings(bool isBlack) {
        this.isBlack = isBlack;
    }

    public static Settings GetPlayer(bool isBlack)
    {
        return isBlack ? black : white;
    }
}
