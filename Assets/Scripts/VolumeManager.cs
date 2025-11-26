using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [Space]
    [Header("=== Componentes ===")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume") || PlayerPrefs.HasKey("sfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetVolumeMusic();
        }
    }

    public void SetVolumeMusic()
    {
        float volume = musicSlider.value;
        mixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetVolumeSFX()
    {
        float volume = sfxSlider.value;
        mixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void ActivateMusicLowPass(bool ativado)
    {
        if (ativado)
        {
            mixer.SetFloat("music-lowpass", 500f);
        }
        else
        {
            mixer.SetFloat("music-lowpass", 22000f);
        }
    }

    void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("musicVolume");
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume");

        SetVolumeMusic();
        SetVolumeSFX();
    }
}
