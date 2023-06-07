using System.Collections;
using UnityEngine;


public class AttackEnemyState : IState
{
    FSM<IAEnemyState> _fsm;

    

    public AttackEnemyState(FSM<IAEnemyState> fsm)
    {
        _fsm = fsm;
    }

    public void OnEnter()
    {
        Debug.Log("Entre a Attack Enemy");
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
        Debug.Log("Sali del Attack Enemy");
    }
}