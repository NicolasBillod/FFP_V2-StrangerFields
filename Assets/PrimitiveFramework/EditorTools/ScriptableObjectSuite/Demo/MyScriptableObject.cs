using PrimitiveFactory.Framework.EditorTools;
using UnityEngine;

public class MyScriptableObject : ScriptableObjectExtended
{
    public string StringField;
    public int IntField;
    public GameObject ObjectField;
    public Sprite SpriteField;
    public AudioClip SoundField;
    public Color ColorField;

    // Character game data
    [System.NonSerialized]
    public GameObject CharacterPrefab;
    [SerializeField]
    [LoadOnDemand("CharacterPrefab")]
    private string m_CharacterPrefabPath;
}
