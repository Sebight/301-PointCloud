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

public class PointsSpawner : MonoBehaviour
{
    public List<Point> points = new List<Point>();

    public List<PointWrap> spawned = new List<PointWrap>();

    [Range(0.0f, 1.0f)] public float slider;

    public float lastSliderChange = 0.0f;

    public Transform parent;
    // public Slider slider;


    public CubeBehaviour cube;
    public GameObject dotParticle;


    public bool log = false;
    public bool spawn = false;

    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        var textFile = Resources.Load<TextAsset>("pointsList");
        // points = JsonConvert.DeserializeObject<List<Point>>(File.ReadAllText(Application.dataPath + "/pointsList.txt"));
        points = JsonConvert.DeserializeObject<List<Point>>(textFile.text);
        for (int i = 0; i < points.Count; i++)
        {
            points[i].position = new Vector3(points[i].x, points[i].y, points[i].z);
        }

        for (int i = 0; i < 50_000; i++)
        {
            Point point = new Point(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), 1f);
            points.Add(point);
        }
            
        // StartCoroutine(SpawnPoints());
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
            // GameObject point = Instantiate(dotParticle);
            // point.transform.position = new Vector3(points[i].x, points[i].y, points[i].z);
            // point.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            // point.transform.parent = parent;
            PointWrap wrap = new PointWrap();
            // wrap.go = point;
            wrap.acualPoint = points[i];
            spawned.Add(wrap);
            text.text = spawned.Count.ToString();
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


    public void StartBenchmark()
    {
        log = true;
        cube.gameObject.SetActive(true);
        cube.speed = 2;
        cube.move = true;
        spawn = false;
    }

    // Update is called once per frame
    void Update()
    {
        // FilterPoints();
        if (log)
        {
            log = false;
            cube.CheckCollision(points);
            // cube.CheckCollision(spawned);
        }
    }
}