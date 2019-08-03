using PrimitiveFactory.Framework.PatternsAndStructures;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class UTAController : SingletonMonoBehaviour<UTAController>
    {
        public interface IStatusChanger
        {
            void OnUTAStatusChanged(bool status);
        }

        [SerializeField]
        private CanvasGroup m_MainCanvasGroup;

        private Dictionary<string, UTADirector> m_Directors;
        private List<UTADirector> m_DirectorsIT;
        private List<string> m_ScreenStack = new List<string>(); // List instead of stack because we want to be able to iterate through it

        private List<IStatusChanger> m_StatusChangers;

        private void Awake()
        {
            m_StatusChangers = new List<IStatusChanger>();
            InitializeDirectorDictionary();
        }

        private void Update()
        {
            bool isPlaying = false;

            for (int i = 0; i < m_DirectorsIT.Count; i++)
            {
                if (m_DirectorsIT[i].IsPlaying())
                {
                    isPlaying = true;
                    break;
                }
            }

            if (m_MainCanvasGroup.interactable == isPlaying)
            {
                m_MainCanvasGroup.interactable = !isPlaying;
                foreach (IStatusChanger changer in m_StatusChangers)
                    changer.OnUTAStatusChanged(m_MainCanvasGroup.interactable);
            }
        }

        private void InitializeDirectorDictionary()
        {
            m_Directors = new Dictionary<string, UTADirector>();
            m_DirectorsIT = new List<UTADirector>();
            foreach (Transform t in transform)
            {
                UTADirector d = t.GetComponent<UTADirector>();
                m_Directors.Add(t.name, d);
                m_DirectorsIT.Add(d);
            }           
        }

        public UTADirector CreateNewAnimation(PlayableAsset animation)
        {
            GameObject child = new GameObject(animation.name);
            child.transform.SetParent(transform);

            UTADirector utaDirector = child.AddComponent<UTADirector>();
            utaDirector.Prepare(animation);

            return utaDirector;
        }

        public void UpdateObjectNames()
        {
            foreach (Transform t in transform)
            {
                t.name = t.GetComponent<PlayableDirector>().playableAsset.name;
            }
        }

        public void PlayAnimation(string animationName)
        {
            string[] animations = animationName.Split('&');
            for (int i = 0; i < animations.Length; i++)
            {
                _PlayAnimation(animations[i]);
            }

            //string[] animations = animationName.Split(new[] { ';' },2);

            //if (animations.Length == 1)
            //{
            //    _PlayAnimation(animations[0]);
            //}
            //else
            //{
            //    _PlayAnimation(animations[0], animations[1]);
            //}
        }

        private void _PlayAnimation(string animationName, string chainEvent = null)
        {
            m_Directors[animationName].Stop();
            m_Directors[animationName].Play(chainEvent);
        }

        /******************
        * Status changers *
        ******************/
        public void AddStatusChangerListener(IStatusChanger changer)
        {
            m_StatusChangers.Add(changer);
        }

        public void RemoveStatusChangerListener(IStatusChanger changer)
        {
            m_StatusChangers.Remove(changer);
        }

        /*********************
        * Back history queue *
        *********************/
        public void PushBackScreen(string eventName)
        {
            if (m_ScreenStack.Contains(eventName))
            {
                m_ScreenStack.Remove(eventName);
            }

            m_ScreenStack.Insert(0, eventName);
        }

        public void ChangeBackScreen(string eventName)
        {
            m_ScreenStack[0] = eventName;
        }

        public void GoBackSequence()
        {
            string current = m_ScreenStack[0]; // Pop
            m_ScreenStack.RemoveAt(0);
            string destination = m_ScreenStack[0]; // Peek
            PlayAnimation(string.Concat("Hide", current, ";Show", destination));
        }

        public void GoBackParallel()
        {
            string current = m_ScreenStack[0]; // Pop
            m_ScreenStack.RemoveAt(0);
            string destination = m_ScreenStack[0]; // Peek
            PlayAnimation(string.Concat("Hide", current, "&Show", destination));
        }

        public void GoToScreenWithHistory_Sequence(string screenName)
        {
            string current = m_ScreenStack[0]; // Peek
            PlayAnimation(string.Concat("Hide", current, ";Show", screenName));
            PushBackScreen(screenName);
        }

        public void GoToScreenWithHistory_Parallel(string screenName)
        {
            string current = m_ScreenStack[0]; // Peek
            PlayAnimation(string.Concat("Hide", current, "&Show", screenName));
            PushBackScreen(screenName);
        }
    }
}