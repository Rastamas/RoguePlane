using System;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    public float moveSpeed;
    public bool resetInProgress;
    private GameController _gameController;
    private Collider _movementCollider;
    private Vector3 _movementOffset;
    private Vector3 _preResetPosition;
    private float _resetTriggerTime;
    private Quaternion _startingRotation;
    private GameObject _powerupPanel;
    private bool _enabled;
    private const float _resetDurationInS = 1;
    private const float _tiltStrength = 7.5f;
    private const float _tiltBalanceStrength = 0.2f;

    public void Start()
    {
        _movementCollider = EnemySpawner.instance.gameObject.GetComponent<Collider>();
        _gameController = GameController.instance;
        _startingRotation = transform.rotation;
        _powerupPanel = UIController.instance.powerupPanel.gameObject;
        _enabled = true;

        moveSpeed *= PermanentProgressionManager.savedGame.GetStat(StatType.MoveSpeed);
    }

    public void FixedUpdate()
    {
        if (Time.timeScale == 0 || _powerupPanel.activeInHierarchy)
        {
            if (!ScreenUtil.TouchingScreen)
            {
                _movementOffset = Vector3.zero;
            }

            BalanceTilt();
            return;
        }

        UpdatePosition();
    }

    public void ResetPosition()
    {
        gameObject.layer = Constants.ImmuneLayer;
        GameController.GetPlayerController().SetImmunity();
        _movementOffset = Vector3.zero;
        _resetTriggerTime = Time.time;
        _preResetPosition = transform.position;
        resetInProgress = true;
    }

    private Vector3 InitialPosition => _gameController.transform.position + _gameController.playerSpawnOffset;

    public bool IsInStartingPosition()
    {
        return (transform.position - InitialPosition).magnitude < .1f;
    }

    private void UpdateTilt(Vector3 newPosition)
    {
        var positionChange = newPosition - transform.position;

        if (positionChange.magnitude > float.Epsilon)
        {
            var tiltAngle = positionChange.x * _tiltStrength;

            transform.Rotate(Vector3.back, tiltAngle);
        }
    }

    private void BalanceTilt()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _startingRotation, _tiltBalanceStrength);
    }

    private void UpdatePosition()
    {
        if (resetInProgress)
        {
            ChangePositionBasedOnForcedMovement();
            return;
        }

        if (ScreenUtil.TouchingScreen)
        {
            ChangePositionBasedOnInput();
        }
        else
        {
            _movementOffset = Vector3.zero;
        }

        BalanceTilt();
    }

    internal void DisableMovement()
    {
        _enabled = false;
    }

    private void ChangePositionBasedOnForcedMovement()
    {
        var newPosition = Vector3.Slerp(_preResetPosition, InitialPosition, (Time.time - _resetTriggerTime) / _resetDurationInS);

        UpdateTilt(newPosition);

        transform.position = newPosition;

        if (IsInStartingPosition())
        {
            gameObject.layer = Constants.PlayerLayer;
            resetInProgress = false;
            _enabled = true;
            GameController.GetPlayerController().RemoveImmunity();
        }
    }

    private void ChangePositionBasedOnInput()
    {
        if (!_enabled)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(ScreenUtil.TouchedPoint);

        if (!_movementCollider.Raycast(ray, out var touch, 100))
        {
            return;
        }

        if (_movementOffset == Vector3.zero)
        {
            _movementOffset = transform.position - touch.point;
        }

        var bottomLeftBound = EnemySpawner.instance.GetCorner(Corner.BottomLeft);
        var topRightBound = EnemySpawner.instance.GetCorner(Corner.TopRight);

        var desiredPosition = touch.point + _movementOffset;
        var newPosition = Vector3.Lerp(transform.position, desiredPosition, moveSpeed / 100);

        newPosition.x = Math.Min(Math.Max(newPosition.x, bottomLeftBound.x), topRightBound.x);
        newPosition.z = Math.Min(Math.Max(newPosition.z, bottomLeftBound.z), topRightBound.z);

        UpdateTilt(newPosition);

        transform.position = newPosition;
    }
}
