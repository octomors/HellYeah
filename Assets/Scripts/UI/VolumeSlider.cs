using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider slider;

    private void Start()
    {
        // Восстановить сохранённую громкость
        float saved = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        slider.value = saved;
        SetVolume(saved);

        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        // Audio Mixer работает в децибелах - конвертируем из 0-1
        float db = value > 0.001f ? Mathf.Log10(value) * 20 : -80f;
        mixer.SetFloat("MasterVolume", db);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}