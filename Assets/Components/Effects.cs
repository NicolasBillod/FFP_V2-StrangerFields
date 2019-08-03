using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
	public List<Bonus.TYPE> current = new List<Bonus.TYPE>();
	public List<Bonus.TYPE> next = new List<Bonus.TYPE>();

	public Effects()
    {
		current = new List<Bonus.TYPE>();
		next = new List<Bonus.TYPE>();
	}
}
