using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Diagnostics;

public class BetterPhysics
{
    public bool IsPointInVolume(Vector3 point, Vector3 volumeOrigin, Vector3 volumeDimensions, Vector3 rotation)
    {
        Stopwatch s = new Stopwatch();
        s.Start();
        
        // Get the starting positions of volume
        
        float startX = volumeOrigin.x;
        float endX = volumeOrigin.x + volumeDimensions.x;

        float startY = volumeOrigin.y;
        float endY = volumeOrigin.y + volumeDimensions.y;

        float startZ = volumeOrigin.z;
        float endZ = volumeOrigin.z + volumeDimensions.z;
        
        Vector3 centerOfOriginalVolume = new Vector3((startX + endX) / 2, (startY + endY) / 2, (startZ + endZ) / 2);
        Vector3 originalDirection = point - centerOfOriginalVolume;
        
        // New volume with reset origin
        Vector3 origin = new Vector3(0, 0, 0);
        
        float newStartX = origin.x;
        float newEndX = origin.x + volumeDimensions.x;

        float newStartY = origin.y;
        float newEndY = origin.y + volumeDimensions.y;

        float newStartZ = origin.z;
        float newEndZ = origin.z + volumeDimensions.z;
        
        //Calculate the new center and place the point in the correct position
        Vector3 newCenterOfVolume = new Vector3((newStartX + newEndX) / 2, (newStartY + newEndY) / 2, (newStartZ + newEndZ) / 2);
        point = newCenterOfVolume + originalDirection;
        point = Quaternion.Inverse(Quaternion.Euler(rotation)) * (point - newCenterOfVolume) + newCenterOfVolume;

        //Simple dimensions check
        bool x = false;
        bool y = false;
        bool z = false;
        
        //BUG: This might be causing weird bugs when rotated
        if (newStartX <= point.x && point.x <= newEndX) x = true;

        if (newStartY <= point.y && point.y <= newEndY) y = true;

        if (newStartZ <= point.z && point.z <= newEndZ) z = true;

        s.Stop();
        UnityEngine.Debug.Log(s.Elapsed);
        return (x && y && z);
    }
}
