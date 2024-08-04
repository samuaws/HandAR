using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ARInteractions : MonoBehaviour
{
    public ARRaycastManager arRaycastManager;
    public GameObject[] postItPrefabs;  // Array to hold post-it prefabs with numbers 1 to 10

    private GameObject objectToPlace;
    private bool isPostItMode = false;
    private int postItIndex = 0;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    // Ignore the touch if it's over a UI element
                    return;
                }

                Vector2 touchPosition = touch.position;

                if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
                {
                    Pose hitPose = hits[0].pose;

                    if (isPostItMode)
                    {
                        // Spawn post-it prefabs sequentially
                        if (postItIndex < postItPrefabs.Length)
                        {
                            Instantiate(postItPrefabs[postItIndex], hitPose.position, hitPose.rotation);
                            postItIndex++;
                        }
                    }
                    else if (objectToPlace != null)
                    {
                        Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
                    }
                }
            }
        }
    }

    public void SelectObjectToPlace(GameObject selectedObject)
    {
        objectToPlace = selectedObject;
        isPostItMode = false;
    }

    public void ActivatePostItMode()
    {
        isPostItMode = true;
        postItIndex = 0;
    }
}
