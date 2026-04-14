using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the contract used by i state.
/// </summary>
public interface IState
{
    void OnEnter();

    void OnUpdate();

    void OnFixedUpdate();

    void OnExit();
}
