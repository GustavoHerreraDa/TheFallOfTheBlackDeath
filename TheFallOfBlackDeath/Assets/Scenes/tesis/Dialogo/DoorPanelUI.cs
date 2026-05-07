using UnityEngine;

public class DoorPanelUI : MonoBehaviour
{
    public void OpenCurrentDoor()
    {
        PanelTrigger.Current?.OpenDoor();
    }

    public void DismissCurrentPanel()
    {
        PanelTrigger.Current?.DismissPanel();
    }
}