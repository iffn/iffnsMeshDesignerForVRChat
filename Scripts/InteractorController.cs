
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractorController : UdonSharpBehaviour
{
    [SerializeField] VRCPlayerApi.TrackingDataType handType;
    [SerializeField] Transform VRUI;
    [SerializeField] GameObject DesktopUI;
    [SerializeField] Transform DesktopUIScaler;
    [SerializeField] Transform IndicatorArrowVR;
    [SerializeField] Transform IndicatorArrowDesktop;

    public InteractionTypes debugInteractorType;

    MeshBuilder linkedMeshBuilder;
    const float sectionAngle = 1f / 45;

    public float indicatorScale = 1f;
    public float IndicatorScale
    {
        get
        {
            return indicatorScale;
        }
        set
        {
            Vector3 scale = value * Vector3.one;

            if (Networking.LocalPlayer.IsUserInVR())
            {
                VRUI.localScale = scale;
            }
            else
            {
                DesktopUIScaler.localScale = scale;
            }

            indicatorScale = value;
        }
    }

    public void Setup(MeshBuilder linkedMeshBuilder)
    {
        this.linkedMeshBuilder = linkedMeshBuilder;

        int index = (int)linkedMeshBuilder.CurrentInteractionType;

        if (Networking.LocalPlayer.IsUserInVR())
        {
            GameObject.Destroy(DesktopUI);
            IndicatorArrowVR.localRotation = Quaternion.Euler(0, 180, -index * 45);
            indicatorScale = VRUI.localScale.x;
            transform.parent = null;
        }
        else
        {
            GameObject.Destroy(VRUI.gameObject);
            IndicatorArrowVR.localRotation = Quaternion.Euler(0, 0, -index * 45);
            indicatorScale = DesktopUIScaler.localScale.x;
            transform.parent = null;
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
                float angleDeg = Mathf.Atan2(rightJoytickPosition.x, rightJoytickPosition.y) * Mathf.Rad2Deg;

                if(angleDeg < 90)
                {
                    angleDeg = 90 - angleDeg;
                }
                else
                {
                    angleDeg = 450 - angleDeg;
                }

                Index = (int)Mathf.Round(angleDeg * sectionAngle);
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
