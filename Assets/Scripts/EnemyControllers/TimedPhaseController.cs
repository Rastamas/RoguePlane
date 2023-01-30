using System;
using System.Collections.Generic;
using UnityEngine;

public class TimedPhaseController : MonoBehaviour
{
    protected Dictionary<int, (Action action, float durationInS, Action closingAction)> _phases;
    protected int _currentPhase;
    protected float _phaseStartTime;

    protected float CurrentPhaseDuration => _phases[_currentPhase].durationInS;

    protected void Start()
    {
        _currentPhase = 0;
        _phaseStartTime = Time.time;
    }

    protected void Update()
    {
        var (currentAction, durationInS, closingAction) = _phases[_currentPhase];

        currentAction?.Invoke();

        if (durationInS <= Time.time - _phaseStartTime)
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
