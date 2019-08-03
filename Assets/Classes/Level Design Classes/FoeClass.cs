using System;
using UnityEngine;

[Serializable]
public class FoeClass
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 fireIntensity;
    public float angle;

    public FoeClass(Vector3 thePosition, Quaternion theRotation, Vector3 theFireIntensity, float theAngle)
    {
        position = thePosition;
        rotation = theRotation;
        fireIntensity = theFireIntensity;
        angle = theAngle;
    }
}
