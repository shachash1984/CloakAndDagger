using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingCastle : MonoBehaviour, IPlace {

    [SerializeField] private Vector2Int _mapPosition;
    [SerializeField] private Texture2D[] _flagTextures;
    private SkinnedMeshRenderer _flag;
    public Vector2Int mapPosition
    {
        get
        {
            return _mapPosition;
        }
        set
        {
            _mapPosition = value;
        }
    }

    private void Start()
    {
        if (!_flag)
            _flag = transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>();
    }

    private void OnMouseEnter()
    {
        UIManager.S.DisplayHelpMessage("King castle");
    }

    private void OnMouseExit()
    {
        UIManager.S.HideHelpMessage();
    }

    private void SetSpyMasterFlag()
    {
        _flag.material.SetTexture("_MainTex", _flagTextures[1]);
    }
}

