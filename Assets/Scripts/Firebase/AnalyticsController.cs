using Common;
using Firebase.Analytics;
using UnityEngine;

namespace Firebase
{
    public class AnalyticsController : SingletonHelper<AnalyticsController>
    {
        void Start()
        {
            FirebaseInitialize.OnFirebaseInitialized += InitializeAnalytics;
        }

        private void OnDestroy()
        {
            FirebaseInitialize.OnFirebaseInitialized -= InitializeAnalytics;
        }

        private void InitializeAnalytics()
        {
            Debug.Log("Firebase Analytics initialized.");
            LogAppStartEvent();
        }

        public void LogAppStartEvent()
        {
            FirebaseAnalytics.LogEvent("app_start");
        }

        public void LogLevelStart(string levelName)
        {
            FirebaseAnalytics.LogEvent("level_start", new Parameter("level_name", levelName));
        }

        public void LogCustomEvent(string eventName, params Parameter[] parameters)
        {
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }
        public void LogReward(string rewardName, double amount, string currency)
        {
            FirebaseAnalytics.LogEvent(
                "reward_earned",
                new Parameter("reward_name", rewardName),
                new Parameter("amount", amount),
                new Parameter("currency", currency)
            );
        }
    }
}
