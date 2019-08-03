using PrimitiveFactory.Framework.GUITools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public abstract class UTABasePlayableBehaviour : PlayableBehaviour
    {
        private bool m_Started;
        
        // At creation
        internal abstract void PrepareAnimation();

        // At launch
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if DEBUG
            if (Application.isPlaying)
            {
#endif
                m_Started = true;

                Start();
                Progress(ComputeProgress(0));
#if DEBUG
            }
#endif
        }

        protected virtual void Start() { }
        protected virtual void End() { }

        // On each frame
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
#if DEBUG
            if (Application.isPlaying)
            {
#endif
                float progress = Mathf.Clamp((float)(playable.GetTime() / playable.GetDuration()), 0, 1);
                Progress(ComputeProgress(progress));
#if DEBUG
            }
#endif
        }

        internal abstract void Progress(float progress);

        // At end
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if DEBUG
            if (Application.isPlaying)
            {
#endif
                if (m_Started)
                {
                    Progress(ComputeProgress(1));
                    End();
                }
#if DEBUG
            }
#endif
        }

        // Helpers
        protected virtual float ComputeProgress(float baseProgress)
        {
            return baseProgress;
        }

        protected UIPositionStorer AddPositionStorer(GameObject o)
        {
            UIPositionStorer storer = o.GetComponent<UIPositionStorer>();
            if (storer == null)
            {
                storer = o.AddComponent<UIPositionStorer>();
                storer.StoredPosition = o.transform.position;
            }

            return storer;
        }
    }
}