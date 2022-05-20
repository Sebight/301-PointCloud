using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Debug = UnityEngine.Debug;
using BP;

public struct CubicJob : IJob, JobHelper.IJobDisposable
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
        // Debug.Log("Job is done!");
        // points.Dispose();
        // result.Dispose();
    }

    public void OnDispose()
    {
        points.Dispose();
        result.Dispose();
    }
}


public struct SphericalJob : IJob, JobHelper.IJobDisposable
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

    public void OnDispose()
    {
        points.Dispose();
        result.Dispose();
    }
}

public enum DetectionMode
{
    Simple,
    Enhanced
}

public class Collisions : MonoBehaviour
{
    //Variables used mostly for testing
    public bool check = false;

    [SerializeField] private Vector3 exampleOrigin;
    [SerializeField] private Vector3 exampleSize;
    [SerializeField] private Vector3 rotateBy;
    [SerializeField] private Vector3 centerOfVolume;
    [SerializeField] private Vector3 newCenterOfVolume;

    //Variables used for visualization, could be deleted.
    private List<Vector3> positions;
    private List<GameObject> gos = new List<GameObject>();


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

    public readonly Dictionary<string, BetterPhysics.Collider> Colliders =
        new Dictionary<string, BetterPhysics.Collider>();

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
        RegisterCollider("cubic-1", new BetterPhysics.CubicCollider(exampleOrigin, exampleSize, rotateBy, "cubic-1"));
        RegisterCollider("spherical-1", new BetterPhysics.SphericalCollider(new Vector3(100, 0, 0), 5f, "spherical-1"));

        //Prepare the worker threads => run CheckPointsCollisionAsnyc once.
        CheckPointsCollisionAsync(pointsManager.Points, new List<BetterPhysics.Collider>() { Colliders["cubic-1"], Colliders["spherical-1"] }, (b) => { });
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

    public BetterPhysics.Collider RegisterCollider(string id, BetterPhysics.Collider collider)
    {
        Colliders.Add(id, collider);
        return collider;
    }

    private List<Vector3> GetTempPoints(List<Point> points, int segmentStart, int segmentEnd)
    {
        List<Vector3> tempPoints = new List<Vector3>();
        for (int j = segmentStart; j < segmentEnd; j++)
        {
            if (j >= points.Count - 1) break;

            tempPoints.Add(points[j].position);
        }

        return tempPoints;
    }

    /// <summary>
    ///    Checks if collider collides with any of the provided points.
    /// </summary>
    /// <param name="points">List of all recorded points</param>
    /// <param name="collider">BetterPhysics Collider of any type</param>
    /// <param name="multithread">Indicates if the function should run as multithreaded or not.</param>
    /// <param name="collisionFound">Callback which gets sent index of affected point(s).</param>
    /// <param name="detectionMode">Indicates which detection mode should be used.</param>
    /// <returns>Boolean of whether any points from the list collides with the collider.</returns>
    public bool CheckPointsCollision(List<Point> points, BetterPhysics.Collider collider, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        //* This can be messy to read at first, but when you understand the core of this function, it's pretty easy to understand.
        //* The idea is to check if the collider collides with any of the points. This is already implemented with multithreading, so it's pretty fast (but messy).

        bool inVolume = false;
        List<int> affectedPoints = new List<int>();

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
            }
            else if (collider.GetType() == typeof(BetterPhysics.CubicCollider))
            {
                BetterPhysics.CubicCollider cube = (BetterPhysics.CubicCollider)collider;
                if (physics.IsPointInVolume(points[i].position, cube.Size, cube.Rotation, exampleOrigin))
                {
                    inVolume = true;
                    break;
                }
            }
        }

        if (detectionMode == DetectionMode.Enhanced)
        {
            foreach (int pointIndex in affectedPoints)
            {
                Debug.Log("invoking");
                collisionFound?.Invoke(pointIndex);
            }
        }

        Debug.Log(inVolume);
        return inVolume;
    }

    private IEnumerator HandleJobAsync(List<Point> points, BetterPhysics.Collider collider, Action<bool> jobFinishedCallback, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        bool inVolume = false;
        List<int> affectedPoints = new List<int>();
        //How many points fit into one job
        int jobBuffer = 500;

        List<Vector3> tempPoints = new List<Vector3>();

        //Create nativearray to store all the jobHandles, with the appropriate size.
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(points.Count / jobBuffer + 10, Allocator.Temp);

        Dictionary<int, bool> resultsWithIndices = new Dictionary<int, bool>();

        int jobIndex = 0;

        int jobsCompleted = 0;


        for (int i = 0; i < points.Count; i += jobBuffer)
        {
            int segmentStart = i;
            int segmentEnd = i + jobBuffer;

            tempPoints.Clear();
            tempPoints = GetTempPoints(points, segmentStart, segmentEnd);

            NativeArray<bool> _result = new NativeArray<bool>(jobBuffer, Allocator.TempJob);


            if (collider.GetType() == typeof(BetterPhysics.CubicCollider))
            {
                //* Cubical collider
                BetterPhysics.CubicCollider col = (BetterPhysics.CubicCollider)collider;

                CubicJob job = new CubicJob();
                job.points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                job.volumeDimensions = col.Size;
                job.rotation = col.Rotation;
                job.volumeCenter = col.Origin;
                job.result = _result;


                JobHelper.JobExecution execution = JobHelper.AddScheduledJob(job, job.Schedule(), (jobExecutor) =>
                {
                    for (int j = 0; j < _result.Length; j++)
                    {
                        resultsWithIndices.Add(segmentStart + j, _result[j]);
                    }

                    jobsCompleted++;
                    jobExecutor.Dispose();
                });
            }
            else if (collider.GetType() == typeof(BetterPhysics.SphericalCollider))
            {
                //* SphericalCollider
                BetterPhysics.SphericalCollider col = (BetterPhysics.SphericalCollider)collider;

                SphericalJob job = new SphericalJob();
                job.points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                job.volumeCenter = col.Origin;
                job.radius = col.Radius;
                job.result = _result;

                JobHelper.JobExecution execution = JobHelper.AddScheduledJob(job, job.Schedule(), (jobExecutor) =>
                {
                    for (int j = 0; j < _result.Length; j++)
                    {
                        resultsWithIndices.Add(segmentStart + j, _result[j]);
                    }

                    jobsCompleted++;
                    jobExecutor.Dispose();
                });
            }

            jobIndex++;
        }

        JobHandle.ScheduleBatchedJobs();
        JobHandle.CompleteAll(jobHandles);

        yield return new WaitWhile(() => jobsCompleted < jobIndex);
        //* Working with results
        if (detectionMode == DetectionMode.Simple)
        {
            int pointIndex = 0;
            foreach (KeyValuePair<int, bool> pointResult in resultsWithIndices)
            {
                // Debug.Log(pointResult + " - " + (segmentStart + pointIndex));
                if (pointResult.Value)
                {
                    inVolume = true;
                    //Convert the local pointIndex into the original point index
                    int originalIndex = pointResult.Key;
                    //Anything you want to do with the colliding point
                    collisionFound?.Invoke(originalIndex);
                    break;
                }

                pointIndex++;
            }
        }
        else if (detectionMode == DetectionMode.Enhanced)
        {
            //Gathers all the points that are inside the volume
            int pointIndex = 0;
            foreach (KeyValuePair<int, bool> pointResult in resultsWithIndices)
            {
                if (pointResult.Value)
                {
                    affectedPoints.Add(pointResult.Key);
                    if (!inVolume) inVolume = true;
                }

                pointIndex++;
            }

            foreach (int affectedIndex in affectedPoints)
            {
                collisionFound?.Invoke(affectedIndex);
            }
        }

        jobFinishedCallback?.Invoke(inVolume);
    }

    public void CheckPointsCollisionAsync(List<Point> points, BetterPhysics.Collider collider, Action<bool> jobFinishedCallback, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        StartCoroutine(HandleJobAsync(points, collider, jobFinishedCallback, collisionFound, detectionMode));
    }

    public void CheckPointsCollisionAsync(List<Point> points, List<BetterPhysics.Collider> colliders, Action<bool> jobFinishedCallback, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        //TODO: This is currently fairly useless, as it is reporting two results not dependant on each other. This should be rather rewritten so one bool is called in the callback.
        foreach (BetterPhysics.Collider col in colliders)
        {
            CheckPointsCollisionAsync(points, col, jobFinishedCallback, collisionFound, detectionMode);
        }
    }


    /// <summary>
    /// Returns string,bool dictionary, which contains id of the collider and whether it is colliding or not.
    /// </summary>
    /// <param name="points">List of all points</param>
    /// <param name="colliders">List of colliders</param>
    /// <param name="multithread">Whether to run the function multithread or not.</param>
    /// <param name="collisionFound">Callback which is sent the colliding point index.</param>
    /// <param name="detectionMode">Whether to send only one (the first colliding) point through the callback, or all that are colliding.</param>
    /// <returns></returns>
    public Dictionary<string, bool> CheckPointsCollision(List<Point> points, List<BetterPhysics.Collider> colliders, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        Dictionary<string, bool> collisionResults = new Dictionary<string, bool>();
        //Same logic as CheckPointsCollision, but with a list of colliders
        foreach (BetterPhysics.Collider collider in colliders)
        {
            bool collides = CheckPointsCollision(points, collider, (index) => Debug.Log(index + " is in."), detectionMode);
            collisionResults.Add(collider.Id, collides);
        }

        return collisionResults;
    }

    void Update()
    {
        //Interaction with editor (usecase)
        if (check)
        {
            check = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();


            CheckPointsCollisionAsync(pointsManager.Points, Colliders["spherical-1"], (b) =>
            {
                if (b)
                {
                    Debug.Log("Colliding");
                }
                else
                {
                    Debug.Log("Not Colliding");
                }
            }, (pointIndex) =>
            {
                Debug.Log("Point " + pointIndex + " is in the volume"); 
                
            }, DetectionMode.Simple);
            
            // Debug.Log(CheckPointsCollision(pointsManager.Points, Colliders["spherical-1"], (index) => Debug.Log(index + " is in."), DetectionMode.Simple));
            sw.Stop();

            Debug.Log("Finished checking " + pointsManager.Points.Count + " points in " + sw.ElapsedMilliseconds + " ms");
        }

        //Update the collider position => this doesn't need to be in Update(), but it could be at the place you move the collider from.
        BetterPhysics.Collider col;
        bool hasCollider = Colliders.TryGetValue("cubic-1", out col);
        if (hasCollider)
        {
            BetterPhysics.CubicCollider c = (BetterPhysics.CubicCollider)col;
            //exampleOrigin is lower left corner => convert it to center
            c.Origin = new Vector3(exampleOrigin.x + c.Size.x / 2, exampleOrigin.y + c.Size.y / 2, exampleOrigin.z + c.Size.z / 2);
            c.Size = exampleSize;
            c.Rotation = rotateBy;
        }

        //Visualization
        RebuildVolume();
    }
}