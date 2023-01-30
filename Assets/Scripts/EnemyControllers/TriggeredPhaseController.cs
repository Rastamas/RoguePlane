using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggeredPhaseController : MonoBehaviour
{
    protected Dictionary<int, (Action action, Func<bool> checkAction, Action closingAction)> _phases;
    protected int _currentPhase;
    protected float _phaseStartTime;
    protected bool _disablePhases;

    protected void Start()
    {
        _currentPhase = 0;
        _phaseStartTime = Time.time;
    }

    protected void Update()
    {
        if (_disablePhases)
        {
            return;
        }

        var (currentAction, checkAction, closingAction) = _phases[_currentPhase];

        currentAction?.Invoke();

        if (checkAction?.Invoke() ?? false)
        {
            if (!_phases.ContainsKey(++_currentPhase))
            {
                _currentPhase = 1;
            }

            _phaseStartTime = Time.time;

            closingAction?.Invoke();
        }
    }
}
