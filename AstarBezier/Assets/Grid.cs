using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using System;

public class Grid
{
    public int Width, Height, CellSize;
    public Cell[,] grid;
    public Vector2 startPos, endPos;
    public Grid(int width, int height, int cellSize)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        grid = new Cell[width, height];
        startPos = endPos = Vector2.negativeInfinity;
        GenerateGrid();
    }
    
    public void GenerateGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
       //         Utils.CreateWorldText(null, grid[x, y].ToString(), GetWorldPosition(x, y) + new Vector3(CellSize, CellSize) * .5f, 20, Color.white,
       //           TextAnchor.MiddleCenter, TextAlignment.Center, 1);
                
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                grid[x, y] = new Cell(true, x, y);
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
            Debug.Log("StartBlock has been selected " + x + " " + y);
            startPos = new Vector2(x, y);
        }
    }

    public void SetEndBlock(float mouseX, float mouseY)
    {
        ConvertToObjectSpace(mouseX, mouseY, out int x, out int y);
        
        if (x >= 0 && y >= 0 && x < Width && y < Height)
        {
            Debug.Log("EndBlock has been selected " + x + " " + y);
            if (startPos != Vector2.negativeInfinity)
            {
                endPos = new Vector2(x, y);
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                Astar solver = new Astar(startPos, endPos, grid, Width, Height);

                sw.Start();
                List<Cell> path = solver.Process();
                sw.Stop();
                
                string s = "";
                foreach (var cell in solver.Process())
                {
                    s += "(" + cell.x + ", " + cell.y + ") ";
                }
                Debug.Log("path: " + s);
                Debug.Log("Elapsed = " + sw.ElapsedMilliseconds);
            }
                
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
