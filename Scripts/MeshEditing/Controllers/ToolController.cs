//#define debugLog

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using UnityEngine.UI;
using UnityEditor.EditorTools;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class ToolController : UdonSharpBehaviour
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

        //Runtime variables
        InteractionTypeSelectorButton currentButton;
        readonly Quaternion additionalRotation = Quaternion.Euler(0, 20, 0);
        InteractionTypeSelectorButton[] buttons;
        readonly Vector3 InteractorOffsetVector = Vector3.down;
        MeshEditTool currentEditTool;
        MeshEditor linkedMeshEditor;
        VRCPlayerApi localPlayer;
        bool isInVR;
        public bool OverUIElement = false;
        bool useAndGrabAreTheSame;
        Transform meshTransform;

        public void Setup(MeshEditor linkedMeshEditor, Transform meshTransform)
        {
            this.linkedMeshEditor = linkedMeshEditor;
            localPlayer = Networking.LocalPlayer;
            isInVR = localPlayer.IsUserInVR();
            this.meshTransform = meshTransform;

            //Input type sdetection
            useAndGrabAreTheSame = !isInVR;

            string[] controllers = Input.GetJoystickNames();

            foreach (string controller in controllers)
            {
                if (!controller.ToLower().Contains("vive")) continue;

                useAndGrabAreTheSame = true;
                break;
            }
        }

        //Settings
        public HandType PrimaryHand = HandType.RIGHT;
        public float vertexInteractionOffset = 0.05f;
        public HandType primaryHand = HandType.RIGHT;

        float vertexInteractionDistance;
        public float VertexInteractionDistance
        {
            get
            {
                return vertexInteractionDistance;
            }
            set
            {
                vertexInteractionDistance = value;
                linkedMeshEditor.VertexIndicatorRadius = value;
            }
        }

        //UI
        private void Update()
        {
            if (isInVR)
            {
                LinkedVRHandIndicator.position = InteractionPosition;

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

                if (index >= 0 && index < EditTools.Length)
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
                        currentButton.Highlighted = false;
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

                    currentEditTool = currentButton.LinkedTool;
                }

                value.OnActivation();
            }
        }

        //Access
        public Vector3 LocalInteractionPositionWithoutMirrorLineSnap
        {
            get
            {
                return meshTransform.InverseTransformPoint(InteractionPosition);
            }
        }

        public Vector3 LocalInteractionPositionWithMirrorLineSnap
        {
            get
            {
                Vector3 position = meshTransform.InverseTransformPoint(InteractionPosition);

                if (Mathf.Abs(position.x) < vertexInteractionDistance) position.x = 0;

                return position;
            }
        }

        public int SelectVertex()
        {
            if (isInVR) return SelectClosestVertexInVR();
            else return SelectVertexInDesktop();
        }

        public Vector3 LocalHeadPosition
        {
            get
            {
                return meshTransform.InverseTransformPoint(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
            }
        }

        //Utility functions
        float PlayerHeight
        {
            get
            {
                return (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - localPlayer.GetPosition()).magnitude;
            }
        }

        Vector3 InteractionPosition
        {
            get
            {
                if (isInVR)
                {
                    if (PrimaryHand == HandType.LEFT)
                    {
                        VRCPlayerApi.TrackingData hand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);

                        return hand.position + vertexInteractionOffset * (hand.rotation * InteractorOffsetVector.normalized);
                    }
                    else
                    {
                        VRCPlayerApi.TrackingData hand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

                        return hand.position + vertexInteractionOffset * (hand.rotation * InteractorOffsetVector.normalized);
                    }
                }
                else
                {
                    VRCPlayerApi.TrackingData currentHandData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                    float currentDesktopPickupDistance = PlayerHeight * 0.5f;

                    return currentHandData.position + currentHandData.rotation * (currentDesktopPickupDistance * Vector3.forward);
                }
            }
        }

        Quaternion GetPrimaryHandRotation
        {
            get
            {
                if (PrimaryHand == HandType.LEFT)
                {
                    return localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
                }
                else
                {
                    return localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
                }
            }
        }

        int SelectVertexInDesktop()
        {
            VRCPlayerApi.TrackingData data = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            Vector3 localHeadPosition = meshTransform.InverseTransformPoint(data.position);

            Quaternion localHeadRotation = Quaternion.Inverse(meshTransform.rotation) * data.rotation;

            return linkedMeshEditor.GetClosestVectorInCylinder(localHeadPosition, localHeadRotation, vertexInteractionDistance, PlayerHeight);
        }

        int SelectClosestVertexInVR()
        {
            return linkedMeshEditor.GetClosestVertexInRadius(LocalInteractionPositionWithoutMirrorLineSnap, vertexInteractionDistance);
        }



        #region VRChat input functions
        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (OverUIElement) return;

            if (!currentEditTool) return;

            if (args.handType != primaryHand) return; //Currently only one handed

            if (!useAndGrabAreTheSame)
            {
                if (value)
                {
#if debugLog
                Debug.Log("Calling OnPickupUse");
#endif
                    currentEditTool.OnPickupUse();
                }
                else
                {
#if debugLog
                Debug.Log("Calling OnDropUse");
#endif
                    currentEditTool.OnDropUse();
                }
            }
            else
            {
                if (currentEditTool.CallUseInsteadOfPickup)
                {
                    if (value)
                    {
#if debugLog
                    Debug.Log("Calling OnUseDown");
#endif
                        currentEditTool.OnUseDown();
                    }
                    else
                    {
#if debugLog
                    Debug.Log("Calling OnUseUp");
#endif
                        currentEditTool.OnUseUp();
                    }
                }
                else
                {
                    if (value)
                    {
#if debugLog
                    Debug.Log("Calling OnPickupUse");
#endif
                        currentEditTool.OnPickupUse();
                    }
                    else
                    {
#if debugLog
                    Debug.Log("Calling OnDropUse");
#endif
                        currentEditTool.OnDropUse();
                    }
                }
            }
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            //Warning: Currently called twice, at least in desktop mode: https://vrchat.canny.io/vrchat-udon-closed-alpha-bugs/p/1275-inputuse-is-called-twice-per-mouse-click

            if (OverUIElement) return;

            if (!currentEditTool) return;

            if (args.handType != primaryHand) return; //Currently only one handed

            if (!useAndGrabAreTheSame)
            {
                if (value)
                {
#if debugLog
                Debug.Log("Calling OnUseDown");
#endif
                    currentEditTool.OnUseDown();
                }
                else
                {
#if debugLog
                Debug.Log("Calling OnUseUp");
#endif
                    currentEditTool.OnUseUp();
                }
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            useAndGrabAreTheSame = true;

            if (OverUIElement) return;

            if (!currentEditTool) return;

            if (args.handType != primaryHand) return; //Currently only one handed

            if (value)
            {
                if (currentEditTool.CallUseInsteadOfPickup)
                {
#if debugLog
                Debug.Log("Calling OnPickupUse");
#endif
                    currentEditTool.OnPickupUse();
                }
                else
                {
#if debugLog
                Debug.Log("Calling OnDropUse");
#endif
                    currentEditTool.OnDropUse();
                }
            }
            else
            {
#if debugLog
            Debug.Log("Calling OnDropUse");
#endif
                currentEditTool.OnDropUse();
            }
        }
        #endregion
    }
}