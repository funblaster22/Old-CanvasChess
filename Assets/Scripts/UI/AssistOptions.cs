using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssistOptions : MonoBehaviour
{
    public bool isBlack;
    public PieceManager mPieceManager;
    private Color themeColor;
    private Settings settings
    {
        get => isBlack ? Settings.black : Settings.white;
    }

    // Start is called before the first frame update
    void Start()
    {
        themeColor = isBlack ? Globals.red : Globals.blue;
        transform.GetChild(0).GetComponent<Text>().text = (isBlack ? "Red" : "Blue") + " Options";
        Toggle[] toggleSwitchs = GetComponentsInChildren<Toggle>();
        // TODO: more robust way that doesn't depend on order
        toggleSwitchs[1].onValueChanged.AddListener(newVal => { settings.showDefended = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[2].onValueChanged.AddListener(newVal => { settings.showPinned = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[3].onValueChanged.AddListener(newVal => { settings.showDanger = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[4].onValueChanged.AddListener(newVal => { settings.showCaptures = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[5].onValueChanged.AddListener(newVal => { settings.showAllMoves = newVal; mPieceManager.ShowAssist(); });
        toggleSwitchs[6].onValueChanged.AddListener(newVal => { settings.showCurrentMove = newVal; mPieceManager.ShowAssist(); });
    }

    public void ToggleAll(bool newVal)
    {
        foreach (Toggle toggle in GetComponentsInChildren<Toggle>())
        {
            //toggle.enabled = false;
            toggle.isOn = newVal;
        }
    }
}
