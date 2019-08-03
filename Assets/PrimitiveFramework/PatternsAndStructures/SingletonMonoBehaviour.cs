using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
/// 
namespace PrimitiveFactory.Framework.PatternsAndStructures
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static object _lock = new object();

        protected static bool m_DontDestroyOnLoad = false;

        void Awake()
        {
            if (m_DontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public static bool IsCreated()
        {
            return _instance != null;
        }

        public static T Instance
        {
            set
            {
                _instance = value;
            }
            get
            {
                //if (applicationIsQuitting)
                //{
                //	Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                //		"' already destroyed on application quit." +
                //		" Won't create again - returning null.");
                //	return null;
                //}

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogWarning("[Singleton] Something went really wrong with " + typeof(T).ToString() +
                                " - there should never be more than 1 singleton!" +
                                " Reopening the scene might fix it.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();

                            if (m_DontDestroyOnLoad)
                            {
                                // This function doesn't call Object.DontDestroyOnLoad on UNITY_EDITOR, and nobody knows why...
                                // So I will comment it for now and call the function manually
                                //ObjectHelpers.DontDestroyOnLoad(singleton);
                                DontDestroyOnLoad(singleton);
                            }

                            Debug.LogWarning("[Singleton] An instance of " + typeof(T) +
                                " is needed in the scene, so '" + singleton +
                                "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            /*Debug.Log("[Singleton] Using instance already created: " +
                                _instance.gameObject.name);*/
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnDestroy()
        {
            // applicationIsQuitting = true;
            _instance = null;
        }
    }
}