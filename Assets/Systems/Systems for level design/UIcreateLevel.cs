using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using FYFY;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class UIcreateLevel : UtilitySystem
{
    // =================================================================================================================================================================================================================
    // ============================================ VARIABLES ============================================ VARIABLES ============================================ VARIABLES ============================================
    // =================================================================================================================================================================================================================

    // ============================================ Families ============================================= Families ============================================= Families =============================================
    private Family _mainLoopFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));
    private Family _gameInformation = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
    private Family _selectInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SelectInformation)));
    private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
    private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
    private Family _spriteFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SpriteContainerLD)));

    private Family _shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass), typeof(MovableShip)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
    private Family _enemyShipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Enemy), typeof(ShipInfo), typeof(Position), typeof(Mass)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
    private Family _missileFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions), typeof(Movement), typeof(Position), typeof(Mass)));

    private Family _prefabFamily = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)));
    private Family _foeContainerFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FoeContainer)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainUILD), typeof(OpenedMenuPanel), typeof(AddMenuPanel), typeof(GamePanelLD), typeof(CameraGameObject)));

    private Family _fieldsCounterFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FieldCounterCL)));
    private Family _forceFieldFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Field), typeof(Dimensions), typeof(Position)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));

    private Family _PPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));

	private Family _aiComputationFamily = FamilyManager.getFamily (new AllOfComponents (typeof(AIComputing)));
	private Family _refreshInterfaceFamily = FamilyManager.getFamily (new AllOfComponents (typeof(RefreshInterface)));

    // ============================================ Components ============================================ Components =========================================== Components ==========================================
    private GameInformations _GameInfos;
	private Terrain _terr;
	private GameObject _levelInfoGO;
	private const string cst_fieldTag = "Field", cst_foeTag = "EnemyShip", cst_earthTag = "finish", cst_bonusTag = "bonus", cst_obstacleTag = "obstacle";
	private MainLoop _mainLoop;
    private SpriteContainerLD _spriteContainer;

    // ========================================= UI Components ========================================= UI Components ========================================= UI Components =========================================
    private MainUILD _mainInterface;
    private OpenedMenuPanel _openedMenuPanel;
    private AddMenuPanel _addMenuPanel;
    private GamePanelLD _gamePanel;
    private CameraGameObject _camerasObject;

    // ============================================= Raycast ============================================== Raycast ============================================== Raycast =============================================
    private RaycastHit _hit;
    private Ray _ray;
    private List<RaycastResult> _results;
    private EventSystem _EventSystem;

    // =================================================================================================================================================================================================================
    // ============================================= METHODS ============================================== METHODS ============================================== METHODS =============================================
    // =================================================================================================================================================================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    public UIcreateLevel()
    {
		_GameInfos = _gameInformation.First().GetComponent<GameInformations>();
		_terr = _PPlanFamily.First().GetComponent<Terrain>();
        _levelInfoGO = _levelInfoFamily.First();
		_mainLoop = _mainLoopFamily.First ().GetComponent<MainLoop>();
        _spriteContainer = _spriteFamily.First().GetComponent<SpriteContainerLD>();

        InitUI();
        UpdateFFRemaining();

        _results = new List<RaycastResult>();
        _EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        _missileFamily.addExitCallback (OnMissileExited);
		_aiComputationFamily.addExitCallback (OnAIComputationExited);
    }

    /// <summary>
    /// Use to process Families 
    /// As Update / Late Update / FixedUpdate in terms of the Main Loop
    /// </summary>
    /// <param name="familiesUpdateCount"></param>
    protected override void onProcess(int familiesUpdateCount)
    {
		if(_stateFamily.First().GetComponent<State>().state != State.STATES.PAUSED)
        	DetectGameObject();

        if (_stateFamily.First().GetComponent<State>().state == State.STATES.SETUP && _openedMenuPanel.openedMenu.activeSelf == false)
            _gamePanel.gamePanel.SetActive(true);
    }

    // =================================================================================================================================================================================================================
    // ================================== Functions / Procedures ================================== Functions / Procedures =================================== Functions / Procedures ==================================
    // =================================================================================================================================================================================================================

    // ============================ Common Functions / Procedures ============================= Common Functions / Procedures ============================= Common Functions / Procedures ==============================

    /// <summary>
    /// UI Initialization Manager 
    /// </summary>
    protected void InitUI()
    {
        // Recovery GameObject Interface Family
        GameObject goInterface = _interfaceFamily.First();
        
        // Main Interface
        _mainInterface = goInterface.GetComponent<MainUILD>();
        _mainInterface.openMenuButton.onClick.AddListener(() => MenuPanel(true));
        _mainInterface.addButton.onClick.AddListener(() => AddPanel(true));
        _mainInterface.autoDestructionButton.onClick.AddListener(() => Autodestruction());

        // Opened Menu Panel
        _openedMenuPanel = goInterface.GetComponent<OpenedMenuPanel>();
        _openedMenuPanel.closeButton.onClick.AddListener(() => MenuPanel(false));
        _openedMenuPanel.createLevelButton.onClick.AddListener(() => CreateLevelPanel(true));
        _openedMenuPanel.loadLevelButton.onClick.AddListener(() => LoadLevelPanel(true));
        _openedMenuPanel.restartButton.onClick.AddListener(() => RestartPanel(true));
        _openedMenuPanel.BackMenuButton.onClick.AddListener(() => BackMenuPanel(true));
        // Create Level Panel
        _openedMenuPanel.closeButton.onClick.AddListener(() => CreateLevelPanel(false));
        // Load Level Panel
        _openedMenuPanel.closeLoadButton.onClick.AddListener(() => LoadLevelPanel(false));
        // Restart Panel
        _openedMenuPanel.yesRestButton.onClick.AddListener(() => RestartLevel());
        _openedMenuPanel.noRestButton.onClick.AddListener(() => RestartPanel(false));
        // Back Menu Panel
        _openedMenuPanel.yesBackButton.onClick.AddListener(() => BackMainMenu());
        _openedMenuPanel.noRestButton.onClick.AddListener(() => BackMenuPanel(false));

        // Add Menu Panel
        _addMenuPanel = goInterface.GetComponent<AddMenuPanel>();
        _addMenuPanel.closeAddButton.onClick.AddListener(() => AddPanel(false));
        _addMenuPanel.foeButton.onClick.AddListener(() => AddFoe());
        _addMenuPanel.obstacleButton.onClick.AddListener(() => AddObject("obstacle"));
        _addMenuPanel.breakObstButton.onClick.AddListener(() => AddObject("breakObst"));
        _addMenuPanel.damageButton.onClick.AddListener(() => AddObject("damage"));
        _addMenuPanel.playerButton.onClick.AddListener(() => AddObject("player"));
        _addMenuPanel.earthButton.onClick.AddListener(() => AddObject("earth"));
        _addMenuPanel.foeLifeButton.onClick.AddListener(() => AddObject("foeLife"));

        // Game Panel
        _gamePanel = goInterface.GetComponent<GamePanelLD>();
        // Level Design Buttons
        _gamePanel.attractiveButtonLD.onClick.AddListener(() => AddFF("attractiveLD"));
        _gamePanel.repulsiveButtonLD.onClick.AddListener(() => AddFF("repulsiveLD"));
        _gamePanel.switchCameraButton.onClick.AddListener(() => SwitchCamera());
        // Normal Buttons
        _gamePanel.attractiveButton.onClick.AddListener(() => AddFF("attractive"));
        _gamePanel.repulsiveButton.onClick.AddListener(() => AddFF("repulsive"));
        _gamePanel.fireButton.onClick.AddListener(() => OnFireButton());
        // Ship Panel
        _gamePanel.speedSlider.onValueChanged.AddListener((float value) => MissileSpeed(value));
        _gamePanel.movableToggle.onValueChanged.AddListener((bool value) => MovableShip(value));
        // Force Panel
        _gamePanel.intensitySlider.onValueChanged.AddListener((float value) => IntensityFF(value));
        _gamePanel.deleteButton.onClick.AddListener(() => DeleteFF());
        // Bonus Malus Panel
        _gamePanel.deleteBmButton.onClick.AddListener(() => DeleteObj());
        // Object Panel
        _gamePanel.deleteObjButton.onClick.AddListener(() => DeleteObj());

        // Cameras
        _camerasObject = goInterface.GetComponent<CameraGameObject>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected void DetectGameObject()
    {
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Layer Mask
        int layerMask = 1 << 24;

        if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, layerMask))
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;
                EventSystem.current.RaycastAll(pointerData, _results);

                if (_results.Count == 0)
                {
                    GameObject theGo = _hit.collider.transform.parent.gameObject;

                    ActiveSelection(theGo);

                    if (theGo == _shipFamily.First())
                    {
                        _gamePanel.shipPanel.SetActive(true);
                    }
                    else if (theGo.CompareTag(cst_fieldTag))
                    {
                        _gamePanel.forcePanel.SetActive(true);
                        ForceInformation(theGo);
                    }
                    else if (theGo.CompareTag(cst_bonusTag))
                    {
                        _gamePanel.bmPanel.SetActive(true);
                        BmInformation(theGo);
                    }
                    else
                    {
                        _gamePanel.bmPanel.SetActive(false);
                        ObjectInformation(theGo);
                    }
                }

                _results.Clear();
            }
        }
    }

    /// <summary>
    /// Active the canvas having the image for selection of the Gameobject in param
    /// </summary>
    /// <param name="theGo"></param>
    private void ActiveSelection(GameObject theGo)
    {
        ResetSelection();

        SelectInformation currentSelection = _selectInfoFamily.First().GetComponent<SelectInformation>();
        currentSelection.selectedGameObject = theGo;
        currentSelection.selectedGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        currentSelection.selectedGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        currentSelection.canvasSelectionGO = theGo.transform.Find("CanvasSelection").gameObject;
        currentSelection.canvasSelectionGO.SetActive(true);

        if (theGo.CompareTag(cst_fieldTag))
        {
            currentSelection.otherCanvasGO = theGo.transform.Find("CanvasTriangle").gameObject;
            currentSelection.otherCanvasGO.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ResetSelection()
    {
        SelectInformation currentSelection = _selectInfoFamily.First().GetComponent<SelectInformation>();

        if (currentSelection.selectedGameObject != null)
        {
            currentSelection.selectedGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            currentSelection.selectedGameObject = null;
        }

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
    private void DisableAllPanel()
    {
        _gamePanel.shipPanel.SetActive(false);
        _gamePanel.forcePanel.SetActive(false);
        _gamePanel.bmPanel.SetActive(false);
        _gamePanel.objectPanel.SetActive(false);
    }

    /// <summary>
    /// Locks or unlocks the interface.
    /// </summary>
    /// <param name="lockOrUnlock">If set to <c>true</c> locks interface; if set to <c>false</c> unlocks it.</param>
    protected void LockedInterface(bool lockOrUnlock, bool activeAutoDestruction)
    {
        DisableAllPanel();
        _gamePanel.gamePanel.SetActive(!lockOrUnlock);
        _openedMenuPanel.openedMenu.SetActive(!lockOrUnlock);
        _addMenuPanel.addMenu.SetActive(!lockOrUnlock);
        _mainInterface.autoDestructionButton.gameObject.SetActive(activeAutoDestruction);
        _mainInterface.autoDestructionButton.interactable = true;
    }

    /// <summary>
    /// 
    /// </summary>
    protected void Retry()
    {
        State currentState = _stateFamily.First().GetComponent<State>();
        currentState.state = State.STATES.SETUP;

        GameObject ship = _shipFamily.First();
        if (ship.GetComponent<Editable>() != null)
        {
            ship.GetComponent<Editable>().editable = true;
        }

        /*foreach (GameObject src in _forceFieldFamily)
        {
            src.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            src.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }*/

        GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
    }

    // ======================================== Main Interface ========================================= Main Interface ========================================= Main Interface ========================================

    /// <summary>
    /// 
    /// </summary>
    private void Autodestruction()
    {
        foreach (GameObject missile in _missileFamily)
        {
            if (missile.layer == 10)
                GameObjectManager.addComponent<Kamikaze>(missile);
        }

        _mainInterface.autoDestructionButton.interactable = false;
    }

    // =========================================== Menu Panel ============================================ Menu Panel ============================================ Menu Panel ===========================================

    /// <summary>
    /// 
    /// </summary>
    protected void MenuPanel(bool activated)
    {
        _openedMenuPanel.openedMenu.SetActive(activated);
        _gamePanel.gamePanel.SetActive(!activated);
        _addMenuPanel.addMenu.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="activated"></param>
    private void CreateLevelPanel(bool activated)
    {
        _openedMenuPanel.createLevelPanel.SetActive(activated);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="activated"></param>
    private void LoadLevelPanel(bool activated)
    {
        _openedMenuPanel.loadLevelPanel.SetActive(activated);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="activated"></param>
    private void RestartPanel(bool activated)
    {
        _openedMenuPanel.restartPanel.SetActive(activated);
    }

    /// <summary>
    /// 
    /// </summary>
    protected void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Button to come back to the Menu
    /// </summary>
    protected void BackMenuPanel(bool activated)
    {
        _openedMenuPanel.backMenuPanel.SetActive(activated);
    }

    /// <summary>
    /// 
    /// </summary>
    protected void BackMainMenu()
    {
        GameObjectManager.loadScene("MenuScene");
    }

    // ============================================ Add Panel ============================================ Add Panel ============================================ Add Panel =============================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="activated"></param>
    private void AddPanel(bool activated)
    {
        _addMenuPanel.addMenu.SetActive(activated);
        _openedMenuPanel.openedMenu.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    private void AddObject(string type)
    {
        PrefabContainer prefabContainer = _prefabFamily.First().GetComponent<PrefabContainer>();
        GameObject theGO = null;
        Vector3 newPosition = new Vector3(18, 8.64F, 2);

        switch (type)
        {
            case "player":
                theGO = GameObject.Instantiate(prefabContainer.bPlayer);
                BmInformation(theGO);
                break;
            case "damage":
                theGO = GameObject.Instantiate(prefabContainer.bDamage);
                BmInformation(theGO);
                break;
            case "earth":
                theGO = GameObject.Instantiate(prefabContainer.mEarth);
                BmInformation(theGO);
                break;
            case "foeLife":
                theGO = GameObject.Instantiate(prefabContainer.mFoeLife);

                BmInformation(theGO);
                break;
            case "obstacle":
                theGO = GameObject.Instantiate(prefabContainer.normalObstacle);
                ObjectInformation(theGO);
                break;
            case "breakObst":
                theGO = GameObject.Instantiate(prefabContainer.breakableObstacle);
                ObjectInformation(theGO);
                break;
        }

        GameObjectManager.bind(theGO);
        theGO.transform.position = newPosition;
        ActiveSelection(theGO);
        _gamePanel.objectPanel.SetActive(true);

    }

    /// <summary>
    /// 
    /// </summary>
    protected void AddFoe()
    {
        FoeContainer foeContainer = _foeContainerFamily.First().GetComponent<FoeContainer>();
        GameObject foe = null;

        if (foeContainer.foesPlaced < foeContainer.numberFoes)
        {
            foe = foeContainer.foes[foeContainer.foesPlaced].gameObject;
            foeContainer.foesPlaced++;
        }

        GameObjectManager.setGameObjectState(foe, true);

        ActiveSelection(foe);
        ObjectInformation(foe);
        _gamePanel.objectPanel.SetActive(true);
    }

    // =========================================== GAME PANEL ============================================ GAME PANEL ============================================ GAME PANEL ===========================================

    // ========================================== Button Panel ========================================== Button Panel ========================================== Button Panel ==========================================

    /// <summary>
    /// Creates the missiles.
    /// </summary>
    /// <param name="shipLauncher">Ship launcher.</param>
    protected void CreateMissiles(GameObject ship)
    {
        int angle = 0;
        int signe = 1;
        bool alternate = true;
        float oldMagnitude = ship.GetComponent<ShipInfo>().fireIntensity.magnitude;
        ShipInfo sInfo = ship.GetComponent<ShipInfo>();

        for (int i = 0; i < sInfo.nbMissiles; i++)
        {
            Vector3 terrDims = _terr.terrainData.size;
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
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    protected void OnMissileExited(int id)
    {
        if (_missileFamily.Count == 0)
        { // the last one to exit
            if (_aiComputationFamily.Count != 0)
            {
                Debug.Log("Computing...");
            }
            else
            {
                Debug.Log("Time to retry!");
                LockedInterface(false, false);
                Retry();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    protected void OnAIComputationExited(int id)
    {
        if (_aiComputationFamily.Count == 0)
        { // the last one to exit
            Debug.Log("Computation over!");
            if (_missileFamily.Count != 0)
            {
                Debug.Log("Still playing...");
            }
            else
            {
                Debug.Log("Let's retry!");
                LockedInterface(false, false);
                Retry();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected void OnFireButton()
    {
        // Locks everything
        LockedInterface(true, true);

        // Systems needed
        State currentState = _stateFamily.First().GetComponent<State>();
        ResetSelection();

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
        {
            ship.GetComponent<Editable>().editable = false;
        }

        /*// make sources not editable anymore
        foreach (GameObject source in _forceFieldFamily)
        {
            source.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }*/

        //_levelInfoFamily.First ().GetComponent<LevelInformations> ().updateEnemyTrajectories = true;
        GameObjectManager.addComponent<AIShouldCompute>(_levelInfoGO);

        currentState.state = State.STATES.PLAYING;
    }

    /// <summary>
    /// Button to switch camera between Main Camera and Top View Camera
    /// And change the interface
    /// </summary>
    protected void SwitchCamera()
    {
        LockedInterface(true, false);

        // Main Camera -> Top View Camera
        if (_camerasObject.mainCamera.gameObject.activeSelf)
        {
            _mainLoop.StartCoroutine(SmoothChangeCamera(true));
        }
        // Top View Camera -> Main Camera
        else
        {
            _mainLoop.StartCoroutine(SmoothChangeCamera(false));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private Matrix4x4 MatrixLerp(Matrix4x4 src, Matrix4x4 dest, float time)
    {
        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
        {
            ret[i] = Mathf.Lerp(src[i], dest[i], time);
        }
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromMainCamera"></param>
    /// <returns></returns>
	private IEnumerator SmoothChangeCamera(bool fromMainCamera)
    {
        float duration = 1f;

        GameObject origin, goal;

        if (fromMainCamera)
        {
            origin = _camerasObject.mainCamera.gameObject;
            goal = _camerasObject.TopViewCamera.gameObject;
        }
        else
        {
            origin = _camerasObject.TopViewCamera.gameObject;
            goal = _camerasObject.mainCamera.gameObject;
        }

        _camerasObject.transitionCamera.transform.position = origin.transform.position;
        _camerasObject.transitionCamera.transform.rotation = origin.transform.rotation;


        origin.SetActive(false);
        origin.GetComponent<Camera>().enabled = false;
        _camerasObject.transitionCamera.SetActive(true);

        Matrix4x4 ortho = _camerasObject.TopViewCamera.projectionMatrix;
        Matrix4x4 perspective = _camerasObject.mainCamera.projectionMatrix;

        // if we came from Top View Camera, we need to start be switching between orthographic -> perspective before rotating; other way aroung later
        if (!fromMainCamera)
        {
            _camerasObject.transitionCamera.GetComponent<Camera>().projectionMatrix = ortho;
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                _camerasObject.transitionCamera.GetComponent<Camera>().projectionMatrix = MatrixLerp(_camerasObject.transitionCamera.GetComponent<Camera>().projectionMatrix, perspective, 0.1f);
                yield return null;
            }
        }

        while (Vector3.Distance(_camerasObject.transitionCamera.transform.position, goal.transform.position) > 0.1f || Quaternion.Angle(_camerasObject.transitionCamera.transform.rotation, goal.transform.rotation) > 1)
        {
            _camerasObject.transitionCamera.transform.position = Vector3.MoveTowards(_camerasObject.transitionCamera.transform.position, goal.transform.position, Time.deltaTime * 10f);
            _camerasObject.transitionCamera.transform.rotation = Quaternion.RotateTowards(_camerasObject.transitionCamera.transform.rotation, goal.transform.rotation, Time.deltaTime * 75f);
            yield return null;
        }

        // if we came from Main Camera, we need to start be switching between perpective and orthographic after rotating
        if (fromMainCamera)
        {
            _camerasObject.transitionCamera.GetComponent<Camera>().projectionMatrix = perspective;
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                _camerasObject.transitionCamera.GetComponent<Camera>().projectionMatrix = MatrixLerp(_camerasObject.transitionCamera.GetComponent<Camera>().projectionMatrix, ortho, 0.1f);
                yield return null;
            }
        }

        _camerasObject.transitionCamera.SetActive(false);
        goal.SetActive(true);
        goal.GetComponent<Camera>().enabled = true;
        _gamePanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    protected void AddFF(string type)
    {
        FieldCounterCL fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldCounterCL>();
        GameObject field = null;
        Vector3 appearPosition = new Vector3(10F, 8.64F, 10F);

        switch (type)
        {
            case "attractive":
                if (fieldsRemaining.fieldsAttPlaced < fieldsRemaining.fieldsAttToPlace)
                {
                    field = fieldsRemaining.poolAttractiveToPlace[fieldsRemaining.fieldsAttPlaced].gameObject;
                    fieldsRemaining.fieldsAttPlaced++;

                }
                break;
            case "repulsive":
                if (fieldsRemaining.fieldsRepPlaced < fieldsRemaining.fieldsRepToPlace)
                {
                    field = fieldsRemaining.poolRepulsiveToPlace[fieldsRemaining.fieldsRepPlaced].gameObject;
                    fieldsRemaining.fieldsRepPlaced++;

                }
                break;
            case "attractiveLD":
                if (fieldsRemaining.fieldsAttPlacedLD < fieldsRemaining.fieldsAttToPlaceLD)
                {
                    field = fieldsRemaining.poolAttractivePlaced[fieldsRemaining.fieldsAttPlacedLD].gameObject;
                    fieldsRemaining.fieldsAttPlacedLD++;

                }
                break;
            case "repulsiveLD":
                if (fieldsRemaining.fieldsRepPlacedLD < fieldsRemaining.fieldsRepToPlaceLD)
                {
                    field = fieldsRemaining.poolRepulsivePlaced[fieldsRemaining.fieldsRepPlacedLD].gameObject;
                    fieldsRemaining.fieldsRepPlacedLD++;

                }
                break;
        }

        changePosition(field.GetComponent<Position>(), field.GetComponent<Position>().initialPos, _terr.terrainData.size);
        UpdateFFRemaining();

        GameObjectManager.setGameObjectState(field, true);
        GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
        GameObjectManager.addComponent<RefreshTerrain>(_levelInfoGO, new { action = RefreshTerrain.ACTIONS.ADD, source = field });

        ActiveSelection(field);
        ForceInformation(field);
        _gamePanel.forcePanel.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    protected void UpdateFFRemaining()
    {
        FieldCounterCL fieldsAlreadyPlaced = _fieldsCounterFamily.First().GetComponent<FieldCounterCL>();

        int fieldsRepRemaining = fieldsAlreadyPlaced.fieldsRepToPlace - fieldsAlreadyPlaced.fieldsRepPlaced;
        int fieldsAttRemaining = fieldsAlreadyPlaced.fieldsAttToPlace - fieldsAlreadyPlaced.fieldsAttPlaced;
        int fieldsRepRemainingCL = fieldsAlreadyPlaced.fieldsRepToPlaceLD - fieldsAlreadyPlaced.fieldsRepPlacedLD;
        int fieldsAttRemainingCL = fieldsAlreadyPlaced.fieldsAttToPlaceLD - fieldsAlreadyPlaced.fieldsAttPlacedLD;

        _gamePanel.attractiveRemaining.text = fieldsAttRemaining.ToString();
        _gamePanel.repulsiveRemaining.text = fieldsRepRemaining.ToString();
        _gamePanel.attractiveLDRemaining.text = fieldsAttRemainingCL.ToString();
        _gamePanel.repulsiveLDRemaining.text = fieldsRepRemainingCL.ToString();

        if (fieldsAttRemaining > 0)
            _gamePanel.attractiveButton.interactable = true;
        else
            _gamePanel.attractiveButton.interactable = false;

        if (fieldsRepRemaining > 0)
            _gamePanel.repulsiveButton.interactable = true;
        else
            _gamePanel.repulsiveButton.interactable = false;

        if (fieldsAttRemainingCL > 0)
            _gamePanel.attractiveButtonLD.interactable = true;
        else
            _gamePanel.attractiveButtonLD.interactable = false;

        if (fieldsRepRemainingCL > 0)
            _gamePanel.attractiveButtonLD.interactable = true;
        else
            _gamePanel.attractiveButtonLD.interactable = false;
    }

    // =========================================== Ship Panel ============================================ Ship Panel ============================================ Ship Panel ===========================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    protected void MissileSpeed(float value)
    {
        _gamePanel.speedText.text = Math.Round(value).ToString();

        value = value / 100f;
        ShipInfo _shipInfo = _shipFamily.First().GetComponent<ShipInfo>();

        // New vector for speed and display the new speed
        _shipInfo.fireIntensity.Normalize();
        _shipInfo.fireIntensity *= value;

        GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isOn"></param>
    private void MovableShip(bool isOn)
    {
        _shipFamily.First().GetComponent<MovableShip>().isMovable = isOn;
    }

    // =========================================== Force Panel =========================================== Force Panel =========================================== Force Panel ==========================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    protected void IntensityFF(float value)
    {
        value = value / 100f;
        SelectInformation currentSelection = _selectInfoFamily.First().GetComponent<SelectInformation>();
        GameObject go = currentSelection.selectedGameObject;
        Field theField = go.GetComponent<Field>();

        if (theField.isRepulsive)
        {
            go.GetComponent<Field>().A = value;
            _gamePanel.intensityText.text = Math.Round((value * 100)).ToString();
        }

        else
        {
            go.GetComponent<Field>().A = value * -1;
            _gamePanel.intensityText.text = Math.Round((value * 100 * -1)).ToString();
        }

        GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);
        GameObjectManager.addComponent<RefreshTerrain>(_levelInfoGO, new { action = RefreshTerrain.ACTIONS.MODIFY, source = go });

    }

    /// <summary>
    /// 
    /// </summary>
    protected void DeleteFF()
    {
        Debug.Log("DeleteFF");
        SelectInformation currentSelection = _selectInfoFamily.First().GetComponent<SelectInformation>();
        GameObject go = currentSelection.selectedGameObject;

        FieldCounterCL fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldCounterCL>();

        if (go != null && go.GetComponent<Field>() != null)
        {
            if (go.GetComponent<Editable>() != null)
            {
                if (go.GetComponent<Field>().isRepulsive)
                {
                    fieldsRemaining.fieldsRepPlaced--;
                }
                else
                {
                    fieldsRemaining.fieldsAttPlaced--;
                }
            }
            else
            {
                if (go.GetComponent<Field>().isRepulsive)
                {
                    fieldsRemaining.fieldsRepPlacedLD--;
                }
                else
                {
                    fieldsRemaining.fieldsAttPlacedLD--;
                }
            }

            GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoGO);

            GameObjectManager.addComponent<RefreshTerrain>(_levelInfoGO, new { action = RefreshTerrain.ACTIONS.DELETE, source = go });
            GameObjectManager.setGameObjectState(go, false);

            UpdateFFRemaining();
            ResetSelection();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitGo"></param>
    protected void ForceInformation(GameObject hitGo)
    {
        if (hitGo.GetComponent<Field>() == null)
        {
            _gamePanel.typeText.text = "N/A";
            _gamePanel.intensityText.text = "N/A";
        }
        else
        {
            // You have to save that value before changing the min/max because it will call the listener and modify it
            float nextValue = hitGo.GetComponent<Field>().A * 100;

            if (hitGo.GetComponent<Field>().isRepulsive)
            {
                _gamePanel.fieldImage.sprite = _spriteContainer.repulsiveFF;
                _gamePanel.typeText.text = "Repulsive";
                _gamePanel.intensityText.text = Math.Round(nextValue).ToString();
            }
            else
            {
                _gamePanel.fieldImage.sprite = _spriteContainer.attractiveFF;
                _gamePanel.typeText.text = "Attractive";
                _gamePanel.intensityText.text = Math.Round(nextValue).ToString();
                nextValue = nextValue * -1;
            }

            _gamePanel.intensitySlider.value = nextValue;
        }
    }

    // ======================================= Bonus Malus Panel ====================================== Bonus Malus Panel ====================================== Bonus Malus Panel ======================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thBm"></param>
    private void BmInformation(GameObject theGo)
    {
        Bonus theBm = theGo.GetComponent<Bonus>();

        switch (theBm.type)
        {
            case Bonus.TYPE.B_Damage :
                _gamePanel.bmTitle.text = "Bonus";
                _gamePanel.bmImage.sprite = _spriteContainer.bonus;
                _gamePanel.bmName.text = "Dégâts missiles";
                _gamePanel.bmDescription.text = "Les dégâts de nos missiles sont doublés pour un tir";
                break;
            case Bonus.TYPE.B_Player :
                _gamePanel.bmTitle.text = "Bonus";
                _gamePanel.bmImage.sprite = _spriteContainer.bonus;
                _gamePanel.bmName.text = "Vaisseau Invincible";
                _gamePanel.bmDescription.text = "Le Potential Gravity invincible pour un tir";
                break;
            case Bonus.TYPE.M_Earth :
                _gamePanel.bmTitle.text = "Malus";
                _gamePanel.bmImage.sprite = _spriteContainer.malus;
                _gamePanel.bmName.text = "Terre Invincible";
                _gamePanel.bmDescription.text = "La Terre est invincible pour un tir";
                break;
            case Bonus.TYPE.M_FoeLife :
                _gamePanel.bmTitle.text = "Malus";
                _gamePanel.bmImage.sprite = _spriteContainer.malus;
                _gamePanel.bmName.text = "Vie Ennemi";
                _gamePanel.bmDescription.text = "Les vaisseaux ennemis récupèrent 2 points de vie";
                break;
        }
    }

    // ========================================== Object Panel ========================================== Object Panel ========================================== Object Panel ==========================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitGo"></param>
    private void ObjectInformation(GameObject hitGo)
    {
        switch (hitGo.tag)
        {
            case cst_foeTag :
                _gamePanel.objectName.text = "Great Eagle";
                _gamePanel.objectImage.sprite = _spriteContainer.foe;
                break;
            case cst_earthTag :
                _gamePanel.objectName.text = "La Terre";
                _gamePanel.objectImage.sprite = _spriteContainer.earth;
                break;
            case cst_obstacleTag :
                if (hitGo.layer == 20)
                {
                    _gamePanel.objectName.text = "Obstacle Indestructible";
                    _gamePanel.objectImage.sprite = _spriteContainer.obstacle;
                }
                else
                {
                    _gamePanel.objectName.text = "Obstacle Destructible";
                    _gamePanel.objectImage.sprite = _spriteContainer.breakObstacle;
                }
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void DeleteObj()
    {
        SelectInformation currentSelection = _selectInfoFamily.First().GetComponent<SelectInformation>();
        GameObject go = currentSelection.selectedGameObject;

        if (go.CompareTag(cst_foeTag))
        {
            go.SetActive(false);
        }
        else
        {
            GameObjectManager.unbind(go);
            GameObject.Destroy(go);
        }

        ResetSelection();
    }
}