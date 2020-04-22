using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bezier
{
    private static float Fact(int n)
    {
        if (n == 0 || n == 1)
            return 1;

        return n * Fact(n - 1);
    }
    
    private static float Bernstein(int i, int n, float t)
    {
        return Fact(n) / (Fact(i) * Fact(n - i)) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }
    
    public static Vector3 Apply(List<Cell> points, float t)
    {
        Vector3 res = Vector3.zero;
        int n = points.Count - 1;

        for (int i = 0; i <= n; i++)
        {
            res.x += points[i].worldPos.x * Bernstein(i, n, t);
            res.y += points[i].worldPos.y * Bernstein(i, n, t);
        }

        return res;
    }
}