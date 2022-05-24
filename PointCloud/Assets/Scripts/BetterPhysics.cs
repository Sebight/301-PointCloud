using System;
using UnityEngine;

namespace BP
{
    //"BetterPhysics" is a "wrong" name, it actually contains WorserPhysics then anything else
    //Perhaps more fitting names are: "SimplePhysics", "PointPhysics", "VolumeHelper", "VolumePhysics"..

    //* Sub classes as "Collider" and their derivatives might actually benefit to be in the same scope as the parent class
    //* This would help the implementation a lot since it now has to get the static type via writing the parent class first

    //IsInVolume has a second override and which changes has different definition fo the "volume". 
    //Since we tend to implement other shapes, the name should be more obvious. I promose to separate the methods with different names:
    //"IsPointInCube", "IsPointInSphere"

    //Last note, if we have a class for Collider definition we perhaps don't need a separate "BetterPhysics" to "host" the volume logic.
    //It is not static although, it doesn't have any inner state

    //BetterPhysics could be static and contain the implementation of the "IsPointInVolumeXYZ" concept
    //OR the implementation could be directly in the Collider definition
    //Both options would be more "clean"

    [Serializable]
    public class PointCollider
    {
        public string Id;

        public PointCollider(string id)
        {
            Id = id;
        }
    }
    
    [Serializable]
    public class PointCubicCollider : PointCollider
    {
        public Vector3 Origin;
        public Vector3 Size;
        public Vector3 Rotation;

        public PointCubicCollider(Vector3 origin, Vector3 size, Vector3 rotation, string id) : base(id)
        {
            Origin = origin;
            Size = size;
            Rotation = rotation;
        }
    }

    [Serializable]
    public class PointSphericalCollider : PointCollider
    {
        public Vector3 Origin;
        public float Radius;

        public PointSphericalCollider(Vector3 origin, float radius, string id) : base(id)
        {
            Origin = origin;
            Radius = radius;
        }
    }

    [Serializable]
    public class BetterPhysics
    {
        /// <summary>
        /// Calculates if the point is within the rectangular volume defined from the parameters.
        /// </summary>
        /// <param name="point">Vector3 point which we want to work with.</param>
        /// <param name="volumeDimensions">Size of volume</param>
        /// <param name="rotation">Rotation of volume</param>
        /// <param name="volumeCenter">Center of volume</param>
        /// <returns>Boolean of if point is or is not in constructed volume.</returns>
        public static bool IsPointInVolume(Vector3 point, Vector3 volumeDimensions, Vector3 rotation, Vector3 volumeCenter)
        {
            if (volumeDimensions == Vector3.zero) return false;
            
            // Calculate the original direction
            Vector3 originalDirection = point - volumeCenter;

            // New volume with reset origin
            Vector3 newCenterOfVolume = new Vector3(0, 0, 0);

            float newStartX = newCenterOfVolume.x - volumeDimensions.x / 2;
            float newEndX = newCenterOfVolume.x + volumeDimensions.x / 2;

            float newStartY = newCenterOfVolume.y - volumeDimensions.y / 2;
            float newEndY = newCenterOfVolume.y + volumeDimensions.y / 2;

            float newStartZ = newCenterOfVolume.z - volumeDimensions.z / 2;
            float newEndZ = newCenterOfVolume.z + volumeDimensions.z / 2;

            // Place the point in correct position
            Vector3 translatedPointPosition = newCenterOfVolume + originalDirection;
            translatedPointPosition = Quaternion.Inverse(Quaternion.Euler(rotation)) * (translatedPointPosition - newCenterOfVolume) + newCenterOfVolume;

            //Simple dimensions check
            bool x = false;
            bool y = false;
            bool z = false;

            if (newStartX <= translatedPointPosition.x && translatedPointPosition.x <= newEndX) x = true;

            if (newStartY <= translatedPointPosition.y && translatedPointPosition.y <= newEndY) y = true;

            if (newStartZ <= translatedPointPosition.z && translatedPointPosition.z <= newEndZ) z = true;

            return (x && y && z);
        }

        /// <summary>
        /// Calculates if the point is within the spherical volume defined from the parameters.
        /// </summary>
        /// <param name="point">Vector3 point in world-space which we calculate with.</param>
        /// <param name="volumeCenter">Volume center in world-space.</param>
        /// <param name="radius">Distance from center to border of sphere.</param>
        /// <returns>Boolean of whether point is within volume or not.</returns>
        public static bool IsPointInVolume(Vector3 point, Vector3 volumeCenter, float radius)
        {
            if (radius == 0.0f) return false;
            
            float centerToPointDistance = Vector3.Distance(point, volumeCenter);
            return (centerToPointDistance <= radius);
        }
    }
}