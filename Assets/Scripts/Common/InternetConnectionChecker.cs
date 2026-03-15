using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Common
{
    public class InternetConnectionChecker : SingletonHelper<InternetConnectionChecker>
    {
        public static event Action OnInternetConnected;
        public static event Action OnInternetDisconnected;
        
        [Header("Connection Status")]
        [SerializeField] private bool _isInternetConnected = false;
        
        [Header("Settings")]
        [SerializeField] private float _checkInterval = 0.5f;
        [SerializeField] private float _requestTimeout = 0.5f;
        
        private Coroutine _checkCoroutine;

        void Start()
        {
            // Start continuous checking
            _checkCoroutine = StartCoroutine(ContinuousConnectionCheck());
        }

        private IEnumerator ContinuousConnectionCheck()
        {
            while (true)
            {
                yield return CheckConnection();
                yield return new WaitForSeconds(_checkInterval);
            }
        }

        private IEnumerator CheckConnection()
        {
            bool isConnected = false;
            
            // Check if network is reachable
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                // Try to ping Google
                using (UnityWebRequest request = UnityWebRequest.Head("https://www.google.com"))
                {
                    request.timeout = (int)_requestTimeout;
                    yield return request.SendWebRequest();
                    
                    isConnected = (request.result == UnityWebRequest.Result.Success);
                }
            }
            
            // Update status and fire events if changed
            UpdateConnectionStatus(isConnected);
        }

        private void UpdateConnectionStatus(bool newStatus)
        {
            if (newStatus != _isInternetConnected)
            {
                _isInternetConnected = newStatus;
                
                if (_isInternetConnected)
                {
                    Debug.Log("✓ Internet Connected");
                    OnInternetConnected?.Invoke();
                }
                else
                {
                    Debug.LogWarning("✗ Internet Disconnected");
                    OnInternetDisconnected?.Invoke();
                }
            }
        }
        public bool IsInternetConnected()
        {
            return _isInternetConnected;
        }

        private void OnDestroy()
        {
            if (_checkCoroutine != null)
            {
                StopCoroutine(_checkCoroutine);
            }
        }
    }
}