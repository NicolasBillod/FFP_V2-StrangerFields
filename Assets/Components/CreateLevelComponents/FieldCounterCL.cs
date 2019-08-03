using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldCounterCL : MonoBehaviour
{
    public int fieldsRepPlaced;
    public int fieldsAttPlaced;

    public int fieldsRepToPlace = 5;
    public int fieldsAttToPlace = 5;

    public List<GameObject> poolRepulsiveToPlace;
    public List<GameObject> poolAttractiveToPlace;

    public int fieldsRepPlacedLD;
    public int fieldsAttPlacedLD;

    public int fieldsRepToPlaceLD = 5;
    public int fieldsAttToPlaceLD = 5;

    public List<GameObject> poolRepulsivePlaced;
    public List<GameObject> poolAttractivePlaced;
}
