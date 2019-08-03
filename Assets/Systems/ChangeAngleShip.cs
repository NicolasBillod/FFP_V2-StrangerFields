using UnityEngine;
using FYFY;

/**************************************************
 * 
 * System : ChangeAngleShip
 * Change the rotation of the ship on the screen
 * Apply the effects on player ship trajectories
 * 
 *************************************************/
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ChangeAngleShip : UtilitySystem
{
    // ======================================================================================================================================================================
    // ================================= VARIABLES ================================= VARIABLES ================================== VARIABLES =================================
    // ======================================================================================================================================================================

    // ================================== Families ================================== Families ================================== Families ==================================
    private Family _shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass)));
    private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
    private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
	private Family _previousActionFamily = FamilyManager.getFamily (new AllOfComponents (typeof (PreviousActions)));
	private Family _otherInfoFamily = FamilyManager.getFamily (new AllOfComponents (typeof(OtherInformation)));
	private Family _tutoInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(TutoInformation)));


    // =============================== Components ================================= Components ================================= Components =================================
    private IsDragging _shipDrag;
    private ShipInfo _shipInfo;
    private State _state;
    private GameObject _starship;
	private PreviousActions _previousActions;
	private OtherInformation _otherInfo;

    // ================================== Boolean =================================== Boolean ==================================== Boolean ==================================
    private bool _isDragging = false;

    // ================================== Raycast =================================== Raycast ==================================== Raycast ==================================
    private Ray _ray;
    private RaycastHit _hit;

	// Raycaster UI things
	List<RaycastResult> _results;
	EventSystem _EventSystem;

    // ================================== Vector3 =================================== Vector3 ==================================== Vector3 ==================================
    private Vector3 _rotation;
	private Vector3 _mouseReference;
	private Vector3 _mouseCurrent;
	private Vector3 _cross;
	private Vector3 _mouseOffset;
	private Vector3 _shipInScreen;

	// ================================== Temp
	private DataGO _startData;
	private float _startAngle;

    // ======================================================================================================================================================================
    // ================================== METHODS =================================== METHODS ==================================== METHODS ==================================
    // ======================================================================================================================================================================

    /// <summary>
    /// Constructor -- like Unity Start()
    /// </summary>
    public ChangeAngleShip()
    {
        _starship = _shipFamily.First();
        _shipInfo = _starship.GetComponent<ShipInfo>();
        _shipDrag = _dragFamily.First().GetComponent<IsDragging>();
        _state = _stateFamily.First().GetComponent<State>();
		_previousActions = _previousActionFamily.First ().GetComponent<PreviousActions>();
		_otherInfo = _otherInfoFamily.First ().GetComponent<OtherInformation> ();

		_results = new List<RaycastResult> ();
		_EventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();
    }

    /// <summary>
    /// Use to process families
    /// </summary>
    /// <param name="familiesUpdateCount"></param>
	protected override void onProcess(int familiesUpdateCount)
    {
        // Layer of selectable elements
		int layerMask = LayerMask.GetMask("Selectable");

		if (GetMouseOrTouchDown() && _state.state == State.STATES.SETUP)
        {
			_ray = Camera.main.ScreenPointToRay(MouseOrTouchPosition());

            // If the gameobject is the player ship
			if (Physics.Raycast (_ray, out _hit, Mathf.Infinity, layerMask) && _hit.collider.transform.parent.gameObject.CompareTag("ShipTag"))
            {
				// Different raycast for UI to check if we are clicking of a button of a slider
				PointerEventData pointerData = new PointerEventData (EventSystem.current);
				pointerData.position = MouseOrTouchPosition();
				EventSystem.current.RaycastAll (pointerData, _results);

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

				if (!isOverUI) {
					_isDragging = true;
					_shipDrag.isDragging = true;
					_rotation = Vector3.zero;
					_mouseCurrent = MouseOrTouchPosition();
					_shipInScreen = Camera.main.WorldToScreenPoint (_starship.transform.position);
					_shipInScreen [2] = 0;
					_mouseReference = new Vector3 (_mouseCurrent.x - _shipInScreen.x, _mouseCurrent.y - _shipInScreen.y, _mouseCurrent.z - _shipInScreen.z);
					_otherInfo.rotationAngle = 0f;

					//_previousActions.listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.ANGLE);
					//_previousActions.cancelData.Add (StoreGameObjectComponents (_starship));
					//_previousActions.listOfRedoActions.Clear ();
					_startData = StoreGameObjectComponents(_starship);
					_startAngle = _starship.GetComponent<ShipInfo> ().angle;
				}

				_results.Clear ();
			}
		}

        if (_isDragging)
        {
			if (GetMouseOrTouchHold())
            {
				_mouseCurrent = MouseOrTouchPosition();
				_mouseCurrent = new Vector3 (_mouseCurrent.x - _shipInScreen.x, _mouseCurrent.y - _shipInScreen.y, _mouseCurrent.z - _shipInScreen.z);

                // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction
                _rotation.y = Vector3.SignedAngle (_mouseReference, _mouseCurrent, Vector3.back) * 0.6f;
				_starship.transform.Rotate (_rotation);
				_otherInfo.rotationAngle += _rotation.y;

				NewFireIntensity (_starship.transform.rotation.eulerAngles.y * (-1));
				_mouseReference = _mouseCurrent;
            }
        }

		if (GetMouseOrTouchUp() && _isDragging)
        {
            _isDragging = false;
            _shipDrag.isDragging = false;

			if (Mathf.Abs(_starship.GetComponent<ShipInfo> ().angle - _startAngle) > 1f) {
				_previousActions.listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.ANGLE);
				_previousActions.cancelData.Add (_startData);
				_previousActions.listOfRedoActions.Clear ();
			}
        }
    }

    /// <summary>
    /// Change player ship's fire intensity when he changes the rotation
    /// Refresh trajectories
    /// </summary>
    /// <param name="value"></param>
    private void NewFireIntensity(float value)
    {
        float oldMagnitude = _shipInfo.fireIntensity.magnitude;
        float ang = value * -1;
        _shipInfo.angle = value;
        _shipInfo.fireIntensity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * ang), Mathf.Sin(Mathf.Deg2Rad * ang), 0);
        _shipInfo.fireIntensity.Normalize();
        _shipInfo.fireIntensity *= oldMagnitude;

        GameObjectManager.addComponent<RefreshTrajectory>(_levelInfoFamily.First());
    }
}