using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LordCastle : MonoBehaviour, IPlace {


    #region Private Fields
    [SerializeField] private Vector2Int _mapPosition;
    [SerializeField] private Texture2D[] _flagTextures;
    private GameObject _halo;
    private SkinnedMeshRenderer _flag;
    #endregion

    #region Public Fields
    public bool isOwned = false;
    public static LordCastle selected;
    #endregion

    #region Properties
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
    #endregion

    #region Events and Delegates
    public static event Action<LordCastle> OnLordCastleSelected;
    public static event Action<LordCastle> OnLordBecameOwned;
    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        if (!_halo)
            _halo = transform.GetChild(0).gameObject;
        if (!_flag)
            _flag = transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>();
    }

    private void OnEnable()
    {
        SpyMaster.OnSpyMasterTurnEnded += ClearSelectedLordCastle;
    }

    private void OnDisable()
    {
        SpyMaster.OnSpyMasterTurnEnded -= ClearSelectedLordCastle;
    }

    private void OnMouseDown()
    {
        if (!isOwned)
        {
            Spy s = SpyMaster.S.GetSelectedSpy();
            if (s != null && IsSpyAdjacent(s))
            {
                if (OnLordCastleSelected != null)
                    OnLordCastleSelected(this);
                selected = this;
                ToggleHalo(selected == this);
            }
            else
            {
                //play cant be selected sound
                //show cant be selected feedback
                UIManager.S.DisplayMessage("Not Close Enough");
            }
        }
        else
            UIManager.S.DisplayMessage("Lord already owned");
    }

    private void OnMouseEnter()
    {
        UIManager.S.DisplayHelpMessage("Lord castle");
    }

    private void OnMouseExit()
    {
        UIManager.S.HideHelpMessage();
    }
    #endregion

    #region Private Methods
    private bool IsSpyAdjacent(Spy s)
    {
        return Map.S.IsAdjacent(mapPosition, s.mapPosition);
    }

    private void SetSpyMasterFlag()
    {
        _flag.material.SetTexture("_MainTex", _flagTextures[1]);
    }
    #endregion

    #region Public Methods
    public void ToggleHalo(bool on)
    {
        if (!isOwned)
            _halo.SetActive(on);
        else
            _halo.SetActive(false);
    }

    public void ClearSelectedLordCastle()
    {
        ToggleHalo(false);
    }

    public void ToggleOwned(bool own)
    {
        isOwned = own;
        if (isOwned)
        {
            if (OnLordBecameOwned != null)
                OnLordBecameOwned(this);
            SetSpyMasterFlag();
        }

    }
    #endregion



}
