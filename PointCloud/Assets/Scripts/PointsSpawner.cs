using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;

public class PointWrap
{
    public GameObject go;
    public Point acualPoint;
}

public class PointsSpawner : MonoBehaviour
{
    public List<Point> points = new List<Point>();

    public List<PointWrap> spawned = new List<PointWrap>();

    [Range(0.0f, 1.0f)] public float slider;

    public float lastSliderChange = 0.0f;
    // public Slider slider;


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
        for (int i = 0; i < points.Count; i++)
        {
            while (!spawn) yield return null;
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
            point.transform.position = new Vector3(points[i].x, points[i].y, points[i].z);
            point.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            Debug.Log("[SpawnPoints] Progress (" + i + "/" + points.Count + ")");
            PointWrap wrap = new PointWrap();
            wrap.go = point;
            wrap.acualPoint = points[i];
            Debug.Log(points[i].confidence);
            spawned.Add(wrap);
            yield return new WaitForSeconds(0.0001f);
        }
    }

    public void FilterPoints()
    {
        if (lastSliderChange == slider) return;
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i].acualPoint.confidence < slider)
            {
                spawned[i].go.SetActive(false);
            }
            else
            {
                spawned[i].go.SetActive(true);
            }
        }
        lastSliderChange = slider;
    }

    // Update is called once per frame
    void Update()
    {
        FilterPoints();
        if (log)
        {
            //cube.CheckCollision(points);
            cube.CheckCollision(spawned);
        }
    }
}