using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllarEventTestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);
        OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch);

        Debug.Log("");
        Debug.Log("OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)(X): " + OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch)(Y)" + OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)(A)" + OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)(B)" + OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch));
        
        Debug.Log("OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch): " + OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch));
        Debug.Log("OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch): " + OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch));

        Debug.Log("OVRInput.GetDown(OVRInput.Button.One)" + OVRInput.GetUp(OVRInput.Button.One));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.One)" + OVRInput.Get(OVRInput.Button.One));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.One)" + OVRInput.GetDown(OVRInput.Button.One));
        Debug.Log("OVRInput.GetUp(OVRInput.Button.Two)" + OVRInput.GetUp(OVRInput.Button.Two));
        Debug.Log("OVRInput.Get(OVRInput.Button.Two)" + OVRInput.Get(OVRInput.Button.Two));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.Two)" + OVRInput.GetDown(OVRInput.Button.Two));
        Debug.Log("OVRInput.GetUp(OVRInput.Button.Three)" + OVRInput.GetUp(OVRInput.Button.Three));
        Debug.Log("OVRInput.Get(OVRInput.Button.Three)" + OVRInput.Get(OVRInput.Button.Three));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.Three)" + OVRInput.GetDown(OVRInput.Button.Three));
        Debug.Log("OVRInput.GetUp(OVRInput.Button.Four)" + OVRInput.GetUp(OVRInput.Button.Four));
        Debug.Log("OVRInput.Get(OVRInput.Button.Four)" + OVRInput.Get(OVRInput.Button.Four));
        Debug.Log("OVRInput.GetDown(OVRInput.Button.Four)" + OVRInput.GetDown(OVRInput.Button.Four));
        Debug.Log("");
    }
}
