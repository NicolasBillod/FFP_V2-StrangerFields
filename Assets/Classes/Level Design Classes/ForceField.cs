using System;
using UnityEngine;

[Serializable]
public class ForceField
{
    public Vector3 position;
    public float intensity;
    public bool isRepulsive;

    public ForceField(Vector3 thePosition, float theIntensity, bool isRep)
    {
        position = thePosition;
        intensity = theIntensity;
        isRepulsive = isRep;
    }
}
