using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public abstract class UTACurveClip<PlayableBehaviourType> : UTABaseClip<PlayableBehaviourType> where PlayableBehaviourType : UTACurvePlayableBehaviour, new()
    { 
        public ExposedReference<RectTransform> m_AffectedTransform;
        public AnimationCurve m_AnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        protected override void PrepareReferences(PlayableBehaviourType clone, IExposedPropertyTable resolver)
        {
            base.PrepareReferences(clone, resolver);

            clone.m_AffectedTransform = m_AffectedTransform.Resolve(resolver);
            clone.m_AnimationCurve = m_AnimationCurve;
        }
    }
}