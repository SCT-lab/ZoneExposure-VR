# ZoneExposure-VR
Zone-based exposure telemetry toolkit for standalone VR

Exposure-aware, zone-based telemetry for standalone VR learning environments.
The toolkit logs when a learner enters and leaves predefined zones, which audio
narrations start, finish or are interrupted, and the learner's head position at
5 Hz.

Accompanying study: Exposure-Aware, Zone-Based Spatial Telemetry for Analyzing Learner Behavior in an Unguided VR Microbiology Laboratory "under review".

## Contents

| Folder | Description |
|---|---|
| `Unity/Scripts/` | C# scripts for Unity (`VRTelemetryManager`, `TelemetryStarter`, `TelemetryZone`, `VRDataLogger`) |
| `sample_data/` | Synthetic example logs (no real participant data) |

## Requirements

- Unity 2022.3.20f1
- Meta Quest 2, standalone Android build
- Analysis: Python 3.13 (tested in Google Colab);

## Setup in Unity

1. Copy the scripts from `Unity/Scripts/` into your project's `Assets` folder.
2. Add an empty GameObject with `VRTelemetryManager`. Assign your headset camera
   (e.g. `CenterEyeAnchor`) to `Hmd Transform`.
3. Add a second GameObject with `TelemetryStarter`. Choose automatic or manual
   participant codes in the Inspector.
4. For each zone, add a trigger collider and the `TelemetryZone` component.
   Set the zone ID and the narration audio clip.
5. Build and run on the headset.

## Log format

One CSV file per session, written to the headset's app data folder.

```
elapsed_s,participant_id,session_id,event_type,x,y,z,yaw,zone_id,detail
```

| Column | Meaning |
|---|---|
| `elapsed_s` | Seconds since the session started |
| `participant_id` | Participant code (automatic or entered manually) |
| `session_id` | Run label + participant code, e.g. `P018_PGPN5RL` |
| `event_type` | `position_sample`, `zone_enter`, `zone_exit`, `audio_started`, `audio_completed`, `audio_interrupted` |
| `x,y,z,yaw` | Head position and yaw (only for `position_sample`) |
| `zone_id` | Zone for `zone_enter` / `zone_exit` |
| `detail` | Zone and clip information for audio events |

## Getting the logs from the headset

```
adb pull /sdcard/Android/data/<package>/files/Telemetry/ ./telemetry_local
```


## Citation

If you use this toolkit, please cite: Exposure-Aware, Zone-Based Spatial Telemetry for Analyzing Learner Behavior in an Unguided VR Microbiology Laboratory.

## License

MIT License. See `LICENSE`.

## Contact

Faezeh Mousavian Parsa, Wageningen University & Research, fa.parsaa@gmail.com
