using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    [TrackColor(186f/255f, 193f/255f, 43f/255f)]
    [TrackClipType(typeof(UTAEvent))]
    public class UTAEventTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                // Update duration to 1 frame for events
                if (clip.asset != null)
                {
                    clip.duration = clip.clipAssetDuration;
                }
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
