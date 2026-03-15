using System;
using System.Collections;
using UnityEngine;

namespace Common
{
    public class Helper : SingletonHelper<Helper>
    {
        /// <summary>
        /// Wait for seconds, then execute callback.
        /// </summary>
        public static void Wait(float seconds, Action callback)
        {
            if (Instance == null)
            {
                Debug.LogError("[Helper] No Helper instance found in scene. Cannot execute Wait. " +
                               "Please add a Helper component to the scene.");
                return;
            }

            Instance.StartCoroutine(Instance.WaitCoroutine(seconds, callback));
        }

        /// <summary>
        /// Wait for seconds using unscaled time, then execute callback.
        /// </summary>
        public static void WaitRealtime(float seconds, Action callback)
        {
            if (Instance == null)
            {
                Debug.LogError("[Helper] No Helper instance found in scene. Cannot execute WaitRealtime.");
                return;
            }

            Instance.StartCoroutine(Instance.WaitRealtimeCoroutine(seconds, callback));
        }

        /// <summary>
        /// Execute callback on the next frame.
        /// </summary>
        public static void WaitForEndOfFrame(Action callback)
        {
            if (Instance == null)
            {
                Debug.LogError("[Helper] No Helper instance found in scene. Cannot execute WaitForEndOfFrame.");
                return;
            }

            Instance.StartCoroutine(Instance.WaitForEndOfFrameCoroutine(callback));
        }

        /// <summary>
        /// Stop a running coroutine if Helper instance exists.
        /// </summary>
        public static void StopWait(Coroutine coroutine)
        {
            if (Instance != null && coroutine != null)
            {
                Instance.StopCoroutine(coroutine);
            }
        }

        private IEnumerator WaitCoroutine(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        private IEnumerator WaitRealtimeCoroutine(float seconds, Action callback)
        {
            yield return new WaitForSecondsRealtime(seconds);
            callback?.Invoke();
        }

        private IEnumerator WaitForEndOfFrameCoroutine(Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        protected override void OnDestroy()
        {
            // Stop all coroutines when destroyed
            StopAllCoroutines();
            base.OnDestroy();
        }
    }
}