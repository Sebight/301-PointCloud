using System;
using UnityEngine;

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
    public bool IsPointInVolume(Vector3 point, Vector3 volumeDimensions, Vector3 rotation, Vector3 volumeCenter)
    {
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
    public bool IsPointInVolume(Vector3 point, Vector3 volumeCenter, float radius)
    {
        // float centerToPointDistance = Vector3.Distance(point, volumeCenter);
        // return (centerToPointDistance <= radius);

        float minX = volumeCenter.x - radius;
        float maxX = volumeCenter.x + radius;
        float minY = volumeCenter.y - radius;
        float maxY = volumeCenter.y + radius;
        float minZ = volumeCenter.z - radius;
        float maxZ = volumeCenter.z + radius;

        bool x = minX <= point.x && point.x <= maxX;
        bool y = minY <= point.y && point.y <= maxY;
        bool z = minZ <= point.z && point.z <= maxZ;

        return (x && y && z);
    }
}