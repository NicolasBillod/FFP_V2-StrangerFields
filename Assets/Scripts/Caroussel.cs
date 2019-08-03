using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Caroussel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	// Panel into which we will instantiate the scroll dots images (needs to be after in hierarchy, so it can show over this panel)
	public GameObject carousselScrollPanel;

	// Currently, the prefab of a Theme Du Mois
	public GameObject prefab;
	public int numberOfThemes;
	public List<RectTransform> listOfInstances;


	// Currently, the image for the scroll dots
	public GameObject imageScrollPrefab;
	public int spaceBetweenScrollImages = 15;
	public Color baseColor;
	public Color selectedColor;
	private RectTransform[] listOfScrollImages;

	public Sprite middlePop;
	public Sprite sidePop;
	public Sprite middleStarLeft;
	public Sprite middleStarCenter;
	public Sprite middleStarRight;
	public Sprite sideStarLeft;
	public Sprite sideStarCenter;
	public Sprite sideStarRight;

	private bool isHovering = false; // Is the cursor hovering over this panel
	private bool canSwipe = false;
	private float mousePositionStartX;
	private float mousePositionEndX;
	private float dragDistance;
	private float swipeThreshold = 200; // distance to drag before it changes the theme displayed
	private float currentDecalage;
	private float previousDecalage;

	private float wide; // of this panel
	private float height; // of this panel

	private bool shouldResetDecalage;
	private bool isCoroutineOver;

	public int indexList;
	public int indexImages;
	private int state; // to left = -1 ; stay = 0 ; right = 1

	private string[] titles;
	private int[] scores;

	// Initialization
	void Start () {
		titles = new string[]{ "level1", "level2", "level3", "level4"};
		scores = new int[]{ 1, 2, 3, 4};
		listOfInstances = new List<RectTransform> ();
		//wide = this.GetComponent<RectTransform> ().rect.width;
		//height = this.GetComponent<RectTransform> ().rect.height;
		wide = prefab.GetComponent<RectTransform>().rect.width;
		height = prefab.GetComponent<RectTransform> ().rect.height;

		shouldResetDecalage = false;
		isCoroutineOver = true;
		GameObject go;
		listOfScrollImages = new RectTransform[numberOfThemes];

		indexList = 0;//numberOfThemes;
		indexImages = 0;
		currentDecalage = 0;//wide * -indexList;

		for (int i = 0; i < numberOfThemes; i++) { // We create numberOfThemes *2 and we start in the middle, so we can already go left and right since start
			// instanciate & position
			listOfInstances.Add (GameObject.Instantiate (prefab, this.transform).GetComponent<RectTransform> ());
			listOfInstances [i].anchoredPosition = new Vector2 (wide * i + currentDecalage, 0);

			// == Fill each prefab with its content -- TODO: get the real data from PlayerPrefs and/or ScriptableObjects
			listOfInstances [i].Find("TitleLevel").GetComponent<Text>().text = titles[i%4];
			listOfInstances [i].Find("ScorePanel").transform.GetChild(1).GetComponent<Text>().text = scores[i%4].ToString();
			listOfInstances [i].Find ("Star_Mid").GetComponentInChildren<Text> ().text = (i%4+1).ToString();


			if (i < numberOfThemes) { // only numberOfThemes scroll dots
				listOfScrollImages [i] = GameObject.Instantiate (imageScrollPrefab, carousselScrollPanel.transform).GetComponent<RectTransform> ();
				listOfScrollImages [i].anchoredPosition = new Vector2 ((listOfScrollImages [i].rect.width + spaceBetweenScrollImages) * i - (numberOfThemes - 1) * (listOfScrollImages [i].rect.width + spaceBetweenScrollImages) / 2, 0);
				listOfScrollImages [i].GetComponent<Image> ().color = baseColor;
			}
		}
		listOfScrollImages [indexImages].GetComponent<Image> ().color = selectedColor;
	}





	void Update () {
		
		if (shouldResetDecalage && isCoroutineOver) {
			currentDecalage = Mathf.Lerp (previousDecalage, wide * -indexList, 0.1f);
			StartCoroutine (SmoothSwipe ());
		}
			

		if(isCoroutineOver){
			// When we press the mouse button & we are over the panel
			if (Input.GetMouseButtonDown (0) && isHovering) {
				canSwipe = true;
				mousePositionStartX = Input.mousePosition.x;
				previousDecalage = currentDecalage;
			}

			// When we hold the mouse button down & we can swipe
			if (Input.GetMouseButton (0) && canSwipe) {
				mousePositionEndX = Input.mousePosition.x;
				dragDistance = mousePositionEndX - mousePositionStartX;
				currentDecalage = previousDecalage + dragDistance;
			}

			// When we drag past the threshold distance
			if (Mathf.Abs (dragDistance) > swipeThreshold && canSwipe) {
				canSwipe = false;
				previousDecalage = currentDecalage;
				shouldResetDecalage = true;


				// Change of index
				if (dragDistance < 0) { // This side =>
					state = 1;

					if (indexList < numberOfThemes-1){
						listOfScrollImages [indexImages].GetComponent<Image> ().color = baseColor;
						indexImages++;
						indexImages %= numberOfThemes;
						listOfScrollImages [indexImages].GetComponent<Image> ().color = selectedColor;

						indexList++;
						//indexList %= numberOfThemes * 2;
					}

				} 
				else { // This side <=
					state = -1;

					if(indexList > 0){
						listOfScrollImages [indexImages].GetComponent<Image> ().color = baseColor;
						indexImages--;
						indexImages %= numberOfThemes;
						//if (indexImages < 0) indexImages += numberOfThemes;
						listOfScrollImages [indexImages].GetComponent<Image> ().color = selectedColor;

						indexList--;
						//indexList %= numberOfThemes * 2;
						//if (indexList < 0) indexList += numberOfThemes * 2;
					}
				}

			}

			// When we let go the click before dragging enough
			if (Input.GetMouseButtonUp (0) && Mathf.Abs (dragDistance) < swipeThreshold) {
				canSwipe = false;
				state = 0;
				previousDecalage = currentDecalage;
				shouldResetDecalage = true;
			}
				

		}

		// Update the position of all the Themes Du Mois + size and color
		for (int i = 0; i < numberOfThemes; i++) {
			listOfInstances [i].anchoredPosition = new Vector2 (currentDecalage + (wide * i), 0);

			if (i == indexList) {
				//listOfInstances [i].localScale = Vector3.Lerp (listOfInstances [i].localScale, new Vector3 (1f, 1f, 1f), Time.deltaTime * 5);
				//Color temp = listOfInstances [i].GetComponent<Image> ().color;
				//listOfInstances [i].GetComponent<Image> ().color = new Color (temp.r, temp.g, temp.b, 1f);
				MiddlePanel(listOfInstances[i].gameObject);
			}
			else {
				//listOfInstances [i].localScale = Vector3.Lerp (listOfInstances [i].localScale, new Vector3 (0.7f, 0.7f, 0.7f), Time.deltaTime * 5);
				//Color temp = listOfInstances [i].GetComponent<Image> ().color;
				//listOfInstances [i].GetComponent<Image> ().color = new Color (temp.r, temp.g, temp.b, 0.1f);
				SidePanel(listOfInstances[i].gameObject);
			}
		}
	}


	private void MiddlePanel(GameObject go){
		go.transform.localScale = Vector3.Lerp (go.transform.localScale, new Vector3 (1f, 1f, 1f), Time.deltaTime * 5);
		go.GetComponent<Image> ().sprite = middlePop;
		go.transform.Find ("Star_Left").GetComponent<Image> ().sprite = middleStarLeft;
		go.transform.Find ("Star_Mid").GetComponent<Image> ().sprite = middleStarCenter;
		go.transform.Find ("Star_Right").GetComponent<Image> ().sprite = middleStarRight;

	}
		
	private void SidePanel(GameObject go){
		go.transform.localScale = Vector3.Lerp (go.transform.localScale, new Vector3 (0.7f, 0.7f, 0.7f), Time.deltaTime * 5);
		go.GetComponent<Image> ().sprite = sidePop;
		go.transform.Find ("Star_Left").GetComponent<Image> ().sprite = sideStarLeft;
		go.transform.Find ("Star_Mid").GetComponent<Image> ().sprite = sideStarCenter;
		go.transform.Find ("Star_Right").GetComponent<Image> ().sprite = sideStarRight;
	}

	/// <summary>
	/// Automatically ends a swipe or when let go of the drag.
	/// </summary>
	public IEnumerator SmoothSwipe(){
		isCoroutineOver = false;

		while (Mathf.Abs(currentDecalage - (wide * -indexList)) > 5f) {
			currentDecalage = Mathf.Lerp (currentDecalage, wide * -indexList, 0.1f);
			yield return null;
		}

		/*
		if (state == 1) { // =>
			//listOfInstances.Add (listOfInstances [0]);
			//listOfInstances.RemoveAt (0);
			indexList--;
			//indexList %= numberOfThemes * 2;
		}
		else if (state == -1){ // <=
			//listOfInstances.Insert(0, listOfInstances[numberOfThemes * 2 - 1]);
			//listOfInstances.RemoveAt (numberOfThemes * 2);
			indexList++;
			//indexList %= numberOfThemes * 2;
		}*/


		currentDecalage = wide * -indexList;
		previousDecalage = currentDecalage;
		shouldResetDecalage = false;
		isCoroutineOver = true;

	}


	// Case where the swipe wasn't over and we did something (like change tab)
	public void OnDisable(){
		if (!isCoroutineOver) {
			if (state == 1) { // =>
				listOfInstances.Add (listOfInstances [0]);
				listOfInstances.RemoveAt (0);
				indexList--;
				indexList %= numberOfThemes * 2;
			}
			else if (state == -1){ // <=
				listOfInstances.Insert(0, listOfInstances[numberOfThemes * 2 - 1]);
				listOfInstances.RemoveAt (numberOfThemes * 2);
				indexList++;
				indexList %= numberOfThemes * 2;
			}

			currentDecalage = wide * -indexList;
			previousDecalage = currentDecalage;
			shouldResetDecalage = false;
			isCoroutineOver = true;
		}
	}

	public void OnPointerEnter(PointerEventData eventData){
		isHovering = true;
	}
	public void OnPointerExit(PointerEventData eventData){
		isHovering = false;
	}
}
