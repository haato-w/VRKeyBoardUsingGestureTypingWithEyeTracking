using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllarBeamSampleScript : MonoBehaviour
{
    public Transform leftContorollerTransform;
    public Transform rightContorollerTransform;
    public LineRenderer leftContorollerLineComp;
    public LineRenderer rightContorollerLineComp;

    // Start is called before the first frame update
    void Start()
    {
        leftContorollerLineComp.widthMultiplier = 0.01f;
        rightContorollerLineComp.widthMultiplier = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        leftContorollerLineComp.SetPosition(0, leftContorollerTransform.position);
        leftContorollerLineComp.SetPosition(1,  leftContorollerTransform.position + leftContorollerTransform.forward * 50);
        rightContorollerLineComp.SetPosition(0, rightContorollerTransform.position);
        rightContorollerLineComp.SetPosition(1,  rightContorollerTransform.position + rightContorollerTransform.forward * 50);
    }
}
