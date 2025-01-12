using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class Setting_UI : MonoBehaviour
{
    [SerializeField] Toggle Fullscreen;
    [SerializeField] TextMeshProUGUI textFullscreen;
    [SerializeField] Button Fullscreen_Button;
    [SerializeField] Toggle Auto_JoyStick_Enable;
    [SerializeField] TextMeshProUGUI textAuto_JoyStick;
    [SerializeField] Button Auto_JoyStick_Button;
    [SerializeField] Slider Music;
    [SerializeField] TextMeshProUGUI textMusic;
    [SerializeField] Slider SFX;
    [SerializeField] TextMeshProUGUI textSFX;
    [SerializeField] AudioMixer MusicMixer;
    [SerializeField] AudioMixer SFXMixer;
    [SerializeField]private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        if (!Input.touchSupported || Application.platform == RuntimePlatform.Android)
        {
            Auto_JoyStick_Enable.interactable = false;
            Auto_JoyStick_Enable.isOn = false;
            Game_Setting.Is_Auto_JoyStick_Enable = false;
            Auto_JoyStick_Button.interactable = false;
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                textAuto_JoyStick.text = "�ػ�ó�����ͧ�Ѻ";
            }
            else
            {
                textFullscreen.text = "�Դ��ҹ";
                textAuto_JoyStick.text = "�Դ��ҹ";
                Fullscreen.interactable = false;
                Fullscreen.isOn = false;
                Fullscreen_Button.interactable = false;
            }
        }

        if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                textFullscreen.text = "�Դ";
                Fullscreen.isOn = true;
            }
            else
            {
                textFullscreen.text = "�Դ";
                Fullscreen.isOn = false;
            }
        }

        Update_Setting_Slider();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        PlayAnim(true);
    }

    public void PlayAnim(bool IsPlayIn)
    {
        if (IsPlayIn)
        {
            animator.SetBool("IsIn", true);
            animator.SetBool("IsOut", false);
        }
        else
        {
            animator.SetBool("IsIn", false);
            animator.SetBool("IsOut", true);
        }
    }

    public void Update_Setting_Slider()
    {
        MusicMixer.GetFloat("MusicVol", out float musicoutput);
        Music.value = musicoutput;
        SFXMixer.GetFloat("SFXVol", out float SFXoutput);
        SFX.value = SFXoutput;
    }

    public void Set_Music()
    {
        float value = Music.value;
        //print(value);
        MusicMixer.SetFloat("MusicVol", value);
        textMusic.text = (int)(((value + 80) / 80) * 100) +"%";
    }

    public void Set_SFX()
    {
        float value = SFX.value;
        //print(value);
        SFXMixer.SetFloat("SFXVol", value);
        textSFX.text = (int)(((value + 80) / 80) * 100) + "%";
    }

    public void Set_Fullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            textFullscreen.text = "�Դ";
            Fullscreen.isOn = false;
        }
        else
        {

            Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, FullScreenMode.FullScreenWindow);
            textFullscreen.text = "�Դ";
            Fullscreen.isOn = true;
        }

    }

    public void Set_Auto_JoyStick_Enable()
    {
        Game_Setting.Is_Auto_JoyStick_Enable = !Game_Setting.Is_Auto_JoyStick_Enable;
        if (Input.touchSupported && Application.platform != RuntimePlatform.Android)
        {
            Auto_JoyStick_Enable.isOn = Game_Setting.Is_Auto_JoyStick_Enable;
            if (Game_Setting.Is_Auto_JoyStick_Enable)
            {
                textAuto_JoyStick.text = "�Դ";
            }
            else
            {
                textAuto_JoyStick.text = "�Դ";
            }
        }
    }
}
