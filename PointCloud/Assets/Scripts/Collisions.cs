using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using Debug = UnityEngine.Debug;


public struct VolumeJob : IJob
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

public class Collisions : MonoBehaviour
{
    private bool move = false;
    public bool back = false;

    public bool check = false;

    public GameObject examplePoint;
    public GameObject volume;

    public Vector3 exampleOrigin;
    public Vector3 exampleSize;
    public Vector3 rotateBy;


    List<Vector3> positions;
    private Vector3 centerOfVolume;
    private List<GameObject> gos = new List<GameObject>();

    private Vector3 newCenterOfVolume;

    private System.Action<bool> callback;

    public PointsSpawner pointsSpawner;

    BetterPhysics physics = new BetterPhysics();

    private int currentSegment = 0;

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

    public Vector3 DrawNewVolume(Vector3 size)
    {
        Vector3 origin = new Vector3(0, 0, 0);
        float startX = origin.x;
        float endX = origin.x + size.x;

        float startY = origin.y;
        float endY = origin.y + size.y;

        float startZ = origin.z;
        float endZ = origin.z + size.z;

        positions = new List<Vector3>()
        {
            new Vector3(startX, startY, startZ), new Vector3(endX, startY, startZ), new Vector3(startX, endY, startZ),
            new Vector3(startX, startY, endZ), new Vector3(endX, endY, startZ), new Vector3(startX, endY, endZ),
            new Vector3(endX, endY, endZ), new Vector3(endX, startY, endZ)
        };

        foreach (var position in positions)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            if (position == exampleOrigin) cube.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }

        newCenterOfVolume = new Vector3((startX + endX) / 2, (startY + endY) / 2, (startZ + endZ) / 2);
        return newCenterOfVolume;
    }

    void Start()
    {
        callback = (bool isIn) =>
        {
            if (isIn)
            {
                foreach (var volumeCorner in gos)
                {
                    volumeCorner.GetComponent<MeshRenderer>().material.color = Color.green;
                }
            }
            else
            {
                foreach (var volumeCorner in gos)
                {
                    volumeCorner.GetComponent<MeshRenderer>().material.color = Color.red;
                }
            }
        };

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
    }

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

    private void ColorVolume(bool isIn)
    {
        if (isIn)
        {
            foreach (var volumeCorner in gos)
            {
                volumeCorner.GetComponent<MeshRenderer>().material.color = Color.green;
            }
        }
        else
        {
            foreach (var volumeCorner in gos)
            {
                volumeCorner.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
    }

    public void Check() => check = true;

    void Update()
    {
        if (back)
        {
            //NEW SOLUTION
            Vector3 newVolumeCenter = DrawNewVolume(exampleSize);

            //Get the old volume direction between the center and the point
            Vector3 oldVolumeDirection = examplePoint.transform.position - centerOfVolume;

            //Place the point at the new center with direction of the old volume direction
            examplePoint.transform.position = newVolumeCenter + oldVolumeDirection;
            examplePoint.transform.position =
                Quaternion.Inverse(Quaternion.Euler(rotateBy)) * (examplePoint.transform.position - newVolumeCenter) +
                newVolumeCenter;

            back = false;
        }


        if (check)
        {
            // Timeslicing + parallelization  + check every x (maybe 4) seconds
            //How many poitns fit into one job
            int jobBuffer = 500;
            check = false;
            Stopwatch sw = new Stopwatch();

            // NativeArray<Vector3> tempPoints = new NativeArray<Vector3>(1, Allocator.TempJob);

            // tempPoints[0] = pointsSpawner.points[0].position;
            //
            // NativeArray<bool> _result = new NativeArray<bool>(500, Allocator.TempJob);
            // VolumeJob job = new VolumeJob();
            // job.points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
            // job.volumeDimensions = exampleSize;
            // job.rotation = rotateBy;
            // job.volumeCenter = centerOfVolume;
            // job.result = _result;
            //
            // JobHandle handle = job.Schedule();
            // handle.Complete();
            // _result.Dispose();
            // job.points.Dispose();
            
            
            // sw.Start();

            //Loop through segments with size of jobBuffer through points
            for (int i = 0; i < pointsSpawner.points.Count; i += jobBuffer)
            {
                currentSegment++;
                List<Vector3> tempPoints = new List<Vector3>();
                int segmentStart = i;
                int segmentEnd = i + jobBuffer;
                for (int j = segmentStart; j < segmentEnd; j++)
                {
                    tempPoints.Add(pointsSpawner.points[j].position);
                }
            
                NativeArray<bool> _result = new NativeArray<bool>(500, Allocator.TempJob);
                VolumeJob job = new VolumeJob();
                job.points = new NativeArray<Vector3>(tempPoints.ToArray(), Allocator.TempJob);
                job.volumeDimensions = exampleSize;
                job.rotation = rotateBy;
                job.volumeCenter = centerOfVolume;
                job.result = _result;
            
                JobHandle handle = job.Schedule();
                handle.Complete();
                job.points.Dispose();
            
                bool inVolume = false;
                foreach (bool pointResult in _result)
                {
                    if (pointResult)
                    {
                        inVolume = true;
                        break;
                    }
                }
                _result.Dispose();
                Debug.Log(inVolume);
            }
            // sw.Stop();

            UnityEngine.Debug.Log("Finished checking " + pointsSpawner.points.Count + " points in " + sw.ElapsedMilliseconds + " ms");


            //----
            //Visualization
            //----
            // if (physics.IsPointInVolume(examplePoint.transform.position, exampleSize, rotateBy, centerOfVolume))
            // {
            //     ColorVolume(true);
            // }
            // else
            // {
            //     ColorVolume(false);
            // }
        }

        RebuildVolume();
    }
}