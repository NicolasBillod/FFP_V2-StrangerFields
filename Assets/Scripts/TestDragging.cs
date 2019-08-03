using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;

public class TestDragging : MonoBehaviour, IPointerUpHandler
{
    public Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
    public bool _isDragging;

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = _dragFamily.First().GetComponent<IsDragging>().isDragging;
        _isDragging = false;
    }
}
