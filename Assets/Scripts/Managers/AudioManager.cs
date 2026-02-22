using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Settings")]
    public AudioClip[] musicTracks;
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    private AudioSource musicSource;
    private int currentTrackIndex = 0;
    private bool isMuted = false;

    private void Awake()
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

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.volume = musicVolume;
        musicSource.loop = false; // ручное управление циклом
        musicSource.playOnAwake = false;

        if (musicTracks.Length > 0)
        {
            PlayNextTrack();
        }
    }

    private void Update()
    {
        if (!isMuted && musicSource != null && !musicSource.isPlaying && musicTracks.Length > 0)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        if (musicTracks.Length == 0) return;

        musicSource.clip = musicTracks[currentTrackIndex];
        musicSource.Play();
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        musicSource.mute = isMuted;
    }

    public void SetVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = volume;
    }
}
