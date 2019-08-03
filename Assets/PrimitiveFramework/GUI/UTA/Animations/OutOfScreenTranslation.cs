using UnityEngine;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class OutOfScreenTranslation : UTACurveClip<OutOfScreenTranslationBehaviour>
    {
        /****************
        * Related types *
        ****************/
        public enum AnimationType
        {
            FromRight,
            FromLeft,
            FromTop,
            FromBottom,
            ToRight,
            ToLeft,
            ToTop,
            ToBottom
        }

        public AnimationType m_AnimationType;

        protected override void PrepareReferences(OutOfScreenTranslationBehaviour clone, IExposedPropertyTable resolver)
        {
            base.PrepareReferences(clone, resolver);

            clone.m_AnimationType = m_AnimationType;
        }

#if UNITY_EDITOR
        public override string DisplayName { get { return string.Concat(m_AnimationType); } }
#endif
    }
}