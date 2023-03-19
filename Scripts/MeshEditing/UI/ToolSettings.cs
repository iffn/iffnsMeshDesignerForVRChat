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
        [SerializeField] ToolController LinkedToolController;

        [SerializeField] Toggle UseRightHand;
        [SerializeField] Toggle InEditModeToggle;
        [SerializeField] Toggle UseWireframeMaterialToggle;
        [SerializeField] Slider VertexInteractionOffsetSlider;

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

        public void UpdateFromUI()
        {
            if (UseRightHand.isOn)
            {
                LinkedToolController.PrimaryHand = VRC.Udon.Common.HandType.RIGHT;
            }
            else
            {
                LinkedToolController.PrimaryHand = VRC.Udon.Common.HandType.LEFT;
            }

            LinkedToolController.vertexInteractionOffset = VertexInteractionOffsetSlider.value;

            //ToDo: Wireframe material toggle
        }

        public void InderactorSizeX1o25()
        {
            LinkedToolController.VertexInteractionDistance *= 1.25f;
        }

        public void InderactorSizeX0o8()
        {
            LinkedToolController.VertexInteractionDistance *= 0.8f;
        }
    }
}