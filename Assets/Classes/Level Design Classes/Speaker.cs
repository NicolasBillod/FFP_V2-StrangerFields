using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "NewSpeaker", menuName = "Dialogs/Create A New Speaker")]
#endif
public class Speaker : ScriptableObject
{
    public string nameSpeaker;
    public Sprite avatarSpeaker;
}