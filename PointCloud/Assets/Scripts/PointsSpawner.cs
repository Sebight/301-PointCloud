using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using System.IO;

public class PointsSpawner : MonoBehaviour
{

    public List<Point> points = new List<Point>();

    public List<Point> spawned = new List<Point>();

    public CubeBehaviour cube;

    public bool log = false;
    public bool spawn = false;

    // Start is called before the first frame update
    void Start()
    {
        points = JsonConvert.DeserializeObject<List<Point>>(File.ReadAllText(Application.dataPath + "/pointsList.txt"));
        StartCoroutine(SpawnPoints());
        //for (int i = 0; i < points.Count; i++)
        //{
        //    if (i == 0)
        //    {
        //        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        point.transform.position = new Vector3(points[i].x, points[i].y, points[i].z);
        //        point.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //    }
        //}
    }

    IEnumerator SpawnPoints()
    {
        for (int i = 0;i < points.Count;i++)
        {
            while (!spawn) yield return null;
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
            point.transform.position = new Vector3(points[i].x, points[i].y, points[i].z);
            point.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            Debug.Log("[SpawnPoints] Progress (" + i + "/" + points.Count + ")");
            spawned.Add(points[i]);
            yield return new WaitForSeconds(0.0001f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (log)
        {
            //cube.CheckCollision(points);
            cube.CheckCollision(spawned);
        }
    }
}
