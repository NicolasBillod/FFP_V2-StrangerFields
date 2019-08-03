using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(PlayableDirector))]
    public class UTADirector : MonoBehaviour
    {
        // Event management
        [SerializeField]
        private StringUnityEventDictionary m_EventDictionary = StringUnityEventDictionary.New<StringUnityEventDictionary>();
        [SerializeField]
        private bool m_ShouldBreakInteractions = true;

        // Playable
        private PlayableDirector m_Playable;

        // Chain
        private string m_ChainEvent = null;

#if UNITY_EDITOR
        private Dictionary<string, UnityEvent> m_UndoEvents = new Dictionary<string, UnityEvent>();
#endif

        /*****************
        * Initialization *
        *****************/
        private void Awake()
        {
            m_Playable = GetComponent<PlayableDirector>();

#if UNITY_EDITOR
            m_Playable.hideFlags = HideFlags.None;
            this.hideFlags = HideFlags.None;
#endif

            enabled = false;
        }

        internal void Prepare(PlayableAsset animation)
        {
            m_Playable = GetComponent<PlayableDirector>();
            if (m_Playable == null)
                m_Playable = gameObject.AddComponent<PlayableDirector>();
            m_Playable.playOnAwake = false;
            m_Playable.playableAsset = animation;
        }
        
        /*********************
        * Director interface *
        *********************/ 
        internal void Play(string chainEvent)
        {
            m_Playable.Play();
            m_ChainEvent = chainEvent;
            if (chainEvent != null)
                enabled = true;
        }

        internal void Stop()
        {
            m_Playable.Stop();
        }

        internal bool IsPlaying()
        {
            return m_ShouldBreakInteractions && m_Playable.state == PlayState.Playing;
        }

        public PlayableAsset GetPlayableAsset()
        {
            return m_Playable.playableAsset;
        }

        public PlayableDirector GetPlayableDirector()
        {
            return GetComponent<PlayableDirector>();
        }

        /***********
        * Chaining *
        ***********/
        private void Update()
        {
            if (!IsPlaying())
            {
                UTAController.Instance.PlayAnimation(m_ChainEvent);
                m_ChainEvent = null;
                enabled = false;
            }
        }

        /*******************
        * Event management *
        *******************/
        internal UnityEvent AddEvent(string uuid)
        {
            UnityEvent res = new UnityEvent();
            m_EventDictionary.Add(uuid, res);
            return res;
        }

        public UnityEvent GetEvent(string uuid)
        {
#if UNITY_EDITOR
            if (m_UndoEvents.ContainsKey(uuid))
                Undo(uuid);
#endif
            return m_EventDictionary[uuid];
        }

        public void DeleteEvent(string uuid)
        {
#if UNITY_EDITOR
            m_UndoEvents.Add(uuid, m_EventDictionary[uuid]);
            m_EventDictionary.Remove(uuid);
#endif
        }

        public void Undo(string uuid)
        {
#if UNITY_EDITOR
            m_EventDictionary.Add(uuid, m_UndoEvents[uuid]);
            m_UndoEvents.Remove(uuid);
#endif
        }

        public void CheckForDuplicateEvents()
        {
            // HashSet<string> uuids = new HashSet<string>(); // List of parsed uuids

            //TimelineAsset timeline = (TimelineAsset)m_Playable.playableAsset; // Parse all tracks
            //foreach (TrackAsset track in timeline.GetRootTracks())
            //{
            //    if (track.GetType().IsAssignableFrom(typeof(UTAEventTrack)))  // To find Event tracks
            //    {
            //        foreach (TimelineClip clip in track.GetClips())
            //        {
            //            UTAEvent eventClip = (UTAEvent)clip.asset;
            //            if (uuids.Contains(eventClip.m_EventUUID))
            //            {
            //                // Already parsed event UUID, reassign 
            //                UnityEvent refEvent = GetEvent(eventClip.m_EventUUID);
            //                eventClip.m_EventUUID = System.Guid.NewGuid().ToString();
            //                UnityEvent newEvent = AddEvent(eventClip.m_EventUUID);

            //                // Copy
            //                FieldInfo[] fields = refEvent.GetType().BaseType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            //                foreach (FieldInfo field in fields)
            //                {
            //                    if (field.Name == "m_PersistentCalls" || field.Name == "m_TypeName")
            //                        field.SetValue(newEvent, field.GetValue(refEvent));
            //                }

            //                string path = UnityEditor.AssetDatabase.GetAssetPath(timeline);
            //                UnityEditor.EditorUtility.SetDirty(timeline);
            //                UnityEditor.AssetDatabase.SaveAssets();
            //                UnityEditor.AssetDatabase.Refresh();
            //                UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
            //                UnityEditor.AssetDatabase.ForceReserializeAssets(new HashSet<string>() { path });


            //                Debug.Log("Added a copy");
            //            }

            //            uuids.Add(eventClip.m_EventUUID);
            //        }
            //    }
            //}
        }
    }
}