using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TokenGrid : MonoBehaviour
{
    private int rows = 6;
    private int columns = 7;
    private int cellSize = 1;

    //Used to set the starting position for the drop animation when a token is spawned on a column
    [SerializeField] private Transform tokenStartPos;
    public GameObject[] tokenPrefabs = new GameObject[2]; //0 - player token, 1 - enemy token
    private int[,] grid; //6 rows, 7 columns
    private GameObject[,] tokenObjects;

    //In the grid there are 3 possible numbers for each index:
    // -1 - cell taken by the AI opponent
    // 0 - empty cell
    // 1 - cell taken by the player

    private void Awake()
    {
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
                // -1 - AI
                // 1 - Player
                grid[row, column] = playerIndex;
                Vector3 startPos = tokenStartPos.position + new Vector3(column * cellSize, rows * cellSize, 0f);
                Vector3 endPos = tokenStartPos.position + new Vector3(column * cellSize, row * cellSize, 0f);
                GameObject token = Instantiate(tokenPrefabs[playerIndex], startPos, Quaternion.identity, transform);
                tokenObjects[row, column] = token;

                StartCoroutine(MoveTokenToCell(token.transform, startPos, endPos));
                DebugMessenger.DebugMessage("Added token to cell: " + row + ", " + column);
                return;
            }
        }
        DebugMessenger.DebugMessage("Column is full");
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

    private IEnumerator MoveTokenToCell(Transform token, Vector3 start, Vector3 end)
    {
        float elapsedTime = 0f;
        float duration = Vector3.Distance(start, end);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            token.position = Vector3.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }

        token.position = end;
    }

    public bool CheckWinCondition(int playerIndex)
    {
        //Horizontal
        for (int r = 0; r < rows; r++)
            for (int c = 0; c <= columns - 4; c++)
                if (CheckLine(playerIndex, r, c, 0, 1)) return true;

        //Vertical
        for (int c = 0; c < columns; c++)
            for (int r = 0; r <= rows - 4; r++)
                if (CheckLine(playerIndex, r, c, 1, 0)) return true;

        //Diagonal /
        for (int r = 0; r <= rows - 4; r++)
            for (int c = 0; c <= columns - 4; c++)
                if (CheckLine(playerIndex, r, c, 1, 1)) return true;

        //Diagonal \
        for (int r = 3; r < rows; r++)
            for (int c = 0; c <= columns - 4; c++)
                if (CheckLine(playerIndex, r, c, -1, 1)) return true;

        return false;
    }

    private bool CheckLine(int playerIndex, int row, int col, int deltaRow, int deltaCol)
    {
        for (int i = 0; i < 4; i++)
        {
            if (grid[row + i * deltaRow, col + i * deltaCol] != playerIndex)
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
