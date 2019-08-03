using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections;
using PrimitiveFactory.Framework.UITimelineAnimation;


/**************************************************
 * 
 * System : LoadDialog
 * Load dialogs of a level from a Scriptable Object
 * 
 *************************************************/

public class LoadDialog : UtilitySystem
{
    // ======================================================================================================================================================================
    // ================================= VARIABLES ================================= VARIABLES ================================== VARIABLES =================================
    // ======================================================================================================================================================================

    // ================================== Families ================================== Families ================================== Families ==================================
    private Family _GameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations)));
    private Family _mainLoopFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));
    private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GlobalUI), typeof(OtherPanel), typeof(LoadingUI)));
    private Family _levelInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations), typeof(ScoreVar)));
    private Family _dialogCanStartFamily = FamilyManager.getFamily(new AllOfComponents(typeof(DialogCanStart)));
	private Family _soundFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Sound)));

    // =============================== Components ================================= Components ================================= Components =================================
    private int _numLevel;
    private DialogCanStart _dialogCanStart;
    private OtherPanel _otherPanel;
    private GlobalUI _globalUI;
	private Sound _sound;

    // =========================== Scriptable Object =========================== Scriptable Object =========================== Scriptable Object ============================
    private string dataBeginPath = "DialogData/DialogBeginLevel";
    private string dataEndPath = "DialogData/DialogEndLevel";
    private string charaPath = "DialogData/Speakers/";
    DialogLevelData dialogLevel;

    // ================================= Coroutine ================================= Coroutine ================================= Coroutine ==================================
    private MainLoop _mainLoop;
    public float letterPaused = 0.02f;
    private IEnumerator _coroutine;

    // ==================================== Other ===================================== Other ==================================== Other ====================================
    private bool dialogStarted = false; //Turn to a callback?
    private int indexDialogCpt = 0;
    private bool prepareEndDialog = false;

    // ==================================== Consts ===================================== Consts ==================================== Consts =================================
    private const float TIME_BETWEEN_LETTERS = 1f / 60;

    // ======================================================================================================================================================================
    // ================================== METHODS =================================== METHODS ==================================== METHODS ==================================
    // ======================================================================================================================================================================

    // ================================= Lifecycle ================================= Lifecycle ================================= Lifecycle ==================================

    /// <summary>
    /// Constructor -- like Unity Start()
    /// </summary>
    public LoadDialog()
    {
        _numLevel = _GameInfo.First().GetComponent<GameInformations>().noLevel;
        _mainLoop = _mainLoopFamily.First().GetComponent<MainLoop>();
        _otherPanel = _interfaceFamily.First().GetComponent<OtherPanel>();
        _globalUI = _interfaceFamily.First().GetComponent<GlobalUI>();
        _dialogCanStart = _dialogCanStartFamily.First().GetComponent<DialogCanStart>();
		_sound = _soundFamily.First ().GetComponent<Sound> ();
        InitUI();
		if(gameInfoFamily.First().GetComponent<GameInformations>().playMusics)
			_sound.musicGameplay.start ();
    }

    /// <summary>
    /// Use to process your families
    /// </summary>
    /// <param name="familiesUpdateCount"></param>
    protected override void onProcess(int familiesUpdateCount)
    {
        if (_stateFamily.First().GetComponent<State>().state == State.STATES.DIALOG && !dialogStarted && _dialogCanStart.dialogCanStart == true)
        {
            dialogStarted = true;
            _dialogCanStart.dialogCanStart = false;
            LoadNextDialog(indexDialogCpt, false);
            _otherPanel.nextDialogButton.interactable = false;
        }

        if (_stateFamily.First().GetComponent<State>().state == State.STATES.DIALOG && dialogStarted && !_otherPanel.nextDialogButton.interactable && GetMouseOrTouchDown())
        {
            _mainLoop.StopCoroutine(_coroutine);
            _otherPanel.dialogText.text = dialogLevel.dialogs[indexDialogCpt - 1].dialogText;
            _otherPanel.nextDialogButton.interactable = true;
        }

        if (_stateFamily.First().GetComponent<State>().state == State.STATES.WON && !prepareEndDialog)
        {
            prepareEndDialog = true;
            dialogStarted = false;
            indexDialogCpt = 0;
            LoadDialogData(_numLevel, false);
            UTAController.Instance.PlayAnimation("A_TopInGame_Disparition");
            if (_numLevel != 1)
                UTAController.Instance.PlayAnimation("A_InGame_Autodestruction_Disparition");
            UTAController.Instance.PlayAnimation("A_DialogPanel_Apparition");
        }

        if (_stateFamily.First().GetComponent<State>().state == State.STATES.WON && !dialogStarted && _dialogCanStart.dialogCanStart == true)
        {
            dialogStarted = true;
            _dialogCanStart.dialogCanStart = false;
            LoadNextDialog(indexDialogCpt, false);
            _otherPanel.nextDialogButton.interactable = false;
        }

        if (_stateFamily.First().GetComponent<State>().state == State.STATES.WON && prepareEndDialog && !_otherPanel.nextDialogButton.interactable && GetMouseOrTouchDown())
        {
            _mainLoop.StopCoroutine(_coroutine);
            _otherPanel.dialogText.text = dialogLevel.dialogs[indexDialogCpt - 1].dialogText;
            _otherPanel.nextDialogButton.interactable = true;
        }
    }

    // ================================== Dialogs =================================== Dialogs =================================== Dialogs ===================================

    /// <summary>
    /// Coroutine to display lette one by one
    /// </summary>
    /// <param name="dialog"></param>
    /// <returns></returns>
    private IEnumerator AnimText(string dialog)
    {
        _otherPanel.nextDialogButton.interactable = false;

        //Split each char into a char array
        char[] dialogArray = dialog.ToCharArray();
        int dialogLength = dialogArray.Length;

        for (int i = 0; i < dialogLength;)
        {
            int letterCount = Mathf.CeilToInt(Time.deltaTime / TIME_BETWEEN_LETTERS);
            for (int j = 0; j < letterCount && (i + j) < dialogLength; j++)
            {
                _otherPanel.dialogText.text += dialogArray[i + j];
            }

            i += letterCount;
			//_otherPanel.nextDialogButton.GetComponent<Son> ().dialogLetter.stop (FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			//_otherPanel.nextDialogButton.GetComponent<Son> ().dialogLetter.start ();
			if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
				FMODUnity.RuntimeManager.PlayOneShot(_sound.DialogLetterEvent);
            yield return 0;
        }
			
		_otherPanel.nextDialogButton.interactable = true;
    }

    /// <summary>
    /// Recovery UI elements for dialog
    /// </summary>
    private void InitUI()
    {
        _otherPanel.nextDialogButton.onClick.AddListener(() => LoadNextDialog(indexDialogCpt, true));
		_otherPanel.nextDialogButton.onClick.AddListener (() => SoundNextDialog ());
        LoadDialogData(_numLevel, true);
    }

	private void SoundNextDialog(){
		//_otherPanel.nextDialogButton.GetComponent<Son> ().dialogNext = FMODUnity.RuntimeManager.CreateInstance (_otherPanel.nextDialogButton.GetComponent<Son> ().DialogNextEvent);
		//_otherPanel.nextDialogButton.GetComponent<Son> ().dialogNext.start ();
		if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
			FMODUnity.RuntimeManager.PlayOneShot(_sound.DialogNextEvent);
	}

    /// <summary>
    /// Load dialog data from scriptable object in terms of level numero
    /// </summary>
    /// <param name="numLevel"></param>
    private void LoadDialogData(int numLevel, bool beginning)
    {
        if (beginning)
        {
            dialogLevel = (DialogLevelData)Resources.Load(string.Concat(dataBeginPath, numLevel), typeof(DialogLevelData));
            UTAController.Instance.PlayAnimation("A_TopInGame_Apparition");
            UTAController.Instance.PlayAnimation("A_DialogPanel_Apparition");
        }
        else
        {
            dialogLevel = (DialogLevelData)Resources.Load(string.Concat(dataEndPath, numLevel), typeof(DialogLevelData));
        }

        PrepareNextDialog(indexDialogCpt);
    }

    /// <summary>
    /// Prepare the box dialog for the next dialog
    /// Load the next dialog from the Scriptable Object
    /// </summary>
    /// <param name="indexDialog"></param>
    private void PrepareNextDialog(int indexDialog)
    {
        Dialog theDialog = dialogLevel.dialogs[indexDialog];
        Speaker theSpeaker = (Speaker)Resources.Load(string.Concat(charaPath, theDialog.speakerName), typeof(Speaker));
        _otherPanel.dialogText.text = string.Empty;
        _otherPanel.charaAvatar.overrideSprite = theSpeaker.avatarSpeaker;
        _otherPanel.charaName.text = theSpeaker.nameSpeaker;
    }

    /// <summary>
    /// Write the dialog and manage end's animations
    /// </summary>
    /// <param name="indexDialog"></param>
    private void LoadNextDialog(int indexDialog, bool prepareDialog)
    {
        int numberDialogs = dialogLevel.dialogs.Count;

        if (indexDialog < numberDialogs)
        {
            if (indexDialog > 0)
                _mainLoop.StopCoroutine(_coroutine);

            if (prepareDialog)
                PrepareNextDialog(indexDialog);

            Dialog theDialog = dialogLevel.dialogs[indexDialog];
            _coroutine = AnimText(theDialog.dialogText);
            _mainLoop.StartCoroutine(_coroutine);
            indexDialogCpt++;
        }
        else
        {
			
            if (_stateFamily.First().GetComponent<State>().state == State.STATES.DIALOG)
            {
                indexDialogCpt = 0;
                UTAController.Instance.PlayAnimation("A_DialogPanel_DisparitionBegin");
            }

            else if (_stateFamily.First().GetComponent<State>().state == State.STATES.WON)
            {
                UTAController.Instance.PlayAnimation("A_DialogPanel_DisparitionEnd");
                GameObjectManager.addComponent<ShowEndPanel>(_levelInfoFamily.First(), new { wonPanel = true });
            }
        }
    }
}
