using UnityEngine;
using UnityEngine.SceneManagement;

public class DevSceneChanger : MonoBehaviour
{
    [Header("Nombres de las Escenas")]
    public string escena1 = "NombreDeTuEscena1";
    public string escena2 = "NombreDeTuEscena2";

    void Update()
    {
        // Verifica si estás manteniendo apretado el Control (izquierdo o derecho)
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Si apretás el 1 (el de arriba de las letras)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneManager.LoadScene(escena1);
            }
            // Si apretás el 2
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SceneManager.LoadScene(escena2);
            }
        }
    }
}