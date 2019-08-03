using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class UTAEvent : PlayableAsset
    {
        private UTAEventBehaviour template = new UTAEventBehaviour();

        public override double duration
        {
            get { return 12 / 60f; }
        }

        public string m_EventUUID = string.Empty;

        private GameObject m_Owner;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // Reference owner for OnDestroy
            m_Owner = owner;

            // Get event and check for duplicate if Ctrl+D has been used
            UTADirector ownerDirector = owner.GetComponent<UTADirector>();
            ownerDirector.CheckForDuplicateEvents();
            UnityEvent unityEvent = null;
            if (string.IsNullOrEmpty(m_EventUUID))
            {
                m_EventUUID = System.Guid.NewGuid().ToString();
                unityEvent = ownerDirector.AddEvent(m_EventUUID);
            }
            else
            {
                unityEvent = ownerDirector.GetEvent(m_EventUUID);
            }

            // Create playable
            var playable = ScriptPlayable<UTAEventBehaviour>.Create(graph, template);
            UTAEventBehaviour clone = playable.GetBehaviour();
            clone.m_Event = unityEvent;

            return playable;
        }

        public void OnDestroy()
        {
            m_Owner.GetComponent<UTADirector>().DeleteEvent(m_EventUUID);
        }
    }
}