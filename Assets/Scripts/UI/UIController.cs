using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private static UIController _instance;

    public static UIController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIController>();
            }

            return _instance;
        }
    }
    private Slider _healthBar;
    public ProgressBar progressBar;
    public GameOverPopup endGamePanel;
    public PausePanel pausePanel;
    public PowerupPanel powerupPanel;
    public SuperActivator superActivator;
    public Canvas canvas;

    public void Awake()
    {
        _healthBar = GetComponentsInChildren<Slider>().First(t => t.enabled && t.name.ToLowerInvariant() == "HealthBar".ToLowerInvariant());
        progressBar = GetComponentInChildren<ProgressBar>();
    }

    public void ChangeHealth(float ratio)
    {
        _healthBar.value = ratio;
    }

    public void ShowEndGamePopupInSeconds(float seconds = .2f)
    {
        Invoke(nameof(ShowEndGamePopup), seconds);
    }

    private void ShowEndGamePopup()
    {
        ToggleEndGamePopup(true);
    }

    public void ToggleEndGamePopup(bool isVisible)
    {
        _healthBar.gameObject.SetActive(!isVisible);
        progressBar.gameObject.SetActive(!isVisible);
        pausePanel.gameObject.SetActive(!isVisible);
        superActivator.gameObject.SetActive(!isVisible);

        endGamePanel.transform.parent.gameObject.SetActive(isVisible);
    }
}
