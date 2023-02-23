using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Wrapper.Modules;
using static VRC.Dynamics.VRCPhysBoneBase;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class InteractorController : UdonSharpBehaviour
    {
        [SerializeField] VRCPlayerApi.TrackingDataType handType;
        [SerializeField] Transform VRUI;
        [SerializeField] GameObject DesktopUI;
        [SerializeField] InteractionTypeSelectorButton[] LinkedInteractionButtonsDesktop;
        [SerializeField] InteractionTypeSelectorButton[] LinkedInteractionButtonsVR;
        [SerializeField] Transform LinkedVRHandIndicator;

        Quaternion additionalRotation = Quaternion.Euler(0, 20, 0);

        public InteractionTypes debugInteractorType;

        MeshInteractor linkedMeshInteractor;

        bool isInVR;

        InteractionTypeSelectorButton currentButton;

        VRCPlayerApi localPlayer;

        public void Setup(MeshInteractor linkedMeshInteractor)
        {
            localPlayer = Networking.LocalPlayer;

            this.linkedMeshInteractor = linkedMeshInteractor;

            isInVR = Networking.LocalPlayer.IsUserInVR();


            transform.parent = null;
            transform.localScale = Vector3.one;

            if (Networking.LocalPlayer.IsUserInVR())
            {
                foreach(InteractionTypeSelectorButton button in LinkedInteractionButtonsVR)
                {
                    button.Setup(this);
                }

                Destroy(DesktopUI);
            }
            else
            {
                foreach (InteractionTypeSelectorButton button in LinkedInteractionButtonsDesktop)
                {
                    button.Setup(this);
                }

                Destroy(VRUI.gameObject);
                Destroy(LinkedVRHandIndicator.gameObject);
            }
        }

        private void Update()
        {
            if (isInVR)
            {
                LinkedVRHandIndicator.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

                Vector3 handPosition = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
                Vector3 ellbowPosition = localPlayer.GetBonePosition(HumanBodyBones.RightLowerArm);

                Quaternion playerRotation = localPlayer.GetRotation();

                float distance = (handPosition - ellbowPosition).magnitude;

                VRUI.SetPositionAndRotation(
                    localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position + playerRotation * (distance * new Vector3(0.5f, 0.3f, 0)),
                    playerRotation * additionalRotation);

                //VRUI.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position + distance * 0.1f * Vector3.up;
                //VRUI.position = (handPosition * 3 + ellbowPosition) * 0.25f + distance * 0.2f * Vector3.up;
                //VRUI.LookAt(handPosition, Vector3.up);


                //VRUI.LookAt(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, Vector3.up);


                VRUI.localScale = distance * 0.5f * Vector3.one;
            }
            else
            {
                int selection = -1;

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    selection = 1;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    selection = 2;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    selection = 3;
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    selection = 4;
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    selection = 5;
                }
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    selection = 6;
                }
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    selection = 7;
                }
                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    selection = 8;
                }

                int index = selection - 1;

                if(index >= 0 && index < LinkedInteractionButtonsDesktop.Length)
                {
                    CurrentInteractionType = LinkedInteractionButtonsDesktop[index].InteractionType;
                }
            }
        }

        public InteractionTypes CurrentInteractionType
        {
            set
            {
                if(currentButton) currentButton.Highlighted = false;

                if (isInVR)
                {
                    foreach(InteractionTypeSelectorButton button in LinkedInteractionButtonsVR)
                    {
                        if (button.InteractionType != value) continue;

                        if (currentButton == button)
                        {
                            currentButton = null;
                            value = InteractionTypes.Idle;
                        }
                        else
                        {
                            currentButton = button;
                            button.Highlighted = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (InteractionTypeSelectorButton button in LinkedInteractionButtonsDesktop)
                    {
                        if (button.InteractionType != value) continue;

                        if (currentButton == button)
                        {
                            currentButton = null;
                            value = InteractionTypes.Idle;
                        }
                        else
                        {
                            currentButton = button;
                            button.Highlighted = true;
                            break;
                        }
                    }
                }

                linkedMeshInteractor.CurrentInteractionType = value;
            }
        }
    }
}