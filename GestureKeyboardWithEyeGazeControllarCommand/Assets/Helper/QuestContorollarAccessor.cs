using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuestContorollarAccessor
{
    public static bool GetAButton() {
        return OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    public static bool GetDownAButton() {
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    public static bool GetUpAButton() {
        return OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    public static bool GetDownBButton() {
        return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
    }

    public static bool GetXButton() {
        return OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch);
    }

    public static bool GetDownXButton() {
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);
    }

    public static bool GetUpXButton() {
        return OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch);
    }

    public static bool GetDownLeftPrimaryIndexTrigger() {
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
    }

    public static bool GetDownRightPrimaryIndexTrigger() {
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
    }

    public static bool GetLeftPrimaryIndexTrigger() {
        return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
    }

    public static bool GetRightPrimaryIndexTrigger() {
        return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
    }
}