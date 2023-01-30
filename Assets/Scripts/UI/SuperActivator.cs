using System;
using UnityEngine;
using UnityEngine.UI;

public class SuperActivator : MonoBehaviour
{
    private PlayerController _playerController;
    private Image _buttonImage;
    private float _fill;
    private bool _pulseIncreasing;
    private const float maxScale = 1.2f;
    private const float superMaxDurationInS = 4f;
    private const float autoFillBaseTimeInS = 80;

    public void Awake()
    {
        _buttonImage = GetComponent<Image>();
    }

    public void Start()
    {
        _playerController = GameController.GetPlayerController();
    }

    public void Update()
    {
        if (_fill == 1f)
        {
            PulseSize();
        }

        if (_playerController.missiles.isEmitting)
        {
            DrainFill();
        }
        else
        {
            SlowlyIncreaseFill();
        }
    }

    private void SlowlyIncreaseFill()
    {
        var increaseAmount = Time.deltaTime / autoFillBaseTimeInS * GetMultiplier();

        IncreaseFill(increaseAmount);
    }

    private int GetMultiplier()
    {
        var timeWithoutBeingHit = Math.Min(32f, Time.time - _playerController.lastHitTime);

        var multiplier = (int)Math.Log(timeWithoutBeingHit, 2) - 1;

        return Math.Max(0, multiplier);
    }

    private void PulseSize()
    {
        transform.localScale *= 1 + Time.deltaTime * (maxScale - 1) * (_pulseIncreasing ? 1 : -1);

        _pulseIncreasing = _pulseIncreasing
            ? transform.localScale.x <= maxScale
            : transform.localScale.x < 1f;
    }

    public void IncreaseFill(float amount)
    {
        _fill = Mathf.Min(1f, _fill + amount);

        _buttonImage.fillAmount = _fill;
    }

    public void DrainFill()
    {
        _fill = Math.Max(0f, _fill - Time.deltaTime / superMaxDurationInS);

        _buttonImage.fillAmount = _fill;

        if (_fill == 0f)
        {
            _playerController.ToggleSuper(forceOff: true);
        }
    }

    public void TogglerSuper()
    {
        if (_fill < 1f && !_playerController.missiles.isEmitting)
        {
            return;
        }

        _playerController.ToggleSuper();
    }
}
