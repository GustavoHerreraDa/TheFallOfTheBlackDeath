using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

/// <summary>
/// Handles world ogre for the current project workflow.
/// </summary>
public class WorldOgre : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private FollowPlayer _sj;

    public float RaycastDistance;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        _sj = GetComponent<FollowPlayer>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
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
