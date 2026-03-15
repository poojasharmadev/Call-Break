using UnityEngine;

namespace Common
{
    public class SingletonHelper<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object Lock = new object();
        private static bool _applicationIsQuitting = false;
    
        [SerializeField] private bool dontDestroyOnLoad = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + 
                                     "' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (Lock)
                {
                    // Always try to find existing instance first
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));
                    }

                    // Check if we found it
                    if (_instance != null)
                    {
                        // Verify if this should be a singleton (dontDestroyOnLoad)
                        SingletonHelper<T> helper = _instance.GetComponent<SingletonHelper<T>>();
                        
                        if (helper != null && helper.dontDestroyOnLoad)
                        {
                            // True singleton mode - check for duplicates
                            if (FindObjectsOfType(typeof(T)).Length > 1)
                            {
                                Debug.LogError("[Singleton] Multiple instances found when dontDestroyOnLoad is true! " +
                                               "There should only be one singleton. Reopening the scene might fix it.");
                            }
                        }
                        
                        return _instance;
                    }

                    // Instance not found - only create if it's supposed to be a true singleton
                    // For non-singleton (dontDestroyOnLoad = false), we just return null
                    // The derived class should exist in the scene already
                    
                    Debug.LogWarning("[Singleton] No instance of " + typeof(T) + 
                                     " found in scene. Returning null. Make sure an instance exists in the scene.");
                    return null;
                }
            }
        }

        protected virtual void Awake()
        {
            if (dontDestroyOnLoad)
            {
                // True singleton mode - enforce single instance
                if (_instance == null)
                {
                    _instance = this as T;
                    DontDestroyOnLoad(gameObject);
                    Debug.Log("[Singleton] " + typeof(T) + " instance set as persistent singleton.");
                }
                else if (_instance != this)
                {
                    Debug.LogWarning("[Singleton] Another instance of " + typeof(T) + 
                                     " already exists. Destroying duplicate.");
                    Destroy(gameObject);
                }
            }
            else
            {
                // Non-singleton mode - just update reference, allow multiple instances
                _instance = this as T;
                Debug.Log("[Singleton] " + typeof(T) + " instance found (non-persistent mode).");
            }
        }

        protected virtual void OnDestroy()
        {
            // Clear instance reference if this was the active instance
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}