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
    [Range(0f, 1f)] public float defaultMusicVolume = 0.15f; 
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
    private bool isMuffled = false;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }

        // MOVED TO AWAKE: Guarantees volume is set BEFORE DialogueManager asks for it!
        if (musicSource != null) musicSource.volume = defaultMusicVolume;
        if (uiSource != null) uiSource.volume = defaultUiVolume;
        if (sfxSource != null) sfxSource.volume = defaultSfxVolume;

        if (levelMusic != null && musicSource != null) 
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

    // NEW: Updates the volume LIVE while you drag the slider during play mode!
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (musicSource != null && !isMuffled) musicSource.volume = defaultMusicVolume;
            if (uiSource != null) uiSource.volume = defaultUiVolume;
            if (sfxSource != null) sfxSource.volume = defaultSfxVolume;
        }
    }

    public void PlaySFX(AudioClip clip) { if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip); }
    public void PlayUI(AudioClip clip) { if (clip != null && uiSource != null) uiSource.PlayOneShot(clip); }

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
        isMuffled = muffle;
        StopAllCoroutines();
        float target = muffle ? (defaultMusicVolume * 0.3f) : defaultMusicVolume;
        if (gameObject.activeInHierarchy) StartCoroutine(FadeMusic(target)); 
    }

    private IEnumerator FadeMusic(float targetVolume)
    {
        if (musicSource == null) yield break;

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