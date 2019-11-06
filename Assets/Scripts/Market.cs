using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Market : Farmable {

    private void OnMouseEnter()
    {
        UIManager.S.DisplayHelpMessage("Market");
    }

    private void OnMouseExit()
    {
        UIManager.S.HideHelpMessage();
    }
}
