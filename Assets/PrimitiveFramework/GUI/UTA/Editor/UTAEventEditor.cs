using UnityEditor;
using UnityEditor.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    [CustomEditor(typeof(UTAEvent))]
    public class UTAEventEditor : Editor
    {
        private string m_LastKey;
        private SerializedProperty m_LastProperty;
        private SerializedObject m_DirectorObject;

        public override void OnInspectorGUI()
        {
            UTAEvent castedTarget = (UTAEvent)target;

            // Get director
            if (m_DirectorObject == null)
            {
                UTADirector director = TimelineEditor.playableDirector.GetComponent<UTADirector>();
                m_DirectorObject = new SerializedObject(director);
            }
            m_DirectorObject.Update();

            // Get property
            if (m_LastKey != castedTarget.m_EventUUID)
            {
                m_LastKey = castedTarget.m_EventUUID;
                m_LastProperty = GetEventProperty(castedTarget.m_EventUUID);
            }

            // Display property
            if (m_LastProperty != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_LastProperty, true);
                m_LastProperty.serializedObject.ApplyModifiedProperties();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty GetEventProperty(string key)
        {
            SerializedProperty keysProperty = m_DirectorObject.FindProperty("m_EventDictionary").FindPropertyRelative("keys");
            SerializedProperty valuesProperty = m_DirectorObject.FindProperty("m_EventDictionary").FindPropertyRelative("values");

            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                if (keysProperty.GetArrayElementAtIndex(i).stringValue == key)
                {
                    return valuesProperty.GetArrayElementAtIndex(i);
                }
            }

            throw new System.Exception(string.Concat("unknown event with uuid ", key));
        }
    }
}