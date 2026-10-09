using System.IO;
using UnityEngine;

public class VRDataLogger : MonoBehaviour
{
    string path;
    int recordCount = 0;

    void Start()
    {
        Debug.Log("VRDataLogger STARTED");

        
        path = Application.persistentDataPath + "/vr_log.csv";

        Debug.Log("File Path: " + path);

        // -------------------------
        // ALWAYS CREATE NEW FILE
        // -------------------------
        File.WriteAllText(path, "time,x,y,z\n");

        Debug.Log("New CSV file created");
    }

    void Update()
    {
        Vector3 pos = transform.position;

        string line =
            Time.time + "," +
            pos.x + "," +
            pos.y + "," +
            pos.z + "\n";

        File.AppendAllText(path, line);

        recordCount++;

        if (recordCount % 50 == 0)
        {
            Debug.Log("Records saved: " + recordCount);
        }
    }
}