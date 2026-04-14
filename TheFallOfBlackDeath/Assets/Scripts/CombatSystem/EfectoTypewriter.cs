using System.Collections;
using UnityEngine;
using TMPro; // Necesario para TextMeshPro

/// <summary>
/// Supports the combat system by handling efecto typewriter.
/// </summary>
public class EfectoTypewriter : MonoBehaviour
{
    public TextMeshProUGUI textoTMP; // Asigna esto en el inspector
    public float velocidadEscritura = 0.05f; // Tiempo entre letras

    // Llama a esta funciÃ³n para escribir un texto nuevo
    /// <summary>
    /// Executes the escribir texto workflow.
    /// </summary>
    /// <param name="mensaje">The mensaje.</param>
    public void EscribirTexto(string mensaje)
    {
        textoTMP.text = mensaje;
        StartCoroutine(AnimarTexto());
    }

    /// <summary>
    /// Executes the animar texto workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator AnimarTexto()
    {
        // Fuerza al mesh a actualizarse para saber cuÃ¡ntos caracteres tiene realmente
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
