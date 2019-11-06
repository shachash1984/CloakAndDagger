using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : Farmable
{
    private void OnMouseEnter()
    {
        UIManager.S.DisplayHelpMessage("Mosque");
    }

    private void OnMouseExit()
    {
        UIManager.S.HideHelpMessage();
    }
}
