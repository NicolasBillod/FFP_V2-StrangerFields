using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using FYFY;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadLevelLD : FSystem
{
    // ---------- VARIABLES ----------

    //Families        
    private Family _ShipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass)));
    private Family _TerrainFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family _GameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family _FieldsRemaining = FamilyManager.getFamily(new AllOfComponents(typeof(FieldCounterCL)));
    private Family _LevelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
	private Family _ShouldLoadFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ShouldLoad)));
    private Family _PrefabsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)));
    private Family _FoeContainerFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FoeContainer)));
    private Family _TargetFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FinishInformation)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainUILD), typeof(OpenedMenuPanel), typeof(AddMenuPanel), typeof(GamePanelLD), typeof(CameraGameObject)));

	GameObject _GameInfo;
    GameObject _LevelInfoGO;
    FieldCounterCL _fieldsCounter;
    FoeContainer _foeContainer;
    PrefabContainer _prefabContainer;
    OpenedMenuPanel _openedMenu;
    LevelInformations _levelInformation;

    int _CurrentNoLevel;
    string dataPath = "Assets/LevelData/Level";
    List<GameObject> placedAttFields;
    List<GameObject> placedRepFields;
    List<GameObject> attFFToPlace;
    List<GameObject> repFFToplace;
    GameObject[] foesWorld;
    
    public LoadLevelLD()
    {
		_GameInfo = _GameInfoFamily.First();
        _LevelInfoGO = _LevelInfoFamily.First();
        _levelInformation = _LevelInfoGO.GetComponent<LevelInformations>();
		_CurrentNoLevel = _GameInfo.GetComponent<GameInformations>().noLevel;
        _fieldsCounter = _FieldsRemaining.First().GetComponent<FieldCounterCL>();
        _foeContainer = _FoeContainerFamily.First().GetComponent<FoeContainer>();
        _prefabContainer = _PrefabsFamily.First().GetComponent<PrefabContainer>();
        _openedMenu = _interfaceFamily.First().GetComponent<OpenedMenuPanel>();

        placedAttFields = _fieldsCounter.poolAttractivePlaced;

        placedRepFields = _fieldsCounter.poolRepulsivePlaced;

        attFFToPlace = _fieldsCounter.poolAttractiveToPlace;

        repFFToplace = _fieldsCounter.poolRepulsiveToPlace;

        foesWorld = _foeContainer.foes;

        //Init UI
        _openedMenu.loadButton.onClick.AddListener(() => LoadLevelData(int.Parse(_openedMenu.inputLevelLoad.text)));

        LoadLevelData(_CurrentNoLevel);

		_ShouldLoadFamily.addEntryCallback (OnShouldLoadEntered);

		// Collisions to ignore
		// between EnemyShipLayer (13) and Force field (11)
		Physics.IgnoreLayerCollision(13, 11);
    }

    public void LoadLevelData(int numLevel)
    {
        FieldCounterCL fieldsToPlace = _FieldsRemaining.First().GetComponent<FieldCounterCL>();
		Transform trShipPosition = _ShipFamily.First().GetComponent<Transform>();
        Position shipPositionComponent = _ShipFamily.First().GetComponent<Position>();
        Transform trEarthPos = _TargetFamily.First().GetComponent<Transform>();
        Position targetPosComponent = _TargetFamily.First().GetComponent<Position>();
        Vector3 terrainDimensions = _TerrainFamily.First().GetComponent<Terrain>().terrainData.size;

		#if UNITY_EDITOR
        //LevelClassData theData = (LevelClassData) AssetDatabase.LoadAssetAtPath(string.Concat(dataPath, numLevel, ".asset"), typeof(LevelClassData));
		LevelClassData theData = (LevelClassData) Resources.Load(string.Concat("LevelData/Level", numLevel), typeof(LevelClassData));

		#else
		// Put the level assets in the Resources folder
		LevelClassData theData = (LevelClassData) Resources.Load(string.Concat("LevelData/Level", numLevel), typeof(LevelClassData));
		//LevelClassData theData = (LevelClassData) Resources.Load(string.Concat(dataPath, numLevel), typeof(LevelClassData));
		#endif

        if (theData == null)
        {
            Debug.Log("Ficher nul");
        }
        else
        {
            fieldsToPlace.fieldsAttToPlace = 5;
            fieldsToPlace.fieldsRepToPlace = 5;
			trShipPosition.position = theData.shipPosition;
			changeTransformPosToPosition(shipPositionComponent, trShipPosition.position, terrainDimensions);
            trEarthPos.position = theData.targetPosition;
            changeTransformPosToPosition(targetPosComponent, trEarthPos.position, terrainDimensions);

            int numberPlacedAtt = theData.attFFList.Count;
            fieldsToPlace.fieldsAttPlacedLD = numberPlacedAtt;
            for (int i = 0; i < numberPlacedAtt; i++)
            {
                GameObject theField = placedAttFields[i];
                //GameObjectManager.setGameObjectState(theField.gameObject, true);
				theField.gameObject.SetActive(true);
				float fieldIntensity = theField.GetComponent<Field>().A = theData.attFFList[i].intensity;
                Vector3 fieldWorldPos = theField.transform.position = theData.attFFList[i].position;
                Position thePos = theField.GetComponent<Position>();
                changeTransformPosToPosition(thePos, fieldWorldPos, terrainDimensions);
            }

            int numberPlacedRep = theData.repFFList.Count;
            fieldsToPlace.fieldsRepPlacedLD = numberPlacedRep;
            for (int i = 0; i < numberPlacedRep; i++)
            {
                GameObject theField = placedRepFields[i];
                //GameObjectManager.setGameObjectState(theField.gameObject, true);
				theField.gameObject.SetActive(true);
				float fieldIntensity = theField.GetComponent<Field>().A = theData.repFFList[i].intensity;
                Vector3 fieldWorldPos = theField.transform.position = theData.repFFList[i].position;
                Position thePos = theField.GetComponent<Position>();
                changeTransformPosToPosition(thePos, fieldWorldPos, terrainDimensions);
            }

            int numberFoes = theData.foeList.Count;
            for (int i = 0; i < numberFoes; i++)
            {
                FoeClass dataFoe = theData.foeList[i];
                GameObject theFoe = foesWorld[i];
                //theFoe.gameObject.SetActive(true);
                ShipInfo theShipInfo = theFoe.GetComponent<ShipInfo>();
                GameObjectManager.setGameObjectState(theFoe.gameObject, true);
                Vector3 foeWorldPos = theFoe.transform.position = dataFoe.position;
                theFoe.transform.rotation = dataFoe.rotation;
                theShipInfo.fireIntensity = dataFoe.fireIntensity;
                theShipInfo.angle = dataFoe.angle;
				updateRotationTransform (theFoe.transform, theShipInfo.angle);
                Position foePos = theFoe.GetComponent<Position>();
                changeTransformPosToPosition(foePos, foeWorldPos, terrainDimensions);
            }

            int numberBm = theData.bmList.Count;
            for (int i = 0; i < numberBm; i++)
            {
                BonusMalus theBm = theData.bmList[i];
                GameObject BmGo = null;

                switch(theBm.type)
                {
                    case BonusMalus.TYPE.B_PLAYER :
                        BmGo = GameObject.Instantiate(_prefabContainer.bPlayer);
                        break;
                    case BonusMalus.TYPE.B_DAMAGE :
                        BmGo = GameObject.Instantiate(_prefabContainer.bDamage);
                        break;
                    case BonusMalus.TYPE.M_EARTH :
                        BmGo = GameObject.Instantiate(_prefabContainer.mEarth);
                        break;
                    case BonusMalus.TYPE.M_FOELIFE :
                        BmGo = GameObject.Instantiate(_prefabContainer.mFoeLife);
                        break;
                }

                GameObjectManager.bind(BmGo);
                BmGo.transform.position = theBm.position;
            }

            int numberObs = theData.obstacleList.Count;
            for (int i = 0; i < numberObs; i++)
            {
                Obstacle theObstacle = theData.obstacleList[i];
                GameObject obstacleGO = null;

                if (theObstacle.isBreakable)
                {
                    obstacleGO = GameObject.Instantiate(_prefabContainer.breakableObstacle);
                }
                else
                {
                    obstacleGO = GameObject.Instantiate(_prefabContainer.normalObstacle);
                }

                GameObjectManager.bind(obstacleGO);
                obstacleGO.transform.position = theObstacle.position;
                obstacleGO.transform.rotation = theObstacle.rotation;
            }
        }        
    }

    public void changeTransformPosToPosition(Position position, Vector3 thePosition, Vector3 terrDims)
    {
		Constants cst = _GameInfo.GetComponent<Constants>();

        Vector3 newPosition = new Vector3(thePosition.x / cst.TERRAIN_INTERACTABLE_X, thePosition.z / cst.TERRAIN_INTERACTABLE_Y, 0);

        position.pos = newPosition;
		position.initialPos = newPosition;
    }

	public void updateRotationTransform(Transform trsfrm, float angle){
		angle *= -1;
		float angleTemp = angle;
		if (angle >= 0 && angle <= 180)
			angleTemp = angle;
		else //(bestAngle > 180)
			angleTemp = (360 - angle) * (-1);


		trsfrm.rotation = Quaternion.Euler (trsfrm.rotation.eulerAngles[0], angleTemp, trsfrm.rotation.eulerAngles[2]);

	}

	public void OnShouldLoadEntered(GameObject go)
    {
		GameObjectManager.removeComponent<ShouldLoad> (go);
		GameObjectManager.addComponent<RefreshInterface> (_LevelInfoFamily.First ());
	}
}