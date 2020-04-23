﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;

public class InvDrone : MonoBehaviour
{
    private List<RaycastHit2D> rays;
    
    [Range(0.0f, 25.0f)]
    public float radius = 25.0f;

    private int Width, Height, CellSize;
    private Cell[,] grid;
    private Vector2 startPos, endPos;
    
    private List<Cell> path;
    private List<Cell> rawAstarPath;
    private LineRenderer lineRenderer;
    private Vector3[] intermediatePath;
    
    private float MoveSpeed;
    private float Timer;
    
    private static Vector2 CurrentPositionHolder, startLerpingPosition;
    private int CurrentNode;
    
    public bool _firstPosition = true;
    private bool _isSet = false;
    private int recalculateCount;


    void Start()
    {
        rays = new List<RaycastHit2D>();
        MoveSpeed = 50.0f;
    }

    public void Set(Vector2 startPos, Vector2 endPos, Cell[,] grid, int Width, int Height, int CellSize)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.grid = grid;
        this.Width = Width;
        this.Height = Height;
        this.CellSize = CellSize;
        
        path = Astar();
        CheckNode();
        lineRenderer = GetComponent<LineRenderer>();
        _isSet = true;
        recalculateCount = 0;
        
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x - Width / 2, y - Height / 2) * CellSize;
    }

    private Vector2 GetDirectionVector(float radians)
    {
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    void CheckNode()
    {
        Timer = 0;
        CurrentPositionHolder = path[CurrentNode].worldPos;
        startLerpingPosition = transform.position;
    }

    void Update()
    {
        if (_firstPosition && _isSet)
        {
            transform.position = GetWorldPosition((int) startPos.x, (int) startPos.y) +
                                 new Vector3(CellSize, CellSize) * .5f;
            _firstPosition = false;
        }

        rays.Clear();
        Vector2 position = transform.position;
        
        if (_isSet)
        {
            if (intermediatePath[path.Count - 1].x != position.x || intermediatePath[path.Count - 1].y != position.y)
            {
                lineRenderer.positionCount = path.Count;
                lineRenderer.SetPositions(intermediatePath);
                lineRenderer.enabled = true;
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }

        for (int i = 0; i < 36; i += 1)
        {
            float rad = i * 10.0f * Mathf.Deg2Rad;
            Vector2 dir = GetDirectionVector(rad);
            
            rays.Add(
                Physics2D.Raycast(
                    position,
                    dir,
                    radius
                )
            );
            
            if (rays[i].collider != null)
            {
                Debug.DrawRay(position, dir * radius, Color.red);
                
                /* Recalculate Astar */
                
                // 1. Gasim celula care e ocupata
                
                // Corner cases
                // When the Ray hits a border / corner
                float hitX = rays[i].point.x;
                float hitY = rays[i].point.y;
                bool hasHitBorderX = false;
                bool hasHitBorderY = false;
                
                if (FloatEqual(hitX, Math.Round(hitX)))
                {
                    if (position.x < hitX)
                        hitX += CellSize * .5f;
                    else
                        hitX -= CellSize * .5f;
                    hasHitBorderX = true;
                }
                else
                {
                    hasHitBorderX = false;
                }

                if (FloatEqual(hitY, Math.Round(hitY)))
                {
                    if (position.y < hitY)
                        hitY += CellSize * .5f;
                    else
                        hitY -= CellSize * .5f;
                    hasHitBorderY = true;
                }
                else
                {
                    hasHitBorderY = false;
                }

                // hitting corner gives no actual information
                if (hasHitBorderX && hasHitBorderY)
                {
                    //Debug.Log("HIT THE CORNER... [" + rays[i].point.x + ", " + rays[i].point.y + "] ");
                    continue;
                }

                Vector2 possibleCell = ConvertToObjectSpace(new Vector2(hitX, hitY));
                Vector2 pozReala = ConvertToObjectSpace(position);            // only used for debug
                
                // 2. Modificam grid-ul ca ala sa fie not walkable
                int pX = (int) possibleCell.x;
                int pY = (int) possibleCell.y;
                

                if (pX >= Width || pY >= Height || pX < 0 || pY < 0)
                {
                    Debug.Log("Over border with [" + pX + ", " + pY + "] with original " + hitX + ", " +
                              hitY);
                    continue;
                }
                else if (grid[pX, pY].walkable == false)
                    continue;

               Debug.Log("S-a blocat celula: " + pX + " , " + pY + "  vazuta la pozitia: [" + pozReala.x + ", " + pozReala.y + "] with original " + hitX + ", " +
                         hitY);
               
               /*
               if (hasHitBorderX)
                   Debug.Log("Margine pe X " + rays[i].point.x + " iar pozitia acutala e " + position.x + " diferenta " + (rays[i].point.x - position.x));
               if (hasHitBorderY)
                   Debug.Log("Margine pe Y" + rays[i].point.y + " iar pozitia acutala e " + position.y + " diferenta " + (rays[i].point.y - position.y));
                */
               
               grid[pX, pY].walkable = false;

                // 3. Recalculam A* din pozitia path[CurrentNode] daca celula de coliziune este la noi in path
                
                for (int j = 0; j < path.Count; j++)
                {
                    if ((j < rawAstarPath.Count && (rawAstarPath[j].x == pX && rawAstarPath[j].y == pY)) ||
                        path[j].x == pX && path[j].y == pY)
                    {
                        Vector2 pozitieReala = ConvertToObjectSpace(position);
                        int nowX = (int) pozitieReala.x;
                        int nowY = (int) pozitieReala.y;
                        /* Recalculate A* */
                        startPos = new Vector2(nowX, nowY);
                        CurrentNode = 0;   
                        path = Astar();
                        CheckNode();
                        recalculateCount++;
                        break;
                    }
                }
                
                // 4. Continuam animatia dupa noul path si ca sa facem asta, resetam tot ce inseamna drum


            }
            else
            {
                Debug.DrawRay(position, dir * radius, Color.green);
            }
        }
        
        if (_isSet)
        {
            /* Move */
            Timer += Time.deltaTime * MoveSpeed;

            if (position != CurrentPositionHolder)
            {
                transform.position = Vector2.Lerp(startLerpingPosition, CurrentPositionHolder, Timer);
            }
            else
            {
                if (CurrentNode < path.Count - 1)
                {
                    CurrentNode++;
                    CheckNode();
                }
                else if (CurrentNode == path.Count - 1)
                {
                    Debug.Log("Drumul a fost recalculat de " + recalculateCount + " ori!");
                    CurrentNode++;
                }
            }
        }
        
    }

    bool FloatEqual(double f1, double f2)
    {
        return Math.Abs(f1 - f2) < .001f;
    }

    Vector2 ConvertToObjectSpace(Vector2 point)
    {
        Vector2 possibleCell = new Vector2(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y));
        possibleCell += new Vector2(Width, Height) * (CellSize * .5f);
        possibleCell.x = Mathf.FloorToInt(possibleCell.x / CellSize);
        possibleCell.y = Mathf.FloorToInt(possibleCell.y / CellSize);

        return possibleCell;
    }

    Vector3[] ConvertCellsToVector3(List<Cell> list)
    {
        Vector3[] path = new Vector3[list.Count];
        int i = 0;
        foreach (var cell in list)
        {
            path[i] = GetWorldPosition(cell.x, cell.y) + new Vector3(CellSize, CellSize) * .5f;
            i++;
        }
        return path;
    }

    List<Cell> ConvertVector3ToCell(Vector3[] bezPath)
    {
        List<Cell> newPath = new List<Cell>();
        foreach (var point in bezPath)
        {
            Vector2 objPoz = ConvertToObjectSpace(new Vector2(point.x, point.y));
            newPath.Add(new Cell(true, (int)objPoz.x , (int)objPoz.y , point));   
        }
        return newPath;
    }

    List<Cell> CheckingCorners(List<Cell> oldPath)
    {
        List<Cell> newPath = new List<Cell>();
        newPath.Add(oldPath[0]);
        for (int i = 0; i < oldPath.Count - 2; i++)
        {
            Cell first = oldPath[i];
            Cell second = oldPath[i + 1];
            Cell third = oldPath[i + 2];
            if ((first.x == second.x && first.x == third.x) || (first.y == second.y && first.y == third.y))
            {
                newPath.Add(second);
            }
            else
            {
                // the 3 points make a corner and we need to check if the forth (inside one) is an obstacle
                Vector2 posibleObstacle = new Vector2();
                float offsetX = 0;
                float offsetY = 0;
                if (second.x == first.x)
                {
                    posibleObstacle.x = third.x;
                }
                else
                {
                    posibleObstacle.x = first.x;
                }

                if (second.y == first.y)
                {
                    posibleObstacle.y = third.y;
                }
                else
                {
                    posibleObstacle.y = first.y;
                }
                
                if (grid[(int)posibleObstacle.x, (int)posibleObstacle.y].walkable == false)
                {
                    Debug.Log("Am gasit un colt la pozitia : [" + posibleObstacle.x + ", " + posibleObstacle.y + "] ");

                    if (posibleObstacle.x > second.x)
                        offsetX -= CellSize * .5f;
                    else
                        offsetX += CellSize * .5f;

                    if (posibleObstacle.y > second.y)
                        offsetY -= CellSize * .5f;
                    else
                        offsetY += CellSize * .5f;
                }
                Vector3 cornerPos = new Vector3(second.worldPos.x + offsetX, second.worldPos.y + offsetY);
                Vector3 beforeCorner = cornerPos;
                Vector3 afterCorner = cornerPos;
                
                if (first.y == second.y)
                {
                    beforeCorner.x -= offsetX;
                    afterCorner.y -= offsetY;
                }
                else
                {
                    beforeCorner.y -= offsetY;
                    afterCorner.x -= offsetX;
                }
                newPath.Add(new Cell(true, second.x, second.y, beforeCorner));
                newPath.Add(new Cell(true, second.x, second.y, cornerPos));
                newPath.Add(new Cell(true, second.x, second.y, afterCorner));
            }
        }
        newPath.Add(oldPath[oldPath.Count - 1]);
        return newPath;
    }
    
    private float GetEuclidianDistance(Vector2 v1, Vector2 v2)
    {
        float x = v1.x - v2.x;
        float y = v1.y - v2.y;
        return Mathf.Sqrt(x * x + y * y);
    }

    List<Cell> Astar()
    {
        Astar solver = new Astar(startPos, endPos, grid, Width, Height);
        List<Cell> path = solver.Process();
                
        string s = "";
        foreach (var cell in path)
        {
            s += "(" + cell.x + ", " + cell.y + ") ";
        }
        Debug.Log("path: " + s);
        
        /*
        // No Bezier Version
        intermediatePath = ConvertCellsToVector3(path);
        return path;
        */
        
        // Raw A* made for object space
        // TODO make a line/blank object follow this path for visual purposes
        rawAstarPath = path;
        
        // Checking if there are corners near an obstacle and moving the corner point by Cellsize / 2 away on x and y
        path = CheckingCorners(rawAstarPath);
        
        // Restart from same position
        if (_isSet)
            path[0].worldPos = transform.position;

        // Aplying Bezier
	    float tLength = 0;
        for (int i = 0; i < path.Count - 1; i++)
            tLength += GetEuclidianDistance(path[i].worldPos, path[i + 1].worldPos);

        float step = 1 / tLength;

        List<Vector3> newPath = new List<Vector3>();

        for (float t = 0.0f; t <= 1.0f; t += step)
        {
            newPath.Add(Bezier.Apply(path, t));
        }

        // LineReader needs an array
        intermediatePath = newPath.ToArray();
        
        // Make Cells to have both object and world coordinates
        path = ConvertVector3ToCell(intermediatePath);
        
        return path;
    }
    
}
