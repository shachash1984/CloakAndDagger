using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType { information, money}

public abstract class Farmable : MonoBehaviour, IPlace{


    public static event Action<Farmable> OnFarmableSelected;
    protected GameObject halo;

    [SerializeField] private ResourceType _resourceType;
    public ResourceType resourceType
    {
        get
        {
            return _resourceType;
        }

        set
        {
            _resourceType = value;
        }
    }

    [SerializeField] private Vector2Int _mapPosition;
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

    private void Awake()
    {
        if (!halo)
            halo = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        SpyMaster.OnSpyMasterTurnEnded += ClearSelectedFarmable;
    }

    private void OnDisable()
    {
        SpyMaster.OnSpyMasterTurnEnded -= ClearSelectedFarmable;
    }

    private void OnMouseDown()
    {
        Spy s = SpyMaster.S.GetSelectedSpy();
        if (s != null && IsSpyAdjacent(s))
        {
            if (OnFarmableSelected != null)
                OnFarmableSelected(this);
            if (Map.S.selectedFarmable == this)
                ToggleHalo(true);
            else
                ToggleHalo(false);
        }
        else
        {
            //play cant be selected sound
            //show cant be selected feedback
            UIManager.S.DisplayMessage("Not Close Enough");
        }
    }

    public void ToggleHalo(bool on)
    {
        halo.SetActive(on);
    }

    protected bool IsSpyAdjacent(Spy s)
    {
        return Map.S.IsAdjacent(mapPosition, s.mapPosition);
    }

    public void ClearSelectedFarmable()
    {
        ToggleHalo(false);
    }
}
