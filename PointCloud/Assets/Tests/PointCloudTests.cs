using System.Collections;
using System.Collections.Generic;
using BP;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PointCloudTests
{
    private Collisions _collisions;

    [SetUp]
    public void Setup()
    {
        _collisions = GameObject.Find("Collisions").GetComponent<Collisions>();
    }
    
    [UnityTest]
    public IEnumerator IsPointInVolume_ReturnsTrueWithValidCubicCollider()
    {
        Vector3 point = new Vector3(0, 0, 0);
        Vector3 volumeDimensions = new Vector3(1, 1, 1);
        Vector3 volumeOrigin = new Vector3(0, 0, 0);
        Vector3 volumeRotation = new Vector3(0, 0, 0);
        bool result = BetterPhysics.IsPointInVolume(point, volumeDimensions, volumeRotation, volumeOrigin);
        yield return null;
        Assert.AreEqual(true, result);
    }

    [UnityTest]
    public IEnumerator IsPointInVolume_ReturnsFalseWithNoInvalidSphericalCollider()
    {
        Vector3 point = new Vector3(0, 0, 0);
        float radius = 0f;
        Vector3 volumeOrigin = new Vector3(0, 0, 0);

        bool result = BetterPhysics.IsPointInVolume(point, volumeOrigin, radius);
        yield return null;
        Assert.AreEqual(false, result);
    }
    
    [UnityTest]
    public IEnumerator IsPointInVolume_ReturnsTrueWithValidSphericalCollider()
    {
        Vector3 point = new Vector3(0, 0, 0);
        float radius = 1;
        Vector3 volumeOrigin = new Vector3(0, 0, 0);

        bool result = BetterPhysics.IsPointInVolume(point, volumeOrigin, radius);
        yield return null;
        Assert.AreEqual(true, result);
    }
    
    [UnityTest]
    public IEnumerator IsPointInVolume_ReturnsFalseWithInvalidCubicCollider()
    {
        Vector3 point = new Vector3(0, 0, 0);
        Vector3 volumeDimensions = new Vector3(0, 0, 0);
        Vector3 volumeOrigin = new Vector3(0, 0, 0);
        Vector3 volumeRotation = new Vector3(0, 0, 0);
        bool result = BetterPhysics.IsPointInVolume(point, volumeDimensions, volumeRotation, volumeOrigin);
        yield return null;
        Assert.AreEqual(false, result);
    }


    [UnityTest]
    public IEnumerator RegisterCollider_EmptyId()
    {
        PointCubicCollider col = new PointCubicCollider(Vector3.zero, Vector3.zero, Vector3.zero, "");
        yield return null;
        _collisions.RegisterCollider();
    }
    
    [UnityTest]
    public IEnumerator UpdateColliderData_EmptyId()
    {
        PointCubicCollider col = new PointCubicCollider(Vector3.zero, Vector3.zero, Vector3.zero, "");
        yield return null;
        Assert.That(() => _collisions.UpdateColliderData("", new PointCubicCollider(Vector3.zero, Vector3.zero, Vector3.zero, "")), Throws.Exception);
    }
}