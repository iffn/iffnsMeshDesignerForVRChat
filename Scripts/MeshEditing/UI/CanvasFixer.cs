
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CanvasFixer : UdonSharpBehaviour
{
    [SerializeField] Transform FrontCanvas;
    [SerializeField] Transform BackCanvas;

    VRCPlayerApi localPlayer;

    Vector3 initialFrontCanvasPosition;
    Vector3 initialBackCanvasPosition;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;

        initialFrontCanvasPosition = FrontCanvas.localPosition;
        initialBackCanvasPosition = BackCanvas.localPosition;
    }

    private void Update()
    {
        Vector3 headPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        Vector3 localHeadPosition = transform.InverseTransformPoint(headPosition);

        if(localHeadPosition.z < 0)
        {
            FrontCanvas.localPosition = initialFrontCanvasPosition;
            BackCanvas.localPosition = 10000 * Vector3.down;
        }
        else
        {
            BackCanvas.localPosition = initialBackCanvasPosition;
            FrontCanvas.localPosition = 10000 * Vector3.down;
        }
    }
}
