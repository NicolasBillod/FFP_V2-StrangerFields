using UnityEngine;
using UnityEngine.UI;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class Fill : UTACurveClip<FillBehaviour>
    {
        public Image.FillMethod m_FillMethod;
        [HideInInspector]
        public int m_FillOrigin;

        protected override void PrepareReferences(FillBehaviour clone, IExposedPropertyTable resolver)
        {
            base.PrepareReferences(clone, resolver);

            clone.m_FillMethod = m_FillMethod;
            clone.m_FillOrigin = m_FillOrigin;
        }

#if UNITY_EDITOR
        public override string DisplayName { get { return string.Concat("Fill"); } }
#endif
    }
}