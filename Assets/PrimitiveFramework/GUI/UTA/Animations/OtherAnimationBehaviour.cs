using UnityEngine.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class OtherAnimationBehaviour : UTABasePlayableBehaviour
    {
        internal TimelineAsset m_OtherAnimation;

        internal override void PrepareAnimation()
        {
        }

        protected override void Start()
        {
            // Launch other animation
            UTAController.Instance.PlayAnimation(m_OtherAnimation.name);
        }

        internal override void Progress(float progress)
        {
            // Nothing to do here, the other animation will do
        }
    }
}