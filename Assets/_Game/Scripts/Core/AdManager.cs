using System;
using UnityEngine;

#if UNITY_ADS_ENABLE
using GoogleMobileAds.Api;
#endif

namespace VS.Core
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        // 테스트 광고 ID — 출시 전 실제 AdMob ID로 교체할 것
        private const string BannerId      = "ca-app-pub-3940256099942544/6300978111";
        private const string InterstitialId = "ca-app-pub-3940256099942544/1033173712";
        private const string RewardedId    = "ca-app-pub-3940256099942544/5224354917";

        private bool _interstitialReady;
        private bool _rewardedReady;

#if UNITY_ADS_ENABLE
        private BannerView _banner;
        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
#endif

        public bool IsInterstitialReady => _interstitialReady;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
#if UNITY_ADS_ENABLE
            MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
            });
#endif
        }

        // ── 배너 ──────────────────────────────────────────────

        public void ShowBanner()
        {
#if UNITY_ADS_ENABLE
            if (_banner != null) { _banner.Show(); return; }
            _banner = new BannerView(BannerId, AdSize.Banner, AdPosition.Bottom);
            _banner.LoadAd(new AdRequest());
#endif
        }

        public void HideBanner()
        {
#if UNITY_ADS_ENABLE
            _banner?.Hide();
#endif
        }

        public void DestroyBanner()
        {
#if UNITY_ADS_ENABLE
            _banner?.Destroy();
            _banner = null;
#endif
        }

        // ── 전면 광고 ─────────────────────────────────────────

        public void LoadInterstitial()
        {
#if UNITY_ADS_ENABLE
            InterstitialAd.Load(InterstitialId, new AdRequest(), (ad, error) =>
            {
                if (error != null) return;
                _interstitial = ad;
                _interstitialReady = true;
            });
#endif
        }

        public void ShowInterstitial(Action onClosed = null)
        {
#if UNITY_ADS_ENABLE
            if (!_interstitialReady || _interstitial == null) { onClosed?.Invoke(); return; }
            _interstitialReady = false;
            _interstitial.OnAdFullScreenContentClosed += () =>
            {
                _interstitial.Destroy();
                LoadInterstitial();
                onClosed?.Invoke();
            };
            _interstitial.Show();
#else
            onClosed?.Invoke();
#endif
        }

        // ── 보상형 광고 ───────────────────────────────────────

        public void LoadRewarded()
        {
#if UNITY_ADS_ENABLE
            RewardedAd.Load(RewardedId, new AdRequest(), (ad, error) =>
            {
                if (error != null) return;
                _rewarded = ad;
                _rewardedReady = true;
            });
#endif
        }

        public void ShowRewarded(Action<bool> onReward)
        {
#if UNITY_ADS_ENABLE
            if (!_rewardedReady || _rewarded == null) { onReward?.Invoke(false); return; }
            _rewardedReady = false;
            bool rewarded = false;
            _rewarded.OnAdFullScreenContentClosed += () =>
            {
                _rewarded.Destroy();
                LoadRewarded();
                onReward?.Invoke(rewarded);
            };
            _rewarded.Show(_ => rewarded = true);
#else
            onReward?.Invoke(false);
#endif
        }
    }
}
