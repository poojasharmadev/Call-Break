using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    public class SceneLoader : SingletonHelper<SceneLoader>
    {
        public float loadingTime;
        public event Action OnSceneLoadRequested;
        public event Action OnSceneLoaded;
        IEnumerator _loadingRoutine;

        void Start()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        void HandleSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (_loadingRoutine != null)
            {
                StopCoroutine(_loadingRoutine);
            }
            _loadingRoutine = NotifySceneLoadRoutine();
            StartCoroutine(_loadingRoutine);
        }

        public void LoadScene(string sceneName)
        {
           OnSceneLoadRequested?.Invoke();
           SceneManager.LoadScene(sceneName);
        }

        IEnumerator NotifySceneLoadRoutine()
        {
            yield return new WaitForSeconds(loadingTime);
            OnSceneLoaded?.Invoke();
            _loadingRoutine = null;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }
}
