using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IA
{
    /// <summary>
    /// Supports enemy decision-making by handling ia enemy fsm.
    /// </summary>
    public class IAEnemyFSM : MonoBehaviour
    {
        private IAEnemyState currentState;
        FSM<IAEnemyState> _FSM;

        /// <summary>
        /// Initializes cached references and runtime state before the component starts running.
        /// </summary>
        void Awake()
        {
            _FSM = new FSM<IAEnemyState>();

            IState idle = new IdleEnemyState(_FSM);
            _FSM.AddState(IAEnemyState.Idle, idle);

            //IState patrol = new PatrolAgentState(_FSM, this, _allWaypoints);
            //_FSM.AddState(IAEnemyState.Patrol, new PatrolAgentState(_FSM, this, _allWaypoints));

            _FSM.ChangeState(IAEnemyState.Idle);
        }

        /// <summary>
        /// Updates the component each frame while it is active.
        /// </summary>
        void Update()
        {
            _FSM.Update();
        }
    }
}
