using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIOpponent : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    //Difficulty will change as the game goes on
    //Rounds 1, 2 - Easy
    //Rounds 3, 4 - Medium
    //Round 5 - Hard

    //So the more the player loses, the harder the game will become

    private RoundManager roundManager;
    private TokenGrid tokenGrid;
    public Difficulty difficulty;
    private int searchDepth = 5; //Used in a recursive method, be careful with high values

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
        if (roundManager.currentRound <= 2)
        {
            difficulty = Difficulty.Easy;
        }
        else if (roundManager.currentRound <= 4)
        {
            difficulty = Difficulty.Medium;
        }
        else
        {
            difficulty = Difficulty.Hard;
        }
    }

    private void OnAITurn()
    {
        if (roundManager.currentPlayerTurn != RoundManager.PlayerType.AI)
        {
            //Just in case
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
                return GetRandomMove(simulatedGrid);

            case Difficulty.Medium:
                return GetMediumMove(simulatedGrid);

            case Difficulty.Hard:
                return GetBestMove(simulatedGrid, searchDepth);

            default:
                return GetRandomMove(simulatedGrid);
        }
    }

    private int GetRandomMove(int[,] grid)
    {
        List<int> validMoves = tokenGrid.GetValidColumns(grid);
        return validMoves[Random.Range(0, validMoves.Count)];
    }

    private int GetMediumMove(int[,] grid)
    {
        //Make a copy of the board and simulate the AI's next move
        //If it would win the round, the AI makes that move
        foreach (int col in tokenGrid.GetValidColumns(grid))
        {
            int[,] temp = (int[,])grid.Clone();
            tokenGrid.AddSimulatedTokenToColumn(temp, col, -1);
            if (tokenGrid.CheckWinCondition(-1)) return col;
        }

        //If the player would win the game,
        //Add a token to the spot where it would be blocked
        foreach (int col in tokenGrid.GetValidColumns(grid))
        {
            int[,] temp = (int[,])grid.Clone();
            tokenGrid.AddSimulatedTokenToColumn(temp, col, 1);
            if (tokenGrid.CheckWinCondition(1)) return col;
        }

        //If there are no such cases (such as when the game has just started),
            //add a token at random
        return GetRandomMove(grid);
    }

    private int GetBestMove(int[,] board, int depth)
    {
        //Get the best move in the current situation,
            //taking into account multiple future moves
        //This method uses the Minimax algorithm,
            //which simulates the aforementioned future moves and picks the best one
        //Depth in this case means the number of future moves the AI will look into
            //alternating between itself and the player

        int bestScore = int.MinValue;
        int bestCol = -1;
        
        foreach (int col in tokenGrid.GetValidColumns(board))
        {
            int[,] temp = (int[,])board.Clone();
            tokenGrid.AddSimulatedTokenToColumn(temp, col, -1);

            int score = Minimax(temp, depth - 1, false, int.MinValue, int.MaxValue);
            if (score > bestScore)
            {
                bestScore = score;
                bestCol = col;
            }
        }

        //If the AI has found a valid move at all, use that
        //If not, get a random move
        return bestCol != -1 ? bestCol : GetRandomMove(board);
    }

    private int Minimax(int[,] board, int depth, bool isMax, int alpha, int beta)
    {
        //isMax is true if the AI is looking into its own future moves
            //and false if it's looking into the player's future moves
            //Used to maximize its own and minimize the player's score
        //alpha and beta values are used to prevent the AI from evaluating
            //worse outcomes than ones which have already been found
            //used for optimization

        if (tokenGrid.CheckWinCondition(-1))
        {
            //If the AI has found a winning move for itself, return a high positive score
            return 1000 + depth;
        }
        if (tokenGrid.CheckWinCondition(1))
        {
            //If the AI has found a winning move for the player, return a high negative score
            return -1000 - depth;
        }
        if (depth == 0 || tokenGrid.GetValidColumns(board).Count == 0)
        {
            return EvaluateBoard(board);
        }

        int best = isMax ? int.MinValue : int.MaxValue;

        foreach (int col in tokenGrid.GetValidColumns(board))
        {
            int[,] temp = (int[,])board.Clone();
            tokenGrid.AddSimulatedTokenToColumn(temp, col, isMax ? -1 : 1);
            int score = Minimax(temp, depth - 1, !isMax, alpha, beta);

            if (isMax)
            {
                best = Mathf.Max(best, score);
                alpha = Mathf.Max(alpha, best);
            }
            else
            {
                best = Mathf.Min(best, score);
                beta = Mathf.Min(beta, best);
            }

            if (beta <= alpha) break;
        }

        return best;
    }

    private int EvaluateBoard(int[,] board)
    {
        return ScorePosition(board, -1) - ScorePosition(board, 1);
    }

    private int ScorePosition(int[,] board, int player)
    {
        int score = 0;

        for (int r = 0; r < board.GetLength(0); r++)
            for (int c = 0; c < board.GetLength(1); c++)
                score += CountNearbyTokens(board, r, c, player);

        return score;
    }

    private int CountNearbyTokens(int[,] board, int r, int c, int player)
    {
        int score = 0;
        if (board[r, c] != player) return 0;

        // Simplified scoring logic
        if (r + 1 < board.GetLength(0) && board[r + 1, c] == player) score++;
        if (c + 1 < board.GetLength(1) && board[r, c + 1] == player) score++;
        if (r + 1 < board.GetLength(0) && c + 1 < board.GetLength(1) && board[r + 1, c + 1] == player) score++;
        if (r - 1 >= 0 && c + 1 < board.GetLength(1) && board[r - 1, c + 1] == player) score++;

        return score;
    }
}
