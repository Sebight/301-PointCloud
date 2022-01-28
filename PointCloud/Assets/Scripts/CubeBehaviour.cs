using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PointCloudLogger;

public class CubeBehaviour : MonoBehaviour
{

    public BoxCollider collider;

    public void CheckCollision(List<Point> points)
    {
        for (int i = 0; i < points.Count; ++i)
        {
            Point p = points[i];
            Vector3 colliderPos = collider.ClosestPoint(new Vector3(p.x, p.y, p.z));
            float distance = Vector3.Distance(colliderPos, new Vector3(p.x, p.y, p.z));
            Debug.Log(distance);
            if (distance < 2)
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            } else
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
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
