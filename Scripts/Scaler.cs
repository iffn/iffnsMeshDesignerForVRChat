
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class Scaler : UdonSharpBehaviour
{
    [SerializeField] Transform scaleObject;

    bool leftDrop = false;
    bool rightDrop = false;
    float referenceDistance;

    Vector3 originalLocalPosition;
    Vector3 originalLocalScale;

    public void ResetScale()
    {
        scaleObject.transform.localPosition = originalLocalPosition;
        scaleObject.transform.localScale = originalLocalScale;
    }

    private void Start()
    {
        originalLocalPosition = scaleObject.transform.localPosition;
        originalLocalScale = scaleObject.transform.localScale;
    }

    private void Update()
    {
        if (Networking.LocalPlayer.IsUserInVR())
        {
            if (leftDrop && rightDrop)
            {
                Vector3 rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                Vector3 leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

                Vector3 rightToLeft = leftHand - rightHand;

                float currentDistance = rightToLeft.magnitude;

                transform.position = rightHand + currentDistance * 0.5f * rightToLeft;

                transform.localScale = currentDistance / referenceDistance * Vector3.one;
            }
        }
    }

    public override void InputDrop(bool value, UdonInputEventArgs args)
    {
        if (Networking.LocalPlayer.IsUserInVR())
        {
            switch (args.handType)
            {
                case HandType.RIGHT:
                    rightDrop = value;
                    break;
                case HandType.LEFT:
                    leftDrop = value;
                    break;
                default:
                    break;
            }

            if (leftDrop && rightDrop)
            {
                Vector3 rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                Vector3 leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

                Vector3 rightToLeft = leftHand - rightHand;

                referenceDistance = rightToLeft.magnitude;

                transform.position = rightHand + referenceDistance * 0.5f * rightToLeft;

                transform.localScale = Vector3.one;

                transform.parent = scaleObject.parent;
                scaleObject.parent = transform;
            }
            else
            {
                scaleObject.parent = transform.parent;
                transform.parent = scaleObject;
            }
        }
    }
}
