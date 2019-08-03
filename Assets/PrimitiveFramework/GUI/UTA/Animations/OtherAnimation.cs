using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class OtherAnimation : UTABaseClip<OtherAnimationBehaviour>
    {
        public TimelineAsset m_OtherAnimation;

        protected override void PrepareReferences(OtherAnimationBehaviour clone, IExposedPropertyTable resolver)
        {
            base.PrepareReferences(clone, resolver);

            clone.m_OtherAnimation = m_OtherAnimation;
        }

        public override double duration
        {
            get
            {
                if (m_OtherAnimation == null)
                    return 1f;
                else
                    return Mathf.Round((float)(m_OtherAnimation.duration * 1000)) / 1000f; // Some rounding mistakes on Unity's side ?
            }
        }

#if UNITY_EDITOR
        public override string DisplayName { get { return string.Concat("<<", m_OtherAnimation == null ? "Missing" : m_OtherAnimation.name,">>"); } }
#endif
    }
}