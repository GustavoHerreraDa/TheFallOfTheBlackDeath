using System.Collections;
using UnityEngine;


public class HealEnemyState : IState
{
    FSM<IAEnemyState> _fsm;

    float _ticksToPatrol;

    public HealEnemyState(FSM<IAEnemyState> fsm)
    {
        _fsm = fsm;
    }

    public void OnEnter()
    {
        _ticksToPatrol = 0;

        Debug.Log("Entre a Idle");
    }

    public void OnUpdate()
    {
        //_ticksToPatrol += Time.deltaTime;

        //if (_ticksToPatrol >= 3)
        //{
        //    _fsm.ChangeState(IAEnemyState.Patrol);
        //}
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        Debug.Log("Sali del Idle");
    }
}
