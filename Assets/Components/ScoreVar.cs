using UnityEngine;

public class ScoreVar : MonoBehaviour
{
    // ============================= Points =====================================
    private int earthPoint = 7500;
    private int foeHpPoint = 1000;
    private int playerHpPoint = -1000;
    private int fieldRemainPoint = 1500;
    private int foeMissilePoint = 500;
    private int bonusPickedPoint = 1000;
    private int malusPickedPoint = -1000;
    // ======================== Object's numbers ================================
    public int nbLostFoeHp = 0;
    public int nbLostPlayerHp = 0;
    public int nbFieldRemain = 0;
    public int nbDestructFoeMissile = 0;
    public int nbShot = 0;
    public int nbPickedBonus = 0;
    public int nbPickedMalus = 0;

    /// <summary>
    /// Score computation
    /// </summary>
    /// <returns>player' score of this level</returns>
    public int playerScore()
    {
        int foePoint = foeHpPoint * nbLostFoeHp;
        int playerPoint = playerHpPoint * nbLostPlayerHp;
        int fieldPoint = fieldRemainPoint * nbFieldRemain;
        int missilePoint = foeMissilePoint * nbDestructFoeMissile;
        int bonusPoint = bonusPickedPoint * nbPickedBonus;
        int malusPoint = malusPickedPoint * nbPickedMalus;
        float multiShot = 2 - (0.1F * (nbShot - 1));

        int score = Mathf.RoundToInt(((earthPoint + foePoint + playerPoint + fieldPoint + missilePoint) * multiShot) + bonusPoint + malusPoint);

        if (score < 0)
            score = 0;

        return score;
    }
}
