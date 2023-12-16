using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class WorldOgre : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private SeguirJugador _sj;

    public float RaycastDistance;

    void Start()
    {
        _sj = GetComponent<SeguirJugador>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(Physics.Raycast(transform.position, Vector3.down, RaycastDistance))
        {
            _animator.SetBool("HasLanded", true);
            _navMeshAgent.enabled = true;
            _sj.enabled = true;
        }
    }
}
