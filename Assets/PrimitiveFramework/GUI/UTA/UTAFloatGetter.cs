using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class UTAFloatGetter : MonoBehaviour
    {
        [SerializeField]
        private float m_FloatParameter = 1;

        public void SetFloat(float f)
        {
            m_FloatParameter = f;
        }

        internal float GetFloat()
        {
            return m_FloatParameter;
        }
    }
}