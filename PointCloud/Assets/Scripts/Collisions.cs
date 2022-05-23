using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Debug = UnityEngine.Debug;
using BP;

public struct CubicJob : IJob, JobHelper.IJobDisposable
{
    public NativeArray<Vector3> Points;
    public Vector3 VolumeDimensions;
    public Vector3 Rotation;
    public Vector3 VolumeCenter;
    public NativeArray<bool> Result;

    public void Execute()
    {
        BetterPhysics physics = new BetterPhysics();
        for (int i = 0; i < Points.Length; i++)
        {
            bool isIn = physics.IsPointInVolume(Points[i], VolumeDimensions, Rotation, VolumeCenter);
            Result[i] = isIn;
        }
    }

    public void OnDispose()
    {
        Points.Dispose();
        Result.Dispose();
    }
}


public struct SphericalJob : IJob, JobHelper.IJobDisposable
{
    public NativeArray<Vector3> Points;
    public Vector3 VolumeCenter;
    public float Radius;
    public NativeArray<bool> Result;

    public void Execute()
    {
        BetterPhysics physics = new BetterPhysics();
        for (int i = 0; i < Points.Length; i++)
        {
            bool isIn = physics.IsPointInVolume(Points[i], VolumeCenter, Radius);
            Result[i] = isIn;
        }
    }

    public void OnDispose()
    {
        Points.Dispose();
        Result.Dispose();
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

    public PointsManager pointsManager;

    public readonly Dictionary<string, BetterPhysics.Collider> Colliders = new Dictionary<string, BetterPhysics.Collider>();

    void Start()
    {
        //REFACTOR: same comments as in MyCollisions.cs

        //Register colliders
        RegisterCollider(new BetterPhysics.CubicCollider(exampleOrigin, exampleSize, rotateBy, "cubic-1"));
        RegisterCollider(new BetterPhysics.SphericalCollider(new Vector3(100, 0, 0), 5f, "spherical-1"));

        //Why is this depracated? What is the reason?

        //Prepare the worker threads => run CheckPointsCollisionAsnyc once.
        List<BetterPhysics.Collider> colliders = new() 
        {
            Colliders["cubic-1"], 
            Colliders["spherical-1"] 
        };

        CheckPointsCollisionAsync(pointsManager.Points, colliders,  (b) => { });
    }

    /// <summary>
    /// Registers new collider.
    /// </summary>
    /// <param name="id">Unique indetification of newly created collider.</param>
    /// <param name="collider">Collider object</param>
    /// <returns>Newly created collider.</returns>
    public BetterPhysics.Collider RegisterCollider(BetterPhysics.Collider collider)
    {
        Colliders.Add(collider.Id, collider);
        return collider;
    }
    public void RegisterCollider(params BetterPhysics.Collider[] colliders)
    {
        foreach (var x in colliders)
            RegisterCollider(x);
    }

    /// <summary>
    /// Creates new List of points which are suppoed to be passed into a job.
    /// </summary>
    /// <param name="points">List of all points</param>
    /// <param name="segmentStart">First index of the desired points.</param>
    /// <param name="segmentEnd">Last index of the desired points</param>
    /// <returns></returns>
    private List<Vector3> GetTempPoints(List<Point> points, int segmentStart, int segmentEnd)
    {
        //TODO: Could be written using Range.
        List<Vector3> tempPoints = new List<Vector3>();
        for (int j = segmentStart; j < segmentEnd; j++)
        {
            if (j >= points.Count - 1) break;

            tempPoints.Add(points[j].Position);
        }

        return tempPoints;
    }

    /// <summary>
    ///    Checks if collider collides with any of the provided points.
    /// </summary>
    /// <param name="points">List of all recorded points</param>
    /// <param name="collider">BetterPhysics Collider of any type</param>
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
                if (physics.IsPointInVolume(points[i].Position, sphere.Origin, sphere.Radius))
                {
                    inVolume = true;
                    break;
                }
            }
            else if (collider.GetType() == typeof(BetterPhysics.CubicCollider))
            {
                BetterPhysics.CubicCollider cube = (BetterPhysics.CubicCollider)collider;
                if (physics.IsPointInVolume(points[i].Position, cube.Size, cube.Rotation, exampleOrigin))
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

    /// <summary>
    /// Calculater whether any of the colliders from the colliders list collides with any of the points from the points list.
    /// </summary>
    /// <param name="points">List of all points</param>
    /// <param name="colliders">List of colliders we want to check</param>
    /// <param name="collisionFound">Callback when collision is detected</param>
    /// <param name="detectionMode">Whether to deep search all colliding points or only the only one.</param>
    /// <returns>Dictionary of pattern <colliderId, collides></returns>
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
                job.Points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                job.VolumeDimensions = col.Size;
                job.Rotation = col.Rotation;
                job.VolumeCenter = col.Origin;
                job.Result = _result;


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
                job.Points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                job.VolumeCenter = col.Origin;
                job.Radius = col.Radius;
                job.Result = _result;

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

    /// <summary>
    /// Checks if collider collides with given points on multiple threads at once.
    /// </summary>
    /// <param name="points">List of all points</param>
    /// <param name="collider">Collider we want to work with</param>
    /// <param name="jobFinishedCallback">Lamdba function which is sent the "final" boolean of whether the colliders collides with the points or not.</param>
    /// <param name="collisionFound">Lambda function of what to do when new collision(s) is/are detected.</param>
    /// <param name="detectionMode">Whether to provide the collisionFound callback with deep search or only first collision.</param>
    public void CheckPointsCollisionAsync(List<Point> points, BetterPhysics.Collider collider, Action<bool> jobFinishedCallback, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        StartCoroutine(HandleJobAsync(points, collider, jobFinishedCallback, collisionFound, detectionMode));
    }

    /// <summary>
    /// Checks if colliders collide with given points on multiple threads at once. 
    /// </summary>
    /// <param name="points">List of all points</param>
    /// <param name="colliders">List of colliders</param>
    /// <param name="jobFinishedCallback">Lamdba function which is sent the "final" boolean of whether the colliders collides with the points or not.</param>
    /// <param name="collisionFound">Lambda function of what to do when new collision(s) is/are detected.</param>
    /// <param name="detectionMode">Whether to provide the collisionFound callback with deep search or only first collision.</param>
    [Obsolete("Currently useless.")]
    public void CheckPointsCollisionAsync(List<Point> points, List<BetterPhysics.Collider> colliders, Action<bool> jobFinishedCallback, Action<int> collisionFound = null, DetectionMode detectionMode = DetectionMode.Simple)
    {
        //TODO: This is currently fairly useless, as it is reporting two results not dependant on each other. This should be rather rewritten so one bool is sent in the callback.
        foreach (BetterPhysics.Collider col in colliders)
        {
            CheckPointsCollisionAsync(points, col, jobFinishedCallback, collisionFound, detectionMode);
        }
    }

    /// <summary>
    /// Updates colliders meta data.
    /// </summary>
    /// <param name="id">id of collider we want to update.</param>
    /// <param name="colliderData">Collider object which contains data we want to set.</param>
    public void UpdateColliderData(string id, BetterPhysics.Collider colliderData)
    {
        BetterPhysics.Collider col;
        bool hasCollider = Colliders.TryGetValue(id, out col);
        if (hasCollider)
        {
            if (col.GetType() == typeof(BetterPhysics.CubicCollider))
            {
                BetterPhysics.CubicCollider cubicCol = (BetterPhysics.CubicCollider)col;
                BetterPhysics.CubicCollider colliderDataConverted = (BetterPhysics.CubicCollider)colliderData;
                cubicCol.Origin = colliderDataConverted.Origin;
                cubicCol.Size = colliderDataConverted.Size;
                cubicCol.Rotation = colliderDataConverted.Rotation;
                
                Colliders[id] = cubicCol;
            } else if (col.GetType() == typeof(BetterPhysics.SphericalCollider))
            {
                BetterPhysics.SphericalCollider sphericalCol = (BetterPhysics.SphericalCollider)col;
                BetterPhysics.SphericalCollider colliderDataConverted = (BetterPhysics.SphericalCollider)colliderData;
                sphericalCol.Origin = colliderDataConverted.Origin;
                sphericalCol.Radius = colliderDataConverted.Radius;
                
                Colliders[id] = sphericalCol;
            }
        }
    }

    void Update()
    {
        //Update the collider position => this doesn't need to be in Update(), but it could be at the place you move the collider from.
        UpdateColliderData("cubic-1", new BetterPhysics.CubicCollider(exampleOrigin, exampleSize, rotateBy, ""));
    }
}