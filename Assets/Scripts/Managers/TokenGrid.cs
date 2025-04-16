using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TokenGrid : MonoBehaviour
{
    private int rows = 6;
    private int columns = 7;

    public GameObject[] tokenPrefabs = new GameObject[2]; //0 - player token, 1 - AI token
    public Transform[] previewTokens;
    private int[,] grid; //6 rows, 7 columns
    private GameObject[,] tokenObjects;

    private RoundManager roundManager;
    public RoundManager.PlayerType currentPlayer;

    //In the grid there are 3 possible numbers for each index:
    // -1 - cell taken by the AI opponent
    // 0 - empty cell
    // 1 - cell taken by the player

    private void Awake()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
        grid = new int[rows, columns];
        tokenObjects = new GameObject[rows, columns];
    }

    public void AddTokenToColumn(int column, int playerIndex)
    {
        for (int row = 0; row < rows; row++)
        {
            if (grid[row, column] == 0) //If the cell is empty
            {
                //index value:
                // 1 - Player
                // -1 - AI
                grid[row, column] = playerIndex;
                Vector3 startPos = previewTokens[column].position;
                //Get the actual index for the needed prefab, since the index value is different
                int prefabIndex = playerIndex == 1 ? 0 : 1;
                GameObject token = Instantiate(tokenPrefabs[prefabIndex], startPos, Quaternion.identity, transform);
                tokenObjects[row, column] = token;

                //Since theres a delay before the turns switch,
                    //the player can spam a column to spawn tokens as quickly as possible
                //This is prevented using this enum
                currentPlayer = playerIndex == 1 ? RoundManager.PlayerType.AI : RoundManager.PlayerType.Player;
                StartCoroutine(WaitAndRegisterToken(playerIndex));
                DebugMessenger.DebugMessage("Added token to cell: " + row + ", " + column);
                return;
            }
        }
        DebugMessenger.DebugMessage("Column is full");
    }

    private IEnumerator WaitAndRegisterToken(int playerIndex, float waitTime = 1)
    {
        //Done like this, because the tokens are physics-based and
            //it takes them a second to fall down to the target cell
        yield return new WaitForSeconds(waitTime);
        if (CheckWinCondition(playerIndex))
        {
            RoundManager.PlayerType winnerType = playerIndex == 1 ? RoundManager.PlayerType.Player : RoundManager.PlayerType.AI;
            roundManager.WinRound(winnerType);
        }
        else
        {
            roundManager.SwitchTurn();
        }
    }

    public bool IsColumnFull(int column)
    {
        for (int row = 0; row < rows; row++)
        {
            if (grid[row, column] == 0) //If the cell is empty
            {
                return false;
            }
        }
        return true;
    }

    public bool CheckWinCondition(int playerIndex, int[,] customGrid = null)
    {
        int[,] gridToCheck = customGrid ?? grid;
        //Horizontal
        for (int r = 0; r < rows; r++)
            for (int c = 0; c <= columns - 4; c++)
                if (CheckLine(playerIndex, r, c, 0, 1, gridToCheck)) return true;

        //Vertical
        for (int c = 0; c < columns; c++)
            for (int r = 0; r <= rows - 4; r++)
                if (CheckLine(playerIndex, r, c, 1, 0, gridToCheck)) return true;

        //Diagonal /
        for (int r = 0; r <= rows - 4; r++)
            for (int c = 0; c <= columns - 4; c++)
                if (CheckLine(playerIndex, r, c, 1, 1, gridToCheck)) return true;

        //Diagonal \
        for (int r = 3; r < rows; r++)
            for (int c = 0; c <= columns - 4; c++)
                if (CheckLine(playerIndex, r, c, -1, 1, gridToCheck)) return true;

        return false;
    }

    private bool CheckLine(int playerIndex, int row, int col, int deltaRow, int deltaCol, int[,] customGrid = null)
    {
        int[,] gridToCheck = customGrid ?? grid;
        for (int i = 0; i < 4; i++)
        {
            if (gridToCheck[row + i * deltaRow, col + i * deltaCol] != playerIndex)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                grid[r, c] = 0;
                if (tokenObjects[r, c] != null)
                {
                    Destroy(tokenObjects[r, c]);
                    tokenObjects[r, c] = null;
                }
            }
        }
    }

    //Methods which make the AI's decision making easier
    public int[,] GetGridCopy()
    {
        return (int[,])grid.Clone();
    }

    public bool AddSimulatedTokenToColumn(int[,] simulatedGrid, int column, int player)
    {
        for (int row = 0; row < rows; row++)
        {
            if (simulatedGrid[row, column] == 0)
            {
                simulatedGrid[row, column] = player;
                return true;
            }
        }
        return false;
    }

    public List<int> GetValidColumns(int[,] gridState = null)
    {
        //Check all columns in the grid (either simulated or the real one, depends on if gridState is null),
        //and make a list of all valid column indices
        int[,] gridToUse = gridState ?? grid;
        List<int> valid = new List<int>();
        for (int col = 0; col < columns; col++)
        {
            if (gridToUse[rows - 1, col] == 0)
                valid.Add(col);
        }
        return valid;
    }

}
