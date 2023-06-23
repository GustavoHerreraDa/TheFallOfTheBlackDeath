using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
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
    public GameObject controlesPanel;
    public GameObject statsPanel;
    public GameObject introPanel;

    private void Awake()
    {
        VolumenFX.onValueChanged.AddListener(ChangeVolumenMasterFX);
        VolumenMaster.onValueChanged.AddListener(ChangeVolumenMaster);
        if (introPanel != null)
            introPanel.SetActive(false);
    }

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
    public void OpenPanel1(GameObject panel1)
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        panel1.SetActive(true);
        PlaySoundButton();

    }

    public void OpenPanel2(GameObject panelTwo)
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        panelTwo.SetActive(true);
        PlaySoundButton();
    }

    public void ChangeVolumenMaster(float v)
    {
        mixer.SetFloat("VolMaster", v);
    }
    public void ChangeVolumenMasterFX(float v)
    {
        mixer.SetFloat("VolFX", v);
    }
    public void PlaySoundButton()
    {
        fxSource.PlayOneShot(clickSound);
    }
    public void Exit()
    {
        Application.Quit();
    }

    public void PlayPanel()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlesPanel.SetActive(false);
        introPanel.SetActive(true);
    }

}
