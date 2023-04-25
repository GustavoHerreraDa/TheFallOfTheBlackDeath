using System;
using UnityEngine;
using UnityEngine.UI;

public class LogPanel : MonoBehaviour
{
    //Referencia estatica al panel actual
    protected static LogPanel current;
    //Este panel tiene una referencia a la etiqueta de texto
    public Text logLabel;

    private void Awake()
    {
        current = this;
    }
    //Funcion estatica write para escribir un mensaje
    public static void Write(string message)
    {
        current.logLabel.text = message;
    }

    internal static void Write(string idName, string v)
    {
        throw new NotImplementedException();
    }
}