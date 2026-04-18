using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using System.Collections.Generic;

public class EyeGazeController : MonoBehaviour
{
    public GameObject RightTargetObject;
    public GameObject LeftTargetObject;
    public Camera RightTargetCamera;
    public Camera LeftTargetCamera;

    private InputDevice eyeTrackingDevice;

    void Start()
    {
        // Eye Tracking デバイスを取得
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics eyeTrackingCharacteristics =
            InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice;
        InputDevices.GetDevicesWithCharacteristics(eyeTrackingCharacteristics, devices);

        if (devices.Count > 0)
        {
            eyeTrackingDevice = devices[0];
            Debug.Log("Eye Tracking device found: " + eyeTrackingDevice.name);
        }
        else
        {
            Debug.LogWarning("Eye Tracking device not found.");
        }
    }

    void Update()
    {
        if (!eyeTrackingDevice.isValid)
        {
            // デバイスが見つからない場合、再取得を試みる
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice,
                devices);
            if (devices.Count > 0)
            {
                eyeTrackingDevice = devices[0];
            }
            return;
        }

        // 視線の位置と向きを取得
        if (eyeTrackingDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 gazePosition) &&
            eyeTrackingDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion gazeRotation))
        {
            Vector3 gazeDirection = gazeRotation * Vector3.forward;

            // Left Eye
            Vector3 leftRayOrigin = LeftTargetCamera.transform.position;
            if (Physics.Raycast(leftRayOrigin, gazeDirection, out RaycastHit leftHit))
            {
                LeftTargetObject.transform.position = leftHit.point;
            }

            // Right Eye
            Vector3 rightRayOrigin = RightTargetCamera.transform.position;
            if (Physics.Raycast(rightRayOrigin, gazeDirection, out RaycastHit rightHit))
            {
                RightTargetObject.transform.position = rightHit.point;
            }
        }
    }
}