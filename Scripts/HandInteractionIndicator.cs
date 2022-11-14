
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandInteractionIndicator : UdonSharpBehaviour
{
    [SerializeField] VRCPlayerApi.TrackingDataType handType;

    private void Update()
    {
        transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
        transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
    }
}
