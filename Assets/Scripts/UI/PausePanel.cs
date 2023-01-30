using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    public Button _continueButton;
    public Button _exitButton;
    private Volume _volume;
    private DepthOfField _depthOfField;
    private bool _showButtons;
    private static PausePanel _instance;

    public static PausePanel instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PausePanel>();
            }

            return _instance;
        }
    }

    public event EventHandler<bool> OnPauseChanged;

    public void Awake()
    {
        _volume = FindObjectOfType<Volume>();
        _volume.sharedProfile.TryGet(out _depthOfField);
    }

    public void ExitGame()
    {
        BackgroundMusicController.instance.SwitchToMusic(MusicType.Menu);
        SceneManager.LoadScene("Menu");
    }

    public void TogglePause()
    {
        if (UIController.instance.endGamePanel.isActiveAndEnabled)
        {
            return;
        }

        var timeIsRunning = Time.timeScale != 0;

        Time.timeScale = timeIsRunning || UIController.instance.powerupPanel.isActiveAndEnabled
            ? 0
            : 1;

        OnPauseChanged?.Invoke(this, timeIsRunning);

        _showButtons = !_showButtons;

        _continueButton.gameObject.SetActive(_showButtons);
        _exitButton.gameObject.SetActive(_showButtons);

        if (timeIsRunning)
        {
            GameController.GetPlayer().layer = Constants.TerrainLayer;
            foreach (var enemy in EnemySpawner.instance.activeEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                enemy.layer = Constants.TerrainLayer;
            }

            _depthOfField.gaussianStart.Override(0);
            _depthOfField.gaussianEnd.Override(75);
        }
        else
        {
            GameController.GetPlayer().layer = Constants.PlayerLayer;
            foreach (var enemy in EnemySpawner.instance.activeEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                enemy.layer = Constants.EnemiesLayer;
            }

            _depthOfField.gaussianStart.Override(75);
            _depthOfField.gaussianEnd.Override(2000);
        }
    }
}
