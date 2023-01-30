using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using GooglePlayGames;

public class ChallengeMenu : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI trophyCountDisplay;

    private List<Toggle> _toggles;

    private const int containerIndex = 2;
    private const int blockIndex = 0;
    private const int challengeNameIndex = 0;
    private const int challengeDescriptionIndex = 1;

    private static readonly Dictionary<Challenge, string> challengeDescriptions = new Dictionary<Challenge, string>() {
        { Challenge.Beefy, "Enemies have double health"},
        { Challenge.Deadly, "Enemies deal double damage"},
        { Challenge.Explosive, "Enemies spawn explosive bombs on death"},
        { Challenge.Baseline, "Your permanent upgrades are not applied"},
    };

    public void Awake()
    {
        _toggles = new List<Toggle>();
        RefreshTrophyCountDisplay();
        var allChallenges = Enum.GetValues(typeof(Challenge)).Cast<Challenge>();
        var blockContainer = transform.GetChild(containerIndex);
        var challengePrefab = blockContainer.GetChild(blockIndex).gameObject;
        var height = challengePrefab.GetComponent<RectTransform>().rect.height;

        var yPosition = Screen.height / 3f;
        foreach (var challenge in allChallenges)
        {
            var block = Instantiate(challengePrefab, transform.position + new Vector3(0, yPosition, 0), Quaternion.identity, blockContainer);

            yPosition -= height * 1.25f * challengePrefab.transform.localScale.y;
            block.transform.GetChild(challengeNameIndex).GetComponent<TextMeshProUGUI>().text = challenge.ToString();

            block.transform.GetChild(challengeDescriptionIndex).GetComponent<TextMeshProUGUI>().text =
                challengeDescriptions[challenge];

            var toggle = block.GetComponentInChildren<Toggle>();

            toggle.isOn = PermanentProgressionManager.savedGame.challengesActive.Contains(challenge);
            toggle.interactable = toggle.isOn || PermanentProgressionManager.savedGame.trophyCount > 0;
            toggle.onValueChanged.AddListener((value) => UpdateChallenge(value, challenge));

            _toggles.Add(toggle);
        }

        challengePrefab.SetActive(false);
    }

    private void UpdateChallenge(bool value, Challenge challenge)
    {
        if (value)
        {
            PermanentProgressionManager.savedGame.trophyCount--;
            PermanentProgressionManager.savedGame.challengesActive.Add(challenge);

            if (PermanentProgressionManager.savedGame.trophyCount == 0)
            {
                _toggles.ForEach(toggle => toggle.interactable = toggle.isOn);
            }

            GooglePlay.SafeIncrementEvent(GPGSIds.event_challenge_activated, 1);
        }
        else
        {
            PermanentProgressionManager.savedGame.trophyCount++;
            PermanentProgressionManager.savedGame.challengesActive.Remove(challenge);

            _toggles.ForEach(toggle => toggle.interactable = true);
        }

        PermanentProgressionManager.SaveGameToFile();
        RefreshTrophyCountDisplay();
    }

    private void RefreshTrophyCountDisplay()
    {
        trophyCountDisplay.text = $"{PermanentProgressionManager.savedGame.trophyCount}";
    }
}
