using System.Diagnostics;
using BP;
using UnityEngine;
using Collider = BP.BetterPhysics.Collider; //Consider a different name. Perhaps one of these: "DataCollider", "SimpleCollider", "PointCollider", "Volume", "ARCollider" ... 
using Debug = UnityEngine.Debug;

public class MyCollisions : MonoBehaviour
{
    public PointsManager pointsManager;
    public Collisions collisions;

    void Start()
    {
        //Duplicated human IDs is a bad practise
        //Having creation and registering as a separate methods AND a oneliner is bad pracise. Mainly, the "RegisterCollider" doesn't have to return the insance it registered.
        var sphere = new BetterPhysics.SphericalCollider(new Vector3(0, 0, 0), 5f, "nejkulaťoulinkatější");
        //Having another type called Collider may confuse other devs.
        collisions.RegisterCollider(sphere);

        var cube = new BetterPhysics.SphericalCollider(new Vector3(180, 180, 180), 5f, "krabičák");
        collisions.RegisterCollider(cube);

        //Perhaps more convinient would be to implement the registration like so:
        //collisions.RegisterCollider(cube, sphere); //<== uncomment me
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
        collisions.CheckPointsCollisionAsync(pointsManager.Points, collisions.Colliders[id], (collides) =>
        {
            string collisionStatus = collides ? "collides" : "does not collide";
            Debug.Log($"{id} " + collisionStatus);
        }, detectionMode: DetectionMode.Enhanced);
        sw.Stop();
        Debug.Log("Checking done. Took: " + sw.ElapsedMilliseconds + " ms");
    }
}