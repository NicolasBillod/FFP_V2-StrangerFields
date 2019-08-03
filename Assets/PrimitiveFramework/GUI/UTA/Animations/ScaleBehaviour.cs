using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class ScaleBehaviour : UTACurvePlayableBehaviour
    {
        private Vector3 m_ScaleSign;

        internal override void PrepareAnimation()
        {
        }

        protected override void Start()
        {
            Vector3 localScale = m_AffectedTransform.localScale;
            m_ScaleSign = new Vector3(Mathf.Sign(localScale.x), Mathf.Sign(localScale.y), Mathf.Sign(localScale.z));
        }

        internal override void Progress(float progress)
        {
            m_AffectedTransform.localScale = m_ScaleSign * progress;
        }
    }
}