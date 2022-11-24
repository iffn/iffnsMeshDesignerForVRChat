
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class Scaler : UdonSharpBehaviour
{
    [SerializeField] Transform scaleObject;
    [SerializeField] GameObject indicator;

    bool leftDrop = false;
    bool rightDrop = false;
    float referenceDistance;

    bool isScaling = false;

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
            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger") > 0.9f && Input.GetAxis("Oculus_CrossPlatform_SecondaryHandTrigger") > 0.9f)
            {
                if (!isScaling)
                {
                    SetupScaling();
                    isScaling = true;
                }

                Scale();
            }
            else
            {
                if (isScaling)
                {
                    StopScaling();
                    isScaling = false;
                }
            }
        }
    }

    void SetupScaling()
    {
        Vector3 rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        Vector3 leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

        Vector3 rightToLeft = leftHand - rightHand;

        referenceDistance = rightToLeft.magnitude;

        transform.position = rightHand + referenceDistance * 0.5f * rightToLeft;

        transform.localScale = Vector3.one;

        transform.parent = scaleObject.parent;
        scaleObject.parent = transform;

        indicator.SetActive(true);
    }

    void Scale()
    {
        Vector3 rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        Vector3 leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

        Vector3 rightToLeft = leftHand - rightHand;

        float currentDistance = rightToLeft.magnitude;

        transform.position = rightHand + currentDistance * 0.5f * rightToLeft;

        transform.localScale = currentDistance / referenceDistance * Vector3.one;
    }

    void StopScaling()
    {
        scaleObject.parent = transform.parent;
        transform.parent = scaleObject;
        indicator.SetActive(false);
    }
}
