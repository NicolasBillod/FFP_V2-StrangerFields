using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public abstract class UTACurvePlayableBehaviour : UTABasePlayableBehaviour
    {
        internal RectTransform m_AffectedTransform;
        internal AnimationCurve m_AnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        protected override float ComputeProgress(float baseProgress)
        {
            return m_AnimationCurve.Evaluate(baseProgress);
        }
    }
}