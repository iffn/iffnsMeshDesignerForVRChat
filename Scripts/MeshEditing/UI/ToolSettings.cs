﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class ToolSettings : UdonSharpBehaviour
    {
        [Header("Basic Unity assingments")]
        [SerializeField] Toggle RightHandedModeToggle;
        [SerializeField] Toggle InEditModeToggle;
        [SerializeField] Toggle UseWireframeMaterialToggle;
        [SerializeField] Slider VertexInteractionOffsetSlider;
        [SerializeField] Shader NonWireframeShader;
        [SerializeField] string NonWireframeShaderColorIdentifier;
        [SerializeField] Shader WireframeShader;
        [SerializeField] string WireframeShaderColorIdentifier;
        [SerializeField] Material MeshMaterial;
        [SerializeField] Slider MaterialColorRedSlider;
        [SerializeField] Slider MaterialColorGreenSlider;
        [SerializeField] Slider MaterialColorBlueSlider;
        [SerializeField] Image MaterialColorPreviewImage;
        [SerializeField] Transform CanvasHolder;
        [SerializeField] Toggle DebugToggle;
        [SerializeField] GameObject DebugHolder;
        [SerializeField] GameObject FlipText;
        [SerializeField] GameObject[] DesktopOnlyObjects;
        [SerializeField] GameObject[] VROnlyObjects;
        [SerializeField] GameObject StartMessage;

        [Header("Unity assingments for controll system")]
        [SerializeField] TMPro.TextMeshProUGUI ControllerText;
        [SerializeField] Toggle EmulateAlternativeInputTypeToggle;
        [SerializeField] GameObject StandardDesktopControlsText;
        [SerializeField] GameObject AlternativeDesktopControlsText;
        [SerializeField] GameObject StandardUseIsGrabControlsText;
        [SerializeField] GameObject AlternativeUseIsGrabControlsText;
        [SerializeField] GameObject StandardUseIsNotGrabControlsText;
        [SerializeField] GameObject AlternativeUseIsNotGrabControlsText;

        ToolController linkedToolController;
        MeshSyncController linkedMeshSyncController;

        public void Setup(ToolController linkedToolController, MeshSyncController linkedMeshSyncController)
        {
            this.linkedToolController = linkedToolController;
            this.linkedMeshSyncController = linkedMeshSyncController;

            bool isInVR = Networking.LocalPlayer.IsUserInVR();

            if (linkedToolController.UseAndGrabAreTheSame) ControllerText.text = "Controller type: Use = grab";
            else ControllerText.text = "Controller type: Use ≠ grab";

            foreach(GameObject current in DesktopOnlyObjects)
            {
                current.SetActive(!isInVR);
            }

            foreach (GameObject current in VROnlyObjects)
            {
                current.SetActive(isInVR);
            }

            StandardDesktopControlsText.SetActive(false);
            AlternativeDesktopControlsText.SetActive(false);
            StandardUseIsGrabControlsText.SetActive(false);
            AlternativeUseIsGrabControlsText.SetActive(false);
            StandardUseIsNotGrabControlsText.SetActive(false);
            AlternativeUseIsNotGrabControlsText.SetActive(false);

            SetInstructionText();

            DebugHolder.SetActive(DebugToggle.isOn);
        }

        void SetInstructionText()
        {
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                if (!linkedToolController.emulateAlternativeInput)
                {
                    StandardDesktopControlsText.SetActive(true);
                    AlternativeDesktopControlsText.SetActive(false);
                }
                else
                {
                    StandardDesktopControlsText.SetActive(false);
                    AlternativeDesktopControlsText.SetActive(true);
                }
            }
            else
            {
                if (linkedToolController.UseAndGrabAreTheSame)
                {
                    if (!linkedToolController.emulateAlternativeInput)
                    {
                        StandardUseIsGrabControlsText.SetActive(true);
                        AlternativeUseIsGrabControlsText.SetActive(false);
                    }
                    else
                    {
                        StandardUseIsGrabControlsText.SetActive(false);
                        AlternativeUseIsGrabControlsText.SetActive(true);
                    }
                }
                else
                {
                    if (!linkedToolController.emulateAlternativeInput)
                    {
                        StandardUseIsNotGrabControlsText.SetActive(true);
                        AlternativeUseIsNotGrabControlsText.SetActive(false);
                    }
                    else
                    {
                        StandardUseIsNotGrabControlsText.SetActive(false);
                        AlternativeUseIsNotGrabControlsText.SetActive(true);
                    }
                }
            }
        }

        public bool InEditMode
        {
            get
            {
                return InEditModeToggle.isOn;
            }
            set
            {
                StartMessage.SetActive(false);

                InEditModeToggle.SetIsOnWithoutNotify(value);
                linkedToolController.InEditMode = value;
            }
        }

        public bool FlipedCanvas
        {
            set
            {
                if (value) CanvasHolder.localRotation = Quaternion.Euler(0, -90, 0);
                else CanvasHolder.localRotation = Quaternion.Euler(0, 90, 0);

                FlipText.SetActive(value);
            }
        }

        //VRChat UI events
        public void UpdateFromUI()
        {
            if (InEditModeToggle.isOn)
            {
                if (!linkedMeshSyncController.IsOwner)
                {
                    InEditModeToggle.SetIsOnWithoutNotify(false);
                }
            }

            InEditMode = InEditModeToggle.isOn;

            //Controls
            if (RightHandedModeToggle.isOn)
            {
                linkedToolController.PrimaryHand = VRC.Udon.Common.HandType.RIGHT;
            }
            else
            {
                linkedToolController.PrimaryHand = VRC.Udon.Common.HandType.LEFT;
            }

            linkedToolController.vertexInteractionOffset = VertexInteractionOffsetSlider.value;

            linkedToolController.emulateAlternativeInput = EmulateAlternativeInputTypeToggle.isOn;

            SetInstructionText();

            //Color selection
            Color color = new Color(MaterialColorRedSlider.value, MaterialColorGreenSlider.value, MaterialColorBlueSlider.value);
            MaterialColorPreviewImage.color = color;

            //Mesh material
            if (UseWireframeMaterialToggle.isOn)
            {
                MeshMaterial.shader = WireframeShader;
                MeshMaterial.SetColor(WireframeShaderColorIdentifier, color);
            }
            else
            {
                MeshMaterial.shader = NonWireframeShader;
                MeshMaterial.SetColor(NonWireframeShaderColorIdentifier, color);
            }

            DebugHolder.SetActive(DebugToggle.isOn);
        }

        public void InderactorSizeX1o25()
        {
            linkedToolController.VertexInteractionDistance *= 1.25f;
        }

        public void InderactorSizeX0o8()
        {
            linkedToolController.VertexInteractionDistance *= 0.8f;
        }
    }
}