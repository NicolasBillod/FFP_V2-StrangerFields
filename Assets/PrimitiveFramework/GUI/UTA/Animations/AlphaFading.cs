using UnityEngine;
using UnityEngine.UI;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class AlphaFading : UTACurveClip<AlphaFadingBehaviour>
    {
        protected override void PrepareReferences(AlphaFadingBehaviour clone, IExposedPropertyTable resolver)
        {
            base.PrepareReferences(clone, resolver);
        }

#if UNITY_EDITOR
        public override string DisplayName { get { return string.Concat("AlphaFading"); } }
#endif
    }
}