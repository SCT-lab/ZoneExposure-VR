using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Starts a telemetry session. The participant code and the run number can be
/// generated automatically or entered manually.
///
/// Automatic mode:  participant code = random code (e.g. PGPN5RL),
///                  run label        = P + a counter that increases with every session (P001, P002, ...).
/// Manual mode:     participant code = the value typed in the Inspector or set from a UI field.
///
/// Log columns:     participant_id = participant code
///                  session_id     = run label + "_" + participant code (e.g. P018_PGPN5RL)
/// </summary>
public class TelemetryTestStarter : MonoBehaviour
{
    [Header("Participant code")]
    [Tooltip("If on, a random participant code is generated for every session. " +
             "If off, the code in 'Participant Id' below is used (for example the pre-test code).")]
    public bool autoGenerateParticipantCode = true;

    [Tooltip("Used only when 'Auto Generate Participant Code' is off.")]
    public string participantId = "P001";

    [Tooltip("Length of the generated participant code.")]
    public int generatedCodeLength = 7;

    [Header("Run number")]
    [Tooltip("If on, a run label (P001, P002, ...) is added to the session id. " +
             "The counter is stored on the headset and increases with every session.")]
    public bool addRunNumber = true;

    [Tooltip("Prefix of the run label.")]
    public string runPrefix = "P";

    [Tooltip("Number of digits of the run label (3 gives P001).")]
    public int runDigits = 3;

    [Header("Behaviour")]
    [Tooltip("Start the session automatically when the scene loads. " +
             "Disable this if you start it from a UI button via StartTelemetry().")]
    public bool startAutomatically = true;

    [Tooltip("Called after the session has started, with the participant code. " +
             "Can be linked to a UI text so the experimenter can read the code.")]
    public UnityEvent<string> onSessionStarted;

    private const string RunCounterKey = "telemetry_run_counter";

    // Characters without look-alikes (no 0/O, 1/I) so codes are easy to read and copy.
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    void Start()
    {
        if (startAutomatically)
        {
            StartTelemetry();
        }
    }

    /// <summary>Can be hooked to a UI InputField to set the participant code at runtime (manual mode).</summary>
    public void SetParticipantId(string id)
    {
        participantId = id;
    }

    /// <summary>Can be hooked to a UI Button, or called from other scripts.</summary>
    public void StartTelemetry()
    {
        if (VRTelemetryManager.Instance == null)
        {
            Debug.LogError("TelemetryStarter: no VRTelemetryManager found in the scene.");
            return;
        }

        string code = autoGenerateParticipantCode ? GenerateCode(generatedCodeLength) : participantId;

        if (string.IsNullOrWhiteSpace(code))
        {
            Debug.LogError("TelemetryStarter: participant code is empty. Session was not started.");
            return;
        }

        code = Sanitize(code.Trim());
        string runLabel = addRunNumber ? NextRunLabel() : null;

        VRTelemetryManager.Instance.StartSession(code, runLabel);

        Debug.Log("TelemetryStarter: session started. Participant " + code +
                  (runLabel != null ? ", run " + runLabel : ""));
        onSessionStarted?.Invoke(code);
    }

    private string NextRunLabel()
    {
        int next = PlayerPrefs.GetInt(RunCounterKey, 0) + 1;
        PlayerPrefs.SetInt(RunCounterKey, next);
        PlayerPrefs.Save();
        return runPrefix + next.ToString("D" + Mathf.Max(runDigits, 1));
    }

    private static string GenerateCode(int length)
    {
        length = Mathf.Max(length, 4);
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(CodeChars[Random.Range(0, CodeChars.Length)]);
        }
        return sb.ToString();
    }

    // The code is used in the CSV and in the file name, so remove characters
    // that would break either (commas, path separators, other invalid file name characters).
    private static string Sanitize(string id)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            id = id.Replace(c, '_');
        }
        return id.Replace(',', '_');
    }
}