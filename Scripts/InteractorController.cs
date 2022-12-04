
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractorController : UdonSharpBehaviour
{
    [SerializeField] VRCPlayerApi.TrackingDataType handType;
    [SerializeField] Transform VRUI;
    [SerializeField] GameObject DesktopUI;
    [SerializeField] Transform IndicatorArrowVR;
    [SerializeField] Transform IndicatorArrowDesktop;

    public InteractionTypes debugInteractorType;

    MeshBuilder linkedMeshBuilder;
    const float sectionAngle = 1f / 45;

    public void Setup(MeshBuilder linkedMeshBuilder)
    {
        this.linkedMeshBuilder = linkedMeshBuilder;

        int index = (int)linkedMeshBuilder.CurrentInteractionType;

        if (Networking.LocalPlayer.IsUserInVR())
        {
            GameObject.Destroy(DesktopUI);
            IndicatorArrowVR.localRotation = Quaternion.Euler(0, 180, -index * 45);
        }
        else
        {
            GameObject.Destroy(VRUI.gameObject);
            IndicatorArrowVR.localRotation = Quaternion.Euler(0, 0, -index * 45);
        }
    }

    private void Update()
    {
        if (Networking.LocalPlayer.IsUserInVR())
        {
            //Set position:
            transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

            transform.LookAt(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, Vector3.up);

            Vector2 rightJoytickPosition = new Vector2(-Input.GetAxis("Oculus_GearVR_DpadX"), -Input.GetAxis("Oculus_GearVR_RThumbstickY"));

            if (rightJoytickPosition.magnitude > 0.5)
            {
                float angle = Mathf.Atan2(rightJoytickPosition.x, rightJoytickPosition.y);

                Index = (int)Mathf.Round(angle * sectionAngle);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Index = 0;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Index = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Index = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Index = 3;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Index = 4;

            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Index = 5;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Index = 6;
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                Index = 7;
            }
        }
    }

    int Index
    {
        set
        {
            if (Networking.LocalPlayer.IsUserInVR())
            {
                IndicatorArrowVR.localRotation = Quaternion.Euler(0, 180, -value * 45);
            }
            else
            {
                IndicatorArrowDesktop.localRotation = Quaternion.Euler(0, 0, -value * 45);
            }

            linkedMeshBuilder.CurrentInteractionType = (InteractionTypes)value;
        }
    }
}
