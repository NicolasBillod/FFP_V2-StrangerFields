using UnityEngine;
using System.Collections;

public class Bonus : MonoBehaviour
{
	public enum TYPE {B_Player, B_Damage, M_Earth, M_FoeLife};
	public TYPE type;
	public bool isCollected;
}
