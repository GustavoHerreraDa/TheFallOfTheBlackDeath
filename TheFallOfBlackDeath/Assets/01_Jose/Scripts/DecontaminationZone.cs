using UnityEngine;
using UnityEngine.VFX;

/// Controls a decontamination zone.
/// When a character enters, all vents start spinning and all decontamination VFX are enabled.
/// When no characters remain inside, everything shuts down.
public class DecontaminationZone : MonoBehaviour
{
    [Header("Ventilation")]
    [SerializeField] private RotatingVent[] vents;

    [Header("Decontamination VFX")]
    [SerializeField] private VisualEffect[] decontaminationVfx;

    private int charactersInside;

    private void Awake()
    {
        SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter"))
        {
            return;
        }

        charactersInside++;

        if (charactersInside == 1)
        {
            SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Charecter"))
        {
            return;
        }

        charactersInside--;

        if (charactersInside <= 0)
        {
            charactersInside = 0;
            SetActive(false);
        }
    }

    private void SetActive(bool active)
    {
        foreach (var vent in vents)
        {
            if (vent != null)
            {
                vent.SetRunning(active);
            }
        }

        foreach (var vfx in decontaminationVfx)
        {
            if (vfx == null)
            {
                continue;
            }

            if (active)
            {
                vfx.Play();
            }
            else
            {
                vfx.Stop();
            }
        }
    }
}