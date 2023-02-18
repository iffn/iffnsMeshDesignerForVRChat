using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class InteractorController : UdonSharpBehaviour
    {
        [SerializeField] VRCPlayerApi.TrackingDataType handType;
        [SerializeField] Transform VRUI;
        [SerializeField] GameObject DesktopUI;
        [SerializeField] InteractionTypeSelectorButton[] LinkedInteractionButtonsVR;
        [SerializeField] InteractionTypeSelectorButton[] LinkedInteractionButtonsDesktop;

        public InteractionTypes debugInteractorType;

        MeshBuilder linkedMeshBuilder;

        bool isInVR;

        InteractionTypeSelectorButton currentButton;

        public void Setup(MeshBuilder linkedMeshBuilder)
        {
            this.linkedMeshBuilder = linkedMeshBuilder;

            int index = (int)linkedMeshBuilder.CurrentInteractionType;

            isInVR = Networking.LocalPlayer.IsUserInVR();

            if (Networking.LocalPlayer.IsUserInVR())
            {
                Destroy(DesktopUI);
                transform.parent = null;
            }
            else
            {
                Destroy(VRUI.gameObject);
                transform.parent = null;
            }
        }

        private void Update()
        {
            if (isInVR)
            {
                
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
                        }
                    }
                }

                linkedMeshBuilder.CurrentInteractionType = value;
            }
        }
    }
}