using UnityEngine;
using FYFY;

public class ChangeAngleShipLD : FSystem
{
    //Families
    private Family _shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass), typeof(MovableShip)));
    private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));

    //Components
    private IsDragging _shipDrag;
    private ShipInfo _shipInfo;
    private MovableShip _isShipMovable;

    //Bool Variables
    private bool _isDragging = false;

    //Raycast Variables
    private Ray _ray;
    private RaycastHit _hit;

    // Vector3 Variables
	private Vector3 _rotation;
	private Vector3 _mouseReference;
	private Vector3 _mouseCurrent;
	private Vector3 _cross;
	private Vector3 _mouseOffset;

	private Vector3 _shipInScreen;

    GameObject _starship;

    public ChangeAngleShipLD()
    {
        _starship = _shipFamily.First();
        _shipInfo = _starship.GetComponent<ShipInfo>();
        _shipDrag = _dragFamily.First().GetComponent<IsDragging>();
        _isShipMovable = _starship.GetComponent<MovableShip>();
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {   

		int layerMask = 1 << 9; // raycast can only hit the Ship layer

		if (Input.GetMouseButtonDown (0) && _isShipMovable.isMovable == false)
        {
			_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast (_ray, out _hit, Mathf.Infinity, layerMask))
            {
				_isDragging = true;
				_shipDrag.isDragging = true;
				_rotation = Vector3.zero;
				_mouseCurrent = Input.mousePosition;
				_shipInScreen = Camera.main.WorldToScreenPoint (_starship.transform.position);
				_shipInScreen [2] = 0;
				_mouseReference = new Vector3 (_mouseCurrent.x - _shipInScreen.x, _mouseCurrent.y - _shipInScreen.y, _mouseCurrent.z - _shipInScreen.z);
			}
		}

        if (_isDragging)
        {
            if (Input.GetMouseButton(0))
            {
				_mouseCurrent = Input.mousePosition;
				_mouseCurrent = new Vector3 (_mouseCurrent.x - _shipInScreen.x, _mouseCurrent.y - _shipInScreen.y, _mouseCurrent.z - _shipInScreen.z);

				_rotation.y = Vector3.SignedAngle (_mouseReference, _mouseCurrent, Vector3.back) * 0.6f;

				_starship.transform.Rotate (_rotation);
				NewFireIntensity (_starship.transform.rotation.eulerAngles.y * (-1));
				_mouseReference = _mouseCurrent;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            _shipDrag.isDragging = false;
        }
    }

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