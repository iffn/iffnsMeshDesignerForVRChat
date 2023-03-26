using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class ToolSettings : UdonSharpBehaviour
    {
        [Header("Unity assingments")]

        [SerializeField] Toggle RightHandedModeToggle;
        [SerializeField] GameObject RightHandedModeHolder;
        [SerializeField] Toggle InEditModeToggle;
        [SerializeField] Toggle UseWireframeMaterialToggle;
        [SerializeField] Slider VertexInteractionOffsetSlider;
        [SerializeField] GameObject VertexInteractionOffsetHolder;
        [SerializeField] TMPro.TextMeshProUGUI DebugState;

        ToolController linkedToolController;

        public void Setup(ToolController linkedToolController)
        {
            this.linkedToolController = linkedToolController;

            bool isInVR = Networking.LocalPlayer.IsUserInVR();

            VertexInteractionOffsetHolder.SetActive(isInVR);
            RightHandedModeHolder.SetActive(isInVR);
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
            }
        }

        void WriteDebugText()
        {
            string debugText = $"Debug at {Time.time}\n";

            debugText += "\n";

            if (linkedToolController) debugText += linkedToolController.MultiLineDebugState();

            DebugState.text = debugText;
        }

        private void Update()
        {
            WriteDebugText();
        }

        //VRChat UI events
        public void UpdateFromUI()
        {
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

            //ToDo: Wireframe material toggle
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