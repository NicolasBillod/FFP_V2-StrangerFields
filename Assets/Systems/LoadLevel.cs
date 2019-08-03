using UnityEngine;
using System.Collections.Generic;
using FYFY;

/***************************************************************************
 * 
 * System : LoadLevel
 * Inactive all fields and foes
 * Load data from Scriptable Object (position, rotation, field intensity...) 
 * 
 **************************************************************************/

public class LoadLevel : FSystem
{
    // =====================================================================================================================================================================
    // ================================= VARIABLES ================================= VARIABLES ================================= VARIABLES =================================
    // =====================================================================================================================================================================

    // ================================= Families ================================= Families ================================= Families =================================
    private Family _ShipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass)));
    private Family _TerrainFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
    private Family _EarthFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FinishInformation), typeof(Position)));
	private Family _GameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
    private Family _FieldsRemaining = FamilyManager.getFamily(new AllOfComponents(typeof(FieldsCounter)));
    private Family _PrefabsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)));
    private Family _LevelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
	private Family _soundFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Sound)));

    // ================================ Components ================================ Components ================================ Components ================================
    GameObject _GameInfo;
    GameObject _ShipGO;
    GameObject _EarthGO;
    FieldsCounter _fieldsCounter;
    LevelInformations _levelInfo;
    int _CurrentNoLevel;
	private Sound _sound;

    // =============================== Pool Prefabs =============================== Pool Prefabs =============================== Pool Prefabs ===============================
    List<Transform> placedAttFields;
    List<Transform> placedRepFields;
	List<Transform> attFFToPlace;
	List<Transform> repFFToplace;
    List<Transform> foes;

    // ================================= Score Var ================================== Score Var ================================== Score Var =================================
    bool boolEnnemi = false;
    int firstInterField, lastInterField, firstInterEnnemi, lastInterEnnemi, nbFieldsToPlace = 0;

    // ========================================================================================================================================================================
    // =================================== METHODS =================================== METHODS =================================== METHODS ====================================
    // ========================================================================================================================================================================

    /// <summary>
    /// Constructor -- like Unity Start()
    /// </summary>
    public LoadLevel()
    {
        // Recovery GameObject or Components
		_GameInfo = _GameInfoFamily.First();
        _ShipGO = _ShipFamily.First();
        _EarthGO = _EarthFamily.First();
		_CurrentNoLevel = _GameInfo.GetComponent<GameInformations>().noLevel;
        _fieldsCounter = _FieldsRemaining.First().GetComponent<FieldsCounter>();
        _levelInfo = _LevelInfoFamily.First().GetComponent<LevelInformations>();
		_sound = _soundFamily.First ().GetComponent<Sound> ();

		// Load musics and sounds
		_sound.musicGameplay = FMODUnity.RuntimeManager.CreateInstance (_sound.MusicGameplayEvent);
		_sound.musicWin = FMODUnity.RuntimeManager.CreateInstance (_sound.MusicWinEvent);
		_sound.musicLose = FMODUnity.RuntimeManager.CreateInstance (_sound.MusicLoseEvent);

		_sound.fire = FMODUnity.RuntimeManager.CreateInstance (_sound.FireEvent);

        // Inactive canvas selection / fields / foes
        _ShipGO.transform.Find("CanvasSelection").gameObject.SetActive(false);

        placedAttFields = _fieldsCounter.poolAttFFPlaced;
		for (int i = 0; i < placedAttFields.Count; i ++)
		{
            placedAttFields[i].Find("CanvasSelection").gameObject.SetActive(false);
            placedAttFields[i].gameObject.SetActive(false);
		}

        placedRepFields = _fieldsCounter.poolRepFFPlaced;
		for (int i = 0; i < placedRepFields.Count; i ++)
		{
            placedRepFields[i].Find("CanvasSelection").gameObject.SetActive(false);
            placedRepFields[i].gameObject.SetActive(false);
		}

        attFFToPlace = _fieldsCounter.poolAttractive;
		for (int i = 0; i < attFFToPlace.Count; i ++)
		{
            attFFToPlace[i].gameObject.SetActive(false);
		}

        repFFToplace = _fieldsCounter.poolRepulsive;
		for (int i = 0; i < repFFToplace.Count; i ++)
		{
            repFFToplace[i].gameObject.SetActive(false);
		}

        foes = _fieldsCounter.poolFoes;
		for (int i = 0; i < foes.Count; i++)
		{
            GameObjectManager.setGameObjectState(foes[i].gameObject, false);
		}

        // Load Data of the Level n°_CurrentNoLevel
        LoadLevelData(_CurrentNoLevel);

    }

    /// <summary>
    /// Load Data of the Level numLevel from a Scriptable Object
    /// Position / rotation of objets
    /// Intensity fields, number of available fields
    /// First fire intensity of foes
    /// Instantiate bonus / malus / obstacle
    /// </summary>
    /// <param name="numLevel"></param>
    public void LoadLevelData(int numLevel)
    {
        FieldsCounter fieldsToPlace = _FieldsRemaining.First().GetComponent<FieldsCounter>();
        PrefabContainer prefabContainer = _PrefabsFamily.First().GetComponent<PrefabContainer>();

		Transform trShipPosition = _ShipGO.GetComponent<Transform>();
        Position shipPositionComponent = _ShipGO.GetComponent<Position>();

        Transform trTargetPosition = _EarthGO.GetComponent<Transform>();
        Position targetPositionComponent = _EarthGO.GetComponent<Position>();

        Vector3 terrainDimensions = _TerrainFamily.First().GetComponent<Terrain>().terrainData.size;

		LevelClassData theData = (LevelClassData) Resources.Load(string.Concat("LevelData/Level", numLevel), typeof(LevelClassData));

        if (theData == null)
        {
            Debug.Log("Ficher nul");
        }
        else
        {
            fieldsToPlace.fieldsAttToPlace = theData.nbAttractive;
            fieldsToPlace.fieldsRepToPlace = theData.nbRepulsive;
			trShipPosition.position = theData.shipPosition;
			changeTransformPosToPosition(shipPositionComponent, trShipPosition.position, terrainDimensions);
            trTargetPosition.position = theData.targetPosition;
            changeTransformPosToPosition(targetPositionComponent, trTargetPosition.position, terrainDimensions);

			/**** TESTS ORIENTATION SHIP ****/
			trShipPosition.LookAt (trTargetPosition.position);
			trShipPosition.Rotate (Vector3.up * -90);
			ShipInfo shipInfo = _ShipGO.GetComponent<ShipInfo> ();
			if (trShipPosition.rotation.eulerAngles.y < 0) {
				shipInfo.angle = 360 + trShipPosition.rotation.eulerAngles.y * (-1);
			} else {
				shipInfo.angle = trShipPosition.rotation.eulerAngles.y * (-1);
			}
			shipInfo.fireIntensity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * shipInfo.angle), Mathf.Sin(Mathf.Deg2Rad * shipInfo.angle), 0);
			shipInfo.fireIntensity.Normalize();
			shipInfo.fireIntensity *= -0.5f;
			/********** END TESTS ***********/


            int numberPlacedAtt = theData.attFFList.Count;
            for (int i = 0; i < numberPlacedAtt; i++)
            {
                Transform theField = placedAttFields[i];
                theField.gameObject.SetActive(true);
                theField.GetComponent<Field>().A = theData.attFFList[i].intensity;
                Vector3 fieldWorldPos = theField.position = theData.attFFList[i].position;
                Position thePos = theField.GetComponent<Position>();
                changeTransformPosToPosition(thePos, fieldWorldPos, terrainDimensions);
            }

            int numberPlacedRep = theData.repFFList.Count;
            for (int i = 0; i < numberPlacedRep; i++)
            {
                Transform theField = placedRepFields[i];
                theField.gameObject.SetActive(true);
                theField.GetComponent<Field>().A = theData.repFFList[i].intensity;
                Vector3 fieldWorldPos = theField.position = theData.repFFList[i].position;
                Position thePos = theField.GetComponent<Position>();
                changeTransformPosToPosition(thePos, fieldWorldPos, terrainDimensions);
            }

            int numberFoes = theData.foeList.Count;
            for (int i = 0; i < numberFoes; i++)
            {
                FoeClass dataFoe = theData.foeList[i];
                Transform theFoe = foes[i];
                ShipInfo theShipInfo = theFoe.GetComponent<ShipInfo>();
				GameObjectManager.setGameObjectState(theFoe.gameObject, true);
				Vector3 foeWorldPos = theFoe.position = dataFoe.position;
                theFoe.rotation = dataFoe.rotation;
                theShipInfo.fireIntensity = dataFoe.fireIntensity;
                theShipInfo.angle = dataFoe.angle;
				updateRotationTransform (theFoe, theShipInfo.angle);
                Position foePos = theFoe.GetComponent<Position>();
                changeTransformPosToPosition(foePos, foeWorldPos, terrainDimensions);
            }

            int numberBm = theData.bmList.Count;
            for (int i = 0; i < numberBm; i++)
            {
                BonusMalus theBm = theData.bmList[i];
                GameObject BmGo = null;

                switch (theBm.type)
                {
                    case BonusMalus.TYPE.B_PLAYER:
                        BmGo = GameObject.Instantiate(prefabContainer.bPlayer);
                        break;
                    case BonusMalus.TYPE.B_DAMAGE:
                        BmGo = GameObject.Instantiate(prefabContainer.bDamage);
                        break;
                    case BonusMalus.TYPE.M_EARTH:
                        BmGo = GameObject.Instantiate(prefabContainer.mEarth);
                        break;
                    case BonusMalus.TYPE.M_FOELIFE:
                        BmGo = GameObject.Instantiate(prefabContainer.mFoeLife);
                        break;
                }

                GameObjectManager.bind(BmGo);
                BmGo.transform.position = theBm.position;
                BmGo.transform.Find("CanvasSelection").gameObject.SetActive(false);
            }

            int numberObs = theData.obstacleList.Count;
            for (int i = 0; i < numberObs; i++)
            {
                Obstacle theObstacle = theData.obstacleList[i];
                GameObject obstacleGO = null;

                if (theObstacle.isBreakable)
                {
                    obstacleGO = GameObject.Instantiate(prefabContainer.breakableObstacle);
                }
                else
                {
                    obstacleGO = GameObject.Instantiate(prefabContainer.normalObstacle);
                }

                GameObjectManager.bind(obstacleGO);
                obstacleGO.transform.position = theObstacle.position;
                obstacleGO.transform.rotation = theObstacle.rotation;
            }

            nbFieldsToPlace = theData.nbAttractive + theData.nbRepulsive;

            switch (nbFieldsToPlace)
            {
                case 0:
                case 1:
                case 2:
                    firstInterField = 8250;
                    lastInterField = 13500;
                    break;

                case 3:
                case 4:
                    firstInterField = 8250;
                    lastInterField = 14400;
                    break;

                case 5:
                case 6:
                    firstInterField = 9000;
                    lastInterField = 16800;
                    break;

                case 7:
                case 8:
                    firstInterField = 9750;
                    lastInterField = 19200;
                    break;

                case 9:
                case 10:
                    firstInterField = 9750;
                    lastInterField = 21600;
                    break;
            }

            if (numberFoes > 0)
            {
                boolEnnemi = true;

                switch(numberFoes)
                {
                    case 1:
                        firstInterEnnemi = 8250;
                        lastInterEnnemi = 15200;
                        break;

                    case 2:
                        firstInterEnnemi = 7800;
                        lastInterEnnemi = 18400;
                        break;

                    case 3:
                        firstInterEnnemi = 7150;
                        lastInterEnnemi = 21600;
                        break;
                }
            }

            if (boolEnnemi)
            {
                _levelInfo.firstNbIntervalScore = Mathf.RoundToInt((firstInterField + firstInterEnnemi) / 2);
                _levelInfo.lastNbIntervalScore = Mathf.RoundToInt((lastInterField + lastInterEnnemi) / 2);
            }
            else
            {
                _levelInfo.firstNbIntervalScore = firstInterField;
                _levelInfo.lastNbIntervalScore = lastInterField;
            }
        }        
    }

    /// <summary>
    /// Fill the component "Position" of an object in terms of transform position and terrain dimensions
    /// </summary>
    /// <param name="position"></param>
    /// <param name="thePosition"></param>
    /// <param name="terrDims"></param>
    public void changeTransformPosToPosition(Position position, Vector3 thePosition, Vector3 terrDims)
    {
		Constants cst = _GameInfo.GetComponent<Constants>();

        Vector3 newPosition = new Vector3(thePosition.x / cst.TERRAIN_INTERACTABLE_X, thePosition.z / cst.TERRAIN_INTERACTABLE_Y, 0);

        position.pos = newPosition;
		position.initialPos = newPosition;
    }

    /// <summary>
    /// Change foe rotation in terms of its angle in the component "ShipInfo"
    /// </summary>
    /// <param name="trsfrm"></param>
    /// <param name="angle"></param>
	public void updateRotationTransform(Transform trsfrm, float angle)
    {
		angle *= -1;
		float angleTemp = angle;
		if (angle >= 0 && angle <= 180)
			angleTemp = angle;
		else //(bestAngle > 180)
			angleTemp = (360 - angle) * (-1);

		trsfrm.rotation = Quaternion.Euler (trsfrm.rotation.eulerAngles[0], angleTemp, trsfrm.rotation.eulerAngles[2]);
	}
}