using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using FYFY;

public class CreateLevel : FSystem
{
	#if UNITY_EDITOR
    // ----------------- VARIABLES -----------------

    #region Families
    private Family _FamilyPlacedFF = FamilyManager.getFamily(new AllOfComponents(typeof(Field), typeof(Dimensions), typeof(Position), typeof(TerrainDisplay)), new NoneOfComponents(typeof(Editable)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
    private Family _FamilyShip = FamilyManager.getFamily(new AllOfComponents(typeof(Editable), typeof(ShipInfo), typeof(Position), typeof(Mass)));
    private Family _FamilyTarget = FamilyManager.getFamily(new AllOfComponents(typeof(FinishInformation), typeof(Position)));
    private Family _FamilyFoe = FamilyManager.getFamily(new AllOfComponents(typeof(Mass), typeof(Position), typeof(ShipInfo), typeof(Enemy)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
    private Family _FamilyBonus = FamilyManager.getFamily(new AllOfComponents(typeof(Bonus), typeof(Position)));
    private Family _FamilyObstacle = FamilyManager.getFamily(new AllOfComponents(typeof(ObstacleInformation), typeof(Position)));
    private Family _FamilyBreakableObstacle = FamilyManager.getFamily(new AllOfComponents(typeof(BreakableObstacle), typeof(Position)));
    private Family _FamilyTerrain = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainUILD), typeof(OpenedMenuPanel), typeof(AddMenuPanel), typeof(GamePanelLD), typeof(CameraGameObject)));
    #endregion

    // ----------------- CONSTRUCTOR / FYFY PROCEDURES -----------------
    OpenedMenuPanel _openedMenu;

    public CreateLevel()
    {
        _openedMenu = _interfaceFamily.First().GetComponent<OpenedMenuPanel>();
        _openedMenu.saveButton.onClick.AddListener(() => SaveLevelData());
    }

    // ----------------- FONCTIONS / PROCEDURES -----------------

    private void SaveLevelData()
    {
        int numLevel = int.Parse(_openedMenu.inputLevel.text);
        Vector3 shipPosition = _FamilyShip.First().transform.position;
        Vector3 targetPosition = _FamilyTarget.First().transform.position;
        int nbAttractive = int.Parse(_openedMenu.inputAttractive.text);
        int nbRepulsive = int.Parse(_openedMenu.inputRepulsive.text);
        List<ForceField> FFList = new List<ForceField>();
        List<FoeClass> foesList = new List<FoeClass>();
        List<BonusMalus> bmList = new List<BonusMalus>();
        List<Obstacle> obstacleList = new List<Obstacle>();

        foreach (GameObject aFF in _FamilyPlacedFF)
        {
            Vector3 ffPosition = aFF.transform.position;
            float ffIntensity = aFF.GetComponent<Field>().A;
            bool ffIsRepulsive = aFF.GetComponent<Field>().isRepulsive;

            ForceField theFF = new ForceField(ffPosition, ffIntensity, ffIsRepulsive);
            FFList.Add(theFF);
        }

        foreach(GameObject aFoe in _FamilyFoe)
        {
            ShipInfo theShipInfo = aFoe.GetComponent<ShipInfo>();
            FoeClass theFoe = new FoeClass(aFoe.transform.position, aFoe.transform.rotation, theShipInfo.fireIntensity, theShipInfo.angle);
            foesList.Add(theFoe);
        }

        foreach (GameObject aBonus in _FamilyBonus)
        {
            Bonus bonusItem = aBonus.GetComponent<Bonus>();
            BonusMalus theBm = null;

            switch (bonusItem.type)
            {
                case Bonus.TYPE.B_Damage:
                    theBm = new BonusMalus(BonusMalus.TYPE.B_DAMAGE, aBonus.transform.position);
                    break;
                case Bonus.TYPE.B_Player:
                    theBm = new BonusMalus(BonusMalus.TYPE.B_PLAYER, aBonus.transform.position);
                    break;
                case Bonus.TYPE.M_Earth:
                    theBm = new BonusMalus(BonusMalus.TYPE.M_EARTH, aBonus.transform.position);
                    break;
                case Bonus.TYPE.M_FoeLife:
                    theBm = new BonusMalus(BonusMalus.TYPE.M_FOELIFE, aBonus.transform.position);
                    break;
            }

            bmList.Add(theBm);
        }

        foreach (GameObject aObstacle in _FamilyObstacle)
        {
            Obstacle theObstacle = new Obstacle(aObstacle.transform.position, aObstacle.transform.rotation, false);
            obstacleList.Add(theObstacle);
        }

        foreach (GameObject aBreakableObstacle in _FamilyBreakableObstacle)
        {
            Obstacle theObstacle = new Obstacle(aBreakableObstacle.transform.position, aBreakableObstacle.transform.rotation, true);
            obstacleList.Add(theObstacle);
        }

        LevelClassData levelData = LevelClassData.CreateInstance(numLevel, shipPosition, targetPosition, nbAttractive, nbRepulsive, FFList, foesList, bmList, obstacleList);
    }
	#endif
}