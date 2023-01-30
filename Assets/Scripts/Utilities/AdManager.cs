using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdManager
{
    // #if UNITY_ANDROID
    // Test ad unit ID: ca-app-pub-3940256099942544/3419835294
    private const string AD_UNIT_ID_INTERSTITIAL = "ca-app-pub-6916535752895902/6200939433";
    private const string AD_UNIT_ID_REWARDED = "ca-app-pub-6916535752895902/2789764307";
    // #elif UNITY_IOS
    //     // Test ad unit ID: ca-app-pub-3940256099942544/5662855259
    //     private const string AD_UNIT_ID = "<YOUR_IOS_APPOPEN_AD_UNIT_ID>";
    // #else
    //     private const string AD_UNIT_ID = "unexpected_platform";
    // #endif

    private static AdManager instance;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private EventHandler<EventArgs> latestAdhandler;
    private EventHandler<Reward> latestRewardHandler;

    public static AdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AdManager();
            }

            return instance;
        }
    }

    public void Initialize()
    {
        LoadNewInterstitial();
        LoadNewRewarded();
    }

    private static void LoadNewRewarded()
    {
        instance.rewardedAd = new RewardedAd(AD_UNIT_ID_REWARDED);
        instance.rewardedAd.LoadAd(new AdRequest.Builder().Build());
        instance.rewardedAd.OnAdClosed += HandleRewardedClosed;
    }

    private static void HandleRewardedClosed(object sender, EventArgs e)
    {
        LoadNewRewarded();
    }

    private static void LoadNewInterstitial()
    {
        instance.interstitialAd = new InterstitialAd(AD_UNIT_ID_INTERSTITIAL);
        instance.interstitialAd.LoadAd(new AdRequest.Builder().Build());
        instance.interstitialAd.OnAdClosed += HandleInterstitialClosed;
    }

    private static void HandleInterstitialClosed(object sender, EventArgs e)
    {
        LoadNewInterstitial();
    }

    private bool IsAdAvailable(AdType type)
    {
        return type switch
        {
            AdType.Interstitial => interstitialAd != null && interstitialAd.IsLoaded(),
            AdType.RewardedVideo => rewardedAd != null && rewardedAd.IsLoaded(),
            _ => throw new NotImplementedException("Unexpected ad type: " + type)
        };
    }

    public void ShowAd(AdType type, EventHandler<EventArgs> onAdCompleted)
    {
        if (!IsAdAvailable(type))
        {
            return;
        }

        if (type == AdType.Interstitial)
        {
            if (latestAdhandler != null)
            {
                interstitialAd.OnAdClosed -= latestAdhandler;
            }

            interstitialAd.OnAdClosed += onAdCompleted;
            interstitialAd.Show();
        }
        else
        {
            throw new NotImplementedException("Unexpected ad type: " + type);
        }

        latestAdhandler = onAdCompleted;
    }

    public void ShowRewardedAd(AdType type, EventHandler<Reward> onRewardGained)
    {
        if (!IsAdAvailable(type))
        {
            return;
        }

        if (type == AdType.RewardedVideo)
        {
            if (latestAdhandler != null)
            {
                rewardedAd.OnUserEarnedReward -= onRewardGained;
            }

            rewardedAd.OnUserEarnedReward += onRewardGained;
            rewardedAd.Show();
        }
        else
        {
            throw new NotImplementedException("Unexpected ad type: " + type);
        }

        latestRewardHandler = onRewardGained;
    }


    public enum AdType
    {
        Interstitial,
        RewardedVideo
    }
}