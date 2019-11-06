using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using DG.Tweening;

public class Spy : MonoBehaviour {

    #region Fields
    public int id;
    public Vector2Int mapPosition;
    public Vector2Int nextPos;
    private const int MIN_RANDOM_ADDITION = 1;
    private const int MAX_RANDOM_ADDITION = 100;
    private const int SUCCESS_CHANCE = 75;
    private const int MIN_FARM_VALUE = 1;
    private const int MAX_FARM_VALUE = 6;
    private const float Y_POS_OFFSET = 0.635f;
    #endregion

    #region Properties
    public bool selected { get; set; }
    public GameObject halo { get; set; }
    #endregion

    #region Events & Delegates
    public static event Action<Spy> OnSpySelected;
    public static event Action<int> OnSpyMove;
    public static event Action OnSpyEndedMove;
    #endregion

    #region Monobehaviour callbacks
    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        OnSpySelected += ToggleHalo;
    }

    private void OnDisable()
    {
        OnSpySelected -= ToggleHalo;
    }

    private void OnMouseDown()
    {
        ToggleSelection(!selected);
        if (OnSpySelected != null)
            OnSpySelected(this);
    }

    private void OnMouseEnter()
    {
        UIManager.S.DisplayHelpMessage("Spy");
    }

    private void OnMouseExit()
    {
        UIManager.S.HideHelpMessage();
    }
    #endregion

    #region  Public Methods
    public Vector2Int[] GetRouteToPosition(Vector2Int currentPos, Vector2Int wantedPos, int allowedMoves = 6)
    {
        Vector2Int closestTile = Map.S.GetClosestTileToDestinationForSpy(currentPos, wantedPos, allowedMoves);
        if (closestTile == currentPos)
            return null;
        Vector2Int[] route = new Vector2Int[allowedMoves];
        
        for (int i = 0; i < route.Length; i++)
        {
            route[i] = closestTile;
            closestTile = Map.S.GetClosestTileToDestinationForSpy(closestTile, wantedPos, allowedMoves);
        }
        return route;
    }

    public IEnumerator Move(Tile tileToMoveTo, int ap)
    {
        if (!Map.CanMoveToTile(tileToMoveTo.mapPosition))
            goto End;
        
        Vector2Int[] route = GetRouteToPosition(mapPosition, tileToMoveTo.mapPosition, ap);

        Sequence movement = DOTween.Sequence();
        Vector3[] tileWorldPositions = new Vector3[route.Length];
        for (int i = 0; i < route.Length; i++)
        {
            if (route[i].x == 255 || route[i].y == 255)
                break;
            tileWorldPositions[i] = Map.S.tileMap[route[i]].transform.position;
            tileWorldPositions[i].y = Y_POS_OFFSET;
           
            movement.Append(transform.DOMove(tileWorldPositions[i], 0.5f));
            movement.AppendCallback(() =>
            {
                if (OnSpyMove != null)
                    OnSpyMove(1);
            });
        }
        movement.Play().OnComplete(()=>
        {
            Map.S.GetTile(mapPosition).available = true;
            Map.S.GetTile(mapPosition).tileStatus = TileStatus.Empty;
            tileToMoveTo.available = false;
            mapPosition = tileToMoveTo.mapPosition;
            Map.S.SetTileStatus(tileToMoveTo, TileStatus.Spy);
            if(SpyMaster.S.actionPoints == 0)
            {
                ToggleHalo(false);
                ToggleSelection(false);
                SpyMaster.S.SetSelectedSpy(null);
            }
            
            if (OnSpyEndedMove != null)
                OnSpyEndedMove();
            
        });
        yield return new WaitUntil(() => DOTween.IsTweening(movement));
        
        End:
        yield return null;
    }

    public bool Bribe(int sum)
    {
        if (sum + UnityEngine.Random.Range(MIN_RANDOM_ADDITION, MAX_RANDOM_ADDITION + 1) > SUCCESS_CHANCE)
            return true;
        return false;
    }

    public bool Blackmail(int sum)
    {
        if (sum + UnityEngine.Random.Range(MIN_RANDOM_ADDITION, MAX_RANDOM_ADDITION + 1) > SUCCESS_CHANCE)
            return true;
        return false;
    }

    public int Farm()
    {
        return UnityEngine.Random.Range(MIN_FARM_VALUE, MAX_FARM_VALUE);
    }

    public void StartTurn()
    {

    }

    public void ToggleSelection(bool select)
    {
        selected = select;
    }

    public void ToggleHalo(Spy s)
    {
        ToggleHalo(s == this);
    }

    public void ToggleHalo(bool on)
    {
        halo.SetActive(on);
    }
    #endregion

    #region Private Methods
    private void Init()
    {
        halo = transform.GetChild(0).gameObject;
        ToggleSelection(false);
    }

    #endregion

}
