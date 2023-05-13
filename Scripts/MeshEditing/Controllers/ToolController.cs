//#define debugLog
//#define inputDebug

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using UnityEngine.UI;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
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
        [SerializeField] Material DistanceMaterialNormal;
        [SerializeField] Material DistanceMaterialSelected;
        [SerializeField] Material DistanceMaterialReadyToRemove;

        [Header("Unity assingments VR")]
        [SerializeField] VRToolController VRUI;
        [SerializeField] InteractionTypeSelectorButton ButtonTemplateVR;
        [SerializeField] Transform LinkedVRHandIndicator;

        //Runtime variables
        readonly string distancePropertyName = "_BaseDistance";
        InteractionTypeSelectorButton currentButton;
        readonly Quaternion additionalRotation = Quaternion.Euler(0, 20, 0);
        InteractionTypeSelectorButton[] buttons = new InteractionTypeSelectorButton[0];
        readonly Vector3 InteractorOffsetVector = Vector3.down;
        MeshEditTool currentEditTool;
        MeshEditor linkedMeshEditor;
        ToolSettings linkedToolSettings;
        VRCPlayerApi localPlayer;
        bool isInVR;
        
        Transform meshTransform;
        float armLengthInVR = 1;
        float lastUpdateTime;
        float desktopPickupDistance = 1;
        float desktopPickupDistanceMultiplier = 1;
        public bool emulateAlternativeInput;

        bool UIFocusOnSecondaryHand;

        #if inputDebug
            string inputs = "";
        #endif

        //Settings
        public HandType primaryHand = HandType.RIGHT;
        public HandType PrimaryHand
        {
            get
            {
                return primaryHand;
            }
            set
            {
                primaryHand = value;
                UIFocusOnSecondaryHand = !UIFocusOnSecondaryHand;
            }
        }

        public float vertexInteractionOffset = 0.05f;

        float vertexInteractionDistance = 0.01f;
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

        bool useAndGrabAreTheSame;
        public bool UseAndGrabAreTheSame
        {
            get
            {
                return useAndGrabAreTheSame;
            }
        }

        public bool SymmetryMode {private get; set; } = false;

        bool inEditMode;
        public bool InEditMode
        {
            get
            {
                return inEditMode;
            }
            set
            {
                inEditMode = value;

                if (!value)
                {
                    CurrentInteractorTool = null;
                }

                if (isInVR)
                {
                    VRUI.gameObject.SetActive(value);

                    VRUI.ColliderEnabled = true;

                    LinkedVRHandIndicator.gameObject.SetActive(value);
                }
                else
                {
                    DesktopUI.SetActive(value);
                }

                linkedMeshEditor.InEditMode = value;

                //linkedToolSettings.InEditMode = value; //For call from ownership transfer
            }
        }

        public string MultiLineDebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(ToolController)} at {Time.time:0.000}:\n"
                + $"• {nameof(lastUpdateTime)}: {lastUpdateTime:0.000}\n"
                + $"• {nameof(buttons)}.length: {buttons.Length}\n"
                + $"• {nameof(vertexInteractionDistance)}: {vertexInteractionDistance}\n"
                + $"• {nameof(useAndGrabAreTheSame)}: {useAndGrabAreTheSame}\n"
                + $"• {nameof(desktopPickupDistance)}: {desktopPickupDistance}\n";

            if (isInVR)
            {
                returnString += $"• {nameof(PrimaryHand)}: {(PrimaryHand == HandType.RIGHT ? "Right" : "Left")}\n";
                returnString += $"• {nameof(VRUI.OverUIElement)}: {VRUI.OverUIElement}\n";
                returnString += $"• {nameof(UIFocusOnSecondaryHand)}: {UIFocusOnSecondaryHand}\n";
            }

            returnString += $"• {nameof(inEditMode)}: {inEditMode}\n"
                + $"• {nameof(currentEditTool)}: {(currentEditTool ? currentEditTool.name : "null")}\n"
                + $"• {nameof(currentButton)}.{nameof(currentButton.LinkedTool)}.{nameof(currentButton.LinkedTool.name)}: {(currentButton ? currentButton.LinkedTool.name : "null")}\n"
                + "\n";
            
            if (currentEditTool) returnString += $"{currentEditTool.MultiLineDebugState()}\n";

            #if inputDebug
                returnString += $"\n{inputs}";
            #endif

            returnString += linkedMeshEditor.MultiLineDebugState();

            return returnString;
        }

        public void Setup(ToolSettings linkedToolSettings, MeshEditor linkedMeshEditor, MeshInteractionInterface linkedInteractionInterface, Transform meshTransform)
        {

            lastUpdateTime = Time.time;

            this.linkedToolSettings = linkedToolSettings;

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

            //Setup tools
            foreach(MeshEditTool tool in EditTools)
            {
                tool.Setup(this, linkedMeshEditor, linkedInteractionInterface);
            }

            //Setup UI
            
            if (isInVR)
            {
                VRUI.gameObject.SetActive(InEditMode);

                VRUI.Setup(EditTools.Length);

                GameObject.Destroy(DesktopUI);
            }
            else
            {
                DesktopUI.SetActive(InEditMode);

                GameObject.Destroy(VRUI.gameObject);
            }

            buttons = new InteractionTypeSelectorButton[EditTools.Length];
            Transform holder = (isInVR ? VRUI.ButtonHolder : EditButtonHolderDesktop).transform;
            GameObject template = (isInVR ? ButtonTemplateVR : ButtonTemplateDesktop).gameObject;
            

            for (int i = 0; i<EditTools.Length; i++)
            {
                GameObject newButtonObject = GameObject.Instantiate(template, holder, false);

                InteractionTypeSelectorButton button = newButtonObject.transform.GetComponent<InteractionTypeSelectorButton>();

                button.Setup(this, EditTools[i]);

                buttons[i] = button;
            }
        }

        //UI
        private void Update()
        {
            lastUpdateTime = Time.time;

            if (!inEditMode) return;

            if (isInVR)
            {
                //UI
                Vector3 secondaryHandPosition;
                Vector3 ellbowPosition;

                if (PrimaryHand == HandType.RIGHT)
                {
                    secondaryHandPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    //secondaryHandPosition = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                    ellbowPosition = localPlayer.GetBonePosition(HumanBodyBones.LeftLowerArm);
                }
                else
                {
                    secondaryHandPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    //secondaryHandPosition = localPlayer.GetBonePosition(HumanBodyBones.RightHand);
                    ellbowPosition = localPlayer.GetBonePosition(HumanBodyBones.RightLowerArm);
                }

                armLengthInVR = (secondaryHandPosition - ellbowPosition).magnitude;

                VRUI.UpdatePosition(localPlayer, PrimaryHand, secondaryHandPosition, armLengthInVR);

                LinkedVRHandIndicator.position = InteractionPosition;
                LinkedVRHandIndicator.localScale = vertexInteractionDistance * 0.3f * Vector3.one;
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

                float scrollInput = Input.GetAxis("Mouse ScrollWheel");

                desktopPickupDistanceMultiplier *= (1 + scrollInput);
            }

            if (currentEditTool)
            {
                currentEditTool.UpdateWhenActive();
            }

            if (!isInVR)
            {
                desktopPickupDistance = PlayerHeight * 0.5f * desktopPickupDistanceMultiplier - vertexInteractionDistance;

                DistanceMaterialNormal.SetFloat(distancePropertyName, desktopPickupDistance);
                DistanceMaterialSelected.SetFloat(distancePropertyName, desktopPickupDistance);
                DistanceMaterialReadyToRemove.SetFloat(distancePropertyName, desktopPickupDistance);
            }
        }

        void DeselectTool()
        {
            if (currentEditTool == null) return;

            currentButton.Highlighted = false;
            currentButton = null;
            currentEditTool = null;

            linkedToolSettings.FlipedCanvas = false;

            if (isInVR)
            {
                VRUI.CurrentToolTextVR = "Current tool = None";
            }
            else
            {
                CurrentToolTextDesktop.text = "Current tool = None";
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
                        DeselectTool();
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
                    DeselectTool();
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
                        VRUI.CurrentToolTextVR = "Current tool = " + currentButton.LinkedTool.ToolName;
                    }
                    else
                    {
                        CurrentToolTextDesktop.text = "Current tool = " + currentButton.LinkedTool.ToolName;
                    }

                    currentEditTool = currentButton.LinkedTool;
                }

                value.OnActivation();

                if(linkedToolSettings == null)
                {
                    Debug.Log("Somehow null");
                    return;
                }

                linkedToolSettings.FlipedCanvas = value != null;
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

                if (SymmetryMode && Mathf.Abs(position.x) < vertexInteractionDistance) position.x = 0;

                return position;
            }
        }

        public int SelectVertex(int ignoreVertex)
        {
            if (isInVR) return SelectClosestVertexInVR(ignoreVertex);
            else return SelectVertexInDesktop(ignoreVertex);
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

                        return hand.position + vertexInteractionOffset * armLengthInVR * (hand.rotation * InteractorOffsetVector.normalized);
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

                    return currentHandData.position + currentHandData.rotation * (desktopPickupDistance * Vector3.forward);
                }
            }
        }

        Quaternion PrimaryHandRotation
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

        int SelectVertexInDesktop(int ignoreVertex)
        {
            VRCPlayerApi.TrackingData data = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            Vector3 localHeadPosition = meshTransform.InverseTransformPoint(data.position);

            Quaternion localHeadRotation = Quaternion.Inverse(meshTransform.rotation) * data.rotation;

            return linkedMeshEditor.GetClosestVectorInCylinder(localHeadPosition, localHeadRotation, vertexInteractionDistance, PlayerHeight, ignoreVertex);
        }

        int SelectClosestVertexInVR(int ignoreVertex)
        {
            return linkedMeshEditor.GetClosestVertexInRadius(LocalInteractionPositionWithoutMirrorLineSnap, vertexInteractionDistance, ignoreVertex);
        }

        //VRChat UI function calls
        public void ExitEditMode()
        {
            linkedToolSettings.InEditMode = false;
        }

        #region VRChat input functions

        float lastInputUseTime = 0;
        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            bool UIFocusWasOnSecondaryHand = UIFocusOnSecondaryHand;

            if (value && isInVR)
            {
                UIFocusOnSecondaryHand = args.handType != PrimaryHand;
            }

            //Warning: Currently called twice, at least in desktop mode
            if (lastInputUseTime == Time.time) return; //VRChat being buggy VRChat https://vrchat.canny.io/vrchat-udon-closed-alpha-bugs/p/1275-inputuse-is-called-twice-per-mouse-click
            lastInputUseTime = Time.time;

            if (VRUI.OverUIElement)
            {
                #if inputDebug
                    inputs += $"Input dismissed because over UI\n";
                #endif
                return;
            }

            if (!currentEditTool) return;

            if (args.handType != PrimaryHand) return; //Currently only one handed

            if (isInVR && UIFocusWasOnSecondaryHand && VRUI.ColliderEnabled) return; //Ignore input if UI focus is not on primary hand with VRUI enabled

            if (value)
            {
                InputUseDown();
            }
            else
            {
                InputUseUp();
            }
        }

        void InputUseDown()
        {
            if (!useAndGrabAreTheSame)
            {
                if (emulateAlternativeInput && !currentEditTool.ForceDifferentUseAndGrab)
                {
                    if (currentEditTool.IsHeld)
                    {
                        currentEditTool.OnUseDown();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnUseDown)} called\n";
                        #endif
                    }
                    else
                    {
                        currentEditTool.OnPickupDown();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnPickupDown)} called\n";
                        #endif
                    }
                }
                else
                {
                    currentEditTool.OnUseDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnUseDown)} called\n";
                    #endif
                }
            }
            else
            {
                if (emulateAlternativeInput || currentEditTool.ForceDifferentUseAndGrab)
                {
                    currentEditTool.OnUseDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnUseDown)} called\n";
                    #endif
                }
                else
                {
                    if (currentEditTool.IsHeld)
                    {
                        currentEditTool.OnUseDown();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnUseDown)} called\n";
                        #endif
                    }
                    else
                    {
                        currentEditTool.OnPickupDown();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnPickupDown)} called\n";
                        #endif
                    }
                }
            }
        }

        void InputUseUp()
        {
            if (!useAndGrabAreTheSame)
            {
                if (emulateAlternativeInput && !currentEditTool.ForceDifferentUseAndGrab)
                {
                    if (currentEditTool.IsHeld)
                    {
                        currentEditTool.OnUseUp();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnUseUp)} called\n";
                        #endif
                    }
                    else
                    {
                        currentEditTool.OnDropDown();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnDropDown)} called\n";
                        #endif
                    }
                }
                else
                {
                    currentEditTool.OnUseUp();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnUseUp)} called\n";
                    #endif
                }
            }
            else
            {
                if (emulateAlternativeInput || currentEditTool.ForceDifferentUseAndGrab)
                {
                    currentEditTool.OnUseUp();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnUseUp)} called\n";
                    #endif
                }
                else
                {
                    if (currentEditTool.IsHeld)
                    {
                        currentEditTool.OnUseUp();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnUseUp)} called\n";
                        #endif
                    }
                    else
                    {
                        currentEditTool.OnDropDown();
                        #if inputDebug
                            inputs += $"{nameof(currentEditTool.OnDropDown)} called\n";
                        #endif
                    }
                }
            }
        }

        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (VRUI.OverUIElement)
            {
                #if inputDebug
                    inputs += $"Input dismissed because over UI\n";
                #endif
                return;
            }

            if (!currentEditTool) return;

            if (args.handType != PrimaryHand) return; //Currently only one handed

            if (value)
            {
                InputGrabDown();
            }
            else
            {
                InputGrabUp();
            }
        }

        void InputGrabDown()
        {
            if (!useAndGrabAreTheSame)
            {
                if (!emulateAlternativeInput || currentEditTool.ForceDifferentUseAndGrab)
                {
                    currentEditTool.OnPickupDown();
                    #if inputDebug 
                        inputs += $"{nameof(currentEditTool.OnPickupDown)} called\n";
                    #endif
                }
                else
                {
                    currentEditTool.OnDropDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnDropDown)} called\n";
                    #endif
                }
            }
        }

        void InputGrabUp()
        {
            if (!useAndGrabAreTheSame || currentEditTool.ForceDifferentUseAndGrab)
            {
                if (!emulateAlternativeInput || currentEditTool.ForceDifferentUseAndGrab)
                {
                    currentEditTool.OnDropDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnDropDown)} called\n";
                    #endif
                }
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            if (value && isInVR) UIFocusOnSecondaryHand = args.handType != PrimaryHand;

            useAndGrabAreTheSame = true;

            //Over UI check not needed since not interacting with UI

            if (!currentEditTool) return;

            if (args.handType != PrimaryHand) return; //Currently only one handed

            if (value)
            {
                InputDropDown();
            }
            else
            {
                InputDropUp();
            }
        }

        void InputDropDown()
        {
            if (useAndGrabAreTheSame)
            {
                if(emulateAlternativeInput || currentEditTool.ForceDifferentUseAndGrab)
                {
                    currentEditTool.OnPickupDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnPickupDown)} called\n";
                    #endif
                }
                else
                {
                    currentEditTool.OnDropDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnDropDown)} called\n";
                    #endif
                }
            }
        }

        void InputDropUp()
        {
            if (useAndGrabAreTheSame || currentEditTool.ForceDifferentUseAndGrab)
            {
                if (emulateAlternativeInput)
                {
                    currentEditTool.OnDropDown();
                    #if inputDebug
                        inputs += $"{nameof(currentEditTool.OnDropDown)} called\n";
                    #endif
                }
            }
        }
        #endregion

        #if inputDebug
        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            inputs = "";
        }
        #endif
    }
}