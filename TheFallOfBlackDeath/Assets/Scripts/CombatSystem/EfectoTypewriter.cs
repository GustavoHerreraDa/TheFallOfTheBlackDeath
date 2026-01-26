using System.Collections;
using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class EfectoTypewriter : MonoBehaviour
{
    public TextMeshProUGUI textoTMP; // Asigna esto en el inspector
    public float velocidadEscritura = 0.05f; // Tiempo entre letras

    // Llama a esta función para escribir un texto nuevo
    public void EscribirTexto(string mensaje)
    {
        textoTMP.text = mensaje;
        StartCoroutine(AnimarTexto());
    }

    IEnumerator AnimarTexto()
    {
        // Fuerza al mesh a actualizarse para saber cuántos caracteres tiene realmente
        textoTMP.ForceMeshUpdate(); 
        
        int totalCaracteres = textoTMP.textInfo.characterCount;
        int contador = 0;

        while (contador <= totalCaracteres)
        {
            textoTMP.maxVisibleCharacters = contador; // Muestra X letras
            contador++;
            yield return new WaitForSeconds(velocidadEscritura);
        }
    }
}