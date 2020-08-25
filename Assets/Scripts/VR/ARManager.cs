using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;

public class ARManager : MonoBehaviour
{
    [HideInInspector]
    public static ARManager instance;
    [HideInInspector]
    public ARInterface currentInterface;

    public ViveSR_DualCameraRig srCamera;

    [Header("--- Attributes ---")]
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject objects;


    public void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        srCamera.Mode = DualCameraDisplayMode.VIRTUAL;
    }

    // Update is called once per frame
    void Update()
    {
        //arCamera.transform.position = vrCamera.transform.position;
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ChangeCameraModeToVirtual();
            //currentInterface.ReleaseObject();
        }
    }

    public void ChangeCameraModeToVirtual()
    {
        srCamera.Mode = DualCameraDisplayMode.VIRTUAL;
        //leftHand.SetActive(true);
        //rightHand.SetActive(true);
        objects.SetActive(true);
    }

    public void ChangeCameraModeToReal()
    {
        srCamera.Mode = DualCameraDisplayMode.REAL;
        //leftHand.SetActive(false);
        //rightHand.SetActive(false);
        objects.SetActive(false);
    }
}
