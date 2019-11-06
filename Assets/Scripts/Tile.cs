using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Direction { N, NE, SE, S, SW, NW}

public enum TileStatus { LordCastle, City, Market, Spy, KingCastle, Guard, Empty }

public class Tile : MonoBehaviour {

    public bool selected = false;
    public bool available = true;
    public Vector2Int mapPosition;
    public TileStatus tileStatus;
    public SpriteRenderer spriteRenderer;
    public static Action<Tile, Spy> OnTileSelected;
    private Color _lastColor;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetTileStatus(TileStatus ts)
    {
        tileStatus = ts;
    }

    private void OnMouseDown()
    {
        if (SpyMaster.S.GetSelectedSpy() != null)
            ToggleSelection(true);
    }

    public void ToggleSelection(bool select)
    {
        selected = select;
        if (select)
        {
            _lastColor = spriteRenderer.color;
            spriteRenderer.color = Color.blue;
            Map.S.SetSelectedTile(this);
            if (OnTileSelected != null)
                OnTileSelected(this, SpyMaster.S.GetSelectedSpy());
        }
        else
            spriteRenderer.color = _lastColor;
    }

    public void DisplayAvailability(bool on)
    {
        spriteRenderer.color = on ? Color.green : Color.clear;
    }

    //public Farmable GetFarmable()
    //{
    //    if (Map.S.placeMap[this.mapPosition] is Farmable)
    //        return Map.S.placeMap[this.mapPosition] as Farmable;
    //    else
    //    {
    //        Debug.LogError("This tile does not have a farmable type on it");
    //        return null;
    //    }
    //}

    public bool IsAdjacent(Tile other)
    {
        return (Mathf.Abs(this.mapPosition.x - other.mapPosition.x) <= 1 && Mathf.Abs(this.mapPosition.y - other.mapPosition.y) <= 1);
    }

    public bool IsAdjacent(Vector2Int other)
    {
        return (Mathf.Abs(this.mapPosition.x - other.x) <= 1 && Mathf.Abs(this.mapPosition.y - other.y) <= 1);
    }

    public Tile GetAdJacentTile(Direction d)
    {
        Vector2Int wantedTilePos = Vector2Int.zero;
        if (mapPosition.x % 2 == 1)
        {
            switch (d)
            {
                case Direction.N:
                    wantedTilePos = new Vector2Int(mapPosition.x, mapPosition.y + 1);
                    break;
                case Direction.NE:
                    wantedTilePos = new Vector2Int(mapPosition.x + 1, mapPosition.y+1);
                    break;
                case Direction.SE:
                    wantedTilePos = new Vector2Int(mapPosition.x + 1, mapPosition.y);
                    break;
                case Direction.S:
                    wantedTilePos = new Vector2Int(mapPosition.x, mapPosition.y - 1);
                    break;
                case Direction.SW:
                    wantedTilePos = new Vector2Int(mapPosition.x - 1, mapPosition.y);
                    break;
                case Direction.NW:
                    wantedTilePos = new Vector2Int(mapPosition.x - 1, mapPosition.y+1);
                    break;
            }
        }
        else
        {
            switch (d)
            {
                case Direction.N:
                    if (mapPosition.x % 2 == 1)

                        wantedTilePos = new Vector2Int(mapPosition.x, mapPosition.y + 1);
                    break;
                case Direction.NE:
                    wantedTilePos = new Vector2Int(mapPosition.x + 1, mapPosition.y);
                    break;
                case Direction.SE:
                    wantedTilePos = new Vector2Int(mapPosition.x + 1, mapPosition.y - 1);
                    break;
                case Direction.S:
                    wantedTilePos = new Vector2Int(mapPosition.x, mapPosition.y - 1);
                    break;
                case Direction.SW:
                    wantedTilePos = new Vector2Int(mapPosition.x - 1, mapPosition.y - 1);
                    break;
                case Direction.NW:
                    wantedTilePos = new Vector2Int(mapPosition.x - 1, mapPosition.y);
                    break;
            }
        }
        
        Tile t = Map.S.GetTile(wantedTilePos);
        if (t != null)
            return t;
        return null;
    }

    public Tile[] GetAllAdjacentTiles()
    {
        Vector2Int[] adjacents = Map.S.GetAdjacentPositions(mapPosition);
        Tile[] adjacentTiles = new Tile[adjacents.Length];
        for (int i = 0; i < adjacentTiles.Length; i++)
        {
            Tile t = Map.S.GetTile(adjacents[i]);
            if (t != null)
                adjacentTiles[i] = t;
        }
        return adjacentTiles;
    }


}
