using UnityEngine;

public class State : MonoBehaviour {


	public enum STATES {ANIM1, ANIM2, DIALOG, SETUP, PLAYING, PAUSED, WON, LOST};
	public STATES state = STATES.ANIM1;

    [EnumAction(typeof(STATES))]
    public void SetState(int state)
    {
        this.state = (STATES)state;
    }
}