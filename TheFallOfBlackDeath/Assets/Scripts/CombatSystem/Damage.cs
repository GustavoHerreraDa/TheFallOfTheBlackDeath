using UnityEngine;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{
    public Text numero;
    public float velocidadAscendente = 1f;
    public float tiempoVida = 1.5f;
    private Transform cam;

    public void Inicializar(int _numero)
    {
        cam = Camera.main.transform;
        numero.text = _numero.ToString();
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.forward); // siempre frente a cámara
            transform.position += Vector3.up * velocidadAscendente * Time.deltaTime;
        }
    }
}
