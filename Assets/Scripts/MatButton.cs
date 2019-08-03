using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Material additive; 

    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponent<Image>().material = additive;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<Image>().material = null;
    }
}
