using System;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public enum PlayerType
    {
        AI,
        Player,
        None
    }

    //Using event here cause only this script should invoke these events
    public event Action OnRoundBegin;
    public event Action OnTurnSwitch;

    [Header("Game Rules")]
    [SerializeField] private int roundsNeededToWin = 3;
    [SerializeField] private PlayerType firstPlayerToAct = PlayerType.Player;

    [Header("Game Parameters")]
    [SerializeField] private TextMeshPro[] playerScores = new TextMeshPro[2]; //0 - AI, 1 - Player
    public PlayerType currentPlayerTurn;
    public int[] roundsWon = new int[2]; //0 - rounds won by the AI, 1 - rounds won by the player
    public int currentRound = 0;
    public bool isGameOver = false;

    public bool hasGameStarted = false;

    [SerializeField] private AudioSource winCounterAS;

    private TokenGrid tokenGrid;
    private Revolver revolver;
    private VentMonster ventMonster;

    private void Awake()
    {
        //Get references before any game logic
        tokenGrid = FindFirstObjectByType<TokenGrid>();
        revolver = FindFirstObjectByType<Revolver>();
        ventMonster = FindFirstObjectByType<VentMonster>();
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
        if ((ventMonster.currentAction == VentMonster.Action.Wait ||
            ventMonster.currentAction == VentMonster.Action.Spawn) &&
            revolver.bulletCount < revolver.defaultBulletCount / 2)
        {
            //If the monster is not active AND the player is low on ammo,
                //reset it at the start of the round so it doesn't instantly attack the player
            //If it's waiting on round end this will also make the player anxious
                //cause he will have heard the growl and will expect it to attack
            ventMonster.PrepareToSpawn();
        }
        
    }

    public void UpdateScores(int playerIndex)
    {
        playerScores[playerIndex].text += "|";
        winCounterAS.Play();
    }

    public void CompleteRound(PlayerType winner)
    {
        int playerIndex = winner == PlayerType.Player ? 1 : winner == PlayerType.AI ? 0 : -1;
        if (playerIndex == -1)
        {
            AIVoiceLines.SayLine(AIVoiceLines.gameTie, 3);
            BeginNewRound();
            return;
        }
        bool hasPlayerWon = playerIndex == 1;
        string playerName = playerIndex == 1 ? "Player" : "AI";
        string roundWinLine = playerIndex == 1 ? AIVoiceLines.roundWonPlayer : AIVoiceLines.roundWonAI;
        string gameWinLine = playerIndex == 1 ? AIVoiceLines.gameWonPlayer1 : AIVoiceLines.gameWonAI;
        
        //Make sure the loser acts first next round
        firstPlayerToAct = winner == PlayerType.Player ? PlayerType.AI : PlayerType.Player;      
        
        roundsWon[playerIndex]++;
        if (roundsWon[playerIndex] >= roundsNeededToWin)
        {
            isGameOver = true;
            AIVoiceLines.SayLine(gameWinLine, 3);

            if (playerIndex == 1)
            {
                AnimationSequencer.PlayerWinSequence();
            }
            else
            {
                AnimationSequencer.PlayerDeathSequence();
            }
            return;
        }

        AnimationSequencer.RoundChangeSequence(playerIndex);

        AIVoiceLines.SayLine(roundWinLine, 3);
    }
}
