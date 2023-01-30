using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GooglePlayGames;
using System;
using System.Linq;
using System.Collections;
using GoogleMobileAds.Api;

public class GameOverPopup : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI timeText;
    public Image timeImprovedImage;
    public TextMeshProUGUI newBestText;
    public TextMeshProUGUI coinText;
    public GameObject adButton;
    private bool _adWatched;

    public void OnEnable()
    {
        var player = GameController.GetPlayerController();
        Time.timeScale = 0;
        var isPlayerAlive = !player.isDead;
        var isBossDead = FindObjectOfType<CarrierController>()?.GetComponent<EnemyController>().health <= 0;

        titleText.text = isPlayerAlive
            ? "VICTORY"
            : isBossDead
                ? "DRAW"
                : "DEFEAT";

        killCountText.text = GameController.instance.killCount.ToString();

        var time = (int)(Time.time - GameController.instance.timeStarted);
        timeText.text = $"{(time / 60).ToString("D2")}:{(time % 60).ToString("D2")}";

        var timeImproved = isPlayerAlive && time < PermanentProgressionManager.savedGame.bestTime;
        timeImprovedImage.gameObject.SetActive(timeImproved);
        newBestText.gameObject.SetActive(timeImproved);

        if (isPlayerAlive)
        {
            if (!PermanentProgressionManager.savedGame.beatBoss)
            {
                PermanentProgressionManager.savedGame.beatBoss = true;
            }

            PermanentProgressionManager.savedGame.trophyCount++;
            PermanentProgressionManager.SaveGameToFile();

            StartCoroutine(ReportAchievements(player));
        }

        if (timeImproved)
        {
            PermanentProgressionManager.savedGame.bestTime = time;
            PermanentProgressionManager.SaveGameToFile();
        }

        coinText.text = GameController.instance.coinCollected.ToString();

        adButton.SetActive(!_adWatched && !isPlayerAlive && !isBossDead);
    }

    private static IEnumerator ReportAchievements(PlayerController player)
    {
        GooglePlay.SafeIncrementEvent(GPGSIds.event_trophy_collected, 1);

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            yield break;
        }

        Social.ReportProgress(GPGSIds.achievement_carrier_has_departed, 100.0d, (_) => { });

        if (Enum.GetValues(typeof(Challenge)).Cast<Challenge>().All(PermanentProgressionManager.IsChallengeEnabed))
        {
            Social.ReportProgress(GPGSIds.achievement_against_all_odds, 100.0d, (_) => { });
        }

        if (!player.damageTaken)
        {
            Social.ReportProgress(GPGSIds.achievement_invincible, 100.0d, (_) => { });
        }

        yield break;
    }

    public void ReviveWithAd()
    {
        if (((AdConsentPopup.UserConsent)PlayerPrefs.GetInt(Constants.AdConsentPlayerPrefKey, 0)) == AdConsentPopup.UserConsent.Unset)
        {
            AdConsentPopup.instance.gameObject.SetActive(true);
            AdConsentPopup.instance.onChoose = ShowAd;
        }
        else
        {
            ShowAd();
        }
    }

    private void ShowAd()
    {
        AdManager.Instance.ShowRewardedAd(AdManager.AdType.RewardedVideo, RewardGained);
    }

    private void RewardGained(object sender, Reward e)
    {
        GameController.GetPlayerController().Revive();
        adButton.SetActive(false);
        _adWatched = true;
        GameController.GetPlayerMovementController().ResetPosition();
        UIController.instance.ToggleEndGamePopup(false);
        Time.timeScale = 1;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void RestartGame()
    {
        GlobalVariables.Set("RestartGame", true);

        SceneManager.LoadScene("Menu");
    }

    public void GoToUpgrade()
    {
        BackgroundMusicController.instance.SwitchToMusic(MusicType.Menu);
        GlobalVariables.Set("GoToUpgrade", true);

        SceneManager.LoadScene("Menu");
    }
}
