using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FYFY;
using System.Collections;

public class LevelsMenu : FSystem
{
    // ======================================================================================================================================================================
    // ================================= VARIABLES ================================= VARIABLES ================================== VARIABLES =================================
    // ======================================================================================================================================================================

    // ================================== Families ================================== Families ================================== Families ==================================
    private Family _levelButtonFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ButtonLevel)));
    private Family _gameInformation = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)));
	private Family _mainLoopFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelMenuUI), typeof(LoadingUI)));
    private Family _playerDataFamily = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerData)));

    // =============================== Components ================================= Components ================================= Components =================================
    private GameInformations _gameInfo;
    private LevelMenuUI _levelUI;
    private LoadingUI _loadingUI;
    private ButtonLevel _buttonLevel;
    private MainLoop _mainLoop;
    private PlayerData _playerData;

    // ======================================================================================================================================================================
    // ================================== METHODS =================================== METHODS ==================================== METHODS ==================================
    // ======================================================================================================================================================================

    // ================================= Lifecycle ================================= Lifecycle ================================= Lifecycle ==================================

    /// <summary>
    /// Constructor as Unity Start
    /// </summary>
	public LevelsMenu()
    {
        _gameInfo = _gameInformation.First().GetComponent<GameInformations>();
        _levelUI = _interfaceFamily.First().GetComponent<LevelMenuUI>();
        _loadingUI = _interfaceFamily.First().GetComponent<LoadingUI>();
        _buttonLevel = _levelButtonFamily.First().GetComponent<ButtonLevel>();
        _mainLoop = _mainLoopFamily.First().GetComponent<MainLoop>();
        _playerData = _playerDataFamily.First().GetComponent<PlayerData>();

        InitLevelsMenu();
	}

    /// <summary>
    /// Initialization of the levels menu
    /// </summary>
    public void InitLevelsMenu()
    {
		_loadingUI.loadingCanvas.enabled = false;

        Button[] listButton = _buttonLevel.buttonLevelPrefab.GetComponentsInChildren<Button>();
        int nbButton = listButton.Length;

		for (int i = 0; i < nbButton; i++)
        {
            int index = i + 1;
			listButton[i].onClick.AddListener (() => LevelToLoad(index));

            //Debug.Log(listButton[i].name);
            //Debug.Log("i = " + i + " ; index = " + index);

            if (index <= _playerData.unlockedLevels)
            {
                ElementLevelPanel _elmtLevelPanel = listButton[i].gameObject.GetComponent<ElementLevelPanel>();

                _elmtLevelPanel.chain.SetActive(false);


                for (int j = 0; j < _playerData.scoreList.Count; j++)
                {
                    if (_playerData.scoreList[j].numLevel == index)
                    {
                        _elmtLevelPanel.score.text = _playerData.scoreList[j].highScore.ToString();

                        if (_playerData.scoreList[j].nbStars >= 1)
                        {
                            _elmtLevelPanel.leftStar.SetActive(true);

                            if (_playerData.scoreList[j].nbStars >= 2)
                            {
                                _elmtLevelPanel.rightStar.SetActive(true);

                                if (_playerData.scoreList[j].nbStars == 3)
                                    _elmtLevelPanel.midStar.SetActive(true);
                            }
                        }
                    }
                }
            }
            else
            {
                listButton[i].interactable = false;
            }

		}

        _levelUI.bakcMenuButton.onClick.AddListener(() => OnBackMenuButton());
	}

    // =================================== Loading =================================== Loading =================================== Loading ===================================

    /// <summary>
    /// Procedure to come back to the Main Menu Scene
    /// </summary>
    protected void OnBackMenuButton()
    {
        GameObjectManager.loadScene("MenuScene");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indexLevel"></param>
    protected void LevelToLoad(int indexLevel)
    {
		_gameInfo.noLevel = indexLevel;
		//_mainLoop.StartCoroutine(LoadScreen());
		GameObjectManager.loadScene("GoodScene"); 
	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadScreen()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GoodScene");
        asyncLoad.allowSceneActivation = false;
        _levelUI.canvasLevel.enabled = false;
        _loadingUI.loadingCanvas.enabled = true;

        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log("progress: " + asyncLoad.progress); // fill the bar according to asyncLoad.progress (between 0 and 0.9)
            _loadingUI.loadingSlider.value = asyncLoad.progress;
            yield return null;
        }

        Debug.Log("Loading ended"); // fill the bar
        _loadingUI.loadingSlider.value = 1.0f;
        asyncLoad.allowSceneActivation = true; // we can activate the scene.
    }
}
