using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class PointsManager : MonoBehaviour
{
    private List<Point> points = new List<Point>();

    //Holds all points loaded from the config file
    public List<Point> Points
    {
        get { return points; }
        set { points = value; }
    }
    
    void Start()
    {
        Application.targetFrameRate = 60;
        
        TextAsset textFile = Resources.Load<TextAsset>("pointsList");
        Points = JsonConvert.DeserializeObject<List<Point>>(textFile.text);
        
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i].Position = new Vector3(points[i].X, points[i].Y, points[i].Z);
        }

        //! Testing and benchmarks
        // points.RemoveRange(10000, points.Count - 10000);
        // for (int i = 0; i < 954_000; i++)
        // {
        //     Point point = new Point(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), 1f);
        //     points.Add(point);
        // }
    }
}