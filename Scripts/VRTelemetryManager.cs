using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;

public class VRTelemetryManager : MonoBehaviour
{
    public static VRTelemetryManager Instance { get; private set; }

    [Header("Assign this to your VR camera / HMD transform (e.g. CenterEyeAnchor)")]
    public Transform hmdTransform;

    [Header("How many position samples per second")]
    public float sampleRateHz = 5f;

    private string path;
    private string participantId;
    private string sessionId;
    private bool sessionActive = false;
    private Stopwatch stopwatch;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Starts a session. The session id is the participant id plus a random 4-digit number.</summary>
    public void StartSession(string participantIdInput)
    {
        StartSession(participantIdInput, null);
    }

    /// <summary>
    /// Starts a session. If runLabel is given (e.g. "P018"), the session id is
    /// runLabel + "_" + participantId (e.g. "P018_PGPN5RL"); otherwise it is
    /// participantId + "_" + a random 4-digit number.
    /// </summary>
    public void StartSession(string participantIdInput, string runLabel)
    {
        if (sessionActive)
        {
            UnityEngine.Debug.LogWarning("Telemetry: StartSession called while a session was already active. Ending previous session first.");
            EndSession();
        }

        participantId = participantIdInput;
        sessionId = string.IsNullOrEmpty(runLabel)
            ? participantId + "_" + Random.Range(1000, 9999)
            : runLabel + "_" + participantId;

        string dir = Path.Combine(Application.persistentDataPath, "Telemetry");
        Directory.CreateDirectory(dir);
        path = Path.Combine(dir, sessionId + ".csv");

        File.WriteAllText(path, "elapsed_s,participant_id,session_id,event_type,x,y,z,yaw,zone_id,detail\n");

        UnityEngine.Debug.Log("Telemetry: Session started. Writing to " + path);

        stopwatch = Stopwatch.StartNew();
        sessionActive = true;
        InvokeRepeating(nameof(SamplePosition), 0f, 1f / Mathf.Max(sampleRateHz, 0.1f));
    }

    public void EndSession()
    {
        if (!sessionActive) return;

        CancelInvoke(nameof(SamplePosition));
        sessionActive = false;
        stopwatch.Stop();

        UnityEngine.Debug.Log("Telemetry: Session ended. File saved at " + path);
    }

    private void SamplePosition()
    {
        if (!sessionActive || hmdTransform == null) return;

        Vector3 pos = hmdTransform.position;
        float yaw = hmdTransform.eulerAngles.y;

        AppendRow(BuildRow("position_sample", pos.x, pos.y, pos.z, yaw, "", ""));
    }

    public void LogZoneEvent(string zoneId, string eventType)
    {
        if (!sessionActive) return;
        AppendRow(BuildRow(eventType, null, null, null, null, zoneId, ""));
    }

    public void LogInteractionEvent(string eventType, string detail)
    {
        if (!sessionActive) return;
        AppendRow(BuildRow(eventType, null, null, null, null, "", detail));
    }

    private string BuildRow(string eventType, float? x, float? y, float? z, float? yaw, string zoneId, string detail)
    {
        string elapsed = stopwatch.Elapsed.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture);
        string xs = x.HasValue ? x.Value.ToString("F2", CultureInfo.InvariantCulture) : "";
        string ys = y.HasValue ? y.Value.ToString("F2", CultureInfo.InvariantCulture) : "";
        string zs = z.HasValue ? z.Value.ToString("F2", CultureInfo.InvariantCulture) : "";
        string yaws = yaw.HasValue ? yaw.Value.ToString("F2", CultureInfo.InvariantCulture) : "";

        return string.Join(",", elapsed, participantId, sessionId, eventType, xs, ys, zs, yaws, CsvEscape(zoneId), CsvEscape(detail));
    }

    private void AppendRow(string row)
    {
        File.AppendAllText(path, row + "\n");
    }

    private static string CsvEscape(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        return field;
    }

    void OnApplicationQuit()
    {
        if (sessionActive) EndSession();
    }
}