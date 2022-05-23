using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PointCloudLogger;
using System.Diagnostics;

public class CubeBehaviour : MonoBehaviour
{
    public MeshCollider collider;

    private Vector3 origin;

    public float speed;

    public bool move = false;


    public bool IsPointInCollider(Collider other, Vector3 point)
    {
        return (other.ClosestPoint(point) == point);
        // if (other.ClosestPoint(point) == point)
        // {
        //     return true;
        // }

        // return false;
    }

    public void CheckCollision(List<Point> points)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        int pointsAffected = 0;
        for (int i = 0; i < points.Count; ++i)
        {

            if (IsPointInCollider(collider, points[i].Position))
            {
                // ParticleSystem.MainModule settings = points[i].go.GetComponent<ParticleSystem>().main;
                // settings.startColor = new ParticleSystem.MinMaxGradient(Color.blue);
                pointsAffected++;
            }

            else
            {
                // points[i].go.GetComponent<MeshRenderer>().material.color = Color.gray;
                // ParticleSystem.MainModule settings = points[i].go.GetComponent<ParticleSystem>().main;
                // settings.startColor = new ParticleSystem.MinMaxGradient(Color.gray);
            }
        }
        stopwatch.Stop();

        // if (pointsAffected > 0)
        // {
        //     gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        // }
        // else
        // {
        //     gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        // }

        TimeSpan ts = stopwatch.Elapsed;
        // string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        long elapsedMillis = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"Time taken checking {points.Count} points: " + elapsedMillis + " ms ("+pointsAffected+")");

    }

    [ContextMenu("Update Origin")]
    public void UpdateOrigin()
    {
        origin = transform.position;
    }


    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            //Move the object on z axis with sine
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Sin(Time.time) * speed);
        }
    }
}