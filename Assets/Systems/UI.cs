using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using FYFY;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using PrimitiveFactory.Framework.UITimelineAnimation;

/*********************************************************
 * 
 * System : UI
 * Manage all UI elements in game (screen and world space)
 * 
*********************************************************/
public class UI : UtilitySystem
{
	// =================================================================================================================================================================================================================
	// ============================================ VARIABLES ============================================ VARIABLES ============================================ VARIABLES ============================================
	// =================================================================================================================================================================================================================

	// ============================================ Families ============================================= Families ============================================= Families =============================================
	private Family _gameInformationFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
	private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations), typeof(ScoreVar)));
    private Family _selectInformationFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SelectInformation)));
	private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
	private Family _previousActionsFamily = FamilyManager.getFamily (new AllOfComponents (typeof (PreviousActions)));
	private Family _spriteContainerFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SpriteContainer)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GlobalUI), typeof(OtherPanel), typeof(LoadingUI)));

	private Family _shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
	private Family _enemyShipFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Enemy), typeof(ShipInfo), typeof(Position), typeof(Mass)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
	private Family _missileFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions), typeof(Movement), typeof(Position), typeof(Mass)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _missileAllyFamily = FamilyManager.getFamily (new AnyOfLayers (10), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));

	private Family _fieldsCounterFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FieldsCounter)));
	private Family _editableFieldsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Field), typeof(Dimensions), typeof(Position), typeof(Editable)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));

	private Family _terrainFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family _effectsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Effects)));
	private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));

	private Family _aiComputationFamily = FamilyManager.getFamily (new AllOfComponents (typeof(AIComputing)));
	private Family _endPanelFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ShowEndPanel)));
	private Family _refreshAddFFfamily = FamilyManager.getFamily (new AllOfComponents (typeof(UpdateUIAddFF)));

	private Family _mainLoopFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));

	private Family _tutoInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(TutoInformation)));

	private Family _idFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FieldID)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _soundFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Sound)));
	private Family _buttonFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Button)), new NoneOfComponents(typeof(AddFFButton)));


	// ============================================ Components ============================================ Components =========================================== Components ==========================================
	private GameInformations _GameInfos;
	private Terrain _theTerrain;
	private GameObject _levelInfoGO;
	private IsDragging _isDragging;
	private SpriteContainer _spriteContainer;
	private State _currentState;
	private MainLoop _mainLoop;
	private PreviousActions _previousActions;
    private ScoreVar _scoreVar;
    private LevelInformations _levelInfoComp;
	private Sound _sound;

    // ========================================= UI Components ========================================= UI Components ========================================= UI Components =========================================
    private GlobalUI _globalUI;
    private OtherPanel _otherPanel;
    private LoadingUI _loadingUI;
    private SliderComponent _SliderComponent;

    // ============================================= Raycast ============================================== Raycast ============================================== Raycast =============================================
    private RaycastHit _hit;
	private Ray _ray;
	// Raycaster UI things
	List<RaycastResult> _results;

    // ============================================== Others ============================================== Others =============================================== Others ==============================================
    private bool boolAutoDestruction;
    private bool currentStateGlobalUI;

    // =================================================================================================================================================================================================================
    // ============================================= METHODS ============================================== METHODS ============================================== METHODS =============================================
    // =================================================================================================================================================================================================================

    /// <summary>
    /// Constructor -- like Unity Start()
    /// </summary>
    public UI()
	{ 
		// Recovery components or gameobjects
		_mainLoop = _mainLoopFamily.First ().GetComponent<MainLoop> ();
		_GameInfos = _gameInformationFamily.First().GetComponent<GameInformations>();
		_theTerrain = _terrainFamily.First().GetComponent<Terrain>();
		_levelInfoGO = _levelInfoFamily.First();
        _levelInfoComp = _levelInfoGO.GetComponent<LevelInformations>();
        _scoreVar = _levelInfoGO.GetComponent<ScoreVar>();
        _spriteContainer = _spriteContainerFamily.First().GetComponent<SpriteContainer>();
		_currentState = _stateFamily.First ().GetComponent<State> ();
		_previousActions = _previousActionsFamily.First ().GetComponent<PreviousActions> ();
        _globalUI = _interfaceFamily.First().GetComponent<GlobalUI>();
        _otherPanel = _interfaceFamily.First().GetComponent<OtherPanel>();
        _loadingUI = _interfaceFamily.First().GetComponent<LoadingUI>();
		_sound = _soundFamily.First ().GetComponent<Sound> ();

        _results = new List<RaycastResult> ();

		// Initialize Interface
		InitUI();
		UpdateLevelText();
		UpdateFFRemaining();

		// Families Callback
		_missileFamily.addExitCallback (OnMissileExited);
		_missileAllyFamily.addExitCallback (OnAllyMissileExited);
		_aiComputationFamily.addExitCallback (OnAIComputationExited);
		_endPanelFamily.addEntryCallback (OnEndPanelEntered);
		_refreshAddFFfamily.addEntryCallback (RefreshAddFFbuttons);

		// Lock the interface and  recovery component isDragging
		_isDragging = _dragFamily.First().GetComponent<IsDragging>();


		// Sound for all buttons except fireButton, DeleteButton and nextDialogButton (has its own sound)
		foreach (GameObject goWithButton in _buttonFamily) {
			if (goWithButton != _globalUI.fireButton.gameObject && goWithButton != _globalUI.deleteButton.gameObject && goWithButton != _interfaceFamily.First().GetComponent<OtherPanel>().nextDialogButton.gameObject )
				goWithButton.GetComponent<Button>().onClick.AddListener(() => AddButtonSound());
		}
	}

	/// <summary>
	/// Use to process families
	/// </summary>
	/// <param name="familiesUpdateCount"></param>
	protected override void onProcess(int familiesUpdateCount)
	{
		if (_currentState.state == State.STATES.SETUP)
			DetectGameObject ();

        if ((_currentState.state == State.STATES.ANIM1 || _currentState.state == State.STATES.ANIM2 || _currentState.state == State.STATES.DIALOG))
        {
            //StateGlobalUI(false);
        }

        if (_currentState.state == State.STATES.SETUP && !currentStateGlobalUI)
        {
            currentStateGlobalUI = true;

            if (_scoreVar.nbShot > 0 && _GameInfos.noLevel != 1)
                UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Disparition");

            AnimApparitionGamePanel(true);
        }

        if (_currentState.state == State.STATES.PLAYING && boolAutoDestruction)
        {
            boolAutoDestruction = false;

            if (_GameInfos.noLevel != 1)
            {
                _globalUI.autodestructionButton.interactable = true;
                UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Apparition");
            }
        }

		if (_previousActionsFamily.First ().GetComponent<PreviousActions>().listOfPreviousActions.Count == 0 && _globalUI.undoButton.interactable)
			_globalUI.undoButton.interactable = false;
		else if (_previousActionsFamily.First ().GetComponent<PreviousActions>().listOfPreviousActions.Count > 0 && !_globalUI.undoButton.interactable)
            _globalUI.undoButton.interactable = true;

		if (_previousActionsFamily.First ().GetComponent<PreviousActions>().listOfRedoActions.Count == 0 && _globalUI.redoButton.interactable)
            _globalUI.redoButton.interactable = false;
		else if (_previousActionsFamily.First ().GetComponent<PreviousActions>().listOfRedoActions.Count > 0 && !_globalUI.redoButton.interactable)
            _globalUI.redoButton.interactable = true;
	}

	/// <summary>
	/// Detection the selected GameObject to display good panel
	/// </summary>
	private void DetectGameObject()
	{
		_ray = Camera.main.ScreenPointToRay(MouseOrTouchPosition());

		// Layer Mask
		int layerMask = 1 << 24;

		if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, layerMask))
		{
			if (GetMouseOrTouchDown())
			{
				// Different raycast for UI
				PointerEventData pointerData = new PointerEventData(EventSystem.current);
				pointerData.position = MouseOrTouchPosition();
				EventSystem.current.RaycastAll(pointerData, _results);

				// If we are not over UI
				bool isOverUI = false;
				if (_results.Count > 0) {
					foreach (RaycastResult rr in _results) {
						// Check if it's an animation from tuto, then ignore it
						if(!rr.gameObject.CompareTag("TutoTag")){
							isOverUI = true;
						}
					}
				}

				if(!isOverUI)
                {
					GameObject theGo = _hit.collider.transform.gameObject;

					theGo = _hit.collider.transform.parent.gameObject;

					ActiveCanvasSelection(theGo);
					DisableAllPanel();

					if (theGo == _shipFamily.First())
                    {
						_globalUI.shipPanel.SetActive(true);
					}
					else
                    {
						switch (theGo.tag)
                        {
						case "Field":
							_globalUI.forcePanel.SetActive(true);
							ForceInformation(theGo);
							break;
						case "bonus":
							_globalUI.bmPanel.SetActive(true);
							BonusMalusInformation(theGo);
							break;
						}
					}
				}

				_results.Clear ();
			}
		}
	}

	/// <summary>
	/// Disable all panels in the interface
	/// </summary>
	private void DisableAllPanel()
	{
		_globalUI.shipPanel.SetActive(false);
		_globalUI.forcePanel.SetActive(false);
		_globalUI.bmPanel.SetActive(false);
	}

	/// <summary>
	/// UI Initialization Manager 
	/// </summary>
	private void InitUI()
	{
		// Back Menu Button
		_globalUI.backMenuButton.onClick.AddListener(() => DisplayBackMenuPanel());

        // Restart Button
        _globalUI.restartButton.onClick.AddListener(() => DisplayRestartPanel());

        //Other Buttons
        _globalUI.autodestructionButton.onClick.AddListener(() => OnAutoDestruction());
        _globalUI.undoButton.onClick.AddListener(() => OnCancel(true));
        _globalUI.redoButton.onClick.AddListener(() => OnCancel(false));

        // Button Panel
        _globalUI.fireButton.onClick.AddListener(() => OnFireButton());
        //_AttractiveButton.onClick.AddListener(() => OnAddFFButton("attractive"));
        //_RepulsiveButton.onClick.AddListener(() => OnAddFFButton("repulsive"));

        // Ship Panel
        _globalUI.speedSlider.onValueChanged.AddListener((float value) => OnSliderSpeed(value));

        // Force Panel 
        _globalUI.intensitySlider.onValueChanged.AddListener((float value) => OnIntensityForceSlider(value));
        _SliderComponent = _globalUI.intensitySlider.GetComponent<SliderComponent>();
        _globalUI.deleteButton.onClick.AddListener(() => OnDeleteFFButton());

        // Back Menu Confirmation Panel
        _otherPanel.yesBackButton.onClick.AddListener(() => OnYesBackButton());
        _otherPanel.noBackButton.onClick.AddListener(() => OnNoBackButton());

		// Restart Confirmation Panel
		_otherPanel.yesRestButton.onClick.AddListener(() => OnYesRestButton());
		_otherPanel.noRestButton.onClick.AddListener(() => OnNoRestButton());

		// Won Panel
		_otherPanel.retryWonButton.onClick.AddListener(() => OnYesRestButton());
		_otherPanel.backMenuWonButton.onClick.AddListener(() => OnYesBackButton());
		_otherPanel.nextLevelButton.onClick.AddListener(() => OnNextLevelButton());

		// Lost Panel
		_otherPanel.retryLostButton.onClick.AddListener(() => OnYesRestButton());
        _otherPanel.backMenuLostButton.onClick.AddListener(() => OnYesBackButton());

        _loadingUI.loadingCanvas.enabled = false;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="theState"></param>
    private void StateGlobalUI(bool theState)
    {
        _globalUI.miniMap.SetActive(theState);
        _globalUI.gamePanel.SetActive(theState);

        currentStateGlobalUI = theState;
    }

	/// <summary>
	/// When the Foe AI finished its computation
	/// Check if there are any remaining missiles
	/// Else change the state to the state Setup where the player can interact with objects
	/// Unlock the interface and apply effect of any bonus / malus
	/// </summary>
	/// <param name="id"></param>
	private void OnAIComputationExited(int id)
	{
		if (_aiComputationFamily.Count == 0)
		{ 
			// the last one to exit
			Debug.Log("Computation over!");
			// Hide Computing Panel
			_globalUI.AIPanel.SetActive(false);

			if (_missileFamily.Count != 0)
				Debug.Log("Still playing...");
			else
			{
				if (_currentState.state == State.STATES.PLAYING)
				{
					Debug.Log ("Let's retry!");
                    _globalUI.autodestructionButton.interactable = false;
                    Retry ();
					ChangeCurrentEffects ();
				}
			}
		}
	}

	/// <summary>
	/// During the state playing and there is no remaining missile
	/// Check if there are any remaining AiComputation components
	/// Else change the state to the state Setup where the player can interact with objects
	/// Unlock the interface and apply effect of any bonus / malus
	/// </summary>
	/// <param name="id"></param>
	private void OnMissileExited(int id)
	{
		if (_missileFamily.Count == 0)
		{
			// the last one to exit
			if (_aiComputationFamily.Count != 0) {
				Debug.Log ("Computing...");
				// Show Computing Panel
				_globalUI.AIPanel.SetActive(true);
			}
			else
			{
				if (_currentState.state == State.STATES.PLAYING)
				{
					Debug.Log ("Time to retry!");
                    _globalUI.autodestructionButton.interactable = false;
					Retry ();
					ChangeCurrentEffects ();
				}
			}
		}
	}

	private void OnAllyMissileExited(int id){
		if (_missileAllyFamily.Count == 0) {
			_globalUI.autodestructionButton.interactable = false;
			_sound.fire.stop (FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	/// <summary>
	/// Changes the bonus / malus effect in the current round
	/// </summary>
	private void ChangeCurrentEffects()
	{
		Effects effects;

		foreach (GameObject go in _effectsFamily)
		{
			effects = go.GetComponent<Effects> ();
			effects.current.Clear ();

			foreach (Bonus.TYPE bonusMalus in effects.next)
				effects.current.Add (bonusMalus);

			effects.next.Clear ();
		}
	}

	/// <summary>
	/// Change state playing to setup
	/// Enable moving editable force fields
	/// Display trajectories
	/// </summary>
	private void Retry()
	{
		State currentState = _stateFamily.First().GetComponent<State>();
		currentState.state = State.STATES.SETUP;

		GameObject ship = _shipFamily.First();

		if (ship.GetComponent<Editable>() != null)
		{
			ship.GetComponent<Editable>().editable = true;
		}

		SelectInformation selection = _selectInformationFamily.First().GetComponent<SelectInformation>();

		foreach (GameObject src in _editableFieldsFamily)
		{
			src.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
			src.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

			src.GetComponent<Editable>().editable = true;
		}

		GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
	}

	/// <summary>
	/// If the current level is the last one --> disabled the next level button
	/// </summary>
	/// <param name="go"></param>
	private void OnEndPanelEntered(GameObject go)
	{
		bool wonPanel = go.GetComponent<ShowEndPanel>().wonPanel;

		if (wonPanel)
		{
            int score = _scoreVar.playerScore();
            _otherPanel.scoreText.text = score.ToString();

            _otherPanel.wonPanel.SetActive(true);

            if (score <= _levelInfoComp.firstNbIntervalScore)
            {
                UTAController.Instance.PlayAnimation("A_WonPanel_Apparition_01");
            }
            else if (score < _levelInfoComp.lastNbIntervalScore)
            {
                UTAController.Instance.PlayAnimation("A_WonPanel_Apparition_02");
            }
            else
            {
                UTAController.Instance.PlayAnimation("A_WonPanel_Apparition_03");
            }       

			if (_GameInfos.noLevel + 1 > _GameInfos.totalLevels)
			{
				_otherPanel.nextLevelButton.interactable = false;
			}
			else
			{
                _otherPanel.nextLevelButton.interactable = true;

				/*if (_GameInfos.unlockedLevels < _GameInfos.noLevel + 1)
				{
					_GameInfos.unlockedLevels += 1;
				}*/
			}
			if(_GameInfos.playMusics)
				_sound.musicWin.start ();
		}
		else
		{
			_otherPanel.lostPanel.SetActive(true);
            UTAController.Instance.PlayAnimation("A_LostPanel_Apparition");
			if(_GameInfos.playMusics)
				_sound.musicLose.start ();
        }

        _globalUI.autodestructionButton.gameObject.SetActive (false);

		GameObjectManager.removeComponent<ShowEndPanel>(go);
	}

    /// <summary>
    /// Active the canvas having the image for selection of the Gameobject in param
    /// </summary>
    /// <param name="theGo"></param>
    private void ActiveCanvasSelection(GameObject theGo)
    {
        ResetSelection();

        SelectInformation currentSelection = _selectInformationFamily.First().GetComponent<SelectInformation>();
        currentSelection.selectedGameObject = theGo;

        currentSelection.canvasSelectionGO = theGo.transform.Find("CanvasSelection").gameObject;
        currentSelection.canvasSelectionGO.SetActive(true);

        switch (theGo.tag)
        {
            case "ShipTag":
                currentSelection.otherCanvasGO = theGo.transform.Find("CanvasPlayerShip").gameObject;
                break;
            case "Field":
                currentSelection.otherCanvasGO = theGo.transform.Find("CanvasTriangle").gameObject;
                break;
        }

        if (currentSelection.otherCanvasGO != null)
            currentSelection.otherCanvasGO.SetActive(false);
    }

    /// <summary>
    /// Reset the current selection
    /// </summary>
    private void ResetSelection()
    {
        SelectInformation currentSelection = _selectInformationFamily.First().GetComponent<SelectInformation>();
        currentSelection.selectedGameObject = null;

        if (currentSelection.canvasSelectionGO != null)
        {
            currentSelection.canvasSelectionGO.SetActive(false);
            currentSelection.canvasSelectionGO = null;
        }

        if (currentSelection.otherCanvasGO != null)
        {
            currentSelection.otherCanvasGO.SetActive(true);
            currentSelection.otherCanvasGO = null;
        }

        DisableAllPanel();
    }

    /// <summary>
    /// 
    /// </summary>
    private void AnimApparitionGamePanel(bool apparition)
    {
        if (apparition)
        {
            UTAController.Instance.PlayAnimation("A_GamePanel_Apparition");
            UTAController.Instance.PlayAnimation("A_Minimap_Apparition");
        }
        else
        {
            UTAController.Instance.PlayAnimation("A_GamePanel_Disparition");
            UTAController.Instance.PlayAnimation("A_Minimap_Disparition");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apparition"></param>
    private void AnimApparitionTopInGame(bool apparition)
    {
        if (apparition)
            UTAController.Instance.PlayAnimation("A_TopInGame_Apparition");
        else
            UTAController.Instance.PlayAnimation("A_TopInGame_Disparition");
    }

    // ========================================== Level Panel ========================================== Level Panel ========================================== Level Panel ==========================================

    /// <summary>
    /// Display the current level
    /// </summary>
    private void UpdateLevelText()
	{
		_globalUI.levelText.text = "Niveau " + _GameInfos.noLevel;
	}

	// ====================================================== Back Menu ====================================================== Back Menu ======================================================

	/// <summary>
	/// 
	/// </summary>
	private void DisplayBackMenuPanel()
	{
        if (currentStateGlobalUI)
            AnimApparitionGamePanel(false);

        if (_currentState.state == State.STATES.PLAYING)
            UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Disparition");

        if (_currentState.state == State.STATES.DIALOG)
            UTAController.Instance.PlayAnimation("A_DialogPanel_Off");

        AnimApparitionTopInGame(false);

        //_otherPanel.backMenuPanel.SetActive(true);
        UTAController.Instance.PlayAnimation("A_BackMenuPanel_On");
	}

	/// <summary>
	/// When players confirms to come back to the Main Menu
	/// Load the main menu scene
	/// </summary>
	private void OnYesBackButton()
	{
		GameObjectManager.loadScene("MenuScene");
	}

    /// <summary>
    /// 
    /// </summary>
    private void OnNoBackButton()
    {
        UTAController.Instance.PlayAnimation("A_BackMenuPanel_Off");

        if (currentStateGlobalUI)
            AnimApparitionGamePanel(true);

        if (_currentState.state == State.STATES.PLAYING)
            UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Apparition");

        if (_currentState.state == State.STATES.DIALOG)
            UTAController.Instance.PlayAnimation("A_DialogPanel_On");

        AnimApparitionTopInGame(true);
    }

    // ======================================================= Restart ======================================================= Restart =======================================================

    /// <summary>
    /// Display or hide the confirmation panel to restart the level
    /// activeState = true --> active the confirmation panel and hide other UI
    /// activeState = false --> inactive the confirmation panel and display other UI
    /// </summary>
    /// <param name="activeState"></param>
    private void DisplayRestartPanel()
	{
        if (currentStateGlobalUI)
            AnimApparitionGamePanel(false);

        if (_currentState.state == State.STATES.PLAYING)
            UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Disparition");

		if (_currentState.state == State.STATES.DIALOG)
			UTAController.Instance.PlayAnimation ("A_DialogPanel_Off");

        AnimApparitionTopInGame(false);
        //_otherPanel.restartPanel.SetActive(true);
        UTAController.Instance.PlayAnimation("A_RestartMenuPanel_On");
    }

	/// <summary>
	/// If player confirms to restart the level --> load the current scene
	/// </summary>
	private void OnYesRestButton()
	{
		_mainLoop.StartCoroutine(LoadScreen());
	}

    /// <summary>
    /// 
    /// </summary>
    private void OnNoRestButton()
    {
        UTAController.Instance.PlayAnimation("A_RestartMenuPanel_Off");
        //_otherPanel.restartPanel.SetActive(false);

        if (currentStateGlobalUI)
            AnimApparitionGamePanel(true);

        if (_currentState.state == State.STATES.PLAYING)
            UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Apparition");

        if (_currentState.state == State.STATES.DIALOG)
            UTAController.Instance.PlayAnimation("A_DialogPanel_On");

        AnimApparitionTopInGame(true);
    }

	// TODO: Same function as in LevelMenu; maybe use a callback to only define it once
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
	private IEnumerator LoadScreen()
    {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync ("GoodScene");
		asyncLoad.allowSceneActivation = false;
		_globalUI.mainCanvas.enabled = false;
		_loadingUI.loadingCanvas.enabled = true;

		while (asyncLoad.progress < 0.9f)
        {
			Debug.Log ("progress: " + asyncLoad.progress); // fill the bar according to asyncLoad.progress (between 0 and 0.9)
			_loadingUI.loadingSlider.value = asyncLoad.progress;
			yield return null;
		}

		Debug.Log ("Loading ended"); // fill the bar
        _loadingUI.loadingSlider.value = 1.0f;
		asyncLoad.allowSceneActivation = true; // we can activate the scene.
	}

	// ====================================================== GAME PANEL ====================================================== GAME PANEL ======================================================

	// ===================================================== Button Panel ===================================================== Button Panel =====================================================

	/// <summary>
	/// Creates the missiles 
	/// Change missiles speed
	/// Apply any bonus / malus effects
	/// </summary>
	/// <param name="shipLauncher">Ship launcher.</param>
	private void CreateMissiles(GameObject ship)
	{
		int angle = 0;
		int signe = 1;
		bool alternate = true;
		float oldMagnitude = ship.GetComponent<ShipInfo>().fireIntensity.magnitude;
		ShipInfo sInfo = ship.GetComponent<ShipInfo>();

		for (int i = 0; i < sInfo.nbMissiles; i++)
		{
			Vector3 terrDims = _theTerrain.terrainData.size;
			//GameObject missile = GameObject.Instantiate(ship.GetComponent<ShipInfo>().shipMissilePrefab);
			GameObject missile = GetGOFromPool(ship.GetComponent<ShipInfo>().missilePool);
			// Just in case it has been instantiate
			missile.GetComponent<ExplosionMissile>().goWithPool = ship.GetComponent<ShipInfo>().missilePool.GetComponent<PoolMissileImpacts>().poolImpacts[0].GetComponent<ExplosionMissile>().goWithPool;
			//GameObjectManager.bind(missile);
			GameObjectManager.setGameObjectState(missile, true);
			changePosition(missile.GetComponent<Position>(), ship.GetComponent<Position>().initialPos, terrDims);
			Movement mv = missile.GetComponent<Movement>();
			mv.speed = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (sInfo.angle + angle * signe)), Mathf.Sin(Mathf.Deg2Rad * (sInfo.angle + angle * signe)), 0);
			mv.speed.Normalize();
			mv.speed *= oldMagnitude;
			if (alternate)
			{
				angle += 30;
			}
			else
			{
				signe *= -1;
			}
			alternate = !alternate;

			// If bonus damage
			if (ship.GetComponent<Effects> ()) {
				foreach (Bonus.TYPE bonusMalus in ship.GetComponent<Effects>().current) {
					if (bonusMalus == Bonus.TYPE.B_Player)
						missile.GetComponent<ExplosionMissile> ().damage += 1;
				}
			}
		}
	}

	/// <summary>
	/// When the player clicks on Fire Button
	/// Inactive interface and selection
	/// Create missiles, freeze player's force fields position
	/// Start AI computation
	/// Change the state setup to playing
	/// </summary>
	private void OnFireButton()
	{
        // Locks everything
        AnimApparitionGamePanel(false);
        currentStateGlobalUI = false;
        _scoreVar.nbShot++;

        ResetSelection();

		State currentState = _stateFamily.First().GetComponent<State>();

		GameObject ship = _shipFamily.First();

		// Create the missiles if state is SETUP
		if (currentState.state == State.STATES.SETUP)
		{
			CreateMissiles(ship);
			foreach (GameObject enemyShip in _enemyShipFamily)
			{
				CreateMissiles(enemyShip);
			}
		}

		// make ship launcher not editable anymore
		if (ship.GetComponent<Editable>() != null && ship.GetComponent<Editable>().editable)
			ship.GetComponent<Editable>().editable = false;

		// make fields not editable anymore
		foreach (GameObject field in _editableFieldsFamily)
		{
			field.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		}

		GameObjectManager.addComponent<AIShouldCompute>(_levelInfoGO);

		currentState.state = State.STATES.PLAYING;
        boolAutoDestruction = true;

		if(_GameInfos.playSounds)
			//FMODUnity.RuntimeManager.PlayOneShot (_sound.FireEvent);
			_sound.fire.start ();
	}

	/// <summary>
	/// Active a attratice or repulsive force field
	/// Update the number of remaining fields
	/// Active the canvas to see the selection around the force field
	/// Active Force Panel to display information
	/// </summary>
	private void OnAddFFButton(string type)
	{
		FieldsCounter fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();
		GameObject field = null;

		if (type == "attractive")
		{
			if (fieldsRemaining.fieldsAttPlaced < fieldsRemaining.fieldsAttToPlace)
			{
				field = fieldsRemaining.poolAttractive[fieldsRemaining.fieldsAttPlaced].gameObject;
				fieldsRemaining.fieldsAttPlaced++;

				if(_GameInfos.playSounds)
					FMODUnity.RuntimeManager.PlayOneShot (_sound.AttFieldPlacedEvent);
			}
		}
		else if (type == "repulsive")
		{
			if (fieldsRemaining.fieldsRepPlaced < fieldsRemaining.fieldsRepToPlace)
			{
				field = fieldsRemaining.poolRepulsive[fieldsRemaining.fieldsRepPlaced].gameObject;
				fieldsRemaining.fieldsRepPlaced++;

				if(_GameInfos.playSounds)
					FMODUnity.RuntimeManager.PlayOneShot (_sound.RepFieldPlacedEvent);
			}
		}

		GameObjectManager.setGameObjectState(field, true);
		ActiveCanvasSelection(field);
		DisableAllPanel();
        _globalUI.forcePanel.SetActive(true);
		ForceInformation(field);
		UpdateFFRemaining();

		changePosition (field.GetComponent<Position> (), field.GetComponent<Position> ().pos, _theTerrain.terrainData.size);
		//GameObjectManager.setGameObjectState(field.transform.GetChild(0).gameObject, true); // old cylinder 
		GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
		GameObjectManager.addComponent<RefreshTerrain>(_levelInfoGO, new { action = RefreshTerrain.ACTIONS.ADD, source = field });


	}


	public void AddButtonSound(){
		if(_GameInfos.playSounds)
			FMODUnity.RuntimeManager.PlayOneShot (_sound.ButtonEvent);
	}

	/// <summary>
	/// Manage the remaining force fields display
	/// Active added FF's selection
	/// Display FF information
	/// </summary>
	/// <param name="go"></param>
	private void RefreshAddFFbuttons(GameObject go)
	{
		UpdateUIAddFF[] updtUIAddFF = go.GetComponents<UpdateUIAddFF>();

		foreach (UpdateUIAddFF updt in updtUIAddFF)
		{
			// we just added a FF via PlayerActions
			if (updt.addff)
			{
				GameObject field = updt.field; //updtUIAddFF [updtUIAddFF.Length - 1].field;
				ActiveCanvasSelection(field);
				DisableAllPanel();
				_globalUI.forcePanel.SetActive(true);
				ForceInformation(field);
				UpdateFFRemaining();
			}
			// we just dropped a FF in a undropable zone
			else
			{
				UpdateFFRemaining();
                _globalUI.forcePanel.SetActive(false);
			}

			GameObjectManager.removeComponent<UpdateUIAddFF>(go);
		}
	}

	/// <summary>
	/// Update texts to display the number of force field repulsive or attractive the player can still place
	/// If the numer of attractive force fields to place is equal 0 = the button to add attractive ins't interactable
	/// Same for repulsive force fields
	/// </summary>
	private void UpdateFFRemaining()
	{
		Color enabledColor = new Color (1F, 1F, 1F);
		Color disabledColor = new Color (0.3F, 0.3F, 0.3F);

		FieldsCounter fieldsAlreadyPlaced = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();

		int fieldsRepRemaining = fieldsAlreadyPlaced.fieldsRepToPlace - fieldsAlreadyPlaced.fieldsRepPlaced;
		int fieldsAttRemaining = fieldsAlreadyPlaced.fieldsAttToPlace - fieldsAlreadyPlaced.fieldsAttPlaced;

        if (_scoreVar.nbShot > 0)
        {
            if (_scoreVar.nbFieldRemain > fieldsAttRemaining + fieldsRepRemaining)
                _scoreVar.nbFieldRemain = fieldsAttRemaining + fieldsRepRemaining;
        }
        else
            _scoreVar.nbFieldRemain = fieldsAttRemaining + fieldsRepRemaining;

        _globalUI.attractiveRemaining.text = fieldsAttRemaining.ToString();
		_globalUI.repulsiveRemaining.text = fieldsRepRemaining.ToString();

		if (fieldsAttRemaining > 0) {    
			_globalUI.attractiveButton.interactable = true;
			//_globalUI.attRemain.color = enabledColor;
			_globalUI.attractiveRemaining.color = enabledColor;
		} 
		else
		{
			_globalUI.attractiveButton.interactable = false;
            //_globalUI.attRemain.color = disabledColor;
            _globalUI.attractiveRemaining.color = disabledColor;
		}

		if (fieldsRepRemaining > 0)
        {
			_globalUI.repulsiveButton.interactable = true;
			//_globalUI.repRemain.color = enabledColor;
			_globalUI.repulsiveRemaining.color = enabledColor;
		}
		else
		{
            _globalUI.repulsiveButton.interactable = false;
            //_globalUI.repRemain.color = disabledColor;
            _globalUI.repulsiveRemaining.color = disabledColor;
		}
	}

    /// <summary>
    /// 
    /// </summary>
	private void OnAutoDestruction()
    {
		foreach (GameObject missile in _missileFamily)
        {
			if(missile.layer == 10)
				GameObjectManager.addComponent<Kamikaze> (missile);
		}
        _globalUI.autodestructionButton.interactable = false;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
	private GameObject getGOFromID(int i)
    {
		foreach (GameObject go in _idFamily)
        {
			if (go.GetComponent<FieldID> ().id == i)
				return go;
		}
		return null;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="undo"></param>
	private void OnCancel(bool undo){

		int lastAction;
		int lastData;
		DataGO data;
		PreviousActions.CANCELACTIONS action;

		if (undo)
        {
			lastAction = _previousActions.listOfPreviousActions.Count - 1;
			lastData = _previousActions.cancelData.Count - 1;
			data = _previousActions.cancelData [lastData];
			action = _previousActions.listOfPreviousActions [lastAction];
		}
		else
        {
			lastAction = _previousActions.listOfRedoActions.Count-1;
			lastData = _previousActions.redoData.Count - 1;
			data = _previousActions.redoData[lastData];
			action = _previousActions.listOfRedoActions [lastAction];

			if (action == PreviousActions.CANCELACTIONS.DEL)
				action = PreviousActions.CANCELACTIONS.ADD;
			else if (action == PreviousActions.CANCELACTIONS.ADD)
				action = PreviousActions.CANCELACTIONS.DEL;
		}

		GameObject go;

		switch (action)
        {
		    case PreviousActions.CANCELACTIONS.DEL:
			    go = data.theGO;

			    if (go.GetComponent<Field> ().isRepulsive)
				    OnAddFFButton ("repulsive");
			    else
				    OnAddFFButton ("attractive");

			    SetGameObjectComponents (_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject,data);
			    ForceInformation (_selectInformationFamily.First ().GetComponent<SelectInformation> ().selectedGameObject);

			    break;

		    case PreviousActions.CANCELACTIONS.ADD:
			    go = getGOFromID (data.id);
			    OnDeleteFFButton (go);

			    break;

		    case PreviousActions.CANCELACTIONS.MOVE:
			    go = getGOFromID (data.id);

			    // We store the old position
			    if (undo)
				    _previousActions.cancelData [lastData] = StoreGameObjectComponents (go);
			    else
				    _previousActions.redoData [lastData] = StoreGameObjectComponents (go);

                // We need to temporarly remove the listener in case the field was not selected.
                // If we don't, it will consider the IntensityForceSlider value changed and so will ask the ForcesDisplay to update (SetHeightsTerrainDisplay),
                // but since we changed the position (SetGameObjectComponents), it will update the wrong "place" in the world.
                _globalUI.intensitySlider.onValueChanged.RemoveAllListeners();
			    SetGameObjectComponents (go, data, false, typeof(Position));
			    ActiveCanvasSelection (go);
			    DisableAllPanel ();
			    _globalUI.forcePanel.SetActive (true);
			    ForceInformation (go);
                _globalUI.intensitySlider.onValueChanged.AddListener((float value) => OnIntensityForceSlider(value));

			    GameObjectManager.addComponent<RefreshTerrain> (_levelInfoGO, new { action = RefreshTerrain.ACTIONS.MOVE, source = go });
			    GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
			    break;

		    case PreviousActions.CANCELACTIONS.INTENSITY:
			    go = getGOFromID (data.id);

			    // We store the old intensity
			    if (undo) 
				    _previousActions.cancelData [lastData] = StoreGameObjectComponents (go);
			    else
				    _previousActions.redoData [lastData] = StoreGameObjectComponents (go);

			    SetGameObjectComponents (go, data, false, typeof(Field));
			    ActiveCanvasSelection (go);
			    DisableAllPanel ();
			    _globalUI.forcePanel.SetActive (true);
			    ForceInformation (go);

			    if (go.GetComponent<Field> ().isRepulsive)
				    _globalUI.intensitySlider.value = go.GetComponent<Field> ().A * 100f;
			    else
                    _globalUI.intensitySlider.value = go.GetComponent<Field> ().A * 100f * (-1);

			    break;

		    case PreviousActions.CANCELACTIONS.SPEED:
			    go = data.theGO;

			    // We store the old speed
			    if (undo)
				    _previousActions.cancelData [lastData] = StoreGameObjectComponents (go);
			    else
				    _previousActions.redoData [lastData] = StoreGameObjectComponents (go);

			    ShipInfo shipInfo = go.GetComponent<ShipInfo> ();
			    int tempHealth = shipInfo.health;
			    SetGameObjectComponents (go, data, false, typeof(ShipInfo));
			    ActiveCanvasSelection (go);
			    DisableAllPanel ();
			    _globalUI.shipPanel.SetActive (true);
			    shipInfo.health = tempHealth;

			    _globalUI.speedSlider.value = shipInfo.valueSpeedSlider;

			    break;

		    case PreviousActions.CANCELACTIONS.ANGLE:
			    go = data.theGO;

			    // We store the old angle
			    if (undo)
				    _previousActions.cancelData [lastData] = StoreGameObjectComponents (go);
			    else
				    _previousActions.redoData [lastData] = StoreGameObjectComponents (go);

			    shipInfo = go.GetComponent<ShipInfo> ();
			    tempHealth = shipInfo.health;
			    SetGameObjectComponents (go, data);
			    ActiveCanvasSelection (go);
			    DisableAllPanel ();
			    _globalUI.shipPanel.SetActive (true);
			    shipInfo.health = tempHealth;

			    GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoFamily.First());
			    break;

		    default:
			    break;
		}

		if (undo)
        {
			_previousActions.listOfRedoActions.Add (_previousActions.listOfPreviousActions [lastAction]);
			_previousActions.redoData.Add (_previousActions.cancelData [lastData]);

			_previousActions.cancelData.RemoveAt (lastData);
			_previousActions.listOfPreviousActions.RemoveAt (lastAction);

			if (_previousActions.listOfPreviousActions.Count == 0)
				_globalUI.undoButton.interactable = false;
		}
		else
        {
			_previousActions.listOfPreviousActions.Add (_previousActions.listOfRedoActions [lastAction]);
			_previousActions.cancelData.Add (_previousActions.redoData[lastData]);

			_previousActions.redoData.RemoveAt (lastData);
			_previousActions.listOfRedoActions.RemoveAt (lastAction);

			if (_previousActions.listOfRedoActions.Count == 0)
				_globalUI.redoButton.interactable = false;
		}


	}

	// ======================================================= Ship Panel ======================================================= Ship Panel =======================================================

	/// <summary>
	/// Change the fire intensity of player ship
	/// Refresh player's trajectories
	/// </summary>
	/// <param name="value"></param>
	private void OnSliderSpeed(float value)
	{
		if (GetMouseOrTouchDown())
			_globalUI.speedSlider.GetComponent<SliderUpAndDown> ().clickedHandled = false;

		// We clicked on the slider but haven't handled it yet
		if (!_globalUI.speedSlider.GetComponent<SliderUpAndDown>().clickedHandled)
        {
			_previousActions.listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.SPEED);
			_previousActions.cancelData.Add (StoreGameObjectComponents (_shipFamily.First()));
			_previousActions.listOfRedoActions.Clear ();
            _globalUI.speedSlider.GetComponent<SliderUpAndDown> ().clickedHandled = true;
		}

		_globalUI.speedText.text = Math.Round(value).ToString();

		value = value / 100f;
		ShipInfo _shipInfo = _shipFamily.First().GetComponent<ShipInfo>();

		// New vector for speed and display the new speed
		_shipInfo.fireIntensity.Normalize();
		_shipInfo.fireIntensity *= value;

		_shipInfo.valueSpeedSlider = value * 100f;

		GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
	}

	// ====================================================== Force Panel ====================================================== Force Panel ======================================================

	/// <summary>
	/// Change the selected force field's intensity 
	/// Refresh the terrain and trajectories
	/// </summary>
	/// <param name="value"></param>
	private void OnIntensityForceSlider(float value)
	{
		value = value / 100f;
		SelectInformation currentSelection = _selectInformationFamily.First().GetComponent<SelectInformation>();
		GameObject go = currentSelection.selectedGameObject;
		Field theField = go.GetComponent<Field>();


		if (GetMouseOrTouchDown())
        {
			// Are we over the Force Slider ?
			_ray = Camera.main.ScreenPointToRay(MouseOrTouchPosition());

			// Different raycast for UI to check if we are clicking of a button of a slider
			PointerEventData pointerData = new PointerEventData (EventSystem.current);
			pointerData.position = MouseOrTouchPosition();
			EventSystem.current.RaycastAll (pointerData, _results);

			// If we clicked on a button or a slider, don't move camera
			foreach (RaycastResult rr in _results)
            {
				if (rr.gameObject.name == "Background")
                { 
                    _globalUI.intensitySlider.GetComponent<SliderUpAndDown>().clickedHandled = false;
					break;
				}
			}

			_results.Clear ();
		}

		// We clicked on the slider but haven't handled it yet
		if (!_globalUI.intensitySlider.GetComponent<SliderUpAndDown> ().clickedHandled)
        {
			_previousActions.listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.INTENSITY);
			_previousActions.cancelData.Add (StoreGameObjectComponents (go));
			_previousActions.listOfRedoActions.Clear ();
			_globalUI.intensitySlider.GetComponent<SliderUpAndDown> ().clickedHandled = true;
		}

		if (theField.isRepulsive)
		{
			go.GetComponent<Field>().A = value;
            _globalUI.intensityText.text = Math.Round((value * 100)).ToString();
		}
		else
		{
			go.GetComponent<Field>().A = value * -1;
			_globalUI.intensityText.text = Math.Round((value * 100 * -1)).ToString();
		}

		if (go.GetComponent<TerrainDisplay>().dropped)
		{
			GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
			GameObjectManager.addComponent<RefreshTerrain>(_levelInfoGO, new { action = RefreshTerrain.ACTIONS.MODIFY, source = go });
		}

	}

	/// <summary>
	/// Inactive the selected and editable force field
	/// Refresh ships trajectories and terrain
	/// </summary>
	private void OnDeleteFFButton(GameObject entryGO = null)
	{
		SelectInformation currentSelection = _selectInformationFamily.First().GetComponent<SelectInformation>();
		GameObject go;
		if (entryGO == null) {
			go = currentSelection.selectedGameObject;
		}
		else
			go = entryGO;

		FieldsCounter fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();

		if (go != null && go.GetComponent<Editable>() != null && go.GetComponent<Editable>().editable == true)
		{
			if (go.GetComponent<Field>().isRepulsive)
			{
				fieldsRemaining.poolRepulsive.Remove (go.transform);
				fieldsRemaining.poolRepulsive.Add (go.transform);
				fieldsRemaining.fieldsRepPlaced--;
			}
			else
			{
				fieldsRemaining.poolAttractive.Remove (go.transform);
				fieldsRemaining.poolAttractive.Add (go.transform);
				fieldsRemaining.fieldsAttPlaced--;
			}

			if(_GameInfos.playSounds)
				FMODUnity.RuntimeManager.PlayOneShot (_sound.RemoveFieldEvent);

			GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);

			GameObjectManager.addComponent<RefreshTerrain>(_levelInfoGO, new { action = RefreshTerrain.ACTIONS.DELETE, source = go });
			GameObjectManager.setGameObjectState(go, false);
			currentSelection.selectedGameObject = null;

			UpdateFFRemaining();

			_globalUI.forcePanel.SetActive(false);

			if(entryGO == null){
				// We add what action has been done & we add a copy of the GO in the corresponding list
				_previousActionsFamily.First ().GetComponent<PreviousActions> ().listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.DEL);
				_previousActionsFamily.First ().GetComponent<PreviousActions> ().cancelData.Add (StoreGameObjectComponents (go));
				_previousActionsFamily.First ().GetComponent<PreviousActions> ().listOfRedoActions.Clear ();
			}
		}
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="fakeGO"></param>
    /// <param name="setAllComponents"></param>
    /// <param name="componentType"></param>
	private void SetGameObjectComponents(GameObject go, DataGO fakeGO, bool setAllComponents = true, System.Type componentType = null)
    {
		Component[] comps = go.GetComponents(typeof(Component));
		foreach (Component comp in comps)
        {
			System.Type type = comp.GetType ();
			if (setAllComponents)
            {
				System.Reflection.FieldInfo[] fields = type.GetFields ();
				foreach (System.Reflection.FieldInfo field in fields)
                {
					field.SetValue (comp, fakeGO.allTheComponents [type] [field]);
				}
			}
			else
            {
				if (type == componentType)
                {
					System.Reflection.FieldInfo[] fields = type.GetFields ();
					foreach (System.Reflection.FieldInfo field in fields)
                    {
						field.SetValue (comp, fakeGO.allTheComponents [type] [field]);
					}
				}
			}
		}
		go.transform.position = fakeGO.transformPos;
		go.transform.rotation = fakeGO.transformRot;
	}

	/// <summary>
	/// Display force field's information on the panel
	/// If the force field isn't editable --> the slider isn't interactable
	/// </summary>
	/// <param name="hitGo"></param>
	private void ForceInformation(GameObject hitGo)
	{
		// You have to save that value before changing the min/max because it will call the listener and modify it
		float nextValue = hitGo.GetComponent<Field>().A * 100;

		if (hitGo.GetComponent<Field>().isRepulsive)
		{
			_globalUI.typeText.text = "Repulsive";
			_globalUI.fieldImage.sprite = _spriteContainer.repulsiveFF;
			_globalUI.intensityText.text = Math.Round(nextValue).ToString();
		}
		else
		{
            _globalUI.typeText.text = "Attractive";
            _globalUI.fieldImage.sprite = _spriteContainer.attractiveFF;
            _globalUI.intensityText.text = Math.Round(nextValue).ToString();
			nextValue = nextValue * -1;
		}

		_globalUI.intensitySlider.value = nextValue;

		if (hitGo.GetComponent<Editable>() == null || hitGo.GetComponent<Editable>().editable == false)
		{
            _globalUI.intensitySlider.interactable = false;
			_SliderComponent.backgroundSlider.color = _SliderComponent.disabledSlider;
			_SliderComponent.fillSlider.color = _SliderComponent.disabledSlider;
			_SliderComponent.textSlider.color = _SliderComponent.disabledText;
			_globalUI.deleteButton.gameObject.SetActive(false);
		}
		else
		{
            _globalUI.intensitySlider.interactable = true;
			_SliderComponent.backgroundSlider.color = _SliderComponent.enabledSlider;
			_SliderComponent.fillSlider.color = _SliderComponent.enabledSlider;
			_SliderComponent.textSlider.color = _SliderComponent.enabledText;
            _globalUI.deleteButton.gameObject.SetActive(true);
		}
	}

	// =================================================== Bonus Malus Panel =================================================== Bonus Malus Panel ===================================================

	/// <summary>
	/// Display information on the Bonus/Malus Panel in terms of bonus/ malus
	/// </summary>
	/// <param name="hitGo"></param>
	private void BonusMalusInformation(GameObject hitGo)
	{
		string bTitle = "Bonus", mTitle = "Malus";
		string nameBplayer = "Invincibilité", nameBdamage = "Dégâts", nameMearth = "Terre Invincible", nameMfoe = "Vie des Ennemis";
		string desBplayer = "Notre vaisseau est invicible pour le prochain tour", desBdamage = "Dégâts de nos missiles augmentés au prochain tour";
		string desMearth = "La Terre est invincible pour le prochain tour", desMfoe = "Les ennemis récupèrent 1 point de vie au prochain tour";

		Bonus theBM = hitGo.GetComponent<Bonus>();

		switch (theBM.type)
		{
		    case Bonus.TYPE.B_Damage :
			    _globalUI.bmTitle.text = bTitle;
			    _globalUI.bmImage.sprite = _spriteContainer.bonus;
			    _globalUI.bmName.text = nameBplayer;
			    _globalUI.bmDescription.text = desBplayer;

			    break;

		    case Bonus.TYPE.B_Player:
                _globalUI.bmTitle.text = bTitle;
                _globalUI.bmImage.sprite = _spriteContainer.bonus;
                _globalUI.bmName.text = nameBdamage;
                _globalUI.bmDescription.text = desBdamage;

			    break;

		    case Bonus.TYPE.M_Earth:
                _globalUI.bmTitle.text = mTitle;
                _globalUI.bmImage.sprite = _spriteContainer.malus;
                _globalUI.bmName.text = nameMearth;
                _globalUI.bmDescription.text = desMearth;

			    break;

		    case Bonus.TYPE.M_FoeLife:
                _globalUI.bmTitle.text = mTitle;
                _globalUI.bmImage.sprite = _spriteContainer.malus;
                _globalUI.bmName.text = nameMfoe;
                _globalUI.bmDescription.text = desMfoe;

			    break;
		}
	}

	// ===================================================== Won/Lost Panel ===================================================== Won/Lost Panel =====================================================

	/// <summary>
	/// Change the level to n+1
	/// Reload the current scene
	/// </summary>
	private void OnNextLevelButton()
	{
		// You shouldn't be able to press the button if it is last level
		_GameInfos.noLevel += 1;
		SceneManager.LoadScene("GoodScene");
	}
}