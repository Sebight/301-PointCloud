using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BP;
using UnityEngine;
using Collider = BP.BetterPhysics.Collider;
using Debug = UnityEngine.Debug;

public class MyCollisions : MonoBehaviour
{
    public PointsManager pointsManager;
    public Collisions collisions;

    [SerializeField] private bool check;

    // Start is called before the first frame update
    void Start()
    {
        Collider collider = collisions.RegisterCollider("můjCollider", new BetterPhysics.SphericalCollider(new Vector3(0, 0, 0), 5f, "můjCollider"));
        collisions.RegisterCollider("můjCollider2", new BetterPhysics.SphericalCollider(new Vector3(180, 180, 180), 5f, "můjCollider2"));
    }

    // Update is called once per frame
    void Update()
    {
        if (check)
        {
            check = false;

            List<Collider> collidersToCheck = new List<Collider>() { collisions.Colliders["můjCollider"], collisions.Colliders["můjCollider2"] };
            Dictionary<string, bool> colliderResults = collisions.CheckPointsCollision(pointsManager.Points, collidersToCheck);

            foreach (var result in colliderResults)
            {
                Debug.Log(result.Key + ": " + result.Value);
            }
        }
    }
}