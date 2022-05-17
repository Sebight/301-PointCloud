using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;

[System.Serializable]
public class Point
{
    public float x;
    public float y;
    public float z;

    public Vector3 position;

    public float confidence;

    public Point(Vector3 pos, float _confidence)
    {
        position = pos;
        x = pos.x;
        y = pos.y;
        z = pos.z;
        confidence = _confidence;
    }
}

public class PointCloudLogger : MonoBehaviour
{
    public CubeBehaviour cube;


    public ARPointCloudManager pointManager;

    public List<Point> Points = new List<Point>();

    private bool scan = false;

    public Slider slider;

    public void StartScan()
    {
        scan = true;
        cube.gameObject.transform.position = new Vector3(1, 1, 1);
    }

    void Start()
    {
        StartCoroutine(BindCallback());
    }

    IEnumerator BindCallback()
    {
        yield return new WaitForSeconds(2);
        pointManager.pointCloudsChanged += PointCloudsChanged;
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