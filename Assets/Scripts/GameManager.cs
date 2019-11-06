using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState { Standby, SpyMasterTurn, KingTurn, GameOver}

public class GameManager : MonoBehaviour {

    static public GameManager S;
    public delegate void GameStateChanged(GameState prev, GameState current);
    public static event GameStateChanged OnGameStateChanged;
    public static Action OnPlayerPressedEscape;
    public int turnsPlayed { get; private set; }
    public GameState gameState { get; private set; }

    private void Awake()
    {
        S = this;
    }

    private void OnEnable()
    {
        SpyMaster.OnSpyMasterTurnEnded += SwitchTurnToKing;
        King.OnKingTurnEnded += SwitchTurnToSpyMaster;
    }

    private void OnDisable()
    {
        SpyMaster.OnSpyMasterTurnEnded -= SwitchTurnToKing;
        King.OnKingTurnEnded -= SwitchTurnToSpyMaster;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
            StartGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (OnPlayerPressedEscape != null)
                OnPlayerPressedEscape();
        }
            
    }

    public void StartGame()
    {
        turnsPlayed = 0;
        gameState = GameState.Standby;
        SetGameState(GameState.Standby);
    }

    public void SetGameState(GameState newGameState)
    {
        GameState prev = gameState;
        gameState = newGameState;
        if (OnGameStateChanged != null)
            OnGameStateChanged(prev, newGameState);
    }

    public void SwitchTurnToKing()
    {
        SetGameState(GameState.KingTurn);
    }

    public void SwitchTurnToSpyMaster()
    {
        SetGameState(GameState.SpyMasterTurn);
    }
}
