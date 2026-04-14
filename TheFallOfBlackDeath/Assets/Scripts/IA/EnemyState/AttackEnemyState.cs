using System.Collections;
using UnityEngine;


/// <summary>
/// Supports enemy decision-making by handling attack enemy state.
/// </summary>
public class AttackEnemyState : IState
{
    FSM<IAEnemyState> _fsm;

    

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackEnemyState"/> class.
    /// </summary>
    /// <param name="fsm">The fsm.</param>
    public AttackEnemyState(FSM<IAEnemyState> fsm)
    {
        _fsm = fsm;
    }

    /// <summary>
    /// Executes the on enter workflow.
    /// </summary>
    public void OnEnter()
    {
        Debug.Log("Entre a Attack Enemy");
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
        Debug.Log("Sali del Attack Enemy");
    }
}
