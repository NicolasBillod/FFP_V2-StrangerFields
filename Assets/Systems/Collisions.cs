using UnityEngine;
using FYFY;
using FYFY_plugins.CollisionManager;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;
using System.Collections;

public class Collisions : UtilitySystem
{
	// ==== VARIABLES ====

	private Family _missileInCollision = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass), typeof(InCollision3D)));
	private Family _levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations), typeof(ScoreVar)));
	private Family _enemyShipFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Enemy), typeof(ShipInfo), typeof(Position), typeof(Mass)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _shipFamily = FamilyManager.getFamily (new AllOfComponents (typeof(ShipInfo), typeof(Position), typeof(Mass), typeof(Editable)));
	private Family _missileInTriggerFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass), typeof(Triggered3D)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _stateFamily = FamilyManager.getFamily (new AllOfComponents (typeof(State)));
	private Family _kamikazeFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ExplosionMissile), typeof(Kamikaze)));
	private Family _finishFamily = FamilyManager.getFamily (new AllOfComponents (typeof(FinishInformation), typeof(EarthHealthBars)));
    private Family _foeHpFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FoeHealthBar)));

	private Family _missileFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Dimensions), typeof(Movement), typeof(Position), typeof(Mass)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _poolMissileImpactFamily = FamilyManager.getFamily (new AllOfComponents(typeof(PoolMissileImpacts)));

	private Family _mainLoopFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));
	private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GlobalUI), typeof(OtherPanel), typeof(LoadingUI)));
	private Family _soundFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Sound)));

	private float timeSpentInSpace;
	private PoolMissileImpacts _poolMImpacts;
	private MainLoop _mainLoop;
    private ScoreVar _scoreVar;
	private Sound _sound;

    // ==== LIFECYCLE ====
    public Collisions()
    {
		_poolMImpacts = _poolMissileImpactFamily.First ().GetComponent<PoolMissileImpacts> ();
		_missileInTriggerFamily.addEntryCallback (OnShipTriggerEntered);
		_missileInCollision.addEntryCallback (OnMissileCollisionEntered);
		_kamikazeFamily.addEntryCallback (JustBlowUp);
		_mainLoop = _mainLoopFamily.First ().GetComponent<MainLoop> ();
        _scoreVar = _levelInfoFamily.First().GetComponent<ScoreVar>();
		//_enemyShipFamily.addExitCallback (OnEnemyShipExited);
		_sound = _soundFamily.First ().GetComponent<Sound> ();

	}

	protected override void onPause(int currentFrame)
    {
	}

	protected override void onResume(int currentFrame)
    {
	}

	protected override void onProcess(int familiesUpdateCount)
    {
		if (_stateFamily.First ().GetComponent<State> ().state == State.STATES.PLAYING) {
			timeSpentInSpace += Time.deltaTime;
			if (timeSpentInSpace > 10) {
				foreach (GameObject go in _missileFamily) {
					JustBlowUp (go);
				}
			}
		}

		if (_stateFamily.First ().GetComponent<State> ().state == State.STATES.SETUP) {
			timeSpentInSpace = 0;
		}
	}

	// ==== METHODS ====

	/*
	protected float calculateScore()
    {
		LevelInformations infos = GameObject.Find ("LevelInformations").GetComponent<LevelInformations> ();

		// Bonus
		int N_BONUS = infos.NB_BONUS;
		int collectedBonus = infos.collectedBonus;
		float bonusScore = (N_BONUS > 0) ? collectedBonus * 1.0f / N_BONUS : -1; 

		// Time
		float EXPERT_TIME = infos.EXPERT_TIME;
		float travelTime = infos.travelTime;
		float timeScore = EXPERT_TIME/(travelTime > 0 ? travelTime : 0.01f);

		float score = (timeScore + bonusScore) / 2.0f;
		if (bonusScore == -1) {
			score = timeScore;
		}

		return score;
	}
	*/

	public void OnShipTriggerEntered(GameObject ship)
    {
		Triggered3D triggers = ship.GetComponent<Triggered3D> ();
		Target tar = ship.GetComponent<Target> ();

		tar.target = triggers.Targets [0].GetComponentInParent<Position>().pos; // Only the first trigger matters
		//tar.target = triggers.Targets [0].transform.position;
	}

	public void OnMissileCollisionEntered(GameObject missile)
    {
		InCollision3D collisions = missile.GetComponent<InCollision3D> ();
		GameObject target = collisions.Targets [0];

		// What did you just hit?
		switch (target.tag)
        {
			case "ShipTag":
			case "EnemyShip":
				if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
					FMODUnity.RuntimeManager.PlayOneShot (_sound.MissileCollisionShipEvent);

				ShipInfo targetInfo = target.GetComponent<ShipInfo> ();
				targetInfo.health -= missile.GetComponent<ExplosionMissile> ().damage;
				
                if (target.CompareTag("ShipTag"))
                    _scoreVar.nbLostPlayerHp++;
                else
                    _scoreVar.nbLostFoeHp++;

                foreach (GameObject aFoe in _foeHpFamily)
                {
                    if (aFoe == target)
                    {
                        FoeHealthBar foeHp = aFoe.GetComponent<FoeHealthBar>();
                        foeHp.FoeHPsWorld[targetInfo.health].color = new Color32(0x37, 0x37, 0x37, 0xFF);                   
                    }
                }

		        // target is dead
		        if (targetInfo.health <= 0)
                    {
			        if (target.activeSelf && target != null)
                    {
						GameObject explosion = DestructionEntity(target);
						/*
						if (_shipFamily.contains (target.GetInstanceID()))
	                        {
						        Debug.Log ("LOST");
						        _stateFamily.First().GetComponent<State>().state = State.STATES.LOST;
							_mainLoop.StartCoroutine(WaitForExplosion(explosion));
								//GameObjectManager.addComponent<ShowEndPanel> (_levelInfoFamily.First (), new {wonPanel = false});
					    }*/
			        }
			        //GameObject.Destroy (target);
		        }

		        JustBlowUp (missile);
                break;


			case "Earth":
				if (gameInfoFamily.First ().GetComponent<GameInformations> ().playSounds)
					FMODUnity.RuntimeManager.PlayOneShot (_sound.MissileCollisionShipEvent);
				Effects effects = target.GetComponent<Effects> ();
				if (!effects.current.Contains (Bonus.TYPE.M_Earth))
                {
					FinishInformation finishInfo = target.GetComponent<FinishInformation> ();
					finishInfo.health -= missile.GetComponent<ExplosionMissile> ().damage;
					EarthHealthBars earthHp = target.GetComponent<EarthHealthBars> ();
					earthHp.EarthHpsWorld [finishInfo.health].color = new Color32 (0x37, 0x37, 0x37, 0xFF);

					// target is dead
					if (finishInfo.health <= 0)
                    {
						if (target.activeSelf && target != null)
                        {
							//_mainLoop.StartCoroutine (Explosion (target));
							GameObject explosion = DestructionEntity(target);
							//Debug.Log ("WIN");
							//_stateFamily.First ().GetComponent<State> ().state = State.STATES.WON;
							//_mainLoop.StartCoroutine (WaitForExplosion (explosion));

							//GameObjectManager.addComponent<ShowEndPanel> (_levelInfoFamily.First (), new {wonPanel = true});
						}
					}
				}
			    JustBlowUp (missile);
			    break;

		    case "bonus":
			    Bonus b = target.GetComponent<Bonus> ();
			    if (!b.isCollected)
                {
				    GameObjectManager.removeComponent<SphereCollider> (target);
				    //GameObject.Destroy(target.GetComponent<CollisionSensitive3DTarget>());
				    GameObjectManager.setGameObjectState (target, false);

				    switch (b.type)
                    {
				        case Bonus.TYPE.B_Damage:
					        //missile.GetComponent<ExplosionMissile> ().damage += 10;
						    _shipFamily.First().GetComponent<Effects>().next.Add(Bonus.TYPE.B_Player);
                            _scoreVar.nbPickedBonus++;
						    break;
				        case Bonus.TYPE.B_Player:
                            //_shipFamily.First ().GetComponent<ShipInfo> ().health += 10; //Rendre joueur invincible pour un tour
                            _scoreVar.nbPickedBonus++;
                            break;
				        case Bonus.TYPE.M_Earth:
					        //_finishFamily.First ().GetComponent<FinishInformation> ().health += 10;
						    _finishFamily.First().GetComponent<Effects>().next.Add(Bonus.TYPE.M_Earth);
                            _scoreVar.nbPickedMalus++;
						    break;
				        case Bonus.TYPE.M_FoeLife:
					        foreach (GameObject aFoe in _enemyShipFamily)
                            {
                                aFoe.GetComponent<ShipInfo>().health += 2;

                                if (aFoe.GetComponent<ShipInfo>().health > 6)
                                    aFoe.GetComponent<ShipInfo>().health = 6;
                            }
                            _scoreVar.nbPickedMalus++;
                            break;
				        default:
					        Debug.Log ("What kind of bonus is this?");
					        break;
				    }
				    b.isCollected=true;
			    }
			    break;

			case "obstacle":
				if (target.GetComponent<BreakableObstacle> ()) {
					if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
						FMODUnity.RuntimeManager.PlayOneShot (_sound.MissileCollisionBreakObstEvent);
					GameObjectManager.setGameObjectState (target, false);
				}
				else {
					if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
						FMODUnity.RuntimeManager.PlayOneShot (_sound.MissileCollisionObstacleEvent);
				}
			    JustBlowUp (missile);
			    break;

            case "FoeMissile":
                _scoreVar.nbDestructFoeMissile++;
                JustBlowUp(missile);
                break;
		    default:
			    JustBlowUp (missile);
			    break;
		    }

		    /*
		    // Destroy the missile no matter what we hit
		    GameObjectManager.unbind (ship);
		    GameObject.Destroy (ship);
		    */
	}

	public void JustBlowUp(GameObject missile)
    {
		//GameObject explosion = GameObject.Instantiate (missile.GetComponent<ExplosionMissile>().explosionPrefab);
		//explosion.transform.position = missile.transform.position;
		//explosion.GetComponent<ParticleSystem> ().Play ();
		//GameObject.Destroy (explosion, explosion.GetComponent<ParticleSystem> ().main.duration);
		_mainLoop.StartCoroutine(Explosion (missile));
		//GameObjectManager.unbind (missile);
		//GameObject.Destroy (missile);
		if (missile.GetComponent<Kamikaze> ()) {
			Kamikaze[] allKamikazes = missile.GetComponents<Kamikaze> ();
			foreach (Kamikaze k in allKamikazes) {
				GameObjectManager.removeComponent<Kamikaze> (missile);
			}
		}
		missile.GetComponent<Target> ().target = Vector3.zero;
		ReturnGOToPool(missile.transform.parent.gameObject, missile);
		GameObjectManager.setGameObjectState (missile, false);
	}

	public GameObject DestructionEntity(GameObject theEntity)
    {
		//GameObject explosion = GameObject.Instantiate(theEntity.GetComponent<ExplosionMissile>().explosionPrefab);
		//explosion.transform.position = theEntity.transform.position;
		//explosion.GetComponent<ParticleSystem>().Play();
		_mainLoop.StartCoroutine (Explosion (theEntity));
		GameObjectManager.setGameObjectState(theEntity, false);
		//GameObject.Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.duration);

		return null;
    }

	/// <summary>
	/// Deprecated
	/// </summary>
	/// <param name="id">Identifier.</param>
	/*public void OnEnemyShipExited(int id)
    {
		if (_enemyShipFamily.Count == 0 && _stateFamily.First().GetComponent<State>().state == State.STATES.PLAYING) {
			Debug.Log ("WIN"); //TODO: things to do when won
			_stateFamily.First().GetComponent<State>().state = State.STATES.WON;
			GameObjectManager.addComponent<ShowEndPanel> (_levelInfoFamily.First (), new {wonPanel = true});
		}
	}*/


	// IMPACTS
	private IEnumerator Explosion(GameObject generator){
		//GameObject goPool = generator.GetComponent<ExplosionMissile> ().explosionPrefab;
		GameObject goPool = generator.GetComponent<ExplosionMissile>().goWithPool;
		if (_missileFamily.contains (generator.GetInstanceID ())) {
			//goPool = GameObject.Find ("Impacts");
			//goPool = generator.GetComponent<ExplosionMissile>().goWithPool;
		} else if (_shipFamily.contains (generator.GetInstanceID ())) {
			Debug.Log ("LOST");
			_interfaceFamily.First ().GetComponent<GlobalUI> ().autodestructionButton.onClick.Invoke ();
			_stateFamily.First ().GetComponent<State> ().state = State.STATES.LOST;
			if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
				FMODUnity.RuntimeManager.PlayOneShot (_sound.PlayerDestroyedEvent);
			//goPool = GameObject.Find ("ShipExplo");
		} else if (_finishFamily.contains (generator.GetInstanceID ())) {
			Debug.Log ("WIN");
			_interfaceFamily.First ().GetComponent<GlobalUI> ().autodestructionButton.onClick.Invoke ();
			_stateFamily.First ().GetComponent<State> ().state = State.STATES.WON;
			//goPool = GameObject.Find ("EarthExplo");
		} else if (_enemyShipFamily.contains (generator.GetInstanceID ())) {
			if(gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
				FMODUnity.RuntimeManager.PlayOneShot (_sound.EnemyDestroyedEvent);
		}

		GameObject explosion = GetGOFromPool (goPool);
		explosion.transform.position = generator.transform.position;
		explosion.SetActive (true);


		yield return new WaitForSeconds (explosion.GetComponent<ParticleSystem> ().main.duration);
		explosion.SetActive (false);
		ReturnGOToPool (goPool, explosion);

		// If it's the ship, we lost
		if (_shipFamily.contains (generator.GetInstanceID ()))
        {
			GameObjectManager.addComponent<ShowEndPanel> (_levelInfoFamily.First (), new {wonPanel = false});
		}
        /* else if (_finishFamily.contains (generator.GetInstanceID ())) 
         {
			GameObjectManager.addComponent<ShowEndPanel> (_levelInfoFamily.First (), new {wonPanel = true});		
		}*/
	}
}