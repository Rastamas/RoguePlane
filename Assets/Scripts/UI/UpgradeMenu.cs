using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using GooglePlayGames;

public class UpgradeMenu : MonoBehaviour
{
    private SaveGame _savedGame;

    [SerializeField]
    private TextMeshProUGUI coinCountDisplay;

    private const int labelIndex = 0;
    private const int minusButtonIndex = 1;
    private const int sliderIndex = 2;
    private const int plusButtonIndex = 3;
    private const int priceIndex = 4;

    private Dictionary<StatType, GameObject> _upgradeBlocks;

    public void Awake()
    {
        _savedGame = PermanentProgressionManager.savedGame;
        _savedGame.stats ??= new List<StatBlock>();
        _upgradeBlocks = new Dictionary<StatType, GameObject>();

        RefreshCoinCountDisplay();

        var upgradeBlockPrefab = Resources.Load<GameObject>("Prefabs/UI/StatUpgradeBlock");
        var height = upgradeBlockPrefab.GetComponent<RectTransform>().rect.height;
        var yPosition = Screen.height / 3f;
        var stats = EnumExtensions.GetValues<StatType>();

        foreach (var stat in stats)
        {
            var savedStat = GetSavedStat(stat);

            var upgradeBlock = Instantiate(upgradeBlockPrefab, transform.position + new Vector3(0, yPosition, 0), Quaternion.identity, gameObject.transform);

            _upgradeBlocks.Add(stat, upgradeBlock);

            if (!Application.isMobilePlatform && !Application.isEditor)
            {
                upgradeBlock.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            yPosition -= height * 1.5f * upgradeBlock.transform.localScale.y;

            upgradeBlock.transform.GetChild(labelIndex).GetComponent<TextMeshProUGUI>().text
                = stat.ToString().SplitCamelCase();

            SetupSlider(savedStat, upgradeBlock);
            SetupButtons(savedStat, upgradeBlock);
            UpdatePrice(savedStat, upgradeBlock.transform);
        }

        PermanentProgressionManager.SaveGameToFile();
    }

    private static void UpdatePrice(StatBlock savedStat, Transform upgradeBlock)
    {
        var price = GetPrice(savedStat);
        upgradeBlock.GetChild(priceIndex).GetComponent<TextMeshProUGUI>().text = price > 10 ? "-" : price.ToString();
    }

    private void RefreshCoinCountDisplay()
    {
        coinCountDisplay.text = $"{_savedGame.coinCount}";
    }

    private StatBlock GetSavedStat(StatType stat)
    {
        var savedStat = _savedGame.stats.FirstOrDefault(s => s.type == stat);

        if (savedStat == null)
        {
            savedStat ??= StatBlock.Empty(stat);

            _savedGame.stats.Add(savedStat);
        }

        return savedStat;
    }

    private void SetupButtons(StatBlock savedStat, GameObject upgradeBlock)
    {
        var minusButton = upgradeBlock.transform.GetChild(minusButtonIndex).GetComponent<Button>();
        minusButton.onClick.AddListener(delegate { UpgradeStat(false, savedStat); });

        var plusButton = upgradeBlock.transform.GetChild(plusButtonIndex).GetComponent<Button>();
        plusButton.onClick.AddListener(delegate { UpgradeStat(true, savedStat); });
    }

    private static void SetupSlider(StatBlock savedStat, GameObject upgradeBlock)
    {
        var sliderTransform = upgradeBlock.transform.GetChild(sliderIndex);
        var boundaries = StatBlockBoundary.GetBoundary(savedStat.type);

        var slider = sliderTransform.GetComponent<Slider>();
        slider.minValue = boundaries.min * 10f;
        slider.maxValue = boundaries.max * 10f;
        slider.value = savedStat.value * 10f;

        TurnOnDots(savedStat, sliderTransform);
    }

    public void UpgradeStat(bool increase, StatBlock statBlock)
    {
        var price = GetPrice(statBlock);

        if (increase && _savedGame.coinCount - price < 0)
        {
            return;
        }

        var boundaries = StatBlockBoundary.GetBoundary(statBlock.type);
        var statBlockContainer = EventSystem.current.currentSelectedGameObject.transform.parent;

        var newValue = statBlock.value + (boundaries.increment * (increase
            ? 1
            : -1));

        if (newValue > boundaries.max || newValue < boundaries.min)
        {
            return;
        }

        statBlock.value = newValue;

        var slider = statBlockContainer.GetChild(sliderIndex);
        slider.GetComponent<Slider>().value = newValue * 10f;

        TurnOnDots(statBlock, slider);

        UpdateCoinCount(increase, price);
        UpdatePrice(statBlock, statBlockContainer);

        if (increase)
        {
            GooglePlay.SafeIncrementEvent(GPGSIds.event_upgrade_purchased, (uint)price);
        }
        else
        {
            GooglePlay.SafeIncrementEvent(GPGSIds.event_upgrade_refunded, (uint)price - 1);
        }
    }

    private static void TurnOnDots(StatBlock statBlock, Transform slider)
    {
        var onDots = slider.GetChild(slider.childCount - 1);

        var numberOfOnDots = GetPrice(statBlock) - 1;

        for (int i = 0; i < onDots.childCount; i++)
        {
            var dot = onDots.GetChild(i);

            dot.gameObject.SetActive(i <= numberOfOnDots);
        }
    }

    private static int GetPrice(StatBlock statBlock)
    {
        var boundaries = StatBlockBoundary.GetBoundary(statBlock.type);
        return (statBlock.value - boundaries.min) / boundaries.increment + 1;
    }

    private void UpdateCoinCount(bool increase, int price)
    {
        if (increase)
        {
            _savedGame.coinCount -= price;
        }
        else
        {
            _savedGame.coinCount += price - 1;
        }

        RefreshCoinCountDisplay();
    }
}
