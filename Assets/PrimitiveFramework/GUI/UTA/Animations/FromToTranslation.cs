using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class FromToTranslation : UTACurveClip<FromToTranslationBehaviour>
    {
        /****************
        * Related types *
        ****************/
        public enum AnimationType
        {
            MoveTo,
            MoveFrom
        }

        public AnimationType m_AnimationType;
        public ExposedReference<RectTransform> m_TargetTransform;

        protected override void PrepareReferences(FromToTranslationBehaviour clone, IExposedPropertyTable resolver)
        {
            base.PrepareReferences(clone, resolver);

            clone.m_AnimationType = m_AnimationType;
            clone.m_TargetTransform = m_TargetTransform.Resolve(resolver);
        }

#if UNITY_EDITOR
        public override string DisplayName { get { return string.Concat(m_AnimationType); } }
#endif
    }
}