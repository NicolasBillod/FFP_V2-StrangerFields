using UnityEngine;
using FYFY;
using PrimitiveFactory.Framework.UITimelineAnimation;

public class MainMenu : FSystem
{
    // ----- VARIABLES -----

	private Family _createGameInfosFamily = FamilyManager.getFamily(new AllOfComponents(typeof(CreateGameInfos)));
    private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)));
    private Family _menuInterfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainMenuUI)));
    private Family _loadSceneFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LoadCorrectScene)));
	private Family _soundPrefsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(SoundPrefs)));

	private GameObject _gameInfos;
    private MainMenuUI _interface;
    private LoadCorrectScene _loadCorrectScene;
	private SoundPrefs _soundPrefs;

    // ----- LIFECYCLE -----

    public MainMenu()
    {
		Debug.Log ("This is a "+ Graphics.activeTier);
        UTAController.Instance.PlayAnimation("A_Open_MainScreen");
        _gameInfos = _gameInfoFamily.First();
        _interface = _menuInterfaceFamily.First().GetComponent<MainMenuUI>();
        _loadCorrectScene = _loadSceneFamily.First().GetComponent<LoadCorrectScene>();
		_soundPrefs = _soundPrefsFamily.First ().GetComponent<SoundPrefs> ();
        InitMainMenu();

	}

    // ----- FONCTIONS / PROCEDURES -----

    /// <summary>
    /// Initialization of the main menu 
    /// </summary>
    protected void InitMainMenu()
    {
        // TO RESET PLAYER PREF
        //PlayerPrefs.DeleteAll ();

        // Loading saved game informations

        // If _gameInfos is null
        // Fill information inside
		if (_gameInfos == null)
        {
			GameObject _goCreateGameInfos = _createGameInfosFamily.First();
			_gameInfos = GameObject.Instantiate(_goCreateGameInfos.GetComponent<CreateGameInfos>().gameInfosPrefab);
			_gameInfos.name = "GameInformations";
			GameObjectManager.dontDestroyOnLoadAndRebind(_gameInfos);
		}


		// Checking for sound preference
		if (PlayerPrefs.HasKey (_soundPrefs.soundPref)) {
			// 0 == sound; 1 == no sound
			if (PlayerPrefs.GetInt (_soundPrefs.soundPref) > 0)
				_gameInfos.GetComponent<GameInformations> ().playSounds = false;
			else
				_gameInfos.GetComponent<GameInformations> ().playSounds = true;
		}
		else {
			PlayerPrefs.SetInt (_soundPrefs.soundPref, 0);
		}


		// Checking for music preference
		if (PlayerPrefs.HasKey (_soundPrefs.musicPref)) {
			// 0 == sound; 1 == no sound
			if (PlayerPrefs.GetInt (_soundPrefs.musicPref) > 0)
				_gameInfos.GetComponent<GameInformations> ().playMusics = false;
			else
				_gameInfos.GetComponent<GameInformations> ().playMusics = true;
		}
		else {
			PlayerPrefs.SetInt (_soundPrefs.musicPref, 0);
		}

        // If the player played the game
        // Recover the highest unlocked level
		if (PlayerPrefs.HasKey("highestUnlockedLevel"))
        {
			GameInformations _levelInfos = _gameInfos.GetComponent<GameInformations>();
			_levelInfos.unlockedLevels = PlayerPrefs.GetInt("highestUnlockedLevel");
		}

		_interface.playButton.onClick.AddListener(() => OnPlayButton());
        _interface.creditsButton.onClick.AddListener(() => OnCreditsButton());
        _interface.optionButton.onClick.AddListener(() => OnOptionButton());
	}

    /// <summary>
    /// When play button is clicked
    /// Load level scene
    /// </summary>
	protected void OnPlayButton()
    {
        _loadCorrectScene.sceneName = "LevelScene";
        UTAController.Instance.PlayAnimation("A_Close_MainScreen");
        //GameObjectManager.loadScene("LevelScene");
        //SceneManager.LoadScene("LevelScene");
	}

    /// <summary>
    /// When option button is clicked
    /// Load option scene
    /// </summary>
    protected void OnOptionButton()
    {
        _loadCorrectScene.sceneName = "OptionScene";
        UTAController.Instance.PlayAnimation("A_Close_MainScreen");
        //GameObjectManager.loadScene("OptionScene");
    }

    /// <summary>
    /// When option button is clicked
    /// Load option scene
    /// </summary>
    protected void OnCreditsButton()
    {
        _loadCorrectScene.sceneName = "CreditsScene";
        UTAController.Instance.PlayAnimation("A_Close_MainScreen");
        //GameObjectManager.loadScene("CreditsScene");
    }
}
