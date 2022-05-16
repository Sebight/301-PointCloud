using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class PointWrap
{
    public GameObject go;
    public Point acualPoint;
}

public class PointsManager : MonoBehaviour
{
    public List<Point> points = new List<Point>();

    public List<Point> Points
    {
        get { return points; }
        set { points = value; }
    }
    
    void Start()
    {
        Application.targetFrameRate = 60;
        var textFile = Resources.Load<TextAsset>("pointsList");
        Points = JsonConvert.DeserializeObject<List<Point>>(textFile.text);
        for (int i = 0; i < Points.Count; i++)
        {
            points[i].position = new Vector3(points[i].x, points[i].y, points[i].z);
        }

        //! Testing and benchmarks
        // points.RemoveRange(10000, points.Count - 10000);
        // for (int i = 0; i < 954_000; i++)
        // {
        //     Point point = new Point(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), 1f);
        //     points.Add(point);
        // }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}