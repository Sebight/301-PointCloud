using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterPhysics
{
    public bool IsPointInVolume(Vector3 point, Vector3 volumeOrigin, Vector3 volumeDimensions)
    {
        float startX = volumeOrigin.x;
        float endX = volumeOrigin.x + volumeDimensions.x;

        float startY = volumeOrigin.y;
        float endY = volumeOrigin.y + volumeDimensions.y;

        float startZ = volumeOrigin.z;
        float endZ = volumeOrigin.z + volumeDimensions.z;

        bool x = false;
        bool y = false;
        bool z = false;

        if (startX < point.x && point.x < endX) x = true;

        if (startY < point.y && point.y < endY) y = true;

        if (startZ < point.z && point.z < endZ) z = true;

        return (x && y && z);
    }
}

public class Collisions : MonoBehaviour
{
    public bool move = false;
    public bool back = false;

    public GameObject examplePoint;
    public GameObject volume;

    public Vector3 exampleOrigin;
    public Vector3 exampleSize;
    public Vector3 rotateBy;

    public Vector3 volumeToPointDirection;

    BetterPhysics bp = new BetterPhysics();

    List<Vector3> positions;
    private Vector3 centerOfVolume;

    private List<GameObject> gos = new List<GameObject>();

    public Vector3 dir;

    public Vector3 newCenterOfVolume;

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

        positions = new List<Vector3>() {new Vector3(startX, startY, startZ), new Vector3(endX, startY, startZ), new Vector3(startX, endY, startZ), new Vector3(startX, startY, endZ), new Vector3(endX, endY, startZ), new Vector3(startX, endY, endZ), new Vector3(endX, endY, endZ), new Vector3(endX, startY, endZ)};

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
        dir = examplePoint.transform.position - exampleOrigin;
        // exampleOrigin = transform.position;
        volumeToPointDirection = examplePoint.transform.position - exampleOrigin;

        float startX = exampleOrigin.x;
        float endX = exampleOrigin.x + exampleSize.x;

        float startY = exampleOrigin.y;
        float endY = exampleOrigin.y + exampleSize.y;

        float startZ = exampleOrigin.z;
        float endZ = exampleOrigin.z + exampleSize.z;

        positions = new List<Vector3>() {new Vector3(startX, startY, startZ), new Vector3(endX, startY, startZ), new Vector3(startX, endY, startZ), new Vector3(startX, startY, endZ), new Vector3(endX, endY, startZ), new Vector3(startX, endY, endZ), new Vector3(endX, endY, endZ), new Vector3(endX, startY, endZ)};
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

            examplePoint.transform.position = Quaternion.Inverse(Quaternion.Euler(rotateBy)) * (examplePoint.transform.position - newVolumeCenter) + newVolumeCenter;

            back = false;
        }

        if (bp.IsPointInVolume(examplePoint.transform.position, exampleOrigin, exampleSize))
        {
            examplePoint.GetComponent<Renderer>().material.color = Color.red;
        }
        else if (bp.IsPointInVolume(examplePoint.transform.position, newCenterOfVolume, exampleSize))
        {
            examplePoint.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            examplePoint.GetComponent<Renderer>().material.color = Color.black;
        }


        // examplePoint.transform.position = gos[0].transform.position + gos[0].transform.TransformVector(dir);
    }
}