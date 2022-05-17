using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;
using BP;

public struct CubicJob : IJob
{
    public NativeArray<Vector3> points;
    public Vector3 volumeDimensions;
    public Vector3 rotation;
    public Vector3 volumeCenter;
    public NativeArray<bool> result;

    public void Execute()
    {
        BetterPhysics physics = new BetterPhysics();
        for (int i = 0; i < points.Length; i++)
        {
            bool isIn = physics.IsPointInVolume(points[i], volumeDimensions, rotation, volumeCenter);
            result[i] = isIn;
        }
    }
}


public struct SphericalJob : IJob
{
    public NativeArray<Vector3> points;
    public Vector3 volumeCenter;
    public float radius;
    public NativeArray<bool> result;

    public void Execute()
    {
        BetterPhysics physics = new BetterPhysics();
        for (int i = 0; i < points.Length; i++)
        {
            bool isIn = physics.IsPointInVolume(points[i], volumeCenter, radius);
            result[i] = isIn;
        }
    }
}

public class Collisions : MonoBehaviour
{
    public bool check = false;

    public GameObject examplePoint;

    public Vector3 exampleOrigin;
    public Vector3 exampleSize;
    public Vector3 rotateBy;
    private Vector3 centerOfVolume;


    List<Vector3> positions;
    private List<GameObject> gos = new List<GameObject>();

    private Vector3 newCenterOfVolume;
    public PointsManager pointsManager;

    #region Visualization

    public void DrawVolume(List<Vector3> positions)
    {
        if (gos.Count == 0)
        {
            foreach (var position in positions)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = position;
                if (position == exampleOrigin) cube.GetComponent<MeshRenderer>().material.color = Color.yellow;
                gos.Add(cube);
            }
        }

        for (int i = 0; i < gos.Count; i++)
        {
            gos[i].transform.position = positions[i];
        }
    }

    #endregion

    public Dictionary<string, BetterPhysics.Collider> colliders = new Dictionary<string, BetterPhysics.Collider>();

    void Start()
    {
        #region Visualization

        float startX = exampleOrigin.x;
        float endX = exampleOrigin.x + exampleSize.x;

        float startY = exampleOrigin.y;
        float endY = exampleOrigin.y + exampleSize.y;

        float startZ = exampleOrigin.z;
        float endZ = exampleOrigin.z + exampleSize.z;

        positions = new List<Vector3>()
        {
            new Vector3(startX, startY, startZ), new Vector3(endX, startY, startZ), new Vector3(startX, endY, startZ),
            new Vector3(startX, startY, endZ), new Vector3(endX, endY, startZ), new Vector3(startX, endY, endZ),
            new Vector3(endX, endY, endZ), new Vector3(endX, startY, endZ)
        };
        centerOfVolume = new Vector3((startX + endX) / 2, (startY + endY) / 2, (startZ + endZ) / 2);

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            pos = Quaternion.Euler(rotateBy) * (pos - centerOfVolume) + centerOfVolume;
            positions[i] = pos;
        }

        exampleOrigin = positions[0];
        DrawVolume(positions);

        #endregion

        //Register colliders
        colliders.Add("cubic-1", new BetterPhysics.CubicCollider(exampleOrigin, exampleSize, rotateBy));
        colliders.Add("spherical-1", new BetterPhysics.SphericalCollider(new Vector3(20, 20, 20), 5f));
    }

    #region Visualization

    private void RebuildVolume()
    {
        float startX = exampleOrigin.x;
        float endX = exampleOrigin.x + exampleSize.x;

        float startY = exampleOrigin.y;
        float endY = exampleOrigin.y + exampleSize.y;

        float startZ = exampleOrigin.z;
        float endZ = exampleOrigin.z + exampleSize.z;

        positions = new List<Vector3>()
        {
            new Vector3(startX, startY, startZ), new Vector3(endX, startY, startZ), new Vector3(startX, endY, startZ),
            new Vector3(startX, startY, endZ), new Vector3(endX, endY, startZ), new Vector3(startX, endY, endZ),
            new Vector3(endX, endY, endZ), new Vector3(endX, startY, endZ)
        };
        centerOfVolume = new Vector3((startX + endX) / 2, (startY + endY) / 2, (startZ + endZ) / 2);

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            pos = Quaternion.Euler(rotateBy) * (pos - centerOfVolume) + centerOfVolume;
            positions[i] = pos;
        }

        // exampleOrigin = positions[0];
        DrawVolume(positions);
    }

    #endregion

    public void Check() => check = true;

    /// <summary>
    ///    Checks if collider collides with any of the provided points.
    /// </summary>
    /// <param name="points">List of all recorded points</param>
    /// <param name="collider">BetterPhysics Collider of any type</param>
    /// <returns>Boolean of whether any points from the list collides with the collider.</returns>
    public bool CheckPoints(List<Point> points, BetterPhysics.Collider collider, bool multithread = true)
    {
        //This can be messy to read at first, but when you understand the core of this function, it's pretty easy to understand.
        //The idea is to check if the collider collides with any of the points. This is already implemented with multithreading, so it's pretty fast (but messy).

        bool inVolume = false;


        if (!multithread)
        {
            BetterPhysics physics = new BetterPhysics();
            for (int i = 0; i < points.Count; i++)
            {
                if (collider.GetType() == typeof(BetterPhysics.SphericalCollider))
                {
                    BetterPhysics.SphericalCollider sphere = (BetterPhysics.SphericalCollider)collider;
                    if (physics.IsPointInVolume(points[i].position, sphere.Origin, sphere.Radius))
                    {
                        inVolume = true;
                        break;
                    }
                } else if (collider.GetType() == typeof(BetterPhysics.CubicCollider))
                {
                    BetterPhysics.CubicCollider cube = (BetterPhysics.CubicCollider)collider;
                    if (physics.IsPointInVolume(points[i].position, cube.Size, cube.Rotation, exampleOrigin))
                    {
                        inVolume = true;
                        break;
                    }
                }
            }
        }
        else
        {
            int totalPoints = 0;
            int jobBuffer = 500;
            int currentSegment = 0;
            List<Vector3> tempPoints = new List<Vector3>();


            for (int i = 0; i < points.Count; i += jobBuffer)
            {
                tempPoints.Clear();
                currentSegment++;
                int segmentStart = i;
                int segmentEnd = i + jobBuffer;
                for (int j = segmentStart; j < segmentEnd; j++)
                {
                    totalPoints++;
                    if (j >= points.Count - 1)
                    {
                        break;
                    }

                    tempPoints.Add(points[j].position);
                }

                NativeArray<bool> _result = new NativeArray<bool>(500, Allocator.TempJob);

                if (collider.GetType() == typeof(BetterPhysics.CubicCollider))
                {
                    BetterPhysics.CubicCollider col = (BetterPhysics.CubicCollider) collider;

                    CubicJob job = new CubicJob();
                    job.points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                    job.volumeDimensions = col.Size;
                    job.rotation = col.Rotation;
                    job.volumeCenter = col.Origin;
                    job.result = _result;

                    JobHandle handle = job.Schedule();
                    handle.Complete();
                    job.points.Dispose();
                }
                else if (collider.GetType() == typeof(BetterPhysics.SphericalCollider))
                {
                    BetterPhysics.SphericalCollider col = (BetterPhysics.SphericalCollider) collider;

                    SphericalJob job = new SphericalJob();
                    job.points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                    job.volumeCenter = col.Origin;
                    job.radius = col.Radius;
                    job.result = _result;

                    JobHandle handle = job.Schedule();
                    handle.Complete();
                    job.points.Dispose();
                }

                inVolume = false;
                int pointIndex = 0;
                foreach (bool pointResult in _result)
                {
                    if (pointResult)
                    {
                        inVolume = true;
                        //Convert the local pointIndex into the original point index
                        int originalIndex = segmentStart + pointIndex;
                        //TODO: Some implementation for this -> we have the "affected" point index, but we need to do something with it (perhaps some callback)
                        break;
                    }

                    pointIndex++;
                }

                _result.Dispose();
            }
        }

        return inVolume;
    }

    void Update()
    {
        if (check)
        {
            check = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Debug.Log(CheckPoints(pointsManager.Points, colliders["spherical-1"], true));
            sw.Stop();

            UnityEngine.Debug.Log("Finished checking " + pointsManager.Points.Count + " points in " +
                                  sw.ElapsedMilliseconds + " ms");
        }

        //This code is currently not safe.
        BetterPhysics.CubicCollider c = (BetterPhysics.CubicCollider) colliders["cubic-1"]; 
        c.Origin = exampleOrigin;
        c.Size = exampleSize;
        c.Rotation = rotateBy;

        //Collisions
        RebuildVolume();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(0, 0, 0), 1f);
    }
}