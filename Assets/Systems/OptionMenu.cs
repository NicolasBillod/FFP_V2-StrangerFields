using UnityEngine;
using FYFY;
using PrimitiveFactory.Framework.UITimelineAnimation;

public class OptionMenu : FSystem
{
    // ----- VARIABLES -----

    //Families
    private Family _gameInformation = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(OptionUI)));
	private Family _soundPrefsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SoundPrefs)));

    // Game Information
    //private GameInformations _gameInfo;
    private OptionUI _interface;
	private SoundPrefs _soundPrefs;

    // ----- LIFECYCLE -----

    /// <summary>
    /// Call the procedure to initialize the option menu
    /// </summary>
    public OptionMenu()
    {
        //_gameInfo = _gameInformation.First().GetComponent<GameInformations>();
        _interface = _interfaceFamily.First().GetComponent<OptionUI>();
		_soundPrefs = _soundPrefsFamily.First ().GetComponent<SoundPrefs> ();
        UTAController.Instance.PlayAnimation("A_Options_Apparition");
        InitOptionMenu();
	}

    // ----- FONCTIONS / PROCEDURES -----

    /// <summary>
    /// Initialization of the main menu 
    /// </summary>
    protected void InitOptionMenu()
    {
        _interface.backButton.onClick.AddListener(() => OnBackMenuButton());
		if (_gameInformation.First ().GetComponent<GameInformations> ().playSounds) {
			_interface.soundsToggle.isOn = true;
		} else {
			_interface.soundsToggle.isOn = false;
		}

		if (_gameInformation.First ().GetComponent<GameInformations> ().playMusics) {
			_interface.musicsToggle.isOn = true;
		} else {
			_interface.musicsToggle.isOn = false;
		}

		_interface.soundsToggle.onValueChanged.AddListener ((bool value) => OnSoundsToggle (value));
		_interface.musicsToggle.onValueChanged.AddListener ((bool value) => OnMusicsToggle (value));
    }

    /// <summary>
    /// 
    /// </summary>
    protected void OnBackMenuButton()
    {
        UTAController.Instance.PlayAnimation("A_Options_Disparition");
    }

	private void OnSoundsToggle(bool shouldPlay){
		if (shouldPlay) {
			_gameInformation.First ().GetComponent<GameInformations> ().playSounds = true;
			PlayerPrefs.SetInt (_soundPrefs.soundPref, 0);
		}
		else {
			_gameInformation.First ().GetComponent<GameInformations> ().playSounds = false;
			PlayerPrefs.SetInt (_soundPrefs.soundPref, 1);
		}
	}

	private void OnMusicsToggle(bool shouldPlay){
		if (shouldPlay) {
			_gameInformation.First ().GetComponent<GameInformations> ().playMusics = true;
			PlayerPrefs.SetInt (_soundPrefs.musicPref, 0);
		} else {
			_gameInformation.First ().GetComponent<GameInformations> ().playMusics = false;
			PlayerPrefs.SetInt (_soundPrefs.musicPref, 1);
		}
	}
}
