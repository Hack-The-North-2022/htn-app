using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureScript : MonoBehaviour
{
    public Transform trackingSpace;
    public Transform centerEye;
    public float gestureThreshold;
    public float angleThreshold;
    private int gestureCount = -1;
    private int amountInStrikeZone = 0;
    private float duration = 0;
    // Start is called before the first frame update
    Vector3 prevRight;
    Vector3 prevLeft;

    private bool isRecording = false;
    void Start()
    {
        
    }

    public void StartRecording() {
        isRecording = true;
        prevRight = Vector3.zero;
        prevLeft = Vector3.zero;
        duration = 0;
        amountInStrikeZone = 0;
        gestureCount = -1;
    }

    public void StopRecording() {
        APIReq.APIReqs.SendGestures(gestureCount,amountInStrikeZone,duration);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isRecording) {
            Vector3 rightPosition = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
            Vector3 leftPosition = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
            Vector3 headsetPosition = centerEye.position;
            Vector3 to = rightPosition - headsetPosition;
            Vector3 from = leftPosition - headsetPosition;
            float ang = Vector3.Angle(from, to);

            if (prevLeft == Vector3.zero) prevLeft = leftPosition;
            if (prevRight == Vector3.zero) prevRight = rightPosition;

            if ((prevRight - rightPosition).magnitude > gestureThreshold || (prevLeft - leftPosition).magnitude > gestureThreshold) {
                gestureCount += 1;
                if (10 < ang && ang < angleThreshold) {
                    amountInStrikeZone += 1;
                }
                prevLeft = leftPosition;
                prevRight = rightPosition;
            }
            duration += Time.deltaTime;
        }
    }
}
