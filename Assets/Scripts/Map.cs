using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    #region Public Fields
    static public Map S;
    public Tile selectedTile;
    public Farmable selectedFarmable;
    public LordCastle selectedLordCastle;
    public MonoBehaviour[] places;
    #endregion

    #region Private Fields
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Tile[] _tiles;
    private Dictionary<Vector2Int, Tile> _tileMap = new Dictionary<Vector2Int, Tile>();
    private Dictionary<Vector2Int, IPlace> _placeMap = new Dictionary<Vector2Int, IPlace>();
    #endregion

    #region Properties
    public Tile[] tiles
    {
        get { return _tiles; }
        private set { _tiles = value; }
    }
    public Dictionary<Vector2Int, Tile> tileMap
    {
        get { return _tileMap; }
        private set { _tileMap = value; }
    }
    public Dictionary<Vector2Int, IPlace> placeMap
    {
        get { return _placeMap; }
        private set { _placeMap = value; }
    }
    #endregion

    #region Constants
    public const int NUM_OF_ROWS = 8;
    public const int NUM_OF_COLS = 15;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        S = this;
        InitializeTileMap();
    }

    private void OnEnable()
    {
        Spy.OnSpySelected += ShowAvailableTiles;
        UIManager.OnActionButtonPressed += TurnOffAllTiles;
        Farmable.OnFarmableSelected += SetSelectedFarmable;
        Farmable.OnFarmableSelected += TurnOffAllTiles;
        SpyMaster.OnSpyMasterTurnEnded += ClearSelectedFarmable;
        LordCastle.OnLordCastleSelected += SetSelectedLordCastle;
        LordCastle.OnLordCastleSelected += TurnOffAllTiles;
        Guard.OnSpyKilled += ClearKilledSpyTile;
    }

    private void OnDisable()
    {
        Spy.OnSpySelected -= ShowAvailableTiles;
        UIManager.OnActionButtonPressed -= TurnOffAllTiles;
        Farmable.OnFarmableSelected -= SetSelectedFarmable;
        Farmable.OnFarmableSelected -= TurnOffAllTiles;
        SpyMaster.OnSpyMasterTurnEnded -= ClearSelectedFarmable;
        LordCastle.OnLordCastleSelected -= SetSelectedLordCastle;
        LordCastle.OnLordCastleSelected -= TurnOffAllTiles;
        Guard.OnSpyKilled -= ClearKilledSpyTile;
    }
    #endregion

    #region Public Methods
    public void SetSelectedTile(Tile t)
    {
        if (selectedTile)
            selectedTile.ToggleSelection(false);
        selectedTile = t;
    }

    public Vector2Int[] GetAdjacentPositions(Vector2Int pos)
    {
        if (pos.x % 2 == 1)
        {
            return new Vector2Int[] {
            new Vector2Int(pos.x, pos.y+1),
            new Vector2Int(pos.x+1, pos.y+1),
            new Vector2Int(pos.x+1, pos.y),
            new Vector2Int(pos.x, pos.y-1),
            new Vector2Int(pos.x-1, pos.y),
            new Vector2Int(pos.x-1, pos.y+1)};
        }
        else
        {
            return new Vector2Int[] {
            new Vector2Int(pos.x + 1, pos.y-1),
            new Vector2Int(pos.x, pos.y-1),
            new Vector2Int(pos.x-1, pos.y-1),
            new Vector2Int(pos.x-1, pos.y),
            new Vector2Int(pos.x, pos.y+1),
            new Vector2Int(pos.x+1, pos.y)};
        }

    }

    public void InitializeTileMap()
    {
        foreach (Tile t in _tiles)
        {
            t.spriteRenderer = t.GetComponent<SpriteRenderer>();
            _tileMap.Add(t.mapPosition, t);
        }
        foreach (var place in places)
        {
            if (place is LordCastle)
            {
                LordCastle lc = place as LordCastle;
                placeMap.Add(lc.mapPosition, lc);
            }
            else if (place is City)
            {
                City c = place as City;
                placeMap.Add(c.mapPosition, c);
            }
            else if (place is Market)
            {
                Market m = place as Market;
                placeMap.Add(m.mapPosition, m);
            }
            else if (place is KingCastle)
            {
                KingCastle kc = place as KingCastle;
                placeMap.Add(kc.mapPosition, kc);
            }
        }
    }

    public void ShowAvailableTiles(Spy s)
    {
        int ap = SpyMaster.S.GetAPAmount();
        Tile t = GetTile(s.mapPosition);
        TurnOffAllTiles();
        ShowAvailableTiles(t, ap);
    }

    public void ShowAvailableTiles(Tile t, int availableAP)
    {
        if (availableAP <= 0)
            return;
        Tile[] adjacentTiles = t.GetAllAdjacentTiles();
        for (int i = 0; i < adjacentTiles.Length; i++)
        {
            if (adjacentTiles[i] != null && adjacentTiles[i].available)
            {
                adjacentTiles[i].DisplayAvailability(true);
                ShowAvailableTiles(adjacentTiles[i], availableAP - 1);
            }
        }
    }

    ///Helper overload for listening to OnLordCastleSelected Event
    public void TurnOffAllTiles(LordCastle l)
    {
        TurnOffAllTiles();
    }

    ///Helper overload for listening to OnFarmableSelected Event
    public void TurnOffAllTiles(Farmable f)
    {
        TurnOffAllTiles();
    }

    public void TurnOffAllTiles()
    {
        for (int i = 0; i < _tiles.Length; i++)
        {
            _tiles[i].DisplayAvailability(false);
        }
    }

    public Tile GetTile(Vector2Int pos)
    {
        if (_tileMap.ContainsKey(pos))
            return _tileMap[pos];
        return null;
    }

    public Vector2Int GetClosestTileToDestinationForGuard(Vector2Int currentPos, Vector2Int wantedPos)
    {
        if (currentPos == wantedPos)
        {
            //returning an out of bounds vector for the Move coroutine to stop moving the guard
            return new Vector2Int(255, 255);
        }

        float closestDistance = float.MaxValue;
        Vector2Int closestTile = currentPos;
        Vector2Int[] positionsToCheck = GetAdjacentPositions(currentPos);

        for (int i = 0; i < positionsToCheck.Length; i++)
        {
            float distance = Vector2Int.Distance(positionsToCheck[i], wantedPos);
            if (distance < closestDistance)
            {
                if (CanMoveToTile(positionsToCheck[i]))
                {
                    closestDistance = distance;
                    closestTile = positionsToCheck[i];
                }
                else
                    continue;
            }
        }
        return closestTile;
    }

    public Vector2Int GetClosestTileToDestinationForSpy(Vector2Int currentPos, Vector2Int wantedPos, int ap)
    {
        if (currentPos == wantedPos || ap <= 0)
        {
            //returning an out of bounds vector for the Move coroutine to stop moving the spy
            return new Vector2Int(255, 255);
        }

        float closestDistance = float.MaxValue;
        Vector2Int closestTile = currentPos;
        Vector2Int[] positionsToCheck = GetAdjacentPositions(currentPos);
        List<Vector2Int> equalDistanceVectors = new List<Vector2Int>();
        List<float> equalDistances = new List<float>();

        for (int i = 0; i < positionsToCheck.Length; i++)
        {
            float distance = Vector2Int.Distance(positionsToCheck[i], wantedPos);
            if (distance <= closestDistance)
            {
                if (CanMoveToTile(positionsToCheck[i]))
                {
                    closestDistance = distance;
                    closestTile = positionsToCheck[i];
                    equalDistanceVectors.Add(positionsToCheck[i]);
                    equalDistances.Add(closestDistance);
                    for (int j = 0; j < equalDistances.Count; j++)
                    {
                        if (equalDistances[j] > distance)
                        {
                            equalDistanceVectors.Remove(equalDistanceVectors[j]);
                            equalDistances.Remove(equalDistances[j]);
                        }

                    }
                    foreach (Vector2Int v in equalDistanceVectors)
                    {
                        GetClosestTileToDestinationForSpy(v, wantedPos, ap - 1);
                    }
                }
                else
                    continue;
            }
        }
        return closestTile;
    }

    public static bool CanMoveToTile(Vector2Int adjacentTile)
    {
        Tile t = Map.S.GetTile(adjacentTile);
        return t != null && t.available;
    }

    public void SetTileStatus(Tile t, TileStatus ts)
    {
        t.tileStatus = ts;
    }

    public void SetSelectedFarmable(Farmable f)
    {
        selectedFarmable = f;
    }

    public void SetSelectedLordCastle(LordCastle l)
    {
        selectedLordCastle = l;
    }

    public bool IsAdjacent(Tile a, Tile b)
    {
        return a.IsAdjacent(b);
    }

    public bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return GetTile(a).IsAdjacent(b);
    }

    public void ClearSelectedFarmable()
    {
        SetSelectedFarmable(null);
        SetSelectedLordCastle(null);
    }

    public void ClearKilledSpyTile(Spy s)
    {
        GetTile(s.mapPosition).tileStatus = TileStatus.Empty;
    }
    #endregion


}
