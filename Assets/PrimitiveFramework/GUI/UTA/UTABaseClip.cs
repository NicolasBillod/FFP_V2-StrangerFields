using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public abstract class UTABaseClip<PlayableBehaviourType> : PlayableAsset, ITimelineClipAsset, IUTAClip where PlayableBehaviourType : UTABasePlayableBehaviour, new()
    {
        private PlayableBehaviourType template = new PlayableBehaviourType();

        public ClipCaps clipCaps
        {
            get
            {
                return ClipCaps.Extrapolation;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PlayableBehaviourType>.Create(graph, template);
            PlayableBehaviourType clone = playable.GetBehaviour();
            IExposedPropertyTable resolver = graph.GetResolver();

            PrepareReferences(clone, resolver);

            clone.PrepareAnimation();
            return playable;
        }

        protected virtual void PrepareReferences(PlayableBehaviourType clone, IExposedPropertyTable resolver)
        {
        }

#if UNITY_EDITOR
        public abstract string DisplayName { get; }
#endif
    }
}