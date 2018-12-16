﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetector
{
    private static LedgeDetector instance;

    private float minDepth = 0.1f;
    private float minHeight = 0.1f;
    private float hangRoom = 2.1f;
    private float maxAngle = 30f; // TODO: Implement use
    private int rayCount = 16;

    private Vector3 grabPoint;
    private Vector3 direction;
    private LedgeType ledgeType;

    // Singleton to conserve memory and easy management
    private LedgeDetector()
    {

    }

    public bool FindLedgeJump(Vector3 start, Vector3 dir, float maxDistance, float maxHeight, float belowHeight = 2f)
    {
        // Start at the maximum ledge height
        // raycast below that by min ledge height, repeat
        for (float offset = maxHeight - minHeight; 
            offset >= -belowHeight - minHeight; 
            offset -= minHeight)
        {
            if (FindLedgeAtPoint(start + offset * Vector3.up, dir, maxDistance, minHeight))
                return true;
        }

        return false;
    }

    public bool CanClimbUp(Vector3 start, Vector3 dir)
    {
        if (!Physics.Raycast(start + Vector3.up * 2.4f, dir, 0.4f)
            && !Physics.Raycast(start + Vector3.up * 3.9f, dir, 0.4f))
            return true;

        return false;
    }

    public bool FindLedgeAtPoint(Vector3 start, Vector3 dir, float maxDistance, float deltaHeight, bool checkHangRoom = true)
    {
        // Check in front
        RaycastHit hHit;
        Debug.DrawRay(start, dir * maxDistance, Color.red, 1.0f);
        if (Physics.Raycast(start, dir, out hHit, maxDistance))
        {
            // check from top on ledge
            RaycastHit vHit;
            start = new Vector3(hHit.point.x + (minDepth * dir.x), 
                start.y + deltaHeight, 
                hHit.point.z + (minDepth * dir.z));
            Debug.DrawRay(start, Vector3.down * deltaHeight, Color.red, 1.0f);
            if (Physics.Raycast(start, Vector3.down, out vHit, deltaHeight))
            {
                // Check hang room
                start = new Vector3(hHit.point.x - dir.x * 0.1f,
                    vHit.point.y,
                    hHit.point.z - dir.z * 0.1f);
                if (!checkHangRoom || !Physics.Raycast(start, Vector3.down, 2f))
                {
                    // Check min depth
                    start.y += 0.1f;
                    Debug.DrawRay(start, dir * (0.1f + minDepth), Color.green, 5.0f);
                    if (!Physics.Raycast(start, dir, 0.1f + minDepth))
                    {
                        grabPoint = new Vector3(hHit.point.x, vHit.point.y, hHit.point.z);
                        direction = -UMath.ZeroYInVector(hHit.normal);

                        ledgeType = LedgeType.Normal;
                        return true;
                    }
                }
            }
            else if (hHit.collider.CompareTag("Freeclimb"))
            {
                ledgeType = LedgeType.Free;

                grabPoint = new Vector3(hHit.point.x, start.y, hHit.point.z);
                direction = -hHit.normal;

                return true;
            }
        }

        return false;
    }

    public bool FindPlatformInfront(Vector3 start, Vector3 dir, float maxHeight, float depth = 0.25f)
    {
        RaycastHit vHit;
        Vector3 vStart = start + (Vector3.up * 2f) + (dir * depth);
        Debug.DrawRay(vStart, Vector3.down * (maxHeight - 0.01f), Color.red, 1f);
        if (Physics.Raycast(vStart, Vector3.down, out vHit, (maxHeight - 0.01f)))
        {
            RaycastHit hHit;
            start = new Vector3(start.x, vHit.point.y - 0.01f, start.z);
            Debug.DrawRay(start, dir * depth, Color.red, 1f);
            if (Physics.Raycast(start, dir, out hHit, depth))
            {
                if (Vector3.Dot(dir, vHit.normal) < 0.17f
                    && Vector3.Dot(vHit.normal, hHit.normal) < 0.17f)
                {
                    grabPoint = new Vector3(hHit.point.x, vHit.point.y, hHit.point.z);
                    direction = -hHit.normal;
                    return true;
                }
                
            }
        }
        return false;
    }

    public GrabType GetGrabType(Vector3 position, Vector3 dir, float uHorizontal, float uVertical, float gravity)
    {
        float distance = Mathf.Abs(UMath.GetHorizontalMag(grabPoint) - UMath.GetHorizontalMag(position));
        float timeAtX = UMath.TimeAtHorizontalPoint(uHorizontal, distance);
        float yAtTimeAtX = UMath.PredictDisplacement(uVertical, timeAtX, gravity);
        float difference = yAtTimeAtX - (grabPoint.y - position.y);

        if (difference <= -0.875f)
        {
            Vector3 start = grabPoint - dir * 0.1f;
            if (!Physics.Raycast(start, Vector3.down, hangRoom))
                return GrabType.Hand;
        }
        /*else if (difference <= 0f)
            return GrabType.Hip;*/

        return GrabType.Clear;
    }

    public float MinDepth
    {
        get { return minDepth; }
        set { minDepth = value; }
    }

    public float MinHeight
    {
        get { return minHeight; }
        set { minHeight = value; }
    }

    public float HangRoom
    {
        get { return hangRoom; }
        set { hangRoom = value; }
    }

    public float MaxAngle
    {
        get { return maxAngle; }
        set { maxAngle = value; }
    }

    public int RayCount
    {
        get { return rayCount; }
        set { rayCount = value; }
    }

    public Vector3 GrabPoint
    {
        get { return grabPoint; }
    }

    public Vector3 Direction
    {
        get { return direction; }
    }

    public LedgeType WallType
    {
        get { return ledgeType; }
    }

    public static LedgeDetector Instance
    {
        get
        {
            if (instance == null)
                instance = new LedgeDetector();
            return instance;
        }
    }
}

public enum GrabType
{
    Hand,
    Hip,
    Clear
}

public enum LedgeType
{
    Free,
    Normal
}