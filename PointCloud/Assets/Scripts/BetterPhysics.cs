using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class BetterPhysics
{
    public bool IsPointInVolume(Vector3 point, Vector3 volumeOrigin, Vector3 volumeDimensions, Vector3 rotation, Vector3 volumeCenter)
    {
        // Get the starting positions of volume
        //Rewrtie via volumeCenter
        float startX = volumeOrigin.x;
        float endX = volumeOrigin.x + volumeDimensions.x;

        float startY = volumeOrigin.y;
        float endY = volumeOrigin.y + volumeDimensions.y;

        float startZ = volumeOrigin.z;
        float endZ = volumeOrigin.z + volumeDimensions.z;
        
        // New volume with reset origin
        Vector3 origin = new Vector3(0, 0, 0);
        
        float newStartX = origin.x;
        float newEndX = origin.x + volumeDimensions.x;

        float newStartY = origin.y;
        float newEndY = origin.y + volumeDimensions.y;

        float newStartZ = origin.z;
        float newEndZ = origin.z + volumeDimensions.z;
        
        
        //Calculate the new center and place the point in the correct position
        Vector3 centerOfOriginalVolume = volumeCenter;
        Vector3 originalDirection = point - centerOfOriginalVolume;
        
        
        Vector3 newCenterOfVolume = new Vector3((newStartX + newEndX) / 2, (newStartY + newEndY) / 2, (newStartZ + newEndZ) / 2);
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
