using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject Pausemenu;

    bool Pause;
    
    private void Start()
    {
        Pausemenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Pause)
            {
                Resumegame();
            }
            else
            {
                PauseGame();
            }
        }

    }

    private void PauseGame()
    {
        Pausemenu.SetActive(true);
        Time.timeScale = 0f;
        Pause = true;
    }

    private void Resumegame()
    {
        Pausemenu.SetActive(false);
        Time.timeScale = 1f;
        Pause = false;
    }


}

