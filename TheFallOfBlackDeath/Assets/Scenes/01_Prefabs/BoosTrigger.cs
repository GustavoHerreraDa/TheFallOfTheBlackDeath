using UnityEngine;

public class BoosTrigger : MonoBehaviour
{
    [SerializeField] private Boss_Door_ bossDoor; 
    [SerializeField] private AudioSource doorSound; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            bossDoor.ToggleDoor(); // abre/cierra la puerta
            if (doorSound != null) doorSound.Play();
        }
    }
}
