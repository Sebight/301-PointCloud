using System;
using UnityEngine;

public class BetterPhysics
{
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
}