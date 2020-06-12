using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGestureDebug : MonoBehaviour
{
    // Start is called before the first frame update
    public HandGestureManager leftHandManager;
    public HandGestureManager rightHandManager;
    public Vector3 offset;

    public GameObject cubePrefab;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (rightHandManager.GetHandGestureState() == EHandGestureState.Planed || rightHandManager.GetHandGestureState() == EHandGestureState.Idle)
        {
            cubePrefab.SetActive(true);
            cubePrefab.transform.position = rightHandManager.HandPosition().position + offset;
            cubePrefab.transform.rotation = rightHandManager.HandPosition().rotation * Quaternion.Euler(180, 90, 0);
            //cubePrefab.transform.LookAt(Camera.main.transform);
        }
        else
        {
            cubePrefab.SetActive(false);
        }
    }

}
