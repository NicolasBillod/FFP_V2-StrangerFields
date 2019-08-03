using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Levels/Create A New Level")]
#endif
public class LevelClassData : ScriptableObject
{
    public int numLevel;
    public Vector3 shipPosition;
    public Vector3 targetPosition;
    public int nbAttractive;
    public int nbRepulsive;
    public List<ForceField> attFFList = new List<ForceField>();
    public List<ForceField> repFFList = new List<ForceField>();
    public List<FoeClass> foeList = new List<FoeClass>();
    public List<BonusMalus> bmList = new List<BonusMalus>();
    public List<Obstacle> obstacleList = new List<Obstacle>();

	public static void NewLevel()
	{
		LevelClassData data = ScriptableObject.CreateInstance<LevelClassData>();
	}

	public static void SaveToFile(LevelClassData data, string path)
	{
		MemoryStream ms = new MemoryStream();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, data);

		string retValue = Convert.ToBase64String (ms.ToArray ()); // A vérifier

		ms.Close ();
		ms.Dispose ();

		File.WriteAllText (path, retValue);
	}

	public static LevelClassData ReadFromFile(string path)
	{
		byte[] data = Convert.FromBase64String(File.ReadAllText (path)); // A vérifier

		MemoryStream ms = new MemoryStream(data);
		BinaryFormatter bf = new BinaryFormatter ();

		LevelClassData result = (LevelClassData)bf.Deserialize (ms);

		ms.Close ();
		ms.Dispose ();

		return result;
	}

    public void Init(int dataNumLevel, Vector3 dataShipPos, Vector3 dataTargetPos, int dataNbAtt, int dataNbRep, List<ForceField> dataFFList, List<FoeClass> dataFoes, 
                      List<BonusMalus> dataBm, List<Obstacle> dataObstacle)
    {
        numLevel = dataNumLevel;
        shipPosition = dataShipPos;
        targetPosition = dataTargetPos;
        nbAttractive = dataNbAtt;
        nbRepulsive = dataNbRep;
        
        foreach (ForceField aFF in dataFFList)
        {
            if (aFF.isRepulsive)
            {
                repFFList.Add(aFF);
            }
            else
            {
                attFFList.Add(aFF);
            }
        }

        foeList = dataFoes;
        bmList = dataBm;
        obstacleList = dataObstacle;
    }

    public static LevelClassData CreateInstance(int theNumLevel, Vector3 theShipPos, Vector3 theTargetPos, int theNbAtt, int theNbRep, List<ForceField> theFFList, List<FoeClass> theFoesList,
                                                List<BonusMalus> theBmList, List<Obstacle> theObstacleList)
    {
        LevelClassData levelData = ScriptableObject.CreateInstance<LevelClassData>();
        levelData.Init(theNumLevel, theShipPos, theTargetPos, theNbAtt, theNbRep, theFFList, theFoesList, theBmList, theObstacleList);
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(levelData, string.Concat("Assets/LevelData/Level", theNumLevel, ".asset"));
#endif
  //      Debug.Log(AssetDatabase.GetAssetPath(levelData));
        return levelData;
    }
}
