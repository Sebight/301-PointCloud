using System.Collections.Generic;
using System.Diagnostics;
using BP;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MyCollisions : MonoBehaviour
{
    public PointsManager pointsManager;
    public Collisions collisions;

    void Start()
    {
        //Duplicated human IDs is a bad practise
        //Having creation and registering as a separate methods AND a oneliner is bad pracise. Mainly, the "RegisterCollider" doesn't have to return the insance it registered.
        var sphere = new PointSphericalCollider(new Vector3(120, 0, 0), 5f, "kulatak");
        //Having another type called Collider may confuse other devs.
        // collisions.RegisterCollider(sphere);

        var cube = new PointSphericalCollider(new Vector3(180, 180, 180), 5f, "krabičák");
        // collisions.RegisterCollider(cube);

        //Perhaps more convinient would be to implement the registration like so:
        collisions.RegisterCollider(cube, sphere);
    }

    [ContextMenu("Mock - print out collision")]
    void Mock_PrintOutCollision()
    {
        /*
        List<Collider> collidersToCheck = new List<Collider>() { collisions.Colliders["můjCollider"], collisions.Colliders["můjCollider2"] };
        Dictionary<string, bool> colliderResults = collisions.CheckPointsCollision(pointsManager.Points, collidersToCheck);
        
        foreach (var result in colliderResults)
        {
            Debug.Log(result.Key + ": " + result.Value);
        }
        */

        string id = "cubic-1";
        Stopwatch sw = new Stopwatch();
        sw.Start();

        List<PointCollider> colliders = new()
        {
            collisions.Colliders["kulatak"],
            collisions.Colliders["krabičák"]
        };
        
        collisions.CheckPointsCollisionAsync(pointsManager.Points, colliders, (collides) =>
        {
            string collisionStatus = collides ? "collides" : "does not collide";
            Debug.Log($"{id} " + collisionStatus);
        }, detectionMode: DetectionMode.Simple);
        sw.Stop();
        Debug.Log("Checking done. Took: " + sw.ElapsedMilliseconds + " ms");
    }
}