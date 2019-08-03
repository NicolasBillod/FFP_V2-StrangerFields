using UnityEditor;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    [CustomEditor(typeof(Fill))]
    public class UTAFillEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Fill castedTarget = (Fill)target;

            switch (castedTarget.m_FillMethod)
            {
                case UnityEngine.UI.Image.FillMethod.Horizontal:
                    castedTarget.m_FillOrigin = (int)(UnityEngine.UI.Image.OriginHorizontal)EditorGUILayout.EnumPopup("Fill Origin", (UnityEngine.UI.Image.OriginHorizontal)castedTarget.m_FillOrigin);
                    break;

                case UnityEngine.UI.Image.FillMethod.Vertical:
                    castedTarget.m_FillOrigin = (int)(UnityEngine.UI.Image.OriginVertical)EditorGUILayout.EnumPopup("Fill Origin", (UnityEngine.UI.Image.OriginVertical)castedTarget.m_FillOrigin);
                    break;

                case UnityEngine.UI.Image.FillMethod.Radial90:
                    castedTarget.m_FillOrigin = (int)(UnityEngine.UI.Image.Origin90)EditorGUILayout.EnumPopup("Fill Origin", (UnityEngine.UI.Image.Origin90)castedTarget.m_FillOrigin);
                    break;

                case UnityEngine.UI.Image.FillMethod.Radial180:
                    castedTarget.m_FillOrigin = (int)(UnityEngine.UI.Image.Origin180)EditorGUILayout.EnumPopup("Fill Origin", (UnityEngine.UI.Image.Origin180)castedTarget.m_FillOrigin);
                    break;

                case UnityEngine.UI.Image.FillMethod.Radial360:
                    castedTarget.m_FillOrigin = (int)(UnityEngine.UI.Image.Origin360)EditorGUILayout.EnumPopup("Fill Origin", (UnityEngine.UI.Image.Origin360)castedTarget.m_FillOrigin);
                    break;

                default:
                    break;
            }
        }
    }
}