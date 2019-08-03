using System;
using UnityEngine;

[Serializable]
public class Obstacle
{
    public Vector3 position;
    public Quaternion rotation;
    public bool isBreakable;

    public Obstacle(Vector3 thePosition, Quaternion theRotation, bool theBreakable)
    {
        position = thePosition;
        rotation = theRotation;
        isBreakable = theBreakable;
    }
}
