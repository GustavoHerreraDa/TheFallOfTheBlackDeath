using System;
using UnityEngine;

/// Single-piece vertical sliding door (moves up/down like a shutter)
public class VerticalSlidingDoor : MonoBehaviour
{
    [Header("Door Reference")]
    [SerializeField] private Transform door;

    [Header("Settings")]
    [SerializeField] private float openHeight = 2f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private bool open = false;

    private Vector3 closedPos;
    private Vector3 openPos;

    private void Awake()
    {
        closedPos = door.localPosition;
        openPos = closedPos + Vector3.up * openHeight;
    }

    private void Update()
    {
        Vector3 target = open ? openPos : closedPos;
        door.localPosition = Vector3.Lerp(door.localPosition, target, Time.deltaTime * speed);
    }

    public void Toggle()
    {
        open = !open;
    }

    public void Open()
    {
        open = true;
    }

    public void Close()
    {
        open = false;
    }
}