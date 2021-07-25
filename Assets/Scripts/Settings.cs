using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    public static readonly Settings white = new Settings(false);
    public static readonly Settings black = new Settings(true);

    public bool isBlack;

    public Settings(bool isBlack) {
        this.isBlack = isBlack;
    }
}
