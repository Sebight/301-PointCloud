using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PointCloudLogger;

public class CubeBehaviour : MonoBehaviour
{

    public BoxCollider collider;

    public bool IsPointInCollider(Collider other, Vector3 point)
    {
        if (other.ClosestPoint(point) == point)
        {
            return true;
        }

        return false;
    }

    public void CheckCollision(List<PointWrap> points)
    {
        int pointsAffected = 0;
        for (int i = 0; i < points.Count; ++i)
        {
            #region OldCeck                
            // Point p = points[i].acualPoint;
            // Vector3 colliderPos = collider.ClosestPoint(new Vector3(p.x, p.y, p.z));
            // float distance = Vector3.Distance(colliderPos, new Vector3(p.x, p.y, p.z));
            // if (distance < 0.1)
            // {
            //     points[i].go.GetComponent<MeshRenderer>().material.color = Color.green;
            //     gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            // } else
            // {
            //     points[i].go.GetComponent<MeshRenderer>().material.color = Color.gray;
            //     gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            // }
            #endregion

            if (IsPointInCollider(collider, new Vector3(points[i].acualPoint.x, points[i].acualPoint.y, points[i].acualPoint.z)))
            {
                points[i].go.GetComponent<MeshRenderer>().material.color = Color.blue;
                pointsAffected++;
                Debug.Log("yeee");
            }
            else
            {
                points[i].go.GetComponent<MeshRenderer>().material.color = Color.gray;
            }
        }

        if (pointsAffected > 0)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        }

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }
}
