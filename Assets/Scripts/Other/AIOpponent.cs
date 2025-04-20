using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
    public enum Difficulty
    {
        Easy, //Random moves, but if it gets the opportunity to win, it will
        Hard //Same as Easy, but it can also put tokens on the grid to block the player if he's about to win
    }

    //Difficulty will change as the game goes on
    //Rounds 1, 2 - Easy
    //Round 3, 4, 5 - Hard

    //So the longer the game goes, the harder it will become,
    //there will still be at least 1 round in which the AI's difficuly is Hard

    private RoundManager roundManager;
    private TokenGrid tokenGrid;
    [SerializeField] private Difficulty difficulty;

    private bool hasSaidDifficultyLine = false;

    //The time it takes for the AI to make a move when it's its turn
    [SerializeField] private float minWaitTime = 0.5f;
    [SerializeField] private float maxWaitTime = 1.5f;

    private void Awake()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
        tokenGrid = FindFirstObjectByType<TokenGrid>();

        roundManager.OnRoundBegin += OnNewRound;
        roundManager.OnTurnSwitch += OnAITurn;
    }

    private void OnNewRound()
    {
        //For the first 2 rounds, the difficulty will be Easy, after that, it will be set to Hard
        difficulty = roundManager.currentRound <= 2 ? Difficulty.Easy : Difficulty.Hard;
        if (difficulty == Difficulty.Hard && !hasSaidDifficultyLine)
        {
            hasSaidDifficultyLine = true;
            AIVoiceLines.SayLine(AIVoiceLines.difficultySwitch, 3);
        }
        DebugMessenger.DebugMessage("AI's difficulty has changed to " + difficulty.ToString());
    }

    private void OnAITurn()
    {
        if (roundManager.currentPlayerTurn != RoundManager.PlayerType.AI || roundManager.isGameOver)
        {
            //If it's not the AI's turn or the game is over, don't act
            return;
        }
        StartCoroutine(WaitAndAct());
    }

    private IEnumerator WaitAndAct()
    {
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);
        int aiMove = GetAIMove();
        //aiMove is the column chosen by the AI, -1 is the AI's player index
        tokenGrid.AddTokenToColumn(aiMove, -1);
    }

    private int GetAIMove()
    {
        int[,] simulatedGrid = tokenGrid.GetGridCopy();

        switch (difficulty)
        {
            case Difficulty.Easy:
                return GetEasyMove(simulatedGrid);

            case Difficulty.Hard:
                return GetHardMove(simulatedGrid);

            default:
                return GetRandomMove(simulatedGrid);
        }
    }

    private int GetEasyMove(int[,] grid)
    {
        //Make a copy of the board and simulate the AI's next move
        //If it would win the round, the AI makes that move
        int winningColumn = SpecialMove(grid, -1);

        if (winningColumn != -1)
        {
            return winningColumn;
        }

        //If there are no such cases (such as when the game has just started),
        //add a token at random
        return GetRandomMove(grid);
    }

    private int GetRandomMove(int[,] grid)
    {
        DebugMessenger.DebugMessage("Random Move");
        List<int> validMoves = tokenGrid.GetValidColumns(grid);
        return validMoves[Random.Range(0, validMoves.Count)];
    }

    private int GetHardMove(int[,] grid)
    {
        //Same this as Easy Move, but this also checks if the player would win

        int winningColumn = SpecialMove(grid, -1);

        if (winningColumn != -1)
        {
            return winningColumn;
        }

        int blockingColumn = SpecialMove(grid, 1);

        if (blockingColumn != -1)
        {
            return blockingColumn;
        }

        //If there are no such cases (such as when the game has just started),
        //add a token at random
        DebugMessenger.DebugMessage("Hard move impossible");
        return GetRandomMove(grid);
    }

    private int SpecialMove(int[,] grid, int playerIndex)
    {
        //playerIndex:
            // -1 - AI
            // 1 - Player

        //Make a copy of the board and simulate the AI's next move
        //If the AI would win the round and is checking for that, the AI makes that move
        //If it's checking if the player would win, it will block the player's winning move
        foreach (int col in tokenGrid.GetValidColumns(grid))
        {
            int[,] temp = (int[,])grid.Clone();
            tokenGrid.AddSimulatedTokenToColumn(temp, col, playerIndex);
            if (tokenGrid.CheckWinCondition(playerIndex, temp))
            {
                string debugText = playerIndex == -1 ? "Winning Move" : "Blocking Move";
                DebugMessenger.DebugMessage(debugText);
                if (playerIndex == 1)
                {
                    int voiceLineChance = Random.Range(0, 3);
                    //30% chance to laugh when blocking the player
                    if (voiceLineChance == 0)
                    {
                        AIVoiceLines.SayLine(AIVoiceLines.blockPlayer, 1);
                    }
                }
                return col;
            }
        }
        //If no such move has been found, return an invalid number
        return -1;
    }
}
