using System;
using System.Collections.Generic;
using Common;
using Firebase;
using GoogleMobileAds.Api;
using UnityEngine;
namespace GoogleAdmobController
{
    public class GoogleAdsmobController : SingletonHelper<GoogleAdsmobController>
    {
        public static Action OnAdsInitialized;
        [Header("List of Ad Units")]
        [SerializeField] string appId = "ca-app-pub-3940256099942544~3347511713"; // Test App ID    
        
        [Header("Instances of Ad Managers")]
        public BannerManager bannerManager;
        public RewardedManager rewardedManager;
        public InterstitialManager interstitialManager;

        [Header("Ads rule")]
        public ListAdRules adRules;
        void Start()
        {
            InitializeAdRule(RemoteConfigController.Instance.GetValue(Constants.AdRuleKey));
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize((InitializationStatus initstatus) =>
            {
                if (initstatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");
                    return;
                }

                Debug.Log("Google Mobile Ads initialization complete.");
                OnAdsInitialized?.Invoke();

                // Google Mobile Ads events are raised off the Unity Main thread. If you need to
                // access UnityEngine objects after initialization,
                // use MobileAdsEventExecutor.ExecuteInUpdate(). For more information, see:
                // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
            });
        }
        void InitializeAdRule(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            adRules = JsonUtility.FromJson<ListAdRules>(data);
            adRules.Init();
        }
    }
}
[Serializable]
public class ListAdRules
{
    public AdRule[] adRules;
    private Dictionary<string, AdRule> ruleMap;

    public void Init()
    {
        ruleMap = new Dictionary<string, AdRule>();
        foreach (var rule in adRules)
        {
            ruleMap[rule.ruleName] = rule;
        }
    }
    public AdRule GetRule(string ruleName)
    {
        return ruleMap.TryGetValue(ruleName, out var rule) ? rule : null;
    }
}
[Serializable]
public class AdRule
{
    public string ruleName;
    public int ruleCount;
}