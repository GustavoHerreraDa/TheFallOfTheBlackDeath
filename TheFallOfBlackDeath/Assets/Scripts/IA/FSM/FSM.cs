using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports enemy decision-making by handling fsm.
/// </summary>
public class FSM<T>
{
    IState _currentState;

    Dictionary<T, IState> _allStates = new Dictionary<T, IState>();

    /// <summary>
    /// Adds the state.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void AddState(T key, IState value)
    {
        if (!_allStates.ContainsKey(key)) _allStates.Add(key, value);
        else _allStates[key] = value;
    }

    /// <summary>
    /// Changes the state.
    /// </summary>
    /// <param name="nextState">The next state.</param>
    public void ChangeState(T nextState)
    {
        if (_currentState != null) _currentState.OnExit();
        _currentState = _allStates[nextState];
        _currentState.OnEnter();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    public void Update()
    {
        _currentState.OnUpdate();
    }

    /// <summary>
    /// Applies physics-related updates on the fixed timestep.
    /// </summary>
    public void FixedUpdate()
    {
        _currentState.OnFixedUpdate();
    }
}
