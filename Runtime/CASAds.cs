using CAS;
using MirraGames.SDK.Common;
using System;

namespace MirraGames.SDK.CAS
{
    [Provider(typeof(IAds))]
    public class CASAds : CommonAds
    {
        private IMediationManager mediationManager;
        private IAdView bannerView;

        private Action OnInterstitialOpen;
        private Action<bool> OnInterstitialClose;

        private Action OnRewardedOpen;
        private Action<bool> OnRewardedClose;
        private bool IsRewardedSuccess = false;

        public CASAds(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            // Configure MobileAds.settings before initialize.
            mediationManager = GetAdManager();
            bannerView = mediationManager.GetAdView(AdSize.Banner);
            // Banner callbacks.
            bannerView.OnLoaded += OnBannerAdLoaded;
            bannerView.OnFailed += OnBannerAdFailed;
            bannerView.OnClicked += OnBannerAdClicked;
            // Interstitial callbacks.
            mediationManager.OnInterstitialAdLoaded += OnInterstitialAdLoaded;
            mediationManager.OnInterstitialAdFailedToLoad += OnInterstitialAdFailedToLoad;
            mediationManager.OnInterstitialAdShown += OnInterstitialAdShown;
            mediationManager.OnInterstitialAdFailedToShow += OnInterstitialAdFailedToShow;
            mediationManager.OnInterstitialAdClicked += OnInterstitialAdClicked;
            mediationManager.OnInterstitialAdClosed += OnInterstitialAdClosed;
            // Rewarded callbacks.
            mediationManager.OnRewardedAdCompleted += OnRewardedAdCompleted;
            mediationManager.OnRewardedAdLoaded += OnRewardedAdLoaded;
            mediationManager.OnRewardedAdFailedToLoad += OnRewardedAdFailedToLoad;
            mediationManager.OnRewardedAdShown += OnRewardedAdShown;
            mediationManager.OnRewardedAdFailedToShow += OnRewardedAdFailedToShow;
            mediationManager.OnRewardedAdClicked += OnRewardedAdClicked;
            mediationManager.OnRewardedAdClosed += OnRewardedAdClosed;
            // Preload ads.
            mediationManager.LoadAd(AdType.Interstitial);
            mediationManager.LoadAd(AdType.Rewarded);
        }

        private IMediationManager GetAdManager()
        {
            // Configure MobileAds.settings before initialize
            return MobileAds.BuildManager()
               // Optional initialize listener
               .WithCompletionListener((config) => {
                   // The CAS SDK initializes if the error is `null`
                   string initErrorOrNull = config.error;
                   string userCountryISO2OrNull = config.countryCode;
                   IMediationManager manager = config.manager;

                   // True if the user is protected by GDPR or other regulations
                   bool protectionApplied = config.isConsentRequired;

                   // The user completes the consent flow
                   ConsentFlow.Status consentFlowStatus = config.consentFlowStatus;
               })
               .Build();
        }

        protected override void InvokeBannerImpl()
        {
            if (bannerView.isReady == true)
            {
                bannerView.SetActive(true);
            }
        }

        protected override void DisableBannerImpl()
        {
            bannerView.SetActive(false);
        }

        protected override void RefreshBannerImpl()
        {
            Logger.NotImplementedWarning(this, nameof(RefreshBannerImpl));
        }

        protected override void InvokeInterstitialImpl(InterstitialParameters parameters, Action onOpen, Action<bool> onClose)
        {
            OnInterstitialOpen = onOpen;
            OnInterstitialClose = onClose;
            if (mediationManager.IsReadyAd(AdType.Interstitial))
            {
                mediationManager.ShowAd(AdType.Interstitial);
            }
        }

        protected override void InvokeRewardedImpl(RewardedParameters parameters, Action onOpen, Action<bool> onClose)
        {
            IsRewardedSuccess = false;
            OnRewardedOpen = onOpen;
            OnRewardedClose = onClose;
            if (mediationManager.IsReadyAd(AdType.Rewarded))
            {
                mediationManager.ShowAd(AdType.Rewarded);
            }
        }

        // Banner callbacks.

        /// <summary>
        /// Called when the ad loaded and ready to present.
        /// </summary>
        private void OnBannerAdLoaded(IAdView view)
        {
            Logger.CreateText(this, "Banner ad loaded.");
        }

        /// <summary>
        /// Called when an error occurred with the ad.
        /// </summary>
        private void OnBannerAdFailed(IAdView view, AdError error)
        {
            Logger.CreateText(this, "Banner ad failed to load.");
        }

        /// <summary>
        /// Called when the user clicks on the ad.
        /// </summary>
        private void OnBannerAdClicked(IAdView view)
        {
            Logger.CreateText(this, "Banner ad clicked.");
        }

        // Interstitial callbacks.

        private void OnInterstitialAdLoaded()
        {
            Logger.CreateText(this, "Interstitial ad loaded.");
        }

        private void OnInterstitialAdFailedToLoad(AdError error)
        {
            Logger.CreateText(this, $"Interstitial failed to load an ad with error: {error.GetMessage()}.");
        }

        private void OnInterstitialAdShown()
        {
            Logger.CreateText(this, "Interstitial ad full screen content opened.");
            OnInterstitialOpen();
            IsInterstitialVisible = true;
        }

        private void OnInterstitialAdFailedToShow(string error)
        {
            Logger.CreateText(this, $"Interstitial ad failed to open full screen content: {error}.");
            OnInterstitialClose(false);
            IsInterstitialVisible = false;
        }

        private void OnInterstitialAdClicked()
        {
            Logger.CreateText(this, "Interstitial ad was clicked.");
        }

        private void OnInterstitialAdClosed()
        {
            Logger.CreateText(this, "Interstitial ad full screen content closed.");
            OnInterstitialClose(true);
            IsInterstitialVisible = false;
        }

        // Rewarded callbacks.

        private void OnRewardedAdCompleted()
        {
            Logger.CreateText(this, "The user earned the reward.");
            IsRewardedSuccess = true;
            IsRewardedVisible = false;
        }

        private void OnRewardedAdLoaded()
        {
            Logger.CreateText(this, "Rewarded ad loaded.");
        }

        private void OnRewardedAdFailedToLoad(AdError error)
        {
            Logger.CreateText(this, $"Rewarded failed to load an ad with error: {error.GetMessage()}.");
        }

        private void OnRewardedAdShown()
        {
            Logger.CreateText(this, "Rewarded ad full screen content opened.");
            OnRewardedOpen();
            IsRewardedVisible = true;
        }

        private void OnRewardedAdFailedToShow(string error)
        {
            Logger.CreateText(this, $"Rewarded ad failed to open full screen content: {error}.");
            OnRewardedClose(false);
            IsRewardedVisible = false;
        }

        private void OnRewardedAdClicked()
        {
            Logger.CreateText(this, "Rewarded ad was clicked.");
        }

        private void OnRewardedAdClosed()
        {
            Logger.CreateText(this, "Rewarded ad full screen content closed.");
            OnRewardedClose(IsRewardedSuccess);
            IsRewardedVisible = false;
        }
    }
}