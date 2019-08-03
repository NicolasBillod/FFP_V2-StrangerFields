using FYFY;
using UnityEngine;
using System.Collections.Generic;

public class SaveData : FSystem
{
    // ======================================================================================================================================================================
    // ================================= VARIABLES ================================= VARIABLES ================================== VARIABLES =================================
    // ======================================================================================================================================================================

    // ================================== Families ================================== Families ================================== Families ==================================
    private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)));
    private Family _scoreFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ScoreVar)));
    private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
    // =============================== Components ================================= Components ================================= Components =================================
    private GameInformations _gameInfo;
    private ScoreVar _scoreVar;
    private PlayerProgress _playerProgress;
    private State _currentState;
    private LevelInformations _levelInfo;
    // ==================================== Other ==================================== Other ==================================== Other =====================================
    private bool _isSaved;
    private string _stringKey = "playerProgress";

    // ======================================================================================================================================================================
    // ================================== METHODS =================================== METHODS ==================================== METHODS ==================================
    // ======================================================================================================================================================================

    // ================================= Lifecycle ================================= Lifecycle ================================= Lifecycle ==================================

    /// <summary>
    /// Constructor
    /// </summary>
    public SaveData()
    {
        /*#if UNITY_EDITOR
        PlayerPrefs.DeleteKey(_stringKey);
        #endif*/

        _isSaved = false;
        _playerProgress = new PlayerProgress();
        _gameInfo = _gameInfoFamily.First().GetComponent<GameInformations>();
        _scoreVar = _scoreFamily.First().GetComponent<ScoreVar>();
        _currentState = _stateFamily.First().GetComponent<State>();
        _levelInfo = _levelInfoFamily.First().GetComponent<LevelInformations>();

        if (PlayerPrefs.HasKey(_stringKey))
        {
            Debug.Log("load high score data");
            LoadData();
        }
        else
        {
            Debug.Log("first launch");
            string serializedObject = JsonUtility.ToJson(_playerProgress);
            PlayerPrefs.SetString(_stringKey, serializedObject);
        }
    }

    /// <summary>
    /// Use to processe families
    /// </summary>
    /// <param name="familiesUpdateCount"></param>
    protected override void onProcess(int familiesUpdateCount)
    {
        if (_currentState.state == State.STATES.WON && !_isSaved)
        {
            _isSaved = true;
            SaveScore();
        }
    }

    // ================================ Manage Data ================================ Manage Data ================================ Manage Data ================================
    
    /// <summary>
    /// Save player's high score data for a level
    /// </summary>
    public void SaveScore()
    {
        // ============================ Unlocked Levels ============================ Unlocked Levels ============================
        if (_gameInfo.unlockedLevels < _gameInfo.noLevel + 1 && _gameInfo.unlockedLevels < _gameInfo.totalLevels)
        {
            _gameInfo.unlockedLevels = _gameInfo.noLevel + 1;
        }

        _playerProgress.unlockedLevels = _gameInfo.unlockedLevels;

        // ========================== Player High Score =========================== Player High Score ===========================
        int playerScore = _scoreVar.playerScore();
        int nbStars;

        if (playerScore <= _levelInfo.firstNbIntervalScore)
            nbStars = 1;
        else if (playerScore < _levelInfo.lastNbIntervalScore)
            nbStars = 2;
        else
            nbStars = 3;

        bool found = false;
        int i = 0;

        while (i < _playerProgress.scoreList.Count && !found)
        {
            if (_playerProgress.scoreList[i].numLevel == _gameInfo.noLevel)
            {
                found = true;
            }
            else
            {
                i++;
            }
        }

        if (found)
        {
            if (_playerProgress.scoreList[i].highScore < playerScore)
                _playerProgress.scoreList[i].highScore = playerScore;

            if (_playerProgress.scoreList[i].nbStars < nbStars)
                _playerProgress.scoreList[i].nbStars = nbStars;
        }
        else
        {
            HighScoreLevel aHighScoreLevel = new HighScoreLevel();
            aHighScoreLevel.numLevel = _gameInfo.noLevel;
            aHighScoreLevel.highScore = playerScore;
            aHighScoreLevel.nbStars = nbStars;

            _playerProgress.scoreList.Add(aHighScoreLevel);  
        }

        // =============================== Save Data ================================= Save Data ================================
        string serializedObject = JsonUtility.ToJson(_playerProgress);
        PlayerPrefs.SetString(_stringKey, serializedObject);
    }

    /// <summary>
    /// Load player data
    /// </summary>
    /// <returns>Player Progress Data</returns>
    public void LoadData()
    {
        string serializedObject = PlayerPrefs.GetString(_stringKey);
        _playerProgress = JsonUtility.FromJson<PlayerProgress>(serializedObject);
    }
}
