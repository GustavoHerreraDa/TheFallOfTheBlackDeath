using System;
using UnityEngine;

/// Attach this to an empty GameObject (manager).
/// Assign exactly 3 VerticalSlidingDoor instances.
/// Only ONE door can EVER be opened.
/// Once one is opened, others are permanently blocked.
public class HorizontalDoorGroupController : MonoBehaviour
{
    [SerializeField] private HorizontalSlidingDoor[] doors = new HorizontalSlidingDoor[3];

    private int openedDoorIndex = -1;

    public void TryOpenDoor(int index)
    {
        if (!IsValid(index)) return;

        // If no door has been opened yet → allow opening
        if (openedDoorIndex == -1)
        {
            doors[index].Open();
            openedDoorIndex = index;
            return;
        }

        // If it's the same door, do nothing (no toggle, no close)
        if (openedDoorIndex == index)
        {
            return;
        }

        // Any other door → completely blocked
    }

    private bool IsValid(int index)
    {
        return doors != null && index >= 0 && index < doors.Length && doors[index] != null;
    }
}