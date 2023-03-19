﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class ToolSettings : UdonSharpBehaviour
    {
        [Header("Unity assingments")]

        [SerializeField] Toggle UseRightHand;
        [SerializeField] Toggle InEditModeToggle;
        [SerializeField] Toggle UseWireframeMaterialToggle;
        [SerializeField] Slider VertexInteractionOffsetSlider;

        ToolController linkedToolController;

        public void Setup(ToolController linkedToolController)
        {
            this.linkedToolController = linkedToolController;
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

        public void UpdateFromUI()
        {
            if (UseRightHand.isOn)
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