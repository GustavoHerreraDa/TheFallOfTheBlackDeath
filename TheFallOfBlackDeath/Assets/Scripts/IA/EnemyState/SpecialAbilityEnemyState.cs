using System.Collections;
using UnityEngine;


/// <summary>
/// Supports enemy decision-making by handling special ability enemy state.
/// </summary>
public class SpecialAbilityEnemyState : IState
{
    FSM<IAEnemyState> _fsm;

    float _ticksToPatrol;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialAbilityEnemyState"/> class.
    /// </summary>
    /// <param name="fsm">The fsm.</param>
    public SpecialAbilityEnemyState(FSM<IAEnemyState> fsm)
    {
        _fsm = fsm;
    }

    /// <summary>
    /// Executes the on enter workflow.
    /// </summary>
    public void OnEnter()
    {
        _ticksToPatrol = 0;

        Debug.Log("Entre a Special Ability");
    }

    /// <summary>
    /// Executes the on update workflow.
    /// </summary>
    public void OnUpdate()
    {
        //_ticksToPatrol += Time.deltaTime;

        //if (_ticksToPatrol >= 3)
        //{
        //    _fsm.ChangeState(IAEnemyState.Patrol);
        //}
    }

    /// <summary>
    /// Executes the on fixed update workflow.
    /// </summary>
    public void OnFixedUpdate()
    {
    }

    /// <summary>
    /// Executes the on exit workflow.
    /// </summary>
    public void OnExit()
    {
        Debug.Log("Sali del Idle");
    }
}
