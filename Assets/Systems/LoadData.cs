using FYFY;
using UnityEngine;
using System.Collections.Generic;

public class LoadData : FSystem
{
    // ======================================================================================================================================================================
    // ================================= VARIABLES ================================= VARIABLES ================================== VARIABLES =================================
    // ======================================================================================================================================================================

    // ================================== Families ================================== Families ================================== Families ==================================
    private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)));
    private Family _scoreFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ScoreVar)));
    private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
    private Family _playerDataFamily = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerData)));

    // =============================== Components ================================= Components ================================= Components =================================
    private GameInformations _gameInfo;
    private PlayerProgress _playerProgress;
    private PlayerData _playerData;
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
    public LoadData()
    {
        /*#if UNITY_EDITOR
        PlayerPrefs.DeleteKey(_stringKey);
        #endif*/

        _isSaved = false;
        _playerProgress = new PlayerProgress();
        _playerData = _playerDataFamily.First().GetComponent<PlayerData>();
        _gameInfo = _gameInfoFamily.First().GetComponent<GameInformations>();

        if (PlayerPrefs.HasKey(_stringKey))
        {
            LoadScore();
        }
        else
        {
            string serializedObject = JsonUtility.ToJson(_playerProgress);
            PlayerPrefs.SetString(_stringKey, serializedObject);
        }
    }

    // ================================ Manage Data ================================ Manage Data ================================ Manage Data ================================

    /// <summary>
    /// Load player data
    /// </summary>
    /// <returns>Player Progress Data</returns>
    public void LoadScore()
    {
        string serializedObject = PlayerPrefs.GetString(_stringKey);
        _playerProgress = JsonUtility.FromJson<PlayerProgress>(serializedObject);
        _gameInfo.unlockedLevels = _playerProgress.unlockedLevels;
        _playerData.unlockedLevels = _playerProgress.unlockedLevels;
        _playerData.scoreList = _playerProgress.scoreList;
    }
}
