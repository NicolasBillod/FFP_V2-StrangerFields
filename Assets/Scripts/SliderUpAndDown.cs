using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderUpAndDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public bool clickedHandled;

	public SliderUpAndDown(){
		clickedHandled = true;
	}

	public void OnPointerDown(PointerEventData eventData){
		clickedHandled = false;
	}

	public void OnPointerUp(PointerEventData eventData){
	}
		
}
