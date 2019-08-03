using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    [TrackClipType(typeof(FromToTranslation))]
    [TrackClipType(typeof(OtherAnimation))]
    [TrackClipType(typeof(OutOfScreenTranslation))]
    [TrackClipType(typeof(Scale))]
    [TrackClipType(typeof(Fill))]
    [TrackClipType(typeof(AlphaFading))]
    public class UTAPlayableTrack : PlayableTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // hack to make the event names appear on the clips
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset != null)
                {
                    // Change the display name of all assets
                    IUTAClip utaInterface = (IUTAClip)clip.asset;
#if UNITY_EDITOR
                    clip.displayName = utaInterface.DisplayName;
#endif

                    // Force OtherAnimations to get the correct duration
                    if (clip.asset.GetType().IsAssignableFrom(typeof(OtherAnimation)))
                    {
                        clip.duration = clip.clipAssetDuration;
                    }
                }
            }
            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}