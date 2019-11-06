using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Guard : MonoBehaviour
{

    public int id;
    public Vector2Int mapPosition;
    public static event Action OnGuardEndedMove;
    public static event Action<Spy> OnSpyKilled;
    
    public GameObject halo;
    private const float Y_POS_OFFSET = 0.635f;

    private void Start()
    {
        halo = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        King.OnGuardSelected += ToggleHalo;
    }

    private void OnDisable()
    {
        King.OnGuardSelected -= ToggleHalo;
    }

    private void OnMouseEnter()
    {
        UIManager.S.DisplayHelpMessage("Guard");
    }

    private void OnMouseExit()
    {
        UIManager.S.HideHelpMessage();
    }

    public void Move(Vector2Int wantedPos)
    {
        Vector2Int[] path = GetRouteToPosition(mapPosition, wantedPos);
        Vector3[] worldPath = new Vector3[path.Length];
        Sequence movement = DOTween.Sequence();
        for (int i = 0; i < path.Length; i++)
        {
            worldPath[i] = Map.S.tileMap[path[i]].transform.position;
            worldPath[i].y = Y_POS_OFFSET;
            movement.Append(transform.DOMove(worldPath[i], 0.5f));
        }
        movement.Play().OnComplete(()=>
        {
            Map.S.GetTile(mapPosition).available = true;
            Map.S.GetTile(mapPosition).tileStatus = TileStatus.Empty;
            Tile tileToMoveTo = Map.S.GetTile(wantedPos);
            tileToMoveTo.available = false;
            mapPosition = tileToMoveTo.mapPosition;
            if(tileToMoveTo.tileStatus == TileStatus.Spy)
            {
                Spy s = SpyMaster.S.GetSpyByMapPosition(wantedPos);
                if (s != null)
                    Kill(s);
            }
            Map.S.SetTileStatus(tileToMoveTo, TileStatus.Guard);
            if (OnGuardEndedMove != null)
                OnGuardEndedMove();
        });
    }

    public void Kill(Spy s)
    {
        UIManager.S.DisplayMessage("Spy Killed");
        if (OnSpyKilled != null)
            OnSpyKilled(s);
    }

    private Vector2Int[] GetRouteToPosition(Vector2Int currentPos, Vector2Int wantedPos)
    {
        List<Vector2Int> route = new List<Vector2Int>();
        Vector2Int closestTile = currentPos;
        int counter = 0;
        while(closestTile.x != 255 && counter <10)
        {
            closestTile = Map.S.GetClosestTileToDestinationForGuard(closestTile, wantedPos);
            if (Map.S.tileMap.ContainsKey(closestTile) && !Map.S.tileMap[closestTile].available)
            {
                if(Map.S.tileMap[closestTile].tileStatus == TileStatus.Spy)
                {
                    route.Add(closestTile);
                    route.Add(new Vector2Int(255, 255));
                    break;
                }
                else
                continue;
            }
            route.Add(closestTile);
            counter++;
        }
        route.RemoveAt(route.Count - 1);

        return route.ToArray();
    }

    public void ToggleHalo(Guard g)
    {
        ToggleHalo(g == this);
    }

    public void ToggleHalo(bool on)
    {
        halo.SetActive(on);
    }
}
