using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutoInformation : MonoBehaviour
{
	public List<int> listOfTutoLevels;
	public Image panelBlockingWorldInteraction;

	//public GameObject gamePanel;
	//public GameObject minimap;

	//public Button fireButton;

	/*
	public GameObject hole;
	public GameObject darkPanel;
	public GameObject panelTest;
	public GameObject planet;
	*/

	public GameObject tutoContent;
	public GameObject animArrow;
	public GameObject animRotateShip;
	public GameObject animSlider;
	public GameObject animSwip;
	public GameObject animDragndrop;

	public GameObject leftTutoPanel;
	public GameObject rightTutoPanel;
	public Button leftTutoOKButton;
	public Button rightTutoOKButton;

	public TutoInformation()
    {
		listOfTutoLevels = new List<int> ();
		listOfTutoLevels.Add (9);
		listOfTutoLevels.Add (10);
	}

    public void ActiveLeftPanel(bool active)
    {
        leftTutoPanel.SetActive(active);
    }

    public void ActiveRightPanel(bool active)
    {
        rightTutoPanel.SetActive(active);
    }
}
