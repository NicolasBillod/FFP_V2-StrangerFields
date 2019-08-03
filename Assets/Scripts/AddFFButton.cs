using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AddFFButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public Material normalMat;
	public Material notDropableMat;
	public bool clicked = false;

	public void OnPointerDown(PointerEventData eventData){
		#if (UNITY_STANDALONE || UNITY_EDITOR)
		clicked = true;
		#else
		if (Input.touches [0].phase == TouchPhase.Began)
			clicked = true;
		#endif
	}

	public void OnPointerUp(PointerEventData eventData){
		clicked = false;
	}

}
