using PrimitiveFactory.Framework.GUITools;
using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class OutOfScreenTranslationBehaviour : UTACurvePlayableBehaviour
    {
        public OutOfScreenTranslation.AnimationType m_AnimationType;

        private Vector2 m_InitialPosition;
        private Vector2 m_TargetPosition;
        private Vector2 m_Direction;
        private bool m_Reverse;

        internal override void PrepareAnimation()
        {
            switch (m_AnimationType)
            {
                case OutOfScreenTranslation.AnimationType.FromRight:
                    m_Direction = new Vector2(-1, 0);
                    m_Reverse = false;
                    break;
                case OutOfScreenTranslation.AnimationType.FromLeft:
                    m_Direction = new Vector2(1, 0);
                    m_Reverse = false;
                    break;
                case OutOfScreenTranslation.AnimationType.FromTop:
                    m_Direction = new Vector2(0, -1);
                    m_Reverse = false;
                    break;
                case OutOfScreenTranslation.AnimationType.FromBottom:
                    m_Direction = new Vector2(0, 1);
                    m_Reverse = false;
                    break;
                case OutOfScreenTranslation.AnimationType.ToRight:
                    m_Direction = new Vector2(-1, 0);
                    m_Reverse = true;
                    break;
                case OutOfScreenTranslation.AnimationType.ToLeft:
                    m_Direction = new Vector2(1, 0);
                    m_Reverse = true;
                    break;
                case OutOfScreenTranslation.AnimationType.ToTop:
                    m_Direction = new Vector2(0, -1);
                    m_Reverse = true;
                    break;
                case OutOfScreenTranslation.AnimationType.ToBottom:
                    m_Direction = new Vector2(0, 1);
                    m_Reverse = true;
                    break;
                default:
                    break;
            }
        }

        protected override void Start()
        {
            RectTransform foregroundTransform = m_AffectedTransform;
            Canvas canvas = foregroundTransform.GetComponentInParent<Canvas>();
            RectTransform canvasTransform = (RectTransform)canvas.transform;
            float canvasWidth = canvasTransform.rect.size.x * canvasTransform.lossyScale.x;
            float canvasHeight = canvasTransform.rect.size.y * canvasTransform.lossyScale.y;

            UIPositionStorer foregroundStorer = AddPositionStorer(foregroundTransform.gameObject);

            m_TargetPosition = foregroundStorer.StoredPosition;
            m_InitialPosition = foregroundTransform.position;

            if (Mathf.Abs(m_Direction.x) > 0)
            {
                float changeX = 0;
                if (canvas.worldCamera == null)
                {
                    changeX = canvasWidth * 0.5f - m_Direction.x * (canvasWidth * 0.5f + foregroundTransform.rect.size.x * foregroundTransform.lossyScale.x * (m_Direction.x < 0 ? foregroundTransform.pivot.x : 1 - foregroundTransform.pivot.x)); 
                }
                else
                {
                    // TODO
                }

                m_InitialPosition.x = changeX;
                m_InitialPosition.y = foregroundStorer.StoredPosition.y;
            }
            else if (Mathf.Abs(m_Direction.y) > 0)
            {
                float changeY = 0;
                if (canvas.worldCamera == null)
                {
                    changeY = canvasHeight * 0.5f - m_Direction.y * (canvasHeight * 0.5f + foregroundTransform.rect.size.y * foregroundTransform.lossyScale.y * (m_Direction.y < 0 ? foregroundTransform.pivot.y : 1 - foregroundTransform.pivot.y));
                }
                else
                {
                    // TODO
                }

                m_InitialPosition.y = changeY;
                m_InitialPosition.x = foregroundStorer.StoredPosition.x;
            }

            if (!m_Reverse)
            {
                foregroundTransform.gameObject.SetActive(true);
            }
        }

        internal override void Progress(float progress)
        {
            if (m_Reverse)
                progress = 1 - progress;

            Vector3 displacement = m_InitialPosition + (m_TargetPosition - m_InitialPosition) * progress - (Vector2)m_AffectedTransform.position;

            m_AffectedTransform.position += displacement;
        }

        protected override void End()
        {
            //if (m_Reverse)
                // m_AffectedTransform.gameObject.SetActive(false);
        }
    }
}