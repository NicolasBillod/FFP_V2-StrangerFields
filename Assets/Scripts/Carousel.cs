using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Carousel : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    public int nbDisplayedElements = 3;
    public float scaleShift = 0.2f;
    public float borderSize = 50;
    [HideInInspector] public bool positionFeedback = true;
    [HideInInspector] public GameObject selectedDotPrefab;
    [HideInInspector] public GameObject unselectedDotPrefab;
    [HideInInspector] public float dotIntervalSize = 30;

    private int previousNBDisplayed = 3;
    private float previousScaleShift = 0.2f;
    private float previousContentSize;
    private float previousBorderSize;

    private float previousDotIntervalSize;
    private GameObject previousSelectedDotPrefab;
    private GameObject previousUnselectedDotPrefab;

    private ScrollRect scrollRect;
    private List<RectTransform> elemsList;
    private List<RectTransform> dotList;
    private GameObject selectedGO;
    private GameObject selectedDot;
    private int nbDotPrefab = 0;
    private GameObject dotParent;
    private bool selectedChanged = true;
    private bool selectedChangedFromSetter = false;
    private int selectedID = -1;
    private bool dragging = false;
    private bool released = false;
    private float step;
    private RectTransform tmpRectTransform;
    private GameObject tmpGO;


	// ADD BY NICOLAS
	public Sprite middlePop;
	public Sprite sidePop;
	public Sprite middleStarLeft;
	public Sprite middleStarCenter;
	public Sprite middleStarRight;
	public Sprite sideStarLeft;
	public Sprite sideStarCenter;
	public Sprite sideStarRight;

	private Vector3 initialLocalScale;

    public GameObject SelectedGO
    {
        get
        {
            return selectedGO;
        }

        set
        {
            bool inTheList = false;
            int nb = elemsList.Count;
            for (int i = 0; i < nb; i++)
            {
                if (Object.ReferenceEquals(elemsList[i].gameObject, value))
                {
                    selectedID = i;
                    inTheList = true;
                    break;
                }
            }
            if (inTheList)
            {
                selectedChanged = !Object.ReferenceEquals(value, selectedGO);
                selectedChangedFromSetter = selectedChanged;
                //selectedGO.GetComponent<RectTransform>().localScale += Vector3.one * (-scaleShift);
                selectedGO = value;
                //selectedGO.GetComponent<RectTransform>().localScale += Vector3.one * scaleShift;
                StartCoroutine(AdjustScroll((float)selectedID / (elemsList.Count - 1), 5));
            }
            else {
                Debug.Log("Gameobject not in the list.");
            }
        }
    }

    public bool SelectedChanged
    {
        get
        {
            return selectedChanged;
        }
    }

    // Use this for initialization
    void Start ()
    {
        previousScaleShift = scaleShift;
        previousBorderSize = borderSize;
        //previousFeedbackState = positionFeedback;
        scrollRect = this.GetComponent<ScrollRect>();
        elemsList = new List<RectTransform>();
        previousContentSize = scrollRect.content.sizeDelta.x;

        foreach (Transform child in scrollRect.content.gameObject.transform)
        {
            if (child.gameObject)
            {
                //child.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
                elemsList.Add(child.gameObject.GetComponent<RectTransform>());
            }
        }

        //elemsList = elemsList.OrderBy(elem => elem.localPosition.x).ToList();

        int nbElem = elemsList.Count;

        if (nbElem > 2)
        {
            step = elemsList[1].localPosition.x - elemsList[0].localPosition.x - elemsList[0].sizeDelta.x;
        }
        else if(nbElem > 0)
        {
            step = elemsList[0].sizeDelta.x / 5;
        }
        else
        {
            step = 0;
        }

        if(step < 0)
        {
            step = 0;
        }

        if(nbDisplayedElements < 0)
        {
            nbDisplayedElements = 0;
        }
        else if(nbDisplayedElements > nbElem)
        {
            nbDisplayedElements = nbElem;
        }

        if(nbDisplayedElements % 2 == 0)
        {
            if(nbDisplayedElements != 0)
            {
                nbDisplayedElements--;
            }
        }

        previousNBDisplayed = nbDisplayedElements;

        //scrollRect.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(elemsList[0].sizeDelta.x * nbDisplayedElements + step * (nbDisplayedElements-1) + 2 * borderSize, elemsList[0].sizeDelta.y + borderSize * 2);

		initialLocalScale = elemsList [0].localScale;
		Vector3 tmpPos;

        for (int i = 0; i < nbElem; i++)
        {
            scrollRect.horizontalNormalizedPosition = (float)(nbElem - 1 - i) / (nbElem - 1);
            tmpPos = scrollRect.content.GetComponent<RectTransform>().position;
            scrollRect.horizontalNormalizedPosition = 0.5f;
            elemsList[i].localPosition = scrollRect.content.GetComponent<RectTransform>().InverseTransformPoint(tmpPos);
            elemsList[i].localPosition += new Vector3(scrollRect.content.sizeDelta.x / 2, 0, 0);
            elemsList[i].anchoredPosition -= Vector2.up * elemsList[i].anchoredPosition.y;
            //elemsList[i].localScale = Vector3.one * (1-scaleShift);
			SidePanel(elemsList[i].gameObject);
        }

		MiddlePanel(elemsList[0].gameObject);

        scrollRect.horizontalNormalizedPosition = 0;
        selectedID = 0;
        selectedGO = nbElem > 0 ? elemsList[selectedID].gameObject : null;

        if (SelectedGO)
        {
            //selectedGO.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        //OrderSiblings();
        dotParent = new GameObject("PositionFeedback", typeof(RectTransform));
        dotParent.transform.SetParent(scrollRect.transform);
        dotParent.GetComponent<RectTransform>().localPosition = Vector3.down * 100;

        if (positionFeedback)
        {
            Refreshdots();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((dragging || released) && !selectedChangedFromSetter)
        {
            if (selectedGO)
            {
                //selectedGO.GetComponent<RectTransform>().localScale += Vector3.one * (-scaleShift);
            }

            float floatID = scrollRect.horizontalNormalizedPosition * (elemsList.Count-1);
            int id = (int)(floatID) + (int)((floatID % 1) * 2);

            if (id < 0)
            {
                id = 0;
            }
            else if (id > elemsList.Count - 1)
            {
                id = elemsList.Count - 1;
            }

            if(elemsList.Count > 0)
            {
                selectedID = id;
                //Debug.Log("selected : " + selectedGO);
                //Debug.Log("GO : "+ elemsList[id].gameObject);
                selectedChanged = !Object.ReferenceEquals(selectedGO, elemsList[id].gameObject);

				if (selectedChanged)
					SidePanel (selectedGO);

                selectedGO = elemsList[id].gameObject;

				if (selectedChanged)
					MiddlePanel (selectedGO);

                //selectedGO.GetComponent<RectTransform>().localScale += Vector3.one * scaleShift;
            }
            else
            {
                selectedChanged = false;
                selectedGO = null;
            }

            if (selectedChanged)
            {
                //OrderSiblings();

                if (positionFeedback)
                    ChangeSelectedDot();
            }
        }

        if (released)
        {
            if(Mathf.Abs(scrollRect.velocity.x) < 100)
            {
                int nbElem = elemsList.Count;
                for(int i = 0; i < nbElem; i++)
                {
                    if (Object.ReferenceEquals(elemsList[i].gameObject, selectedGO))
                    {
                        StartCoroutine(AdjustScroll((float)i / (nbElem - 1), 2));
                        break;
                    }
                }
                released = false;
            }
        }

        if(dotParent.activeSelf && !positionFeedback)
        {
            dotParent.SetActive(false);
        }
        else if(!dotParent.activeSelf && positionFeedback)
        {
            dotParent.SetActive(true);
            Refreshdots();
        }

        if(previousNBDisplayed != nbDisplayedElements || previousScaleShift != scaleShift || previousContentSize != scrollRect.content.sizeDelta.x || previousBorderSize != borderSize || scrollRect.content.gameObject.transform.childCount != elemsList.Count/* || previousFeedbackState != positionFeedback*/)
        {
            Refresh();
        }

        if(previousDotIntervalSize != dotIntervalSize || !Object.ReferenceEquals(previousSelectedDotPrefab, selectedDotPrefab) || !Object.ReferenceEquals(previousUnselectedDotPrefab, unselectedDotPrefab))
        {
            Refreshdots();
        }
    }

    void OnEnable()
    {
        Refresh();
    }

    private IEnumerator AdjustScroll(float pos, float speed)
    {
        float lerpCount = 0;
        float initialPos = scrollRect.horizontalNormalizedPosition;
        while (lerpCount < 1 && !dragging && (!selectedChanged ||selectedChangedFromSetter))
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(initialPos, pos, lerpCount);
            lerpCount += Time.deltaTime*speed;
            yield return new WaitForEndOfFrame();
        }
        scrollRect.horizontalNormalizedPosition = pos;
        released = false;
        selectedChangedFromSetter = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        released = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
    }

    private void OrderSiblings()
    {
        bool finished;
        for(int i = 0; i < elemsList.Count; i++)
        {
            finished = true;
            if (selectedID - i < elemsList.Count && selectedID - i > -1)
            {
                elemsList[selectedID - i].gameObject.transform.SetAsFirstSibling();
                finished = false;
            }
            if (selectedID + i < elemsList.Count && selectedID + i > -1)
            {
                elemsList[selectedID + i].gameObject.transform.SetAsFirstSibling();
                finished = false;
            }
            if (finished)
            {
                break;
            }
        }
    }

    private void OrderDots()
    {
        bool finished;
        for (int i = 0; i < elemsList.Count; i++)
        {
            finished = true;
            if (selectedID - i < dotList.Count && selectedID - i > -1)
            {
                dotList[selectedID - i].gameObject.transform.SetAsFirstSibling();
                finished = false;
            }
            if (selectedID + i < dotList.Count && selectedID + i > -1)
            {
                dotList[selectedID + i].gameObject.transform.SetAsFirstSibling();
                finished = false;
            }
            if (finished)
            {
                break;
            }
        }
    }

    public void Refresh()
    {
        previousScaleShift = scaleShift;
        previousBorderSize = borderSize;
        scrollRect = this.GetComponent<ScrollRect>();
        elemsList = new List<RectTransform>();
        previousContentSize = scrollRect.content.sizeDelta.x;
        foreach (Transform child in scrollRect.content.gameObject.transform)
        {
            if (child.gameObject)
            {
                //child.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
                elemsList.Add(child.gameObject.GetComponent<RectTransform>());
            }
        }
        elemsList = elemsList.OrderBy(elem => elem.localPosition.x).ToList();
        int nbElem = elemsList.Count;
        if (nbElem > 2)
        {
            step = elemsList[1].localPosition.x - elemsList[0].localPosition.x - elemsList[0].sizeDelta.x;
        }
        else if (nbElem > 0)
        {
            step = elemsList[0].sizeDelta.x / 5;
        }
        else
        {
            step = 0;
        }
        if (step < 0)
        {
            step = 0;
        }
        if (nbDisplayedElements < 0)
        {
            nbDisplayedElements = 0;
        }
        else if (nbDisplayedElements > nbElem)
        {
            nbDisplayedElements = nbElem;
        }
        if (nbDisplayedElements % 2 == 0)
        {
            if (nbDisplayedElements != 0)
            {
                nbDisplayedElements--;
            }
        }
        previousNBDisplayed = nbDisplayedElements;
        //scrollRect.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(elemsList[0].sizeDelta.x * nbDisplayedElements + step * (nbDisplayedElements - 1) + 2 * borderSize, elemsList[0].sizeDelta.y + borderSize * 2);
        Vector3 tmpPos;
        for (int i = 0; i < nbElem; i++)
        {
            scrollRect.horizontalNormalizedPosition = (float)(nbElem - 1 - i) / (nbElem - 1);
            tmpPos = scrollRect.content.GetComponent<RectTransform>().position;
            scrollRect.horizontalNormalizedPosition = 0.5f;
            elemsList[i].localPosition = scrollRect.content.GetComponent<RectTransform>().InverseTransformPoint(tmpPos);
            elemsList[i].localPosition += new Vector3(scrollRect.content.sizeDelta.x / 2, 0, 0);
            elemsList[i].anchoredPosition -= Vector2.up * elemsList[i].anchoredPosition.y;
            //elemsList[i].localScale = Vector3.one * (1 - scaleShift);
        }
        scrollRect.horizontalNormalizedPosition = 0;
        selectedID = 0;
        selectedGO = nbElem > 0 ? elemsList[selectedID].gameObject : null;
        if (selectedGO)
        {
            //selectedGO.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        //OrderSiblings();
        if (positionFeedback)
        {
            Refreshdots();
        }
    }

    //private void MoveContentGO(bool toTheLeft)
    //{
    //    if (toTheLeft)
    //    {
    //        scrollRect.horizontalNormalizedPosition += 1f / (elemsList.Count - 1);
    //        for (int i = 0; i < elemsList.Count - 1; i++)
    //        {
    //            elemsList[i].localPosition = elemsList[i + 1].localPosition;
    //        }
    //        elemsList[elemsList.Count - 1].localPosition = elemsList[0].localPosition;
    //        tmpRectTransform = elemsList[elemsList.Count - 1];
    //        for (int i = elemsList.Count - 1; i > 0; i--)
    //        {
    //            elemsList[i] = elemsList[i - 1];
    //        }
    //        elemsList[0] = tmpRectTransform;
    //        if (selectedID == elemsList.Count - 1)
    //        {
    //            selectedID = 0;
    //        }
    //        else
    //        {
    //            selectedID++;
    //        }
    //    }
    //    else
    //    {
    //        scrollRect.horizontalNormalizedPosition -= 1f / (elemsList.Count - 1);
    //        for(int i = elemsList.Count - 1; i > 0; i--)
    //        {
    //            elemsList[i].localPosition = elemsList[i - 1].localPosition;
    //        }
    //        elemsList[0].localPosition = elemsList[elemsList.Count-1].localPosition;
    //        tmpRectTransform = elemsList[0];
    //        for (int i = 0; i < elemsList.Count-1; i++)
    //        {
    //            elemsList[i] = elemsList[i+1];
    //        }
    //        elemsList[elemsList.Count - 1] = tmpRectTransform;
    //        if(selectedID == 0)
    //        {
    //            selectedID = elemsList.Count - 1;
    //        }
    //        else
    //        {
    //            selectedID--;
    //        }
    //    }
    //}

    private void Refreshdots()
    {
        if(dotList != null)
        {
            for(int i = 0; i < dotList.Count; i++)
            {
                GameObject.Destroy(dotList[i].gameObject);
            }
        }
        dotList = new List<RectTransform>();
        previousDotIntervalSize = dotIntervalSize;
        previousSelectedDotPrefab = selectedDotPrefab;
        previousUnselectedDotPrefab = unselectedDotPrefab;
        if (selectedDotPrefab && unselectedDotPrefab)
        {
            nbDotPrefab = 2;
            for(int i = 0; i < elemsList.Count; i++)
            {
                if(i == 0)
                {
                    tmpGO = GameObject.Instantiate(selectedDotPrefab, dotParent.transform);
                    selectedDot = tmpGO;
                }
                else
                {
                    tmpGO = GameObject.Instantiate(unselectedDotPrefab, dotParent.transform);
                }
                tmpGO.GetComponent<RectTransform>().localPosition = new Vector3((i-((float)(elemsList.Count-1))/2) / elemsList.Count * dotIntervalSize * 10, 0, 0);
                dotList.Add(tmpGO.GetComponent<RectTransform>());
            }
        }
        else if (selectedDotPrefab)
        {
            nbDotPrefab = 1;
            for (int i = 0; i < elemsList.Count; i++)
            {
                tmpGO = GameObject.Instantiate(selectedDotPrefab, dotParent.transform);
                tmpGO.GetComponent<RectTransform>().localPosition = new Vector3((i - ((float)(elemsList.Count - 1)) / 2) / elemsList.Count * dotIntervalSize * 10, 0, 0);
                dotList.Add(tmpGO.GetComponent<RectTransform>());
            }
        }
        else if (unselectedDotPrefab)
        {
            nbDotPrefab = 1;
            for (int i = 0; i < elemsList.Count; i++)
            {
                tmpGO = GameObject.Instantiate(selectedDotPrefab, dotParent.transform);
                tmpGO.GetComponent<RectTransform>().localPosition = new Vector3((i - ((float)(elemsList.Count - 1)) / 2) / elemsList.Count * dotIntervalSize * 10, 0, 0);
                dotList.Add(tmpGO.GetComponent<RectTransform>());
            }
        }
        else
        {
            nbDotPrefab = 0;
        }
        ChangeSelectedDot();
    }

    private void ChangeSelectedDot()
    {
        if(selectedID < dotList.Count)
        {
            if (nbDotPrefab == 2)
            {
                for (int i = 0; i < elemsList.Count; i++)
                {
                    if (Object.ReferenceEquals(dotList[i], selectedDot.GetComponent<RectTransform>()))
                    {
                        tmpRectTransform = dotList[selectedID];
                        dotList[selectedID] = dotList[i];
                        dotList[i] = tmpRectTransform;
                        Vector3 tmpPos = dotList[selectedID].localPosition;
                        dotList[selectedID].localPosition = dotList[i].localPosition;
                        dotList[i].localPosition = tmpPos;
                        break;
                    }
                }
                OrderDots();
            }
            else if (nbDotPrefab == 1)
            {
                for (int i = 0; i < dotList.Count; i++)
                {
                    //dotList[i].localScale = Vector3.one * 0.8f;
                }
                //dotList[selectedID].localScale = Vector3.one;
                OrderDots();
            }
            else
            {
                Debug.Log("No prefab for the position feedback. Add one or disable the feedback.");
            }
        }
    }


	private void MiddlePanel(GameObject go)
    {
		//go.transform.localScale = Vector3.Lerp (go.transform.localScale, new Vector3 (1f, 1f, 1f), Time.deltaTime * 5);
		go.transform.localScale = initialLocalScale;
		go.GetComponent<Image> ().sprite = middlePop;
		go.transform.Find ("Star_Left").GetComponent<Image> ().sprite = middleStarLeft;
		go.transform.Find ("Star_Mid").GetComponent<Image> ().sprite = middleStarCenter;
		go.transform.Find ("Star_Right").GetComponent<Image> ().sprite = middleStarRight;
        go.GetComponent<Button>().enabled = true;

	}

	private void SidePanel(GameObject go){
		//go.transform.localScale = Vector3.Lerp (go.transform.localScale, new Vector3 (0.7f, 0.7f, 0.7f), Time.deltaTime * 5);
		go.transform.localScale = initialLocalScale * 0.7f;
		go.GetComponent<Image> ().sprite = sidePop;
		go.transform.Find ("Star_Left").GetComponent<Image> ().sprite = sideStarLeft;
		go.transform.Find ("Star_Mid").GetComponent<Image> ().sprite = sideStarCenter;
		go.transform.Find ("Star_Right").GetComponent<Image> ().sprite = sideStarRight;
        go.GetComponent<Button>().enabled = false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Carousel))]
public class CarouselEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Carousel myCarousel = target as Carousel;
        myCarousel.positionFeedback = GUILayout.Toggle(myCarousel.positionFeedback, "Position Feedback");

        if (myCarousel.positionFeedback)
        {
            myCarousel.dotIntervalSize = EditorGUILayout.FloatField("Dot Interval Size", myCarousel.dotIntervalSize);
            myCarousel.selectedDotPrefab = EditorGUILayout.ObjectField("Selected Dot Prefab", myCarousel.selectedDotPrefab, typeof(GameObject), true) as GameObject;
            myCarousel.unselectedDotPrefab = EditorGUILayout.ObjectField("Unselected Dot Prefab", myCarousel.unselectedDotPrefab, typeof(GameObject), true) as GameObject;
        }
    }
}
#endif
