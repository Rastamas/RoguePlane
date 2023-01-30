using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShieldGeneratorController : MonoBehaviour, IEnemyDestroyed
{
    private Vector3 _spawnPoint;
    private Vector3 _targetPoint;
    private Vector3 _controlPoint;

    private const float flightDuration = 3;
    private float _flightStartTime;
    private Transform _powerChannel;

    public void Awake()
    {
        _spawnPoint = transform.position;
        _powerChannel = transform.GetChild(0);
    }

    public void FlyTo(Vector3 worldPoint)
    {
        // Workaround for shield generator model being lower than its center
        worldPoint.y += 1f;

        _targetPoint = worldPoint;
        _controlPoint = _spawnPoint + (_targetPoint - _spawnPoint) / 2;
        _controlPoint.y -= 10;
        _flightStartTime = Time.time;
    }

    public void Update()
    {
        var time = Mathf.Min((Time.time - _flightStartTime) / flightDuration, 1f);
        transform.position = Bezier.GetQuadraticBezierPoint(time, _spawnPoint, _controlPoint, _targetPoint);

        _powerChannel.LookAt(transform.parent);
    }

    public void OnEnemyDestroyed()
    {
        transform.parent.GetComponent<CarrierController>().RemoveGenerator(gameObject);
    }
}
