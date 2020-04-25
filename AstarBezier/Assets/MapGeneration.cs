﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class MapGeneration : MonoBehaviour
{
    private Grid grid;
    public Transform backgroundPos;
    public Camera camera;
    public Sprite sprite;
    private Vector2 objPos, mousePos;
    private float mousePosX, mousePosY;
    private Vector3 localScale;
    
    private int _width = 20;
    private int _height = 20;
    private int _cellSize = 5;

    private List<(int, int)> obstacles;

    void Start()
    {
        grid = new Grid(_width, _height, _cellSize);
        backgroundPos = GetComponent<Transform>();
        objPos = backgroundPos.transform.position;
        localScale = backgroundPos.localScale;
        obstacles = new List<(int, int)>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            mousePos = Input.mousePosition;
            mousePos = camera.ScreenToWorldPoint(mousePos);

            mousePosX = mousePos.x - objPos.x + localScale.x * .5f;
            mousePosY = mousePos.y - objPos.y + localScale.y * .5f;

            Vector2? start = grid.SetStartBlock(mousePosX, mousePosY);

            if (start.HasValue)
            {
                ClearElement("Start block");
                GameObject go = new GameObject("Start block");
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = Color.green;

                go.transform.localScale = new Vector3(_cellSize, _cellSize);
                go.transform.position = GetWorldPosition((int) start.Value.x, (int) start.Value.y);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            mousePos = Input.mousePosition;
            mousePos = camera.ScreenToWorldPoint(mousePos);

            mousePosX = mousePos.x - objPos.x + localScale.x * .5f;
            mousePosY = mousePos.y - objPos.y + localScale.y * .5f;

            Vector2? end = grid.SetEndBlock(mousePosX, mousePosY);

            if (end.HasValue)
            {
                ClearElement("End block");
                GameObject go = new GameObject("End block");
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = Color.yellow;

                go.transform.localScale = new Vector3(_cellSize, _cellSize);
                go.transform.position = GetWorldPosition((int) end.Value.x, (int) end.Value.y);
            }
        }

        if (Input.GetMouseButton(0))
        {
            mousePos = Input.mousePosition;
            mousePos = camera.ScreenToWorldPoint(mousePos);

            mousePosX = mousePos.x - objPos.x + localScale.x * .5f;
            mousePosY = mousePos.y - objPos.y + localScale.y * .5f;

            (int?, int?) coords = grid.PlaceBlock(mousePosX, mousePosY);

            if (coords.Item1.HasValue)
            {
                string obs_name = "Obstacle" + coords.Item1.Value + ", " + coords.Item2.Value;
                obstacles.Add((coords.Item1.Value, coords.Item2.Value));

                GameObject go = new GameObject(obs_name);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = Color.red;

                BoxCollider2D b2d = go.AddComponent<BoxCollider2D>();

                go.transform.localScale = new Vector3(_cellSize, _cellSize) * .85f;
                go.transform.position = GetWorldPosition((int) coords.Item1, (int) coords.Item2);
            }

        }

        if (Input.GetMouseButton(1))
        {
            mousePos = Input.mousePosition;
            mousePos = camera.ScreenToWorldPoint(mousePos);

            mousePosX = mousePos.x - objPos.x + localScale.x * .5f;
            mousePosY = mousePos.y - objPos.y + localScale.y * .5f;

            (int?, int?) coords = grid.DestroyBlock(mousePosX, mousePosY);

            if (coords.Item1.HasValue)
            {
                string obs_name = "Obstacle" + coords.Item1.Value + ", " + coords.Item2.Value;
                obstacles.Remove((coords.Item1.Value, coords.Item2.Value));

                GameObject go = GameObject.Find(obs_name);
                if (go)
                {
                    Destroy(go.gameObject);
                    Debug.Log(obs_name + " has been removed.");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //TODO make obstacles unknown for drone
            foreach (var obs in obstacles)
            {
                GameObject go = GameObject.Find("Obstacle" + obs.Item1 + ", " + obs.Item2);
                if (go)
                {
                    grid.ForceDestroyBlock(obs.Item1, obs.Item2);
                    Destroy(go.gameObject);
                }
            }

            obstacles.Clear();

            float chance = 25;
            var rand = new Random();

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    int nr = rand.Next(101);
                    if (nr <= chance)
                    {
                        grid.ForcePlaceBlock(i, j);
                        string obs_name = "Obstacle" + i + ", " + j;
                        obstacles.Add((i, j));

                        GameObject go = new GameObject(obs_name);
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.sprite = sprite;
                        renderer.color = Color.red;

                        BoxCollider2D b2d = go.AddComponent<BoxCollider2D>();

                        go.transform.localScale = new Vector3(_cellSize, _cellSize) * .85f;
                        go.transform.position = GetWorldPosition(i, j);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (var obs in obstacles)
            {
                GameObject go = GameObject.Find("Obstacle" + obs.Item1 + ", " + obs.Item2);
                if (go)
                {
                    grid.ForceDestroyBlock(obs.Item1, obs.Item2);
                    Destroy(go.gameObject);
                }
            }

            obstacles.Clear();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            
            foreach (var obs in obstacles)
            {
                GameObject go = GameObject.Find("Obstacle" + obs.Item1 + ", " + obs.Item2);
                if (go)
                {
                    grid.ForceDestroyBlock(obs.Item1, obs.Item2);
                    Destroy(go.gameObject);
                }
            }

            obstacles.Clear();
            
            grid.Reset();

            ClearElements("Checkpoint");
            ClearElement("End block");
            ClearElement("Start block");

        }

}
    private void ClearElements(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (var obj in objects)
        {
            if (obj)
                GameObject.Destroy(obj.gameObject);
        }

    }

    private void ClearElement(string tag)
    {
        GameObject go = GameObject.Find(tag);
        if (go)
        {
            Destroy(go.gameObject);
            Debug.Log(tag + " has been removed.");
        }
    }
    
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x - _width / 2, y - _height / 2) * _cellSize + new Vector3(_cellSize, _cellSize) * .5f;
    }
}
