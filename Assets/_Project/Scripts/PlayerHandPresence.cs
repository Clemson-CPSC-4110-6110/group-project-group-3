using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class PlayerHandPresence : MonoBehaviour
{
    [SerializeField] private Transform leftHandVisual;
    [SerializeField] private Transform rightHandVisual;
    
    private InputDevice leftDevice;
    private InputDevice rightDevice;

    void Start()
    {
        var leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            leftDevices);
        if (leftDevices.Count > 0) leftDevice = leftDevices[0];

        var rightDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            rightDevices);
        if (rightDevices.Count > 0) rightDevice = rightDevices[0];
    }

    void Update()
    {
        UpdateHandVisual(leftDevice, leftHandVisual);
        UpdateHandVisual(rightDevice, rightHandVisual);
    }

    void UpdateHandVisual(InputDevice device, Transform visual)
    {
        if (!device.isValid || visual == null) return;
        
        device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos);
        device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot);
        
        visual.position = pos;
        visual.rotation = rot;
    }
}