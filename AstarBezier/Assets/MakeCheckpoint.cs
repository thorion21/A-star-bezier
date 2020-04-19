using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeCheckpoint : MonoBehaviour
{
    private GameObject objToSpawn;

    
    private void FixedUpdate()
    {
        objToSpawn = new GameObject("Checkpoint");
        objToSpawn.tag = "Checkpoint";
        objToSpawn.transform.position = this.transform.position;
    }
}
