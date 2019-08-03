using PrimitiveFactory.Framework.GUITools;
using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class FromToTranslationBehaviour : UTACurvePlayableBehaviour
    {
        public FromToTranslation.AnimationType m_AnimationType;
        public RectTransform m_TargetTransform;

        private Vector2 m_InitialPosition;
        private Vector2 m_TargetPosition;
        private bool m_Reverse;

        internal override void PrepareAnimation()
        {
            switch (m_AnimationType)
            {
                case FromToTranslation.AnimationType.MoveTo:
                    m_Reverse = false;
                    break;
                case FromToTranslation.AnimationType.MoveFrom:
                    m_Reverse = true;
                    break;
                default:
                    break;
            }
        }

        protected override void Start()
        {
            UIPositionStorer foregroundStorer = AddPositionStorer(m_AffectedTransform.gameObject); 
            m_TargetPosition = m_TargetTransform.position;
            m_InitialPosition = foregroundStorer.StoredPosition;

            m_AffectedTransform.gameObject.SetActive(true);
        }

        internal override void Progress(float progress)
        {
            if (m_Reverse)
                progress = 1 - progress;

            Vector3 displacement = m_InitialPosition + (m_TargetPosition - m_InitialPosition) * progress - (Vector2)m_AffectedTransform.position;

            m_AffectedTransform.position += displacement;
        }
    }
}