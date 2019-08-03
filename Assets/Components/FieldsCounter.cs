using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FieldsCounter : MonoBehaviour
{
    public int fieldsRepPlaced;
    public int fieldsAttPlaced;

    public int fieldsRepToPlace;
    public int fieldsAttToPlace;

    public List<Transform> poolRepulsive;
    public List<Transform> poolAttractive;

    public List<Transform> poolRepFFPlaced;
    public List<Transform> poolAttFFPlaced;

    public List<Transform> poolFoes;
}
