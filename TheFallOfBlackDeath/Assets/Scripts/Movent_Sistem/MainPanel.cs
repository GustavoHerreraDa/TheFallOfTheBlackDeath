using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Supports exploration and world-state flow by handling main panel.
/// </summary>
public class MainPanel : MonoBehaviour
{
    [Header("Opciones")]
    public Slider VolumenFX;
    public Slider VolumenMaster;
    public Toggle mute;
    public AudioMixer mixer;
    public AudioSource fxSource;
    public AudioClip clickSound;
    private float lastVolumen;
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject statsPanel;
    public GameObject controlesPanel;
    public GameObject introPanel;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        VolumenFX.onValueChanged.AddListener(ChangeVolumenMasterFX);
        VolumenMaster.onValueChanged.AddListener(ChangeVolumenMaster);
        if (introPanel != null)
            introPanel.SetActive(false);
    }

    /// <summary>
    /// Sets the mute.
    /// </summary>
    public void SetMute()
    {
        if (mute.isOn)
        {
            mixer.GetFloat("VolMaster", out lastVolumen);
            mixer.SetFloat("VolMaster", -80);
        }
        else
        {
            mixer.SetFloat("VolMaster", lastVolumen);
        }
    }
    /// <summary>
    /// Executes the open panel1 workflow.
    /// </summary>
    /// <param name="panel1">The panel1.</param>
    public void OpenPanel1(GameObject panel1)
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        panel1.SetActive(true);
        PlaySoundButton();

    }

    /// <summary>
    /// Executes the open panel2 workflow.
    /// </summary>
    /// <param name="panelTwo">The panel two.</param>
    public void OpenPanel2(GameObject panelTwo)
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        panelTwo.SetActive(true);
        PlaySoundButton();
    }

    /// <summary>
    /// Changes the volumen master.
    /// </summary>
    /// <param name="v">The v.</param>
    public void ChangeVolumenMaster(float v)
    {
        mixer.SetFloat("VolMaster", v);
    }
    /// <summary>
    /// Changes the volumen master fx.
    /// </summary>
    /// <param name="v">The v.</param>
    public void ChangeVolumenMasterFX(float v)
    {
        mixer.SetFloat("VolFX", v);
    }
    /// <summary>
    /// Executes the play sound button workflow.
    /// </summary>
    public void PlaySoundButton()
    {
        fxSource.PlayOneShot(clickSound);
    }
    /// <summary>
    /// Executes the exit workflow.
    /// </summary>
    public void Exit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Executes the play panel workflow.
    /// </summary>
    public void PlayPanel()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        introPanel.SetActive(true);
    }
    
    public void ClosePanel()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        introPanel.SetActive(false);
    }

}
