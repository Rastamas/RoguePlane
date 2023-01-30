using Unity.Mathematics;
using UnityEngine;

public class AutoScroll : MonoBehaviour
{
    public float maxScrollSpeed;
    private static AutoScroll _instance;

    public static AutoScroll instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AutoScroll>();
            }

            return _instance;
        }
    }

    private float _currentScrollSpeed;
    private bool _shouldStop;
    private float _startTriggeredAt;
    private float _stopTriggeredAt;
    public const float rampTimeInS = 1;

    public void Start()
    {
        _shouldStop = true;
        _currentScrollSpeed = 0;
        _startTriggeredAt = Time.time;
    }

    public void Update()
    {
        if (_shouldStop && _currentScrollSpeed > float.Epsilon)
        {
            _currentScrollSpeed = Mathf.Max(0f, maxScrollSpeed * (_stopTriggeredAt + rampTimeInS - Time.time) / rampTimeInS);
        }

        if (!_shouldStop && maxScrollSpeed != _currentScrollSpeed)
        {
            _currentScrollSpeed = Mathf.Min(maxScrollSpeed, maxScrollSpeed * (Time.time - _startTriggeredAt) / rampTimeInS);
        }

        transform.position += Vector3.forward * _currentScrollSpeed * Time.deltaTime;
    }

    public void HaltScroll()
    {
        _shouldStop = true;
        _stopTriggeredAt = Time.time;
    }

    public void StartScroll()
    {
        _shouldStop = false;
        _startTriggeredAt = Time.time;
    }
}
