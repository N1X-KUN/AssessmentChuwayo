using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; 

    [Header("Audio Sources (Drag AudioSource components here)")]
    public AudioSource musicSource;
    public AudioSource sfxSource; 
    public AudioSource uiSource;  

    [Header("Volume Settings (Adjust these in Inspector!)")]
    [Range(0f, 1f)] public float defaultMusicVolume = 0.15f; // Set to 15% by default!
    [Range(0f, 1f)] public float defaultSfxVolume = 0.5f;
    [Range(0f, 1f)] public float defaultUiVolume = 0.5f;

    [Header("Background Music")]
    public AudioClip levelMusic; 

    [Header("Button & UI SFX")]
    public AudioClip buttonClick;
    public AudioClip abilityReady;
    public AudioClip scoreUp;
    public AudioClip scoreDown;
    public AudioClip poisonOverlay;

    [Header("Ultimate Ability SFX")]
    public AudioClip timeSoundStart; 
    public AudioClip timeSoundEnd;   

    [Header("Game SFX")]
    public AudioClip bonkSound;
    public AudioClip jetpackCharge;
    public AudioClip jetpackFall;
    public AudioClip kommyJump;
    public AudioClip kommyKnockdownVoice;

    [Header("Dialogue SFX")]
    public AudioClip dialoguePop;

    private AudioLowPassFilter underwaterFilter; 

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Now it uses your Inspector sliders!
        musicSource.volume = defaultMusicVolume;
        uiSource.volume = defaultUiVolume;
        sfxSource.volume = defaultSfxVolume;

        if (levelMusic != null) 
        {
            musicSource.clip = levelMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (musicSource != null)
        {
            underwaterFilter = musicSource.gameObject.GetComponent<AudioLowPassFilter>();
            if (underwaterFilter == null) underwaterFilter = musicSource.gameObject.AddComponent<AudioLowPassFilter>();
            underwaterFilter.enabled = false; 
        }
    }

    public void PlaySFX(AudioClip clip) { if (clip != null) sfxSource.PlayOneShot(clip); }
    public void PlayUI(AudioClip clip) { if (clip != null) uiSource.PlayOneShot(clip); }

    public void SetUnderwaterMusic(bool isUnderwater)
    {
        if (underwaterFilter != null)
        {
            underwaterFilter.enabled = isUnderwater;
            if (isUnderwater) underwaterFilter.cutoffFrequency = 700f; 
        }
    }

    public void MuffleMusic(bool muffle)
    {
        StopAllCoroutines();
        // If muffling, drop to 30% of your chosen volume. Otherwise, return to your chosen volume.
        float target = muffle ? (defaultMusicVolume * 0.3f) : defaultMusicVolume;
        StartCoroutine(FadeMusic(target)); 
    }

    private IEnumerator FadeMusic(float targetVolume)
    {
        float startVol = musicSource.volume;
        float elapsed = 0f;
        while(elapsed < 0.2f) 
        {
            musicSource.volume = Mathf.Lerp(startVol, targetVolume, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        musicSource.volume = targetVolume;
    }
}