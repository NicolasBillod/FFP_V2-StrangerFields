using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour {

	[Header("Dialog")]
	[FMODUnity.EventRef]
	public string DialogNextEvent;
	public FMOD.Studio.EventInstance dialogNext;

	[FMODUnity.EventRef]
	public string DialogLetterEvent;
	public FMOD.Studio.EventInstance dialogLetter;

	[Space(5)]
	[Header("Gameplay")]
	[FMODUnity.EventRef]
	public string AttFieldPlacedEvent;
	[FMODUnity.EventRef]
	public string RepFieldPlacedEvent;
	[FMODUnity.EventRef]
	public string RemoveFieldEvent;
	[FMODUnity.EventRef]
	public string MoveCameraEvent;
	[FMODUnity.EventRef]
	public string FireEvent;
	public FMOD.Studio.EventInstance fire;

	[Space(5)]
	[Header("Collision")]
	[FMODUnity.EventRef]
	public string MissileCollisionShipEvent;
	[FMODUnity.EventRef]
	public string MissileCollisionObstacleEvent;
	[FMODUnity.EventRef]
	public string MissileCollisionBreakObstEvent;
	[FMODUnity.EventRef]
	public string EnemyDestroyedEvent;
	[FMODUnity.EventRef]
	public string PlayerDestroyedEvent;

	[Space(5)]
	[Header("UI")]
	[FMODUnity.EventRef]
	public string ButtonEvent;

	[Space(5)]
	[Header("Music")]
	[FMODUnity.EventRef]
	public string MusicGameplayEvent;
	public FMOD.Studio.EventInstance musicGameplay;

	[FMODUnity.EventRef]
	public string MusicWinEvent;
	public FMOD.Studio.EventInstance musicWin;

	[FMODUnity.EventRef]
	public string MusicLoseEvent;
	public FMOD.Studio.EventInstance musicLose;
}
