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
    public Vector3 point;
    public Vector3 volumeDimensions;
    public Vector3 rotation;
    public Vector3 volumeCenter;
    public NativeArray<bool> result;

    public void Execute()
    {
        BetterPhysics physics = new BetterPhysics();
        bool isIn = physics.IsPointInVolume(point, volumeDimensions, rotation, volumeCenter);
        result[0] = isIn;
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
            check = false;
            Stopwatch sw = new Stopwatch();
            // for (int i = 0; i < pointsSpawner.points.Count; i++)
            // {
            //     Vector3 pointPos = pointsSpawner.points[i].position;
            //     sw.Start();
            //     bool isIn = physics.IsPointInVolume(pointPos, exampleSize, rotateBy, centerOfVolume);
            //     sw.Stop();
            //     ColorVolume(isIn);
            // }
            sw.Start();
            NativeArray<bool> _result = new NativeArray<bool>(1, Allocator.TempJob);
            VolumeJob job = new VolumeJob();
            job.point = pointsSpawner.points[0].position;
            job.volumeDimensions = exampleSize;
            job.rotation = rotateBy;
            job.volumeCenter = centerOfVolume;
            job.result = _result;

            JobHandle handle = job.Schedule();
            handle.Complete();
            Debug.Log(_result[0]);
            _result.Dispose();
            sw.Stop();
            
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