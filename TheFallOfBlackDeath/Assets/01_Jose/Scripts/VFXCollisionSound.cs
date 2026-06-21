using UnityEngine;
using UnityEngine.VFX;

public class VFXCollisionSound : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private AudioClip impactClip;

    private void OnEnable()
    {
        vfx.outputEventReceived += OnOutputEventReceived;
    }

    private void OnDisable()
    {
        vfx.outputEventReceived -= OnOutputEventReceived;
    }

    private void OnOutputEventReceived(VFXOutputEventArgs args)
    {
        AudioSource.PlayClipAtPoint(
            impactClip,
            transform.position
        );
    }
}