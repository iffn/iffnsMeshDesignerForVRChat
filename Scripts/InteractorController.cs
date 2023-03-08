using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer;
using VRC.Udon.Wrapper.Modules;
using static VRC.Dynamics.VRCPhysBoneBase;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class InteractorController : UdonSharpBehaviour
    {
        [Header("Unity assingments general")]
        [SerializeField] MeshEditTool[] EditTools;

        [Header("Unity assingments desktop")]
        [SerializeField] GameObject DesktopUI;
        [SerializeField] InteractionTypeSelectorButton ButtonTemplateDesktop;
        [SerializeField] Transform DesktopUIButtonHolder;
        [SerializeField] GameObject EditButtonHolderDesktop;
        [SerializeField] Text CurrentToolTextDesktop;

        [Header("Unity assingments VR")]
        [SerializeField] Transform VRUI;
        [SerializeField] InteractionTypeSelectorButton ButtonsTemplateVR;
        [SerializeField] Transform VRUIButtonHolder;
        [SerializeField] Transform LinkedVRHandIndicator;
        [SerializeField] GameObject EditButtonHolderVR;
        [SerializeField] Text CurrentToolTextVR;

        InteractionTypeSelectorButton[] buttons;


        Quaternion additionalRotation = Quaternion.Euler(0, 20, 0);

        MeshInteractor linkedMeshInteractor;

        bool isInVR;

        InteractionTypeSelectorButton currentButton;

        VRCPlayerApi localPlayer;

        public bool InEditMode
        {
            set
            {
                if (isInVR)
                {
                    EditButtonHolderVR.SetActive(value);
                    LinkedVRHandIndicator.gameObject.SetActive(value);
                }
                else
                {
                    EditButtonHolderDesktop.SetActive(value);
                }
            }
        }

        public void Setup(MeshInteractor linkedMeshInteractor)
        {
            localPlayer = Networking.LocalPlayer;

            this.linkedMeshInteractor = linkedMeshInteractor;

            isInVR = Networking.LocalPlayer.IsUserInVR();

            transform.parent = null;
            transform.localScale = Vector3.one;

            foreach(MeshEditTool tool in EditTools)
            {
                tool.Setup(linkedMeshInteractor);
            }

            if (buttons == null)
            {
                buttons = new InteractionTypeSelectorButton[EditTools.Length];

                if (Networking.LocalPlayer.IsUserInVR())
                {
                    for (int i = 0; i < EditTools.Length; i++)
                    {
                        GameObject newButton = GameObject.Instantiate(ButtonsTemplateVR.gameObject, VRUIButtonHolder, false);
                        InteractionTypeSelectorButton buttonController = newButton.transform.GetComponent<InteractionTypeSelectorButton>();
                        buttonController.Setup(this, EditTools[i]);
                        buttons[i] = buttonController;
                    }

                    VRUI.gameObject.SetActive(true);

                    if (DesktopUI) Destroy(DesktopUI);
                }
                else
                {
                    for (int i = 0; i < EditTools.Length; i++)
                    {
                        GameObject newButton = GameObject.Instantiate(ButtonTemplateDesktop.gameObject, DesktopUIButtonHolder, false);
                        InteractionTypeSelectorButton buttonController = newButton.transform.GetComponent<InteractionTypeSelectorButton>();
                        buttonController.Setup(this, EditTools[i]);
                        buttons[i] = buttonController;
                    }

                    DesktopUI.gameObject.SetActive(true);

                    if (VRUI) Destroy(VRUI.gameObject);
                    if (LinkedVRHandIndicator) Destroy(LinkedVRHandIndicator.gameObject);
                }
            }
        }

        private void Update()
        {
            if (isInVR)
            {
                LinkedVRHandIndicator.position = linkedMeshInteractor.InteractionPosition;

                Vector3 handPosition = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
                Vector3 ellbowPosition = localPlayer.GetBonePosition(HumanBodyBones.RightLowerArm);

                Quaternion playerRotation = localPlayer.GetRotation();

                float distance = (handPosition - ellbowPosition).magnitude;

                VRUI.SetPositionAndRotation(
                    localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position + playerRotation * (distance * new Vector3(0.5f, 0.3f, 0)),
                    playerRotation * additionalRotation);

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

                if(index >= 0 && index < EditTools.Length)
                {
                    CurrentInteractorTool = EditTools[index];
                }
            }
        }

        public MeshEditTool CurrentInteractorTool
        {
            set
            {
                //Handle current button
                if (currentButton)
                {
                    currentButton.LinkedTool.OnDeactivation();

                    if (currentButton.LinkedTool == value)
                    {
                        //Deselect current tool
                        currentButton = null;
                        return;
                    }
                    else
                    {
                        currentButton.Highlighted = false;
                    }
                }

                //Handle null selection
                if (value == null)
                {
                    currentButton = null;
                    return;
                }

                //Find new button
                for (int i = 0; i < EditTools.Length; i++)
                {
                    if (EditTools[i] != value) continue;

                    currentButton = buttons[i];
                    break;
                }

                //Set new tool
                if (currentButton)
                {
                    currentButton.Highlighted = true;

                    if (isInVR)
                    {
                        CurrentToolTextVR.text = "Current tool = " + currentButton.LinkedTool.name;
                    }
                    else
                    {
                        CurrentToolTextDesktop.text = "Current tool = " + currentButton.LinkedTool.name;
                    }

                    linkedMeshInteractor.CurrentEditTool = currentButton.LinkedTool;
                }

                value.OnActivation();
            }
        }
    }
}