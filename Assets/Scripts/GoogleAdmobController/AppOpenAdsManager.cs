using System;
using Firebase;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleAdmobController
{
    public class AppOpenAdsManager : MonoBehaviour
    {
        [SerializeField] string appOpenAdUnitId =
            "ca-app-pub-3940256099942544/9257395921"; // TEST ID
        AppOpenAd _appOpenAd;
        DateTime _adLoadTime;
        bool _isShowingAd;
        int _retryAttempt;
        const int MaxCacheHours = 4;

        void Awake()
        {
            // Recommended by AdMob
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        }

        void Start()
        {
            LoadAppOpenAd();
        }

        void OnDestroy()
        {
            AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;

            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }
        }

        void OnAppStateChanged(AppState state)
        {
            if (state == AppState.Foreground)
            {
                ShowAdIfReady();
            }
        }

        public void LoadAppOpenAd()
        {
            if (IsAdAvailable())
                return;

            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }

            AdRequest request = new AdRequest();

            AppOpenAd.Load(
                appOpenAdUnitId,
                request,
                (AppOpenAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("App Open failed to load: " + error);
                        RetryLoad();
                        return;
                    }

                    _appOpenAd = ad;
                    _adLoadTime = DateTime.Now;
                    _retryAttempt = 0;

                    RegisterCallbacks(ad);
                    Debug.Log("App Open ad loaded");
                });
        }

        void ShowAdIfReady()
        {
            if (_isShowingAd)
                return;

            if (IsAdAvailable() && _appOpenAd.CanShowAd())
            {
                _isShowingAd = true;
                _appOpenAd.Show();
            }
            else
            {
                LoadAppOpenAd();
            }
        }

        bool IsAdAvailable()
        {
            return _appOpenAd != null &&
                   (DateTime.Now - _adLoadTime).TotalHours < MaxCacheHours;
        }

        void RegisterCallbacks(AppOpenAd ad)
        {
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App Open shown");
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App Open closed");
                _isShowingAd = false;
                LoadAppOpenAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App Open failed to show: " + error);
                _isShowingAd = false;
                LoadAppOpenAd();
            };

            ad.OnAdPaid += (AdValue value) =>
            {
                Debug.Log($"Revenue: {value.Value} {value.CurrencyCode}");
                double amount = value.Value / 1_000_000.0; // micros → currency
                AnalyticsController.Instance.LogReward(
                    "AppOpen_Revenue",
                    amount,
                    value.CurrencyCode
                );
            };
        }

        void RetryLoad()
        {
            _retryAttempt++;
            float delay = Mathf.Pow(2, Mathf.Min(6, _retryAttempt));
            Invoke(nameof(LoadAppOpenAd), delay);
        }
    }
}
