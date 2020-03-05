using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public int Width, Height, CellSize;
    public int[,] grid;
    
    public Grid(int width, int height, int cellSize)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        grid = new int[width, height];
        GenerateGrid();
    }
    
    public void GenerateGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Utils.CreateWorldText(null, grid[x, y].ToString(), GetWorldPosition(x, y) + new Vector3(CellSize, CellSize) * .5f, 20, Color.white,
                    TextAnchor.MiddleCenter, TextAlignment.Center, 1);
                
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
            }
        }
        
        Debug.DrawLine(GetWorldPosition(0, Height), GetWorldPosition(Width, Height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(Width, 0), GetWorldPosition(Width, Height), Color.white, 100f);
    }

    public void SetStartBlock(float mouseX, float mouseY)
    {
        ConvertToObjectSpace(mouseX, mouseY, out int x, out int y);
        
        if (x >= 0 && y >= 0 && x < Width && y < Height)
        {
            Debug.Log("You pressed " + x + " " + y);
        }
    }

    private void ConvertToObjectSpace(float x, float y, out int newX, out int newY)
    {
        newX = Mathf.FloorToInt(x / CellSize);
        newY = Mathf.FloorToInt(y / CellSize);
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x - Width / 2, y - Height / 2) * CellSize;
    }
}
