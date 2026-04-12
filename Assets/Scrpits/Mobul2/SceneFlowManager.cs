using UnityEngine;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    [Header("XR Lock")]
    [SerializeField] private Behaviour[] disableComponents;
    [SerializeField] private GameObject[] hideObjects;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    public bool IsAudioPlaying => audioSource != null && audioSource.isPlaying;

    public float AudioProgress
    {
        get
        {
            if (audioSource == null || audioSource.clip == null || audioSource.clip.length <= 0f)
                return 0f;

            return Mathf.Clamp01(audioSource.time / audioSource.clip.length);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetXRLocked(bool locked)
    {
        for (int i = 0; i < disableComponents.Length; i++)
        {
            if (disableComponents[i] != null)
                disableComponents[i].enabled = !locked;
        }

        for (int i = 0; i < hideObjects.Length; i++)
        {
            if (hideObjects[i] != null)
                hideObjects[i].SetActive(!locked);
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public void SetObjectsActive(GameObject[] objects, bool value)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(value);
        }
    }
}