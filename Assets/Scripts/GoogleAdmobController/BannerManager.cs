using Firebase;
using GoogleMobileAds.Api;
using UnityEngine;

namespace GoogleAdmobController
{
    public class BannerManager : MonoBehaviour
    {
        [SerializeField] private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";

        BannerView _bannerView;
        bool _isBannerLoaded;

        void Start()
        {
            GoogleAdsmobController.OnAdsInitialized += InitializeBannerAds;
        }

        void InitializeBannerAds()
        {
            if (_bannerView != null)
                return;

            // Adaptive banner (recommended by AdMob)
            AdSize adaptiveSize =
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                    AdSize.FullWidth);

            _bannerView = new BannerView(
                bannerAdUnitId,
                adaptiveSize,
                AdPosition.Top);

            RegisterBannerCallbacks();

            AdRequest request = new AdRequest();
            _bannerView.LoadAd(request);
            HideBanner();
        }

        public void ShowBanner()
        {
            if (_bannerView != null && _isBannerLoaded)
            {
                _bannerView.Show();
            }
        }

        public void HideBanner()
        {
            _bannerView?.Hide();
        }

        void DestroyBanner()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
                _isBannerLoaded = false;
            }
        }

        private void RegisterBannerCallbacks()
        {
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner loaded");
                _isBannerLoaded = true;
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner failed to load: " + error);
                _isBannerLoaded = false;
            };

            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner clicked");
            };

            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"Banner revenue: {adValue.Value} {adValue.CurrencyCode}");
                double amount = adValue.Value / 1_000_000.0; // micros → currency
                AnalyticsController.Instance.LogReward(
                    "Banner_Revenue",
                    amount,
                    adValue.CurrencyCode
                );
            };
        }

        void OnDestroy()
        {
            DestroyBanner();
            GoogleAdsmobController.OnAdsInitialized -= InitializeBannerAds;
        }
    }
}
