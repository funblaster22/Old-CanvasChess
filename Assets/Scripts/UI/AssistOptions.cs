using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssistOptions : MonoBehaviour
{
    public bool isBlack;
    public PieceManager mPieceManager;
    private Color themeColor;

    // Start is called before the first frame update
    void Start()
    {
        themeColor = isBlack ? Globals.red : Globals.blue;
        Toggle[] toggleSwitchs = GetComponentsInChildren<Toggle>();
        Settings settings = isBlack ? Settings.black : Settings.white;
        toggleSwitchs[0].onValueChanged.AddListener(newVal => { settings.showDefended = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[1].onValueChanged.AddListener(newVal => { settings.showPinned = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[2].onValueChanged.AddListener(newVal => { settings.showDanger = newVal; mPieceManager.ShowAssist(); });
    }
}
