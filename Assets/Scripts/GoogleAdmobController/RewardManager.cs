using System;
using Firebase;
using GoogleMobileAds.Api;
using UnityEngine;

namespace GoogleAdmobController
{
    public class RewardedManager : MonoBehaviour
    {
        [SerializeField] string adUnitId = "ca-app-pub-3940256099942544/5224354917"; 
        RewardedAd _rewardedAd;
        bool _rewardEarned;

        void Start()
        {
            GoogleAdsmobController.OnAdsInitialized += InitializeRewardedAds;
        }

        void InitializeRewardedAds()
        {
            LoadRewardedAd();
        }

        public void LoadRewardedAd()
        {
            // Destroy old ad
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            AdRequest request = new AdRequest();

            RewardedAd.Load(adUnitId, request,
                (ad, error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded failed to load: " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded");
                    _rewardedAd = ad;
                    _rewardEarned = false;

                    RegisterCallbacks(ad);
                });
        }

        // Show the rewarded ad to the user
        public void ShowRewardedAd(Action onAdWatched = null)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward _) =>
                {
                    onAdWatched?.Invoke(); // Call callback after the ad is watched
                });
            }
            else
            {
                Debug.Log("Rewarded ad not ready, waiting...");
            }
        }

        // Register callbacks for when the ad is opened, closed, clicked, etc.
        void RegisterCallbacks(RewardedAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad shown");
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to display: " + error);
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad closed");
                LoadRewardedAd(); // Load the next ad only after the current one is closed
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad clicked");
            };

            ad.OnAdPaid += adValue =>
            {
                Debug.Log($"Rewarded ad revenue: {adValue.Value} {adValue.CurrencyCode}");
                double amount = adValue.Value / 1_000_000.0; // micros → currency

                AnalyticsController.Instance.LogReward(
                    "RewardedAds_Revenue",
                    amount,
                    adValue.CurrencyCode
                );
            };
        }

        void OnDestroy()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
            GoogleAdsmobController.OnAdsInitialized -= InitializeRewardedAds;
        }
    }
}
