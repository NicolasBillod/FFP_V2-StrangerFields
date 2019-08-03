using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections.Generic;
using PrimitiveFactory.Framework.UITimelineAnimation;

/*********************************************************
 * 
 * System : Tuto System
 * Manage all tutorial elements or new mechanics explainations
 * 
*********************************************************/
public class TutoSystem : UtilitySystem
{
    // =================================================================================================================================================================================================================
    // ============================================ VARIABLES ============================================ VARIABLES ============================================ VARIABLES ============================================
    // =================================================================================================================================================================================================================

    // ============================================ Families ============================================= Families ============================================= Families =============================================  
    private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
    private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
    private Family _selectInformationFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SelectInformation)));
    private Family _otherInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(OtherInformation)));
    private Family _fieldsCounterFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FieldsCounter)));
    private Family _cameraFamily = FamilyManager.getFamily(new AllOfComponents(typeof(CameraParams)));
    private Family _tutoInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(TutoInformation)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GlobalUI), typeof(OtherPanel), typeof(LoadingUI)));

    private Family _obstacleFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ObstacleInformation)));
    private Family _breakObstacleFamily = FamilyManager.getFamily(new AllOfComponents(typeof(BreakableObstacle)));
    private Family _foeFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Enemy)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
    private Family _shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
    private Family _finishFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FinishInformation)));
    private Family _allSourcesFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Field), typeof(Dimensions), typeof(Position)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family _terrainFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));

    // ============================================ Components ============================================ Components =========================================== Components ==========================================
    private TutoInformation _tutoInfo;
    private GameInformations _gameInfo;
    private GlobalUI _globalUI;
    private OtherPanel _otherPanel;

    // ======================================= Action Variables ====================================== Action Variables ====================================== Action Variables ========================================
    private int step = -1;
    private bool okClicked, doneAction, initBackRestButton = false;
    private bool activeBackRestPanel = false;

    // ============================================ Listeners ============================================ Listeners ============================================ Listeners ============================================
    private UnityEngine.Events.UnityAction callHandleOKClick;
    private UnityEngine.Events.UnityAction callBackLevel2;
    private UnityEngine.Events.UnityAction callRestLevel2;
    private UnityEngine.Events.UnityAction callNoBackLevel2;
    private UnityEngine.Events.UnityAction callNoRestLevel2;

    // ============================================== Others ============================================== Others =============================================== Others ==============================================
    private List<GameObject> _fieldsToReactivate;

    // =================================================================================================================================================================================================================
    // ============================================= METHODS ============================================== METHODS ============================================== METHODS =============================================
    // =================================================================================================================================================================================================================

    // ============================================ Lifecycle ============================================= Lifecycle =========================================== Lifecycle ============================================

    /// <summary>
    /// Constructor -- like Unity Start()
    /// </summary>
    public TutoSystem()
    {
        _tutoInfo = _tutoInfoFamily.First().GetComponent<TutoInformation>();
        _gameInfo = _gameInfoFamily.First().GetComponent<GameInformations>();
        _globalUI = _interfaceFamily.First().GetComponent<GlobalUI>();
        _otherPanel = _interfaceFamily.First().GetComponent<OtherPanel>();

        //step = -1;
        _fieldsToReactivate = new List<GameObject>();

        okClicked = false;
        doneAction = false;
        initBackRestButton = false;
        _tutoInfo.leftTutoOKButton.onClick.AddListener(() => HandleOKClick());
        _tutoInfo.rightTutoOKButton.onClick.AddListener(() => HandleOKClick());

        if (_tutoInfo.listOfTutoLevels.Contains(_gameInfo.noLevel))
        {
            _globalUI.fireButton.onClick.AddListener(() => HideTuto());
        }

        callHandleOKClick = (() => HandleOKClick());
        callBackLevel2 = (() => OnBackMenuRestButtonTuto(true, true));
        callNoBackLevel2 = (() => OnBackMenuRestButtonTuto(true, false));
        callRestLevel2 = (() => OnBackMenuRestButtonTuto(false, true));
        callNoRestLevel2 = (() => OnBackMenuRestButtonTuto(false, false));
        //HighlightUIGO (_tutoInfo.panelTest);
    }

    /// <summary>
    /// Constructor -- like Unity Start()
    /// </summary>
    protected override void onProcess(int familiesUpdateCount)
    {
        // Only if we are in a tutorial level
        if (_tutoInfo.listOfTutoLevels.Contains(_gameInfo.noLevel))
        {
            switch (_gameInfo.noLevel)
            {
                case 1:
                    Level1(); // Tuto level 1
                    break;
                case 2:
                    Level2(); // Tuto level 2
                    break;
                case 3:
                    Level3();
                    break;
                case 5:
                    Level5();
                    break;
                default:
                    break;
            }
        }
    }

    // ============================================ Tutorials ============================================= Tutorials =========================================== Tutorials ============================================

    /// <summary>
    /// All tutorial steps of the first level
    /// Explanations :
    /// - Move camera
    /// - Select already placed Force Fields and see its informations
    /// - Select playership and rotate it
    /// - Change missile speed 
    /// - Present minimap
    /// - Reapeat the main goal
    /// </summary>
    public void Level1()
    {
        switch (step)
        {
            case -1: // Set up
                if (_stateFamily.First().GetComponent<State>().state == State.STATES.DIALOG || _stateFamily.First().GetComponent<State>().state == State.STATES.ANIM1 || _stateFamily.First().GetComponent<State>().state == State.STATES.ANIM2)
                {
                    // During animation and dialog
                    _tutoInfo.panelBlockingWorldInteraction.gameObject.SetActive(true);
                    _shipFamily.First().SetActive(false);
                }

                if (_stateFamily.First().GetComponent<State>().state == State.STATES.SETUP)
                {
                    _tutoInfo.tutoContent.SetActive(true);
                    UTAController.Instance.PlayAnimation("A_LeftTutoPanel_Apparition");
                    _tutoInfo.animSwip.SetActive(!activeBackRestPanel);

                    // So the player can move the camera
                    _tutoInfo.panelBlockingWorldInteraction.gameObject.SetActive(false);

                    // Text to display
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Glisse ton doigt sur l'écran pour changer d'angle de vue";

                    if (!initBackRestButton)
                    {
                        initBackRestButton = true;
                        _globalUI.backMenuButton.onClick.RemoveAllListeners();
                        _globalUI.backMenuButton.onClick.AddListener(() => OnBackMenuRestButtonFirstCase(true, true));
                        _otherPanel.noBackButton.onClick.RemoveAllListeners();
                        _otherPanel.noBackButton.onClick.AddListener(() => OnBackMenuRestButtonFirstCase(true, false));
                        _globalUI.restartButton.onClick.RemoveAllListeners();
                        _globalUI.restartButton.onClick.AddListener(() => OnBackMenuRestButtonFirstCase(false, true));
                        _otherPanel.noRestButton.onClick.RemoveAllListeners();
                        _otherPanel.noRestButton.onClick.AddListener(() => OnBackMenuRestButtonFirstCase(false, false));
                    }

                    step++;
                }
                break;


            case 0: // Swipe
                    //TODO: Put at the end of case -1 and not spam it here each frame
                _globalUI.gamePanel.SetActive(false);
                _interfaceFamily.First().GetComponent<GlobalUI>();
                _globalUI.miniMap.SetActive(false);

                if (!doneAction)
                    _tutoInfo.animSwip.SetActive(!activeBackRestPanel);
                else
                    _tutoInfo.animSwip.SetActive(false);

                // if the drag distance > 15
                if (Mathf.Abs(_cameraFamily.First().GetComponent<CameraParams>().distanceDrag_x) > 15 && !doneAction)
                {
                    //_tutoInfo.leftTutoOKButton.gameObject.SetActive(true);
                    doneAction = true;
                    AnimApparitionLeftOKButton(true);
                }

                if (okClicked)
                {
                    okClicked = false;
                    AnimApparitionLeftOKButton(false);
                    step++;
                    doneAction = false;
                    initBackRestButton = false;
                }
                break;


            case 1: // Active fields - once
                _globalUI.fireButton.gameObject.SetActive(false);
                UTAController.Instance.PlayAnimation("A_GamePanel_Apparition");

                Vector3 terrDims = _terrainFamily.First().GetComponent<Terrain>().terrainData.size;
                _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Sélectionne l'un des deux champs de force ! Les champs de force noirs sont attractifs et les blancs sont répulsifs.";
                FieldsCounter fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();

                // Get the firt attractive field
                GameObject fieldAtt = fieldsRemaining.poolAttFFPlaced[0].gameObject;
                changePosition(fieldAtt.GetComponent<Position>(), new Vector3(0.25f, 0.25f, 0), terrDims);
                GameObjectManager.setGameObjectState(fieldAtt, true);
                GameObjectManager.addComponent<RefreshTerrain>(_levelInfoFamily.First(), new { action = RefreshTerrain.ACTIONS.ADD, source = fieldAtt });
                // TODO: Create a new animation for the apparition of an element, such as this:
                /*
				GameObject explosion = GameObject.Instantiate(_shipFamily.First().GetComponent<ExplosionMissile>().explosionPrefab);
                explosion.transform.position = fieldAtt.transform.position;
                explosion.GetComponent<ParticleSystem>().Play();
                GameObject.Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.duration);
				*/
				//
                fieldAtt.transform.Find("CanvasTutoArrow").gameObject.SetActive(true);

                // Get the first repulsive field
                GameObject fieldRep = fieldsRemaining.poolRepFFPlaced[0].gameObject;
                changePosition(fieldRep.GetComponent<Position>(), new Vector3(0.75f, 0.25f, 0), terrDims);
                GameObjectManager.setGameObjectState(fieldRep, true);
                GameObjectManager.addComponent<RefreshTerrain>(_levelInfoFamily.First(), new { action = RefreshTerrain.ACTIONS.ADD, source = fieldRep });
                // TODO: Create a new animation for the apparition of an element, such as this:
                /*
				explosion = GameObject.Instantiate(_shipFamily.First().GetComponent<ExplosionMissile>().explosionPrefab);
                explosion.transform.position = fieldRep.transform.position;
                explosion.GetComponent<ParticleSystem>().Play();
                GameObject.Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.duration);
                */
				//
                fieldRep.transform.Find("CanvasTutoArrow").gameObject.SetActive(true);

                if (!initBackRestButton)
                {
                    initBackRestButton = true;
                    _globalUI.backMenuButton.onClick.RemoveAllListeners();
                    _globalUI.backMenuButton.onClick.AddListener(() => OnBackMenuRestButtonSecondCase(true, true));
                    _otherPanel.noBackButton.onClick.RemoveAllListeners();
                    _otherPanel.noBackButton.onClick.AddListener(() => OnBackMenuRestButtonSecondCase(true, false));
                    _globalUI.restartButton.onClick.RemoveAllListeners();
                    _globalUI.restartButton.onClick.AddListener(() => OnBackMenuRestButtonSecondCase(false, true));
                    _otherPanel.noRestButton.onClick.RemoveAllListeners();
                    _otherPanel.noRestButton.onClick.AddListener(() => OnBackMenuRestButtonSecondCase(false, false));
                }

                step++;
                break;


            case 2: // Check field selection
                foreach (GameObject f in _allSourcesFamily)
                {
                    if (_selectInformationFamily.First().GetComponent<SelectInformation>().selectedGameObject == f && !doneAction)
                    {
                        doneAction = true;
                        //_tutoInfo.leftTutoOKButton.gameObject.SetActive (true);
                        AnimApparitionLeftOKButton(true);

                        foreach (GameObject go in _allSourcesFamily)
                        {
                            go.transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                        }

                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Une fois sélectionné, tu peux voir en bas les informations du champ de force.";
                        _tutoInfo.animArrow.transform.SetParent(_globalUI.gamePanel.transform);
                        _tutoInfo.animArrow.SetActive(true);
                        RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform>();
                        RectTransform r2 = _globalUI.gamePanel.GetComponent<RectTransform>();

                        r1.anchoredPosition = Vector2.up * (r2.rect.height / 2 + r1.GetChild(0).GetComponent<RectTransform>().rect.height * 2 / 3);
                        break;
                    }
                }


                if (okClicked)
                {
                    okClicked = false;
                    AnimApparitionLeftOKButton(false);
                    //_tutoInfo.leftTutoOKButton.gameObject.SetActive (false);
                    _tutoInfo.animArrow.SetActive(false);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Au tour de notre vaisseau maintenant !" + '\n' + "Sélectionne-le et tourne-le pour changer son orientation.";
                    _shipFamily.First().SetActive(true);
                    _globalUI.speedSlider.enabled = false;
                    //_shipFamily.First ().transform.Find ("CanvasTutoArrow").gameObject.SetActive (true);
                    _tutoInfo.animRotateShip.SetActive(!activeBackRestPanel);

                    // TODO: Create a new animation for the apparition of an element, such as this:
                    /*
					explosion = GameObject.Instantiate(_shipFamily.First().GetComponent<ExplosionMissile>().explosionPrefab);
                    explosion.transform.position = _shipFamily.First().transform.position;
                    explosion.GetComponent<ParticleSystem>().Play();
                    GameObject.Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.duration);
                    */
					//
                    step++;
                    doneAction = false;
                    initBackRestButton = false;
                }
                break;


            case 3: // Check ship selection + rotation (rotation > 45)
                    //HighlightWorldGO(_shipFamily.First());
                Vector2 viewportPoint = Camera.main.WorldToViewportPoint(_shipFamily.First().transform.position);
                RectTransform r = _tutoInfo.animRotateShip.GetComponent<RectTransform>();
                r.SetParent(_globalUI.transform);
                r.anchorMin = viewportPoint;
                r.anchorMax = viewportPoint;
                _tutoInfo.tutoContent.transform.SetAsLastSibling();

                if (!doneAction)
                {
                    _tutoInfo.animRotateShip.SetActive(!activeBackRestPanel);
                }

                //_shipFamily.First ().SetActive (true);
                // if the rotationAngle > 45
                if (Mathf.Abs(_otherInfoFamily.First().GetComponent<OtherInformation>().rotationAngle) > 45 && !doneAction)
                {
                    doneAction = true;
                    //HighlightUIGO (GameObject.Find ("GamePanel").transform.Find("Box").gameObject);
                    AnimApparitionLeftOKButton(true);
                    //_tutoInfo.leftTutoOKButton.gameObject.SetActive (true);
                    _tutoInfo.animRotateShip.SetActive(false);
                }

                if (okClicked)
                {
                    okClicked = false;
                    AnimApparitionLeftOKButton(false);
                    //_tutoInfo.leftTutoOKButton.gameObject.SetActive(false);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Maintenant teste le changement de la vitesse de lancement des missiles.";
                    _globalUI.speedSlider.enabled = true;

                    // SLIDER BUMP
                    _tutoInfo.animSlider.transform.SetParent(_globalUI.speedSlider.transform.Find("Handle Slide Area").Find("Handle"));
                    _tutoInfo.animSlider.SetActive(true);
                    RectTransform rt = _tutoInfo.animSlider.GetComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.zero;

                    step++;
                    doneAction = false;
                }
                break;


            case 4: // Check missiles' speed (variation > 5)
                if (!_tutoInfo.leftTutoOKButton.interactable)
                {
                    if (_selectInformationFamily.First().GetComponent<SelectInformation>().selectedGameObject == _shipFamily.First())
                    {
                        _shipFamily.First().transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                        // ARROW
                        _tutoInfo.animArrow.transform.SetParent(_globalUI.gamePanel.transform);
                        _tutoInfo.animArrow.SetActive(true);
                        RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform>();
                        RectTransform r2 = _globalUI.gamePanel.GetComponent<RectTransform>();

                        r1.anchoredPosition = Vector2.up * (r2.rect.height / 2 + r1.GetChild(0).GetComponent<RectTransform>().rect.height * 2 / 3);
                    }
                    else
                    {
                        _tutoInfo.animArrow.SetActive(false);
                        _shipFamily.First().transform.Find("CanvasTutoArrow").gameObject.SetActive(true);
                    }
                }

                if (_shipFamily.First().GetComponent<ShipInfo>().valueSpeedSlider != 0)
                {
                    if (Mathf.Abs(_shipFamily.First().GetComponent<ShipInfo>().valueSpeedSlider - 50) > 5 && !doneAction)
                    {
                        doneAction = true;
                        AnimApparitionLeftOKButton(true);
                        //_tutoInfo.leftTutoOKButton.gameObject.SetActive (true);
                        _tutoInfo.animArrow.SetActive(false);
                        _tutoInfo.animSlider.SetActive(false);
                    }
                }

                if (okClicked)
                {
                    okClicked = false;
                    //_tutoInfo.tutoOKButton.gameObject.SetActive (false);

                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "En bas à gauche, une carte te permet d'avoir un vision globale !";

                    _tutoInfo.animArrow.transform.SetParent(_globalUI.miniMap.transform);
                    RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform>();
                    RectTransform r2 = _globalUI.miniMap.GetComponent<RectTransform>();

                    r1.anchoredPosition = Vector2.right * (r2.rect.width / 2 + r1.GetChild(0).GetComponent<RectTransform>().rect.height * 2 / 3);
                    r1.eulerAngles = Vector3.forward * (-90);
                    _tutoInfo.animArrow.SetActive(true);
                    _globalUI.miniMap.SetActive(true);
                    UTAController.Instance.PlayAnimation("A_Minimap_Apparition");
                    _globalUI.undoButton.gameObject.SetActive(false);
                    _globalUI.redoButton.gameObject.SetActive(false);

                    if (!initBackRestButton)
                    {
                        initBackRestButton = true;
                        _globalUI.backMenuButton.onClick.RemoveAllListeners();
                        _globalUI.backMenuButton.onClick.AddListener(() => OnBackMenuRestButtonThirdCase(true, true));
                        _otherPanel.noBackButton.onClick.RemoveAllListeners();
                        _otherPanel.noBackButton.onClick.AddListener(() => OnBackMenuRestButtonThirdCase(true, false));
                        _globalUI.restartButton.onClick.RemoveAllListeners();
                        _globalUI.restartButton.onClick.AddListener(() => OnBackMenuRestButtonThirdCase(false, true));
                        _otherPanel.noRestButton.onClick.RemoveAllListeners();
                        _otherPanel.noRestButton.onClick.AddListener(() => OnBackMenuRestButtonThirdCase(false, false));
                    }

                    step++;
                    doneAction = false;
                }
                break;


            /*case 5: // Minimap
                if (okClicked) {
                    okClicked = false;
                    _tutoInfo.rightDialogBox.GetComponentInChildren<Text>().text = "Everybody makes mistakes. So, just in case, you can undo and redo them.";

                    _globalUI.undoButton.gameObject.SetActive (true);
                    _globalUI.redoButton.gameObject.SetActive (true);

                    _tutoInfo.leftDialogBox.SetActive (false);
                    _tutoInfo.tutoOKButton.gameObject.SetActive (false);
                    _tutoInfo.rightDialogBox.SetActive (true);
                    _tutoInfo.tutoOKRightButton.gameObject.SetActive (true);

                    _tutoInfo.animArrow.transform.SetParent (_globalUI.undoButton.transform);
                    RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform> ();
                    RectTransform r2 = _globalUI.undoButton.GetComponent<RectTransform> ();

                    r1.anchoredPosition = Vector2.up * (r2.rect.height / 2 + r1.GetChild(0).GetComponent<RectTransform>().rect.height * 2 / 3);
                    r1.eulerAngles = Vector3.forward;

                    step++;
                }
                break;
            */

            case 5: // Undo & Redo
                if (okClicked)
                {
                    okClicked = false;

                    //_tutoInfo.rightDialogBox.SetActive (false);
                    //_tutoInfo.rightTutoOKButton.gameObject.SetActive (false);
                    //_tutoInfo.leftDialogBox.SetActive (true);
                    //_tutoInfo.leftTutoOKButton.gameObject.SetActive (true);
                    AnimApparitionLeftOKButton(false);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Pour réussir chaque étape, vise la planète et réduit ses points de vie à zéro pour la détruire ! Bonne chance !";
                    _globalUI.fireButton.gameObject.SetActive(true);

                    _finishFamily.First().transform.Find("CanvasTutoArrow").gameObject.SetActive(true);
                    _tutoInfo.animArrow.transform.SetParent(_globalUI.fireButton.transform);
                    RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform>();
                    RectTransform r2 = _globalUI.fireButton.GetComponent<RectTransform>();

                    r1.anchoredPosition = Vector2.up * (r2.rect.height / 2 + r1.GetChild(0).GetComponent<RectTransform>().rect.height * 2 / 3);
                    r1.eulerAngles = Vector3.forward;
                    _tutoInfo.animArrow.SetActive(true);

                    step++;
                    doneAction = false;
                    initBackRestButton = false;
                }
                break;


            case 6: // Destroy the Earth!
                if (!doneAction)
                {
                    doneAction = true;
                    AnimApparitionLeftOKButton(true);
                    _globalUI.fireButton.onClick.AddListener(() => HandleOKClick());
                }

                if (okClicked)
                {
                    okClicked = false;
                    //_tutoInfo.leftTutoOKButton.gameObject.SetActive (false);
                    //_tutoInfo.leftDialogBox.SetActive (false);
                    UTAController.Instance.PlayAnimation("A_LeftTutoPanel_Disparition");
                    _tutoInfo.animArrow.SetActive(false);
                    _finishFamily.First().transform.Find("CanvasTutoArrow").gameObject.SetActive(false);

                    if (!initBackRestButton)
                    {
                        initBackRestButton = true;
                        _globalUI.backMenuButton.onClick.RemoveAllListeners();
                        _globalUI.backMenuButton.onClick.AddListener(() => OnBackMenuRestButtonLastCase(true, true));
                        _otherPanel.noBackButton.onClick.RemoveAllListeners();
                        _otherPanel.noBackButton.onClick.AddListener(() => OnBackMenuRestButtonLastCase(true, false));
                        _globalUI.restartButton.onClick.RemoveAllListeners();
                        _globalUI.restartButton.onClick.AddListener(() => OnBackMenuRestButtonLastCase(false, true));
                        _otherPanel.noRestButton.onClick.RemoveAllListeners();
                        _otherPanel.noRestButton.onClick.AddListener(() => OnBackMenuRestButtonLastCase(false, false));
                    }
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// All tutorial steps of the second level
    /// Explanations :
    /// - Add force field, place it and change its intensity
    /// - Delete a force field the player placed
    /// - Undo / redo actions
    /// - Autodestruction of player's missiles, once lauched
    /// </summary>
    public void Level2()
    {
        switch (step)
        {
            case -1: // Set up
                if (_stateFamily.First().GetComponent<State>().state == State.STATES.SETUP)
                {
                    if (!initBackRestButton)
                    {
                        initBackRestButton = true;
                        _globalUI.backMenuButton.onClick.AddListener(callBackLevel2);
                        _otherPanel.noBackButton.onClick.AddListener(callNoBackLevel2);
                        _globalUI.restartButton.onClick.AddListener(callRestLevel2);
                        _otherPanel.noRestButton.onClick.AddListener(callNoRestLevel2);
                    }
					_tutoInfo.tutoContent.SetActive(true);


                    _globalUI.fireButton.gameObject.SetActive(false);
                    //_tutoInfo.tutoContent.SetActive(true);
                    _globalUI.undoButton.gameObject.SetActive(false);
                    _globalUI.redoButton.gameObject.SetActive(false);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Pour ajouter un champ de force, appuyer sur le bouton. Sans relâcher, place le champs de force. Une fois placé, relâche le !";
                    AnimApparitionLeftTutoPanel(true);
                    _tutoInfo.animDragndrop.transform.SetParent(_globalUI.attractiveButton.transform);
                    RectTransform r1 = _tutoInfo.animDragndrop.GetComponent<RectTransform>();
                    r1.anchoredPosition = Vector2.zero;
                    _tutoInfo.animDragndrop.SetActive(!activeBackRestPanel);
                    _globalUI.intensitySlider.gameObject.SetActive(false);

                   step++;
                }
                break;

            case 0: // Check for a field dropped in the middle area

                foreach (GameObject field in _allSourcesFamily)
                {
                    if (field.GetComponent<TerrainDisplay>().dropped)
                    {
                        _tutoInfo.animDragndrop.SetActive(false);
                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Tu ne voulais pas placer ce champ de force ? Supprime le !";
                        step++;
                        _globalUI.deleteButton.onClick.AddListener(callHandleOKClick);
                        break;
                    }
                }
                break;

            case 1: // Update the arrow according to the selection & check undo
				// If selection is empty or not a field: activate arrows on top of ALL fields
				if (_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject == null ||
					!_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject.GetComponent<Field> ()) {
					foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolRepulsive) {
						fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (true);
					}
					foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolAttractive) {
						fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (true);
					}
				}
				// If selection is a field: deactivate arrow on top of ALL fields & activate arrow on UI
				else if (_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject.GetComponent<Field> ()) {
					foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolRepulsive) {
						fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (false);
					}
					foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolAttractive) {
						fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (false);
					}
					_tutoInfo.animArrow.transform.SetParent (_globalUI.deleteButton.transform);
					_tutoInfo.animArrow.SetActive (true);
					RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform> ();
					RectTransform r2 = _globalUI.deleteButton.GetComponent<RectTransform> ();

					r1.anchoredPosition = Vector2.up * (r2.rect.height / 2 + r1.GetChild (0).GetComponent<RectTransform> ().rect.height * 2 / 3); 
				}

                if (okClicked)
                {
                    okClicked = false;
                    _tutoInfo.animArrow.SetActive(false);
                    foreach (GameObject go in _allSourcesFamily)
                    {
                        go.transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                    }
                    _globalUI.deleteButton.onClick.RemoveListener(callHandleOKClick);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Si jamais tu fais une action que tu ne voulais pas, il suffit d'appuyer sur le bouton en haut de la minimap pour annuler l'action !";
                    _tutoInfo.panelBlockingWorldInteraction.gameObject.SetActive(true);
                    _globalUI.undoButton.gameObject.SetActive(true);
                    _globalUI.attractiveButton.gameObject.SetActive(false);
                    _globalUI.repulsiveButton.gameObject.SetActive(false);

                    _globalUI.undoButton.onClick.AddListener(callHandleOKClick);

					foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolRepulsive) {
						fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (false);
					}
					foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolAttractive) {
						fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (false);
					}

                    step++;
                }
                break;

            case 2: // Redo
                if (okClicked)
                {
                    _globalUI.undoButton.onClick.RemoveListener(callHandleOKClick);
                    okClicked = false;
                    _globalUI.redoButton.gameObject.SetActive(true);
                    _globalUI.redoButton.onClick.AddListener(callHandleOKClick);
                    _globalUI.undoButton.gameObject.SetActive(false);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Et au cas où, tu peux rétablir l'action avec le bouton d'à côté !";
                    _globalUI.deleteButton.enabled = false;
                    step++;
                }
                break;

            case 3:
                if (okClicked)
                {
                    okClicked = false;
                    AnimApparitionLeftOKButton(true);
                    _globalUI.redoButton.onClick.RemoveListener(callHandleOKClick);
                    _globalUI.undoButton.gameObject.SetActive(true);
                    step++;
                }
                break;

            case 4: // Redo click checked
                if (okClicked)
                {
                    okClicked = false;
                    AnimApparitionLeftOKButton(false);

                    _globalUI.attractiveButton.gameObject.SetActive(true);
                    _globalUI.repulsiveButton.gameObject.SetActive(true);
                    _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Maintenant, place un autre champ de force";
                    _tutoInfo.animDragndrop.SetActive(true);
                    _tutoInfo.panelBlockingWorldInteraction.gameObject.SetActive(false);
                    _globalUI.deleteButton.enabled = true;
                    _globalUI.intensitySlider.gameObject.SetActive(true);
                    step++;
                }
                break;

            case 5:
                foreach (GameObject field in _allSourcesFamily)
                {
                    if (field.GetComponent<TerrainDisplay>().dropped)
                    {
                        _tutoInfo.animDragndrop.SetActive(false);
                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Avec les informations apparaissant en bas, tu peux modifier l'intensité des champs de force que tu places en les sélectionnant !";
                        step++;
                        break;
                    }
                }
                break;

            case 6:
                if (_allSourcesFamily.Count == 0)
                {
                    okClicked = true;
                    step = 4;
                    _tutoInfo.animArrow.SetActive(false);
					_tutoInfo.animSlider.SetActive (false);
                }
                else
                {

					if (_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject == null ||
						!_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject.GetComponent<Field> ()) {
						foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolRepulsive) {
							fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (true);
						}
						foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolAttractive) {
							fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (true);
						}
						_tutoInfo.animArrow.SetActive (false);
					}

					else if (_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject.GetComponent<Field> ()) {
						foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolRepulsive) {
							fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (false);
						}
						foreach (Transform fieldToPlace in _fieldsCounterFamily.First().GetComponent<FieldsCounter>().poolAttractive) {
							fieldToPlace.Find ("CanvasTutoArrow").gameObject.SetActive (false);
						}
						_tutoInfo.animArrow.transform.SetParent (_globalUI.gamePanel.transform);
						_tutoInfo.animArrow.SetActive (true);
						RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform> ();
						RectTransform r2 = _globalUI.gamePanel.GetComponent<RectTransform> ();
						r1.anchoredPosition = Vector2.up*(r2.rect.height / 2 + r1.GetChild(0).GetComponent<RectTransform> ().rect.height * 2 / 3); 

						// SLIDER BUMP
						_tutoInfo.animSlider.transform.SetParent (_globalUI.intensitySlider.transform.Find ("Handle Slide Area").Find ("Handle"));
						_tutoInfo.animSlider.SetActive (true);
						RectTransform rt = _tutoInfo.animSlider.GetComponent<RectTransform> ();
						rt.anchoredPosition = Vector2.zero;
					}
					/*
                    foreach (GameObject f in _allSourcesFamily)
                    {
                        if (_selectInformationFamily.First().GetComponent<SelectInformation>().selectedGameObject == f)
                        {
                            // SLIDER BUMP
                            _tutoInfo.animSlider.transform.SetParent(_globalUI.intensitySlider.transform.Find("Handle Slide Area").Find("Handle"));
                            _tutoInfo.animSlider.SetActive(true);
                            RectTransform rt = _tutoInfo.animSlider.GetComponent<RectTransform>();
                            rt.anchoredPosition = Vector2.zero;

                            foreach (GameObject go in _allSourcesFamily)
                            {
                                go.transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                            }
                            _tutoInfo.animArrow.transform.SetParent(_globalUI.gamePanel.transform);
                            _tutoInfo.animArrow.SetActive(true);
                            RectTransform r1 = _tutoInfo.animArrow.GetComponent<RectTransform>();
                            RectTransform r2 = _globalUI.gamePanel.GetComponent<RectTransform>();

                            r1.anchoredPosition = Vector2.up * (r2.rect.height / 2 + r1.GetChild(0).GetComponent<RectTransform>().rect.height * 2 / 3);
                            break;
                        }
                    }
					*/
                    foreach (GameObject field in _allSourcesFamily)
                    {
                        if ((Mathf.Abs(Mathf.Abs(field.GetComponent<Field>().A) - 0.5f)) > 0.1f) {
                            _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Une fois la mise à feu déclenché, un bouton d'auto-destruction de tes missiles apparait. Pas besoin d'attendre en cas d'erreur !";
                            _globalUI.undoButton.gameObject.SetActive(true);
                            AnimApparitionLeftOKButton(true);
                            _tutoInfo.animArrow.SetActive(false);
                            _tutoInfo.animSlider.SetActive(false);
                            _globalUI.fireButton.gameObject.SetActive(true);
                            _globalUI.fireButton.onClick.AddListener(callHandleOKClick);
                            step++;
                        }
                    }
                }
                break;

            case 7:
                if (okClicked)
                {
                    okClicked = false;
                    AnimApparitionLeftTutoPanel(false);
                    _globalUI.backMenuButton.onClick.RemoveListener(callBackLevel2);
                    _otherPanel.noBackButton.onClick.RemoveListener(callNoBackLevel2);
                    _globalUI.restartButton.onClick.RemoveListener(callRestLevel2);
                    _otherPanel.noRestButton.onClick.RemoveListener(callNoRestLevel2);
                    //_tutoInfo.tutoContent.SetActive(false);
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Presents 2 obsctacle types
    /// </summary>
    public void Level3()
    {
        switch (step)
        {
            case -1:
                
                if (_stateFamily.First().GetComponent<State>().state == State.STATES.SETUP)
                {
					_tutoInfo.tutoContent.SetActive(true);

                    if (!initBackRestButton)
                    {
                        initBackRestButton = true;
                        _globalUI.backMenuButton.onClick.AddListener(callBackLevel2);
                        _otherPanel.noBackButton.onClick.AddListener(callNoBackLevel2);
                        _globalUI.restartButton.onClick.AddListener(callRestLevel2);
                        _otherPanel.noRestButton.onClick.AddListener(callNoRestLevel2);
                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Ces astéroides-ci font obstacle sur notre chemin, tu dois les contourner !";

                        _globalUI.fireButton.gameObject.SetActive(false);
                        AnimApparitionLeftTutoPanel(true);

                        foreach (GameObject aObstacle in _obstacleFamily)
                        {
                            aObstacle.transform.Find("CanvasTutoArrow").gameObject.SetActive(true);
                        }

                        AnimApparitionLeftOKButton(true);
                    }

                    if (okClicked)
                    {
                        okClicked = false;

                        foreach (GameObject aObstacle in _obstacleFamily)
                        {
                            aObstacle.transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                        }

                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Alors que ces astéroides sont destructibles, un tir suffit à les détruire ! Ce qui permet de dégager la voie pour les tirs suivants ! ";

                        foreach (GameObject aBreakObstacle in _breakObstacleFamily)
                        {
                            aBreakObstacle.transform.Find("CanvasTutoArrow").gameObject.SetActive(true);
                        }

                        AnimApparitionLeftOKButton(true);
                        step++;
                    }                                     
                }
                break;

            case 0:

                if (okClicked)
                {
                    okClicked = false;

                    foreach (GameObject aBreakObstacle in _breakObstacleFamily)
                    {
                        aBreakObstacle.transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                    }

                    AnimApparitionLeftTutoPanel(false);

                    _globalUI.backMenuButton.onClick.RemoveListener(callBackLevel2);
                    _otherPanel.noBackButton.onClick.RemoveListener(callNoBackLevel2);
                    _globalUI.restartButton.onClick.RemoveListener(callRestLevel2);
                    _otherPanel.noRestButton.onClick.RemoveListener(callNoRestLevel2);

                    _globalUI.fireButton.gameObject.SetActive(true);
                }

                break;
        }
    }

    /// <summary>
    /// Present enemy ship
    /// </summary>
    private void Level5()
    {
        switch (step)
        {
            case -1:

                if (_stateFamily.First().GetComponent<State>().state == State.STATES.SETUP)
                {
					_tutoInfo.tutoContent.SetActive(true);

                    if (!initBackRestButton)
                    {
                        initBackRestButton = true;
                        _globalUI.backMenuButton.onClick.AddListener(callBackLevel2);
                        _otherPanel.noBackButton.onClick.AddListener(callNoBackLevel2);
                        _globalUI.restartButton.onClick.AddListener(callRestLevel2);
                        _otherPanel.noRestButton.onClick.AddListener(callNoRestLevel2);

                        _globalUI.fireButton.gameObject.SetActive(false);
                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "Les voilà, nous devons éviter d'être détruit ! On peut aussi leur tirer dessus et dévier leurs missiles avec nos champs de force.";
                        AnimApparitionLeftTutoPanel(true);

                        foreach (GameObject aFoe in _foeFamily)
                        {
                            aFoe.transform.Find("CanvasTutoArrow").gameObject.SetActive(true);
                        }

                        AnimApparitionLeftOKButton(true);
                    }

                    if (okClicked)
                    {
                        okClicked = false;

                        _tutoInfo.leftTutoPanel.GetComponentInChildren<Text>().text = "N'oublie pas une chose, ils recalculent une nouvelle trajectoire pour leurs missiles après chaque mise à feu ! Fais attention.";

                        AnimApparitionLeftOKButton(true);
                        step++;
                    }
                }
                break;

            case 0:

                if (okClicked)
                {
                    okClicked = false;

                    foreach (GameObject aFoe in _foeFamily)
                    {
                        aFoe.transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
                    }

                    AnimApparitionLeftTutoPanel(false);

                    _globalUI.backMenuButton.onClick.RemoveListener(callBackLevel2);
                    _otherPanel.noBackButton.onClick.RemoveListener(callNoBackLevel2);
                    _globalUI.restartButton.onClick.RemoveListener(callRestLevel2);
                    _otherPanel.noRestButton.onClick.RemoveListener(callNoRestLevel2);

                    _globalUI.fireButton.gameObject.SetActive(true);
                }

                break;
        }
    }

    // ================================== Animations & Listeners ================================== Animations & Listeners ================================== Animations & Listeners ===================================

    /// <summary>
    /// Confirmation for the click on OK Button of the tutorial panel
    /// </summary>
    public void HandleOKClick() {
        okClicked = true;
    }

    /// <summary>
    /// Hide all tuto elements 
    /// </summary>
    public void HideTuto()
    {
        _tutoInfo.leftTutoPanel.SetActive(false);
        _tutoInfo.leftTutoOKButton.gameObject.SetActive(false);
        _tutoInfo.animArrow.SetActive(false);
        _finishFamily.First().transform.Find("CanvasTutoArrow").gameObject.SetActive(false);
    }

    /// <summary>
    /// Manage ok button animation
    /// </summary>
    /// <param name="apparition"></param>
    private void AnimApparitionLeftOKButton(bool apparition)
    {
        if (apparition)
        {
            UTAController.Instance.PlayAnimation("A_LeftTutoOK_Apparition");
        }
        else
        {
            UTAController.Instance.PlayAnimation("A_LeftTutoOk_Disparition");
        }

        _tutoInfo.leftTutoOKButton.interactable = apparition;
    }

    /// <summary>
    /// Manage the elements of the screen top for the animation
    /// </summary>
    /// <param name="apparition"></param>
    private void AnimApparitionTopInGame(bool apparition)
    {
        if (apparition)
            UTAController.Instance.PlayAnimation("A_TopInGame_Apparition");
        else
            UTAController.Instance.PlayAnimation("A_TopInGame_Disparition");
    }

    /// <summary>
    /// Manage tuto panel animation
    /// </summary>
    /// <param name="apparition"></param>
    private void AnimApparitionLeftTutoPanel(bool apparition)
    {
        if (apparition)
            UTAController.Instance.PlayAnimation("A_LeftTutoPanel_Apparition");
        else
            UTAController.Instance.PlayAnimation("A_LeftTutoPanel_Disparition");
    }

    /// <summary>
    /// Listeners for back menu button and restart button for the first step in level 1
    /// </summary>
    /// <param name="isBackMenu"></param>
    /// <param name="apparition"></param>
    private void OnBackMenuRestButtonFirstCase(bool isBackMenu, bool apparition)
    {
        if (apparition)
        {
            AnimApparitionTopInGame(false);
            AnimApparitionLeftTutoPanel(false);

            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_On");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_On");   
        }
        else if (!apparition)
        {
            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_Off");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_Off");

            AnimApparitionTopInGame(true);
            AnimApparitionLeftTutoPanel(true);
        }

        activeBackRestPanel = apparition;
    }

    /// <summary>
    /// Listeners for back menu button and restart button for the second step in level 1
    /// </summary>
    /// <param name="isBackMenu"></param>
    /// <param name="apparition"></param>
    private void OnBackMenuRestButtonSecondCase(bool isBackMenu, bool apparition)
    {
        if (apparition)
        {
            AnimApparitionTopInGame(false);
            AnimApparitionLeftTutoPanel(false);
            UTAController.Instance.PlayAnimation("A_GamePanel_Disparition");
            
            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_On");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_On");


        }
        else if (!apparition)
        {
            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_Off");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_Off");

            AnimApparitionTopInGame(true);
            AnimApparitionLeftTutoPanel(true);
            UTAController.Instance.PlayAnimation("A_GamePanel_Apparition");

            
        }

        activeBackRestPanel = apparition;
    }

    /// <summary>
    /// Listeners for back menu button and restart button for the third step in level 1
    /// </summary>
    /// <param name="isBackMenu"></param>
    /// <param name="apparition"></param>
    private void OnBackMenuRestButtonThirdCase(bool isBackMenu, bool apparition)
    {
        if (apparition)
        {
            AnimApparitionTopInGame(false);
            AnimApparitionLeftTutoPanel(false);
            UTAController.Instance.PlayAnimation("A_GamePanel_Disparition");
            UTAController.Instance.PlayAnimation("A_Minimap_Disparition");

            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_On");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_On");
        }
        else if (!apparition)
        {
            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_Off");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_Off");

            AnimApparitionTopInGame(true);
            AnimApparitionLeftTutoPanel(true);
            UTAController.Instance.PlayAnimation("A_GamePanel_Apparition");
            UTAController.Instance.PlayAnimation("A_Minimap_Apparition");
        }

        activeBackRestPanel = apparition;
    }

    /// <summary>
    /// Listeners for back menu button and restart button for the forth step in level 1
    /// </summary>
    /// <param name="isBackMenu"></param>
    /// <param name="apparition"></param>
    private void OnBackMenuRestButtonLastCase(bool isBackMenu, bool apparition)
    {
        if (apparition)
        {
            AnimApparitionTopInGame(false);
            UTAController.Instance.PlayAnimation("A_GamePanel_Disparition");
            UTAController.Instance.PlayAnimation("A_Minimap_Disparition");

            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_On");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_On");
        }
        else if (!apparition)
        {
            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_Off");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_Off");

            AnimApparitionTopInGame(true);
            UTAController.Instance.PlayAnimation("A_GamePanel_Apparition");
            UTAController.Instance.PlayAnimation("A_Minimap_Apparition");
        }
    }

    /// <summary>
    /// Listeners for back menu button and restart button for the level 2 / 3 / 5
    /// </summary>
    /// <param name="isBackMenu"></param>
    /// <param name="apparition"></param>
    private void OnBackMenuRestButtonTuto(bool isBackMenu, bool apparition)
    {
        if (apparition)
        {
            AnimApparitionLeftTutoPanel(false);

            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_On");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_On");
        }
        else if (!apparition)
        {
            if (isBackMenu)
                UTAController.Instance.PlayAnimation("A_BackMenuPanel_Off");
            else
                UTAController.Instance.PlayAnimation("A_RestartMenuPanel_Off");

            AnimApparitionLeftTutoPanel(true);
        }

        activeBackRestPanel = apparition;
    }

    // ======================================= Unused Functions ======================================= Unused Functions ======================================= Unused Functions ======================================

    /*private void HighlightWorldGO(GameObject go)
    {
        RectTransform r1 = _tutoInfo.hole.GetComponent<RectTransform> ();
        r1.parent.SetParent(GameObject.Find ("Canvas").transform);
        Vector2 viewportPoint = Camera.main.WorldToViewportPoint (go.transform.position);
        r1.anchorMin = viewportPoint;
        r1.anchorMax = viewportPoint;
        r1.anchoredPosition = Vector2.zero;

        // Tests for size delta
        RectTransform rectT = go.transform.Find("CanvasSelection").gameObject.GetComponent<RectTransform>();
        //r1.sizeDelta = rectT.sizeDelta;//new Vector2 (rectT.rect.width, rectT.rect.height);

        Debug.Log ("");

        _tutoInfo.hole.SetActive (true);
        _tutoInfo.darkPanel.SetActive (true);
    }

    private void HighlightUIGO(GameObject go)
    {
        RectTransform r1 = _tutoInfo.hole.GetComponent<RectTransform> ();
        RectTransform r2 = go.GetComponent<RectTransform> ();
        if (r2 == null) {
            Debug.Log ("Problem during HighlightUIGO. Couldn't get the RectTransform of "+go.name);
            return;
        }
        r1.SetParent(r2.parent);

        r1.anchorMax = r2.anchorMax;
        r1.anchorMin = r2.anchorMin;
        r1.pivot = r2.pivot;
        r1.anchoredPosition = r2.anchoredPosition;
        r1.sizeDelta = r2.sizeDelta;
        _tutoInfo.hole.SetActive (true);
        _tutoInfo.darkPanel.SetActive (true);
    }*/
}

