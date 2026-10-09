using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TelemetryZone : MonoBehaviour
{
    [Tooltip("Readable identifier for this piece of equipment, e.g. Microscope_01")]
    public string zoneId;

    [Tooltip("Tag on your VR player rig / character controller")]
    public string playerTag = "Player";

    [Header("Audio")]
    [Tooltip("Audio Source containing the explanation audio for this zone")]
    public AudioSource audioSource;

    [Tooltip("Play the audio only until it has been heard IN FULL once. " +
             "An interrupted playback does not count, so it will retry on the next visit.")]
    public bool playOnlyOnce = true;

    private bool hasCompletedAudio = false;
    private Coroutine audioCompletionCoroutine;
    private bool audioIsPlaying = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        VRTelemetryManager.Instance?.LogZoneEvent(zoneId, "zone_enter");

        if (playOnlyOnce && hasCompletedAudio)
            return;

        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogWarning($"TelemetryZone '{zoneId}': No AudioSource or AudioClip assigned.");
            return;
        }

        if (audioIsPlaying)
            return;

        audioSource.Play();
        audioIsPlaying = true;

        VRTelemetryManager.Instance?.LogInteractionEvent("audio_started", zoneId);

        audioCompletionCoroutine = StartCoroutine(WaitForAudioCompletion());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        VRTelemetryManager.Instance?.LogZoneEvent(zoneId, "zone_exit");

        if (audioIsPlaying)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (audioCompletionCoroutine != null)
            {
                StopCoroutine(audioCompletionCoroutine);
                audioCompletionCoroutine = null;
            }

            audioIsPlaying = false;

            VRTelemetryManager.Instance?.LogInteractionEvent("audio_interrupted", zoneId);
        }
    }

    IEnumerator WaitForAudioCompletion()
    {
        yield return new WaitForSeconds(audioSource.clip.length);

        audioIsPlaying = false;
        audioCompletionCoroutine = null;
        hasCompletedAudio = true;

        VRTelemetryManager.Instance?.LogInteractionEvent("audio_completed", zoneId);
    }
}
