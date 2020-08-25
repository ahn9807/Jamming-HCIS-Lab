using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARInterface : MonoBehaviour
{
    [Header("Pump Setting")]
    public float readyPressure = 10;
    public float jammedPressure = 60;
    public float jammingTime = 0;

    Transform originalParent;

    public enum INTERFACE_MODE
    {
        Idle,
        Ready,
        Jammed,
    }

    [HideInInspector]
    public INTERFACE_MODE mode;
    // Start is called before the first frame update
    void Start()
    {
        mode = INTERFACE_MODE.Idle;
        originalParent = this.transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        switch(mode)
        {
            case INTERFACE_MODE.Jammed:
                UpdateJammed();
                break;
            case INTERFACE_MODE.Ready:
                UpdateReady();
                break;
            case INTERFACE_MODE.Idle:
                UpdateIdle();
                break;
        }
    }

    void UpdateIdle()
    {

    }
    void UpdateReady()
    {
        ArduinoPump.SetPressure(readyPressure);
        // size 를 0으로 만든다. 

        if(Input.GetKeyDown(KeyCode.Space))
        {
            mode = INTERFACE_MODE.Jammed;
        }
    }

    void UpdateJammed()
    {
        ArduinoPump.SetPressure(jammedPressure);
        //size를 원하는 크기로 만든다. 

        ARManager.instance.ChangeCameraModeToVirtual();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SDF");
        if(other.gameObject.layer == LayerMask.NameToLayer("Hand") || 
            other.gameObject.layer == LayerMask.NameToLayer("Hi5OtherFingerTail") ||
            other.gameObject.layer == LayerMask.NameToLayer("Hi5OtherFingerOther"))
        {
            if(ARManager.instance.currentInterface == this)
            {
                return;
            }
            ARManager.instance.currentInterface = this;
            ARManager.instance.ChangeCameraModeToReal();
            mode = INTERFACE_MODE.Ready;
            GrabObject(other.gameObject);
        }
    }

    public void GrabObject(GameObject parentObject)
    {
        this.transform.SetParent(parentObject.transform);
        this.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void ReleaseObject()
    {
        this.transform.SetParent(originalParent);
        this.GetComponent<Rigidbody>().isKinematic = false;
    }
}
