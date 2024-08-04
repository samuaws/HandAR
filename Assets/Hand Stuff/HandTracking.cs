using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HandTracking : MonoBehaviour
{
    public HandAnimator handAnimator; // Reference to the HandAnimator script
    public GameObject[] handPoints;

    void Start()
    {
        timeLeft = updateInterval;
    }

    public float updateInterval = 0.5f; // The interval at which to update the FPS display
    private float accum = 0f; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeLeft; // Time left for current interval
    public float shift = 7;

    void Update()
    {
        try
        {
            Vector3[] points = handAnimator.GetHandKeyPoints();

            // Adjust these offsets based on your scene setup
            float xOffset = 0.0f;    // Adjust this based on your scene setup
            float yOffset = 0.0f;    // Adjust this if needed
            float zOffset = 0.0f;    // Adjust this if needed
            float scale = 0.01f;     // Adjust this scale factor if needed

            // Calculate the hand's orientation (based on key points, e.g., wrist to base joint of middle finger)
            Vector3 wristPosition = points[0] * scale; // Assuming point 0 is the wrist
            Vector3 middleFingerBaseJointPosition = points[9] * scale; // Assuming point 9 is the base joint of the middle finger
            Vector3 handDirection = (middleFingerBaseJointPosition - wristPosition).normalized;

            // Calculate the hand's rotation (from the direction vector)
            Quaternion handRotation = Quaternion.LookRotation(handDirection, Vector3.up);

            // Apply offsets and update the 3D hand points
            for (int i = 0; i < points.Length; i++)
            {
                if (i == 5 || i == 9 || i == 13 || i == 17 || i == 21) continue; // Skip the tip points

                float x = xOffset + points[i].x * scale;
                float y = yOffset + points[i].y * scale;
                float z = zOffset + points[i].z * scale;

                // Apply the calculated position
                Vector3 position = new Vector3(x, y, z);

                // Update the position of the corresponding 3D hand point
                handPoints[i].transform.localPosition = position;

                // Calculate and apply rotation for the joints
                if (i % 4 == 1) // Assuming base joint for each finger
                {
                    Vector3 jointDirection = (points[i + 1] - points[i]).normalized;
                    Quaternion jointRotation = Quaternion.LookRotation(jointDirection, Vector3.up);
                    handPoints[i].transform.localRotation = jointRotation;
                }
            }

            // Optionally, update the parent object of the hand points to rotate the entire hand
            //handParent.transform.localRotation = handRotation;
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

}
