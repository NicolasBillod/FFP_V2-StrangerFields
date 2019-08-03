using System;
using UnityEngine;

[Serializable]
public class BonusMalus
{
    public enum TYPE { B_DAMAGE, B_PLAYER, M_EARTH, M_FOELIFE };
    public TYPE type;
    public Vector3 position;

    public BonusMalus(TYPE theType, Vector3 thePosition)
    {
        type = theType;
        position = thePosition;
    }
}
