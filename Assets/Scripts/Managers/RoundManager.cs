using System;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public enum PlayerType
    {
        AI,
        Player
    }

    //Using event here cause only this script should invoke these events
    public event Action OnRoundBegin;
    public event Action OnTurnSwitch;

    [Header("Game Rules")]
    [SerializeField] private int roundsNeededToWin = 3;
    [SerializeField] private PlayerType firstPlayerToAct = PlayerType.Player;

    [Header("Game Parameters")]
    public PlayerType currentPlayerTurn;
    public int[] roundsWon = new int[2]; //0 - rounds won by the AI, 1 - rounds won by the player
    public int currentRound = 0;
    public bool isGameOver = false;

    private TokenGrid tokenGrid;
    private Revolver revolver;

    private void Awake()
    {
        //Get references before any game logic
        tokenGrid = FindFirstObjectByType<TokenGrid>();
        revolver = FindFirstObjectByType<Revolver>();
    }

    private void Start()
    {
        currentRound = 0;
        roundsWon[0] = 0;
        roundsWon[1] = 0;
        BeginNewRound();
    }

    public void SwitchTurn()
    {
        //If the current turn is the AI's, change it to the player's and vice versa
        currentPlayerTurn = currentPlayerTurn == PlayerType.Player ? PlayerType.AI : PlayerType.Player;
        OnTurnSwitch?.Invoke();
    }

    public void BeginNewRound()
    {
        currentPlayerTurn = firstPlayerToAct;
        tokenGrid.currentPlayer = currentPlayerTurn;
        tokenGrid.ClearGrid();
        currentRound++;
        revolver.bulletCount = revolver.defaultBulletCount;
        OnRoundBegin?.Invoke();
        OnTurnSwitch?.Invoke();
    }

    public void WinRound(PlayerType winner)
    {
        int playerIndex = winner == PlayerType.Player ? 1 : 0;
        bool hasPlayerWon = playerIndex == 1;
        string playerName = playerIndex == 1 ? "Player" : "AI";

        DebugMessenger.DebugMessage($"{playerName} has won round " + currentRound + "!");
        roundsWon[playerIndex]++;
        if (roundsWon[playerIndex] >= roundsNeededToWin)
        {
            isGameOver = true;
            DebugMessenger.DebugMessage($"{playerName} has won!");
            EndScreen.EndGame(hasPlayerWon);
            //TODO: Start win/lose sequence
            //Will probably be an animation/coroutine with the player script being disabled, the opponent performing an animation, etc.
            return;
        }
        BeginNewRound();
    }
}
