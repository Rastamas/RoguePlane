using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerupPopupController : MonoBehaviour
{
    public GameObject rerollButton;
    private GameObject _powerupOptionPrefab;
    private List<GameObject> _powerupOptions = new List<GameObject>();

    // Workaround for Admob X TMP bug
    // https://answers.unity.com/questions/1689879/admoob-rewardbasedvideoad-app-crash-after-reward-v.html
    private bool _rerollAds;

    public void OnEnable()
    {
        // Adservice may be prewarming at this point
        var isFirstPowerup = RewardController.instance.chosenPowerups.Count == 0;

        rerollButton.SetActive(!isFirstPowerup);
        Time.timeScale = 0;
    }

    public void Awake()
    {
        _powerupOptionPrefab = transform.GetChild(0).gameObject;
    }

    public void Update()
    {
        if (_rerollAds)
        {
            RemoveExistingPowerups();
            RewardController.instance.SpawnPowerups();
            rerollButton.SetActive(false);
            _rerollAds = false;
        }
    }

    public void AddPowerupOptions(List<PowerUpType> powerups)
    {
        _powerupOptionPrefab.SetActive(true);

        foreach (var powerup in powerups)
        {
            var option = Instantiate(_powerupOptionPrefab, _powerupOptionPrefab.transform.position, _powerupOptionPrefab.transform.rotation, parent: transform);

            option.GetComponent<PowerupOption>().Setup(powerup);

            _powerupOptions.Add(option);
        }

        _powerupOptionPrefab.SetActive(false);
    }

    public void Clear()
    {
        UIController.instance.powerupPanel.gameObject.SetActive(false);

        RemoveExistingPowerups();

        GameController.instance.StartNewEncounter();
    }

    private void RemoveExistingPowerups()
    {
        foreach (var option in _powerupOptions)
        {
            Destroy(option);
        }

        _powerupOptions = new List<GameObject>();
    }


    public void RerollWithAd()
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
        AdManager.Instance.ShowAd(AdManager.AdType.Interstitial, InterstitialClosed);
    }

    private void InterstitialClosed(object sender, EventArgs e)
    {
        _rerollAds = true;
    }
}
