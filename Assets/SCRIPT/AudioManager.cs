using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Voiceovers (Index 0: Sun ... Index 9: Pluto)")]
    [SerializeField] private AudioClip[] celestialVoiceovers;

    

    

    private void Awake()
    {
        if (audioSource == null && !TryGetComponent(out audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayPlanetAudio(int index) => PlayClipFromArray(celestialVoiceovers, index, "Celestial");
    
    

    

    private void PlayClipFromArray(AudioClip[] clipArray, int index, string categoryName)
    {
        if (clipArray == null || clipArray.Length == 0)
        {
            Debug.LogWarning($"AudioManager: No {categoryName} voiceovers assigned in Inspector!");
            return;
        }

        if (index >= 0 && index < clipArray.Length)
        {
            if (clipArray[index] != null)
            {
                audioSource.clip = clipArray[index];
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"AudioManager: Missing AudioClip at index {index} in {categoryName}.");
            }
        }
        else
        {
            Debug.LogError($"AudioManager: Index {index} out of bounds for {categoryName} array (Length: {clipArray.Length}).");
        }
    }

    public bool IsPlaying() => audioSource != null && audioSource.isPlaying;
}