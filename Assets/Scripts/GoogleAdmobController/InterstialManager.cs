using System;
using Firebase;
using GoogleMobileAds.Api;
using UnityEngine;

namespace GoogleAdmobController
{
    public class InterstitialManager : MonoBehaviour
    {
        [SerializeField] string adUnitId = "ca-app-pub-3940256099942544/1033173712";
        InterstitialAd _interstitialAd;
        int _retryAttempt;

        string _saveCount = "Interstitial_Save_Count";

        void Start()
        {
            GoogleAdsmobController.OnAdsInitialized += InitializeInterstitialAds;
        }

        void InitializeInterstitialAds()
        {
            LoadInterstitial();
        }

        void LoadInterstitial()
        {
            // Clean up old ad
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            AdRequest request = new AdRequest();

            InterstitialAd.Load(adUnitId, request,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Interstitial failed to load: " + error);
                        RetryLoad();
                        return;
                    }

                    Debug.Log("Interstitial loaded successfully");
                    _interstitialAd = ad;
                    _retryAttempt = 0;

                    RegisterCallbacks(_interstitialAd);
                });
        }

        public void ShowInterstial(Action onAdWatched = null)
        {
            var rule = GoogleAdsmobController.Instance.adRules.GetRule("Interstitial");

            if (rule == null)
            {
                Debug.LogError("Interstitial rule not found!");
                onAdWatched?.Invoke();
                return;
            }

            int count = PlayerPrefs.GetInt(_saveCount, 0);

            if (count >= rule.ruleCount)
            {
                PlayerPrefs.SetInt(_saveCount, 0);
                ShowInterstitialAfterLoad(onAdWatched);
            }
            else
            {
                PlayerPrefs.SetInt(_saveCount, count + 1);
                onAdWatched?.Invoke();
            }
        }
        void ShowInterstitialAfterLoad(Action onAdWatched = null)
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
                _interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    onAdWatched?.Invoke();
                };
            }
            else
            {
                Debug.Log("Interstitial not ready, loading again...");
                LoadInterstitial();
            }
        }

        void RegisterCallbacks(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial displayed");
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial failed to display: " + error);
                LoadInterstitial();
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial closed");
                LoadInterstitial(); // Preload next ad
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial clicked");
            };

            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"Ad revenue: {adValue.Value} {adValue.CurrencyCode}");
                double amount = adValue.Value / 1_000_000.0; // micros → currency
                AnalyticsController.Instance.LogReward(
                    "Interstial_Revenue",
                    amount,
                    adValue.CurrencyCode
                );
            };
        }

        private void RetryLoad()
        {
            _retryAttempt++;
            float retryDelay = Mathf.Pow(2, Mathf.Min(6, _retryAttempt));
            Invoke(nameof(LoadInterstitial), retryDelay);
        }

        private void OnDestroy()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
            }
            GoogleAdsmobController.OnAdsInitialized -= InitializeInterstitialAds;
        }
    }
}
