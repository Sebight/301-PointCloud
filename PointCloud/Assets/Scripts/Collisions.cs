using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


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

    private Vector3 volumeToPointDirection;

    BetterPhysics bp = new BetterPhysics();

    List<Vector3> positions;
    private Vector3 centerOfVolume;

    private List<GameObject> gos = new List<GameObject>();

    private Vector3 dir;

    private Vector3 newCenterOfVolume;

    private System.Action<bool> callback;

    public void ReturnBack()
    {
        exampleOrigin = new Vector3(0, 0, 0);
        examplePoint.transform.position = new Vector3(0, 0, 0);
        //DrawVolume();
    }

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
        callback = (isIn) =>
        {
            if (isIn)
            {
                examplePoint.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                examplePoint.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        };
        dir = examplePoint.transform.position - exampleOrigin;
        // exampleOrigin = transform.position;
        volumeToPointDirection = examplePoint.transform.position - exampleOrigin;

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
        DrawVolume(positions);

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            pos = Quaternion.Euler(rotateBy) * (pos - centerOfVolume) + centerOfVolume;
            positions[i] = pos;
            // examplePoint.transform.position = Quaternion.Euler(rotateBy) * (examplePoint.transform.position - centerOfVolume) + centerOfVolume;
        }

        exampleOrigin = positions[0];
        DrawVolume(positions);
    }

    void Update()
    {
        // DrawVolume();
        // transform.position = Quaternion.Euler(rotateBy) * transform.position;

        //Keep the point in the same posiiton relative to the volume
        // examplePoint.transform.position = volume.transform.position + volume.transform.TransformVector(dir);
        // examplePoint.transform.rotation = volume.transform.rotation;


        if (move)
        {
            move = false;
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 pos = positions[i];
                pos = Quaternion.Euler(rotateBy) * (pos - centerOfVolume) + centerOfVolume;
                positions[i] = pos;
                // examplePoint.transform.position = Quaternion.Euler(rotateBy) * (examplePoint.transform.position - centerOfVolume) + centerOfVolume;
            }

            exampleOrigin = positions[0];
            DrawVolume(positions);
        }

        if (back)
        {
            // back = false;
            // for (int i = 0; i < positions.Count; i++)
            // {
            //     Vector3 pos = positions[i];
            //     pos = Quaternion.Inverse(Quaternion.Euler(rotateBy)) * pos;
            //     positions[i] = pos;
            //     // examplePoint.transform.position = Quaternion.Inverse(Quaternion.Euler(rotateBy)) * (examplePoint.transform.position - gos[0].transform.position) + gos[0].transform.position;
            // }
            //
            // exampleOrigin = positions[0];
            // DrawVolume(positions);

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
            check = false;
            // Vector3 pointPos = examplePoint.transform.position;
            // Task checkTask = new Task(() =>
            // {
            //     BetterPhysics physics = new BetterPhysics();
            //     bool isPointInVolume = physics.IsPointInVolume(pointPos, exampleOrigin, exampleSize, rotateBy);
            //     Debug.Log(isPointInVolume);
            // });
            // checkTask.Start();
        }

        Vector3 pointPos = examplePoint.transform.position;
        BetterPhysics physics = new BetterPhysics();
        bool isPointInVolume = physics.IsPointInVolume(pointPos, exampleOrigin, exampleSize, rotateBy);
        callback.Invoke(isPointInVolume);

        // examplePoint.transform.position = gos[0].transform.position + gos[0].transform.TransformVector(dir);
    }
}