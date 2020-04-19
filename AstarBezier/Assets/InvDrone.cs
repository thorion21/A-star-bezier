using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InvDrone : MonoBehaviour
{
    private List<RaycastHit2D> rays;
    
    [Range(0.0f, 25.0f)]
    public float radius = 25.0f;

    private int Width, Height, CellSize;
    private Cell[,] grid;
    private Vector2 startPos, endPos;
    private List<Cell> path;
    
    private float MoveSpeed;
    private float Timer;
    private static Vector2 CurrentPositionHolder, startLerpingPosition;
    private int CurrentNode;
    private bool _firstPosition = true;
    private bool _isSet = false;
    
    void Start()
    {
        rays = new List<RaycastHit2D>();
        MoveSpeed = 10.0f;
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
        _isSet = true;
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
            transform.position = GetWorldPosition((int) startPos.x, (int) startPos.y) + new Vector3(CellSize, CellSize) * .5f;
            _firstPosition = false;
        }

        rays.Clear();
        Vector2 position = transform.position;
        
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
                Vector2 possibleCell = ConvertToObjectSpace(rays[i].point);
                Vector2 pozReala = ConvertToObjectSpace(position);            // only used for debug
                // 2. Modificam grid-ul ca ala sa fie not walkable
                int pX = (int) possibleCell.x;
                int pY = (int) possibleCell.y;
                
               if (pX >= Width || pY >= Height || pX < 0 || pY < 0)
                {
                    Debug.Log("Over border with [" + pX + ", " + pY + "] with original " + rays[i].point.x + ", " +
                              rays[i].point.y);
                    continue;
                }
                else if (grid[pX, pY].walkable == false)
                    continue;
               
                Debug.Log("S-a blocat celula: " + pX + " , " + pY + "  vazuta la pozitia: [" + pozReala.x + ", " + pozReala.y + "]");
                
                grid[pX, pY].walkable = false;

                // 3. Recalculam A* din pozitia path[CurrentNode] daca celula de coliziune este la noi in path
                for (int j = CurrentNode; j < path.Count; j++)
                {
                    if (path[j].x == pX && path[j].y == pY)
                    {
                        Vector2 pozitieReala = ConvertToObjectSpace(position);
                        int nowX = (int) pozitieReala.x;
                        int nowY = (int) pozitieReala.y;
                        /* Recalculate A* */
                        startPos = new Vector2(nowX, nowY);
                        CurrentNode = 0;
                        path = Astar();
                        CheckNode();
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
            }
        }
        
    }

    Vector2 ConvertToObjectSpace(Vector2 point)
    {
        Vector2 possibleCell = new Vector2(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y));
        possibleCell += new Vector2(Width, Height) * (CellSize * .5f);
        possibleCell.x = Mathf.FloorToInt(possibleCell.x / CellSize);
        possibleCell.y = Mathf.FloorToInt(possibleCell.y / CellSize);

        return possibleCell;
    }

    List<Cell> Astar()
    {
        Astar solver = new Astar(startPos, endPos, grid, Width, Height);
        List<Cell> path = solver.Process();
                
        string s = "";
        foreach (var cell in solver.Process())
        {
            s += "(" + cell.x + ", " + cell.y + ") ";
        }
        Debug.Log("path: " + s);

        return path;
    }
    
}
























