using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class AlphaFadingBehaviour : UTACurvePlayableBehaviour
    {
        private CanvasGroup m_TargetGroup;

        internal override void PrepareAnimation()
        {
            m_TargetGroup = m_AffectedTransform.GetComponent<CanvasGroup>();
        }

        protected override void Start()
        {
        }

        internal override void Progress(float progress)
        {
            m_TargetGroup.alpha = progress;
        }
    }
}