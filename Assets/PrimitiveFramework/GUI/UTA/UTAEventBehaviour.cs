using UnityEngine.Events;
using UnityEngine.Playables;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class UTAEventBehaviour : PlayableBehaviour
    {
        public UnityEvent m_Event;

        internal void Trigger()
        {
            if (m_Event != null)
                m_Event.Invoke();
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (info.evaluationType == FrameData.EvaluationType.Playback) // ignore scrubs
                Trigger();
        }
    }
}