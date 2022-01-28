using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json;
using System.IO;

[System.Serializable]
public class Point
{
    public float x;
    public float y;
    public float z;

    public Point(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

}

public class PointCloudLogger : MonoBehaviour
{
    public CubeBehaviour cube;
    

    public ARPointCloudManager pointManager;

    public List<Point> points = new List<Point>();

    private bool scan = false;

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
            foreach (var point in pointsList.positions)
            {
                points.Add(new Point(point));
            }
        }

    }

    public void ReportLog()
    {
        Debug.Log(Application.persistentDataPath);
        File.WriteAllText(Application.persistentDataPath + "/pointsLog.txt", JsonConvert.SerializeObject(points));
        new NativeShare().AddFile(Application.persistentDataPath + "/pointsLog.txt")
        .SetSubject("Subject goes here").SetText("Hello world!").SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
        .Share();
    }

    // Update is called once per frame
    void Update()
    {
        cube.CheckCollision(points);
    }
}
