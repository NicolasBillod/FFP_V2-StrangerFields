using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using FYFY;

/*
 *  - Camera from http://wiki.unity3d.com/index.php?title=MouseOrbitZoom and adapted to ECS
 */

public class PlayerActionsLD : UtilitySystem
{
	// ==== VARIABLES ====


	// === Object Selection
	private Family _sourcesFamily 	= FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
	private Family _shipFamily = FamilyManager.getFamily (new AllOfComponents (typeof(ShipInfo), typeof(Position), typeof(Mass), typeof(Editable), typeof(MovableShip)));
    private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
	private Family _selectionFamily = FamilyManager.getFamily (new AllOfComponents (typeof(SelectInformation)));
	private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));
	private Family _levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
    private Family _movableFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Movable)));
    private Family _TerrainFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));

    // === Object drag and drop
    private bool isMouseDrag = false;
	private Vector3 screenPosition;
	private Vector3 offset;
	private IsDragging _objectDrag;
    private GameInformations _GameInfo;
    private Vector3 _terrainDimensions;

    // == Raycast
    private int _layerMask;
	private Ray _ray;
	private RaycastHit _hit;
    private List<RaycastResult> _results;

    // === UI
    private CanvasGroup sourcesInformationsPanel;
	private CanvasGroup shipSpeedPanel;

	// === Components
	private SelectInformation _selectInfo;
	private Constants _cst;
	private GameObject _levelInfoGO;

	// ==== LIFECYCLE ====

	public PlayerActionsLD()
    {
        _GameInfo = _gameInfoFamily.First().GetComponent<GameInformations>();
        _objectDrag = _dragFamily.First().GetComponent<IsDragging>();
		_selectInfo = _selectionFamily.First().GetComponent<SelectInformation> ();
		_cst = _gameInfoFamily.First ().GetComponent<Constants> ();
		_levelInfoGO = _levelInfoFamily.First ();
        _terrainDimensions = _TerrainFamily.First().GetComponent<Terrain>().terrainData.size;


        // Raycast
        /*_layerMask = 1 << 9; // Our ship
		_layerMask += 1 << 11; // Force fields
        _layerMask += 1 << 13; //Foes and Target
        _layerMask += 1 << 17; //Bonus and Malus
        _layerMask += 1 << 18; //Obstacle
        */
		_layerMask += 1 << 24;
	}

	protected override void onProcess(int familiesUpdateCount)
    {
		DetectMouseSelection (); 
	} 

	// ==== METHODS ====

	// === Object Selection and Drag & drop
	protected void DetectMouseSelection()
    {
		if (_objectDrag.cameraChanged)
        {
			_objectDrag.isDragging = isMouseDrag = false;
			_objectDrag.cameraChanged = false;
		}

		if (Input.GetMouseButtonDown(0))
        {
			_ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// touched something
			if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, _layerMask))
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;
                EventSystem.current.RaycastAll(pointerData, _results);

                if (_results.Count == 0)
                {
                    // select now object if it match our criteria
                    GameObject go = _hit.collider.transform.parent.gameObject;
                    isSourceSelected(go);
                    isShipLauncherSelected(go);
                    isMovableObjSelected(go);
                }

                _results.Clear();
			}
		}

		if (Input.GetMouseButtonUp(0) && isMouseDrag)
        {
			if (_selectInfo.selectedGameObject != null) {
				Rigidbody r = _selectInfo.selectedGameObject.GetComponent<Rigidbody> ();
				r.velocity = Vector3.zero;
				_selectInfo.selectedGameObject.GetComponent<Position> ().pos = new Vector3 (_selectInfo.selectedGameObject.transform.position.x / _cst.TERRAIN_INTERACTABLE_X, _selectInfo.selectedGameObject.transform.position.z / _cst.TERRAIN_INTERACTABLE_Y, 0);

				GameObjectManager.addComponent<RefreshTerrain> (_levelInfoGO, new {action = RefreshTerrain.ACTIONS.MOVE, source = _selectInfo.selectedGameObject });
				GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
			}
			_objectDrag.isDragging = isMouseDrag = false;
		}

		if (isMouseDrag)
        {

			//track mouse position.
			Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
			//convert screen position to world position with offset changes.
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;

			Rigidbody r = _selectInfo.selectedGameObject.GetComponent<Rigidbody> ();
			Vector3 pos = new Vector3 (currentPosition.x, _selectInfo.selectedGameObject.transform.position.y, currentPosition.z);
			Vector3 direction = (pos - _selectInfo.selectedGameObject.transform.position);
			r.velocity = direction * 10;

			if (r.velocity.magnitude > 40)
            {
				direction.Normalize ();
				r.velocity = direction * 50;
			}
			_selectInfo.selectedGameObject.GetComponent<Position> ().pos = new Vector3 (_selectInfo.selectedGameObject.transform.position.x / _cst.TERRAIN_INTERACTABLE_X, _selectInfo.selectedGameObject.transform.position.z / _cst.TERRAIN_INTERACTABLE_Y, 0);

			GameObjectManager.addComponent<RefreshTerrain> (_levelInfoGO, new {action = RefreshTerrain.ACTIONS.MOVE, source = _selectInfo.selectedGameObject });
			GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
		}
	}

    protected bool isSourceSelected(GameObject go)
    {
		// Source found
		if (_sourcesFamily.contains (go.GetInstanceID ()))
        {
            MoveObject(go);

            return true;
		}
		return false;
	}

	protected bool isShipLauncherSelected(GameObject go)
    {
		// Ship found
		if (_shipFamily.contains (go.GetInstanceID()))
        {
            bool isShipMove = go.GetComponent<MovableShip>().isMovable;

            if (isShipMove)
                MoveObject(go);

			return true;
		}
		return false;
	}

    private bool isMovableObjSelected(GameObject go)
    {
        if (_movableFamily.contains(go.GetInstanceID()))
        {
            MoveObject(go);

            return true;
        }

        return false;
    }

    private void MoveObject(GameObject go)
    {
        _selectInfo.selectedGameObject = go;

        _objectDrag.isDragging = isMouseDrag = true;
        screenPosition = Camera.main.WorldToScreenPoint(go.transform.position);
        offset = go.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
        Position goPosition = go.GetComponent<Position>();
        changeTransformPosToPosition(goPosition, go.transform.position, _terrainDimensions);

    }

    public void changeTransformPosToPosition(Position position, Vector3 thePosition, Vector3 terrDims)
    {
        Constants cst = _GameInfo.GetComponent<Constants>();

        Vector3 newPosition = new Vector3(thePosition.x / cst.TERRAIN_INTERACTABLE_X, thePosition.z / cst.TERRAIN_INTERACTABLE_Y, 0);

        position.pos = newPosition;
        position.initialPos = newPosition;
    }
}