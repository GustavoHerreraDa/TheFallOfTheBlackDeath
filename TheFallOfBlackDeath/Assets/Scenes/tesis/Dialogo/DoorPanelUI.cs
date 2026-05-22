using UnityEngine;

public class DoorPanelUI : MonoBehaviour
{
    public void OpenCurrentDoor()
    {
        PanelTriggerHorizontal.Current?.OpenDoor();
    }

    public void DismissCurrentPanel()
    {
        PanelTriggerHorizontal.Current?.DismissPanel();
    }
}