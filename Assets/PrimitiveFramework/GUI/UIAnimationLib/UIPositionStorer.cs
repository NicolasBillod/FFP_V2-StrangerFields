using UnityEngine;

namespace PrimitiveFactory.Framework.GUITools
{
    public class UIPositionStorer : MonoBehaviour
    {
        private ScreenOrientation m_Setup = ScreenOrientation.Unknown;
        private Vector2Int m_SetupRatio;
        
        private Vector3 _StoredPosition;
        public Vector3 StoredPosition
        {
            get { return _StoredPosition; }
            set
            {
                _StoredPosition = value;
                m_Setup = Screen.orientation;
                m_SetupRatio = new Vector2Int(Screen.width, Screen.height);
            }
        }

        public void UpdateToRatio()
        {
            Vector2 ratioOfRatios = new Vector2((float)Screen.width / m_SetupRatio.x, (float)Screen.height / m_SetupRatio.y);
            _StoredPosition = new Vector3(_StoredPosition.x * ratioOfRatios.x, _StoredPosition.y * ratioOfRatios.y, _StoredPosition.z);
            m_SetupRatio = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
