using System;
using Firebase.Extensions;
using UnityEngine;
namespace Firebase
{
    public class FirebaseInitialize:MonoBehaviour
    {
        FirebaseApp _app; 
        public static event Action OnFirebaseInitialized;
        void Awake()
        {
            InitializeFirebase();
        }
        void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Initialize the FirebaseApp instance
                    _app = FirebaseApp.DefaultInstance;
                    // Firebase is ready to use
                    Debug.Log("Firebase initialized successfully.");
                    OnFirebaseInitialized?.Invoke();
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    // Firebase Unity SDK is not safe to use here
                }
            });
        }
    }
}
