using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Serialization;

[System.Serializable]
public class Point //REFACTOR: change class to record or struct - can be more easily cached => CPU time is reduced
{
    public float X;
    public float Y;
    public float Z;

    public Vector3 Position;

    public float Confidence;

    public Point(Vector3 pos, float confidence)
    {
        Position = pos;
        X = pos.x;
        Y = pos.y;
        Z = pos.z;
        Confidence = confidence;
    }
}

public class PointCloudLogger : MonoBehaviour
{
    [FormerlySerializedAs("cube")]
     public CubeBehaviour Cube;

    [FormerlySerializedAs("pointManager")]
    public ARPointCloudManager PointManager;

    public List<Point> Points = new List<Point>();

    bool isScanning;

    [FormerlySerializedAs("slider")]
    public Slider Slider;

    public void StartScan()
    {
        isScanning = true;
        Cube.transform.position = new Vector3(1, 1, 1);
    }

    void Start()
    {
        StartCoroutine(BindCallback());
    }

    IEnumerator BindCallback()
    {
        yield return new WaitForSeconds(2); //odd, consider LateStart?
        PointManager.pointCloudsChanged += PointCloudsChanged;
    }

    public void PointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        foreach (var pointsList in obj.updated)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> confidenceValues = new List<float>();

            foreach (var point in pointsList.positions)
            {
                positions.Add(point);
            }

            foreach (var confidence in pointsList.confidenceValues)
            {
                confidenceValues.Add(confidence);
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Points.Add(new Point(positions[i], confidenceValues[i]));
            }
        }
    }

    public void ReportLog()
    {
        Debug.Log(Application.persistentDataPath);
        File.WriteAllText(Application.persistentDataPath + "/pointsLog.txt", JsonConvert.SerializeObject(Points));
        new NativeShare().AddFile(Application.persistentDataPath + "/pointsLog.txt")
            .SetSubject("Subject goes here").SetText("Hello world!").SetCallback((result, shareTarget) =>
                Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
    }

    // Update is called once per frame
    void Update()
    {
        // cube.CheckCollision(points);
    }
}