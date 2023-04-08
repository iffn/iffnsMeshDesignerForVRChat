using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class ToolSettings : UdonSharpBehaviour
    {
        [Header("Basic Unity assingments")]
        [SerializeField] Toggle RightHandedModeToggle;
        [SerializeField] GameObject RightHandedModeHolder;
        [SerializeField] Toggle InEditModeToggle;
        [SerializeField] Toggle UseWireframeMaterialToggle;
        [SerializeField] Slider VertexInteractionOffsetSlider;
        [SerializeField] GameObject VertexInteractionOffsetHolder;
        [SerializeField] Shader NonWireframeShader;
        [SerializeField] Shader WireframeShader;
        [SerializeField] Material MeshMaterial;

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

            VertexInteractionOffsetHolder.SetActive(isInVR);
            RightHandedModeHolder.SetActive(isInVR);

            StandardDesktopControlsText.SetActive(false);
            AlternativeDesktopControlsText.SetActive(false);
            StandardUseIsGrabControlsText.SetActive(false);
            AlternativeUseIsGrabControlsText.SetActive(false);
            StandardUseIsNotGrabControlsText.SetActive(false);
            AlternativeUseIsNotGrabControlsText.SetActive(false);

            SetInstructionText();
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
                InEditModeToggle.SetIsOnWithoutNotify(value);
                linkedToolController.InEditMode = value;
            }
        }

        //VRChat UI events
        public void UpdateFromUI()
        {
            if (InEditModeToggle.isOn)
            {
                if (linkedMeshSyncController.IsOwner)
                {
                    linkedToolController.InEditMode = true;
                }
                else
                {
                    InEditModeToggle.SetIsOnWithoutNotify(false);
                }
            }
            else
            {
                linkedToolController.InEditMode = false;
            }

            linkedToolController.InEditMode = InEditModeToggle.isOn;

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


            if (UseWireframeMaterialToggle.isOn)
            {
                MeshMaterial.shader = WireframeShader;
            }
            else
            {
                MeshMaterial.shader = NonWireframeShader;
            }
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