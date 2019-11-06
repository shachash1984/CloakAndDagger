using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class King : MonoBehaviour {

    #region Public Fields
    public List<Guard> guards;
    public Guard selectedGuard;
    #endregion

    #region Private Fields
    [SerializeField] private float _suspicion;
    [SerializeField] private float _suspicionRate = 0.2f;

    #endregion

    #region Properties
    public float suspicion
    {
        get { return _suspicion; }
        set
        {
            if (value >= 0 && value <= 1)
            {
                _suspicion = value;
                if (OnSuspicionChanged != null)
                    OnSuspicionChanged(value);
            }
        }
    }
    #endregion

    #region Events and Delegates
    public static event Action<float> OnSuspicionChanged;
    public static event Action OnKingTurnEnded;
    public static event Action<Guard> OnGuardSelected;

    #endregion
    private void Start()
    {
        suspicion = 0;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
        //SpyMaster.OnSpyMasterTurnEnded += PlayTurn;
        SpyMaster.OnAttemptSucceeded += RaiseSuspicionRate;
        SpyMaster.OnAttemptFailed += RaiseSuspicion;
        Guard.OnGuardEndedMove += EndTurn;
    }

    private void OnDisable()
    {
        //SpyMaster.OnSpyMasterTurnEnded -= PlayTurn;
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        SpyMaster.OnAttemptSucceeded -= RaiseSuspicionRate;
        SpyMaster.OnAttemptFailed -= RaiseSuspicion;
        Guard.OnGuardEndedMove -= EndTurn;
    }

    private Spy GetClosestSpyToGuard(Guard g)
    {
        float minDistance = float.MaxValue;
        Spy closest = null;
        foreach (Spy s in SpyMaster.S.spies)
        {
            float distance = Vector3.Distance(g.transform.position, s.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = s;
            }
        }
        return closest;
    }

    private Guard GetRandomGuard()
    {
        return guards[UnityEngine.Random.Range(0, guards.Count)];
    }

    public void PlayTurn()
    {
        SetSuspicion(suspicion + _suspicionRate);
        selectedGuard = GetRandomGuard();
        if (OnGuardSelected != null)
            OnGuardSelected(selectedGuard);
        selectedGuard.Move(GetWantedPosition());
    }

    public void EndTurn()
    {
        selectedGuard.ToggleHalo(false);
        selectedGuard = null;
        if (OnKingTurnEnded != null)
            OnKingTurnEnded();
    }

    private void SetSuspicion(float value)
    {
        suspicion = value;
    }

    private void RaiseSuspicionRate()
    {
        _suspicionRate += 0.02f;
    }

    private void RaiseSuspicion()
    {
        _suspicion += _suspicionRate;
    }

    private Vector2Int GetWantedPosition()
    {
        int encounterChance = (int)(suspicion * 100);
        Debug.Log("Encounter chance: " + encounterChance);
        int rand = UnityEngine.Random.Range(1, 100);
        Tile t = null;
        if (rand < encounterChance)
            t = Map.S.GetTile(SpyMaster.S.GetRandomSpy().mapPosition);
        else
        {
            t = Map.S.tiles[UnityEngine.Random.Range(0, Map.S.tiles.Length)];
            int counter = 0;
            while (!t.available && counter < 100)
            {
                t = Map.S.tiles[UnityEngine.Random.Range(0, Map.S.tiles.Length)];
                counter++;
            }
            if (counter >= 100)
                Debug.Log("king counter: " + counter);
        }
        return t.mapPosition;
    }

    public void HandleGameStateChanged(GameState prev, GameState current)
    {
        if (current == GameState.KingTurn)
            PlayTurn();
    }
}
