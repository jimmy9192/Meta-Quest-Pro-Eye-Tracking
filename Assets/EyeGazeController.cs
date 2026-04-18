using UnityEngine;

public class EyeGazeController : MonoBehaviour
{
    private OVRPlugin.EyeGazesState EyeGazeState;
    public GameObject RightTargetObject;
    public GameObject LeftTargetObject;
    public Camera RightTargetCamera;
    public Camera LeftTargetCamera;

    void Start()
    {
        Debug.Log("ÅöÅöÅö EyeGazeController started! ÅöÅöÅö");
    }

    void Update()
    {
        bool success = OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref EyeGazeState);
        Debug.Log("GetEyeGazesState: " + success);

        if (success)
        {
            var LeftEyeGaze = EyeGazeState.EyeGazes[(int)OVRPlugin.Eye.Left];
            var RightEyeGaze = EyeGazeState.EyeGazes[(int)OVRPlugin.Eye.Right];

            Debug.Log("LeftEyeGaze.IsValid: " + LeftEyeGaze.IsValid);
            Debug.Log("RightEyeGaze.IsValid: " + RightEyeGaze.IsValid);

            if (LeftEyeGaze.IsValid)
            {
                var LeftPose = LeftEyeGaze.Pose.ToOVRPose();
                var RightPose = RightEyeGaze.Pose.ToOVRPose();

                Vector3 GazeLeftDirection = LeftPose.orientation * Vector3.forward;
                Vector3 GazeRightDirection = RightPose.orientation * Vector3.forward;

                Vector3 GazeLeftPosition = LeftTargetCamera.transform.position;
                Vector3 GazeRightPosition = RightTargetCamera.transform.position;

                Debug.Log("Left Gaze Direction: " + GazeLeftDirection);
                Debug.Log("Left Camera Position: " + GazeLeftPosition);

                if (Physics.Raycast(GazeLeftPosition, GazeLeftDirection, out RaycastHit lefthitinfo))
                {
                    Debug.Log("Left Hit: " + lefthitinfo.collider.name);
                    LeftTargetObject.transform.position = lefthitinfo.point;
                }
                else
                {
                    Debug.Log("Left Raycast did not hit anything");
                }

                if (Physics.Raycast(GazeRightPosition, GazeRightDirection, out RaycastHit righthitinfo))
                {
                    Debug.Log("Right Hit: " + righthitinfo.collider.name);
                    RightTargetObject.transform.position = righthitinfo.point;
                }
                else
                {
                    Debug.Log("Right Raycast did not hit anything");
                }
            }
        }
    }
}