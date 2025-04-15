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

    private TokenGrid tokenGrid;

    private void Awake()
    {
        //Get references before any game logic
        tokenGrid = FindFirstObjectByType<TokenGrid>();
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
        OnRoundBegin?.Invoke();
        OnTurnSwitch?.Invoke();
    }

    public void WinRound(PlayerType winner)
    {
        if (winner == PlayerType.Player)
        {
            DebugMessenger.DebugMessage("Player has won round " + currentRound + "!");
            roundsWon[1]++;
            if (roundsWon[1] >= roundsNeededToWin)
            {
                DebugMessenger.DebugMessage("Player has won!");
                EndScreen.EndGame(true);
                //TODO: Start win sequence
                //Will probably be an animation/coroutine with the player script being disabled, the opponent performing an animation, etc.
                return;
            }
        }
        else
        {
            DebugMessenger.DebugMessage("AI has won round " + currentRound + "!");
            roundsWon[0]++;
            if (roundsWon[0] >= roundsNeededToWin)
            {
                DebugMessenger.DebugMessage("AI has won!");
                EndScreen.EndGame(false);
                //TODO: Start lose sequence
                return;
            }
        }
        BeginNewRound();
    }
}
