using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [Tooltip("Plays quick sound effects.")]
    public AudioSource sfxSource;
    [Tooltip("Plays the looping background music.")]
    public AudioSource bgmSource;

    [Header("Sound Effects")]
    public AudioClip slashSound;
    public AudioClip loseSound;
    public AudioClip winSound;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; 
        }
    }

    void Start()
    {

        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true; 
            bgmSource.Play();
        }
    }

    // --- Sound Effect Methods ---

    public void PlaySlash()
    {
        if (slashSound != null) sfxSource.PlayOneShot(slashSound);
    }

    public void PlayLose()
    {
        if (loseSound != null)
        {
            sfxSource.PlayOneShot(loseSound);
            
            if (bgmSource != null) bgmSource.Stop();
        }
    }

    public void PlayWin()
    {
        if (winSound != null)
        {
            sfxSource.PlayOneShot(winSound);
            
            if (bgmSource != null) bgmSource.Stop();
        }
    }
}