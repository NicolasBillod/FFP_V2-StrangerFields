using FYFY;
using PrimitiveFactory.Framework.UITimelineAnimation;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonState : FSystem
{
	private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GlobalUI), typeof(OtherPanel), typeof(LoadingUI)));
	private Family _selectInformationFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SelectInformation)));
	private Family _fieldsCounterFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FieldsCounter)));

	private GlobalUI _globalUI;
	private IsDragging _isDragging;

	private bool buttonsAndSliderslocked;

	public ButtonState() {

		buttonsAndSliderslocked = false;
		_globalUI = _interfaceFamily.First ().GetComponent<GlobalUI> ();
		_isDragging = _selectInformationFamily.First ().GetComponent<IsDragging> ();

	}


	protected override void onProcess(int familiesUpdateCount) {
		//#if (UNITY_STANDALONE || UNITY_EDITOR)

		//#else
		if (_isDragging.isDragging){
			if(_globalUI.redoButton.interactable || _globalUI.undoButton.interactable){
				_globalUI.redoButton.interactable = false;
				_globalUI.undoButton.interactable = false;
			}
			if(_globalUI.attractiveButton.interactable ||_globalUI.repulsiveButton.interactable || _globalUI.intensitySlider.interactable){
				_globalUI.attractiveButton.interactable = false;
				_globalUI.repulsiveButton.interactable = false;
				_globalUI.intensitySlider.interactable = false;
			}
			if(/*Input.touchCount >= 1 &&*/ !buttonsAndSliderslocked) {
				buttonsAndSliderslocked = true;

				//_globalUI.attractiveButton.interactable = false;
				_globalUI.backMenuButton.interactable = false;
				_globalUI.fireButton.interactable = false;
				//_globalUI.redoButton.interactable = false;
				//_globalUI.repulsiveButton.interactable = false;
				_globalUI.restartButton.interactable = false;
				//_globalUI.undoButton.interactable = false;
				_globalUI.deleteButton.interactable = false;

				//_globalUI.intensitySlider.interactable = false;
				_globalUI.speedSlider.interactable = false;
			}
		} else if (!_isDragging.isDragging /*&& Input.touchCount == 0*/ && buttonsAndSliderslocked) {
			buttonsAndSliderslocked = false;

			FieldsCounter fieldsAlreadyPlaced = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();

			int fieldsRepRemaining = fieldsAlreadyPlaced.fieldsRepToPlace - fieldsAlreadyPlaced.fieldsRepPlaced;
			int fieldsAttRemaining = fieldsAlreadyPlaced.fieldsAttToPlace - fieldsAlreadyPlaced.fieldsAttPlaced;

			if (fieldsRepRemaining > 0)
				_globalUI.repulsiveButton.interactable = true;

			if(fieldsAttRemaining > 0)
				_globalUI.attractiveButton.interactable = true;
			
			_globalUI.backMenuButton.interactable = true;
			_globalUI.fireButton.interactable = true;
			//_globalUI.redoButton.interactable = true;
			_globalUI.restartButton.interactable = true;
			//_globalUI.undoButton.interactable = true;
			_globalUI.deleteButton.interactable = true;

			_globalUI.intensitySlider.interactable = true;
			_globalUI.speedSlider.interactable = true;
		}
		//#endif
	}

}
