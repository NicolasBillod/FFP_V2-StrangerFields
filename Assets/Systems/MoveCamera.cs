using UnityEngine;
using UnityEngine.UI;
using FYFY;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MoveCamera : UtilitySystem
{
    // ----- VARIABLES -----
	private Family _cameraFamily = FamilyManager.getFamily(new AllOfComponents(typeof(CameraParams)));
	private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
	private Family _tutoInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(TutoInformation)));
	private Family _soundFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Sound)));

	private CameraParams _cameraParams;

	private IsDragging _currentDragging;
	private Sound _sound;

	private RaycastHit _hit;
	private Ray _ray;

	private bool _isMovingCamera;

	// Raycaster things
	private List<RaycastResult> _results;
	private GraphicRaycaster _Raycaster;
	private EventSystem _EventSystem;


	private int _layerMask;
	private float tempLastDrag;

    // ---- FONCTIONS / PROCEDURES -----

	public MoveCamera()
    {
		_sound = _soundFamily.First ().GetComponent<Sound> ();
		// layer mask - not move camera if we clicked on one of those
		_layerMask = LayerMask.GetMask ("Selectable");
        InitCameras();
		#if (UNITY_EDITOR || UNITY_STANDALONE)
		_cameraParams.xSpeed = 10;
		#else
		_cameraParams.xSpeed = 0.2f;
		#endif
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame)
    {

	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame)
    {

	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {

		if (GetMouseOrTouchDown())
        {
			_ray = Camera.main.ScreenPointToRay(MouseOrTouchPosition());

			if (!Physics.Raycast (_ray, out _hit, Mathf.Infinity, _layerMask)) {
				//Debug.Log (_hit.collider.transform.gameObject.name+" "+_hit.collider.transform.gameObject.layer);
				_isMovingCamera = true;

				_cameraParams.pointStartDrag_x = MouseOrTouchPosition().x;
				tempLastDrag = _cameraParams.pointStartDrag_x;

				// Different raycast for UI to check if we are clicking of a button of a slider
				PointerEventData pointerData = new PointerEventData (EventSystem.current);
				pointerData.position = MouseOrTouchPosition();
				EventSystem.current.RaycastAll (pointerData, _results);

				
				// If we clicked on a button or a slider, don't move camera
				/*foreach (RaycastResult rr in _results)
                {
					if (rr.gameObject.GetComponent<Button> () || rr.gameObject.name == "Handle" || rr.gameObject.name == "Fill") {
						_isMovingCamera = false;
						break;
					}
				}*/

				if (_results.Count > 0) {
					foreach (RaycastResult rr in _results) {
						if (!rr.gameObject.CompareTag ("TutoTag")) {
							_isMovingCamera = false;
							_cameraParams.pointStartDrag_x = 0f;
						}
					}
				}
				_results.Clear ();
			}
		}

		if (GetMouseOrTouchUp() && _isMovingCamera) {
			_isMovingCamera = false;
			_cameraParams.distanceDrag_x += Mathf.Abs(tempLastDrag - MouseOrTouchPosition().x);
			_cameraParams.pointStartDrag_x = 0;
			tempLastDrag = 0;
		}

		if (_currentDragging.isDragging == false && GetMouseOrTouchHold() && _isMovingCamera)
		{
			float pointer_x = Input.GetAxis("Mouse X");
			if (Input.touchCount > 0) {
				pointer_x = Input.touches [0].deltaPosition.x;
			}

			//_cameraParams.transform.LookAt (_cameraParams.target);
			//_cameraParams.transform.Rotate (new Vector3 ());
			Vector3 difference = _cameraParams.target.position - _cameraParams.transform.position;
			float rotationY = Mathf.Atan2 (difference.x, difference.z) * Mathf.Rad2Deg;
			_cameraParams.transform.rotation = Quaternion.Euler (_cameraParams.transform.rotation.eulerAngles[0], rotationY, _cameraParams.transform.rotation.eulerAngles[2]);

			_cameraParams.transform.RotateAround(_cameraParams.target.position, Vector3.up, pointer_x * _cameraParams.xSpeed);

			//_cameraParams.distanceDrag_x = Input.mousePosition.x - _cameraParams.pointStartDrag_x;
			_cameraParams.distanceDrag_x += Mathf.Abs(tempLastDrag - MouseOrTouchPosition().x);
			tempLastDrag = MouseOrTouchPosition().x;
//			if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
//				FMODUnity.RuntimeManager.PlayOneShot (_sound.MoveCameraEvent);
		}

    }

    protected void InitCameras()
    {
		_cameraParams = _cameraFamily.First().GetComponent<CameraParams> ();
		_cameraParams.distance = Vector3.Distance(_cameraParams.transform.position, _cameraParams.target.position);

		_currentDragging = _dragFamily.First ().GetComponent<IsDragging> ();
		_isMovingCamera = false;

		_results = new List<RaycastResult> ();
		_EventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();
		_Raycaster = GameObject.Find ("Canvas").GetComponent<GraphicRaycaster> ();
    }
		
}