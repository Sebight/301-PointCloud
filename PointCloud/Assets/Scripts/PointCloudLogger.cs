using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PointCloudLogger : MonoBehaviour
{

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

    public ARPointCloudManager pointManager;

    public List<Point> points = new List<Point>();

    private bool scan = false;

    public void StartScan()
    {
        scan = true;
    }

    void Start()
    {
        StartCoroutine(BindCallback());
    }

    IEnumerator BindCallback()
    {
        yield return new WaitForSeconds(2);
        pointManager.pointCloudsChanged += PointCloudsChanged;
        Debug.Log("bind");
    }

    public void PointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        // foreach (var pointsList in obj.added)
        // {
        //     foreach (var point in pointsList.positions)
        //     {
        //         Debug.Log("added");
        //         points.Add(new Point(point));
        //     }
        // }



        foreach (var pointsList in obj.updated)
        {
            foreach (var point in pointsList.positions)
            {
                // if (scan)
                // {
                Debug.Log("added");
                points.Add(new Point(point));
                if (points.Count < 10000)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    cube.transform.position = point;
                    scan = false;
                }
                // }
            }
        }

        // foreach (var pointsList in obj.removed)
        // {
        //     foreach (var point in pointsList.positions)
        //     {
        //         points.Remove(point);
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Points: " + points.Count);
    }
}
