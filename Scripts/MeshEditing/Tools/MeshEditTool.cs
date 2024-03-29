﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public abstract class MeshEditTool : UdonSharpBehaviour
    {
        [SerializeField] Sprite icon;

        //Runtime variables
        MeshEditor linkedMeshEditor;
        ToolController linkedToolController;
        protected MeshInteractionInterface LinkedInteractionInterface { get; private set; }

        public void Setup(ToolController linkedToolController, MeshEditor linkedMeshEditor, MeshInteractionInterface interactionInterface)
        {
            this.linkedToolController = linkedToolController;
            this.linkedMeshEditor = linkedMeshEditor;
            this.LinkedInteractionInterface = interactionInterface;
        }

        public Sprite Icon
        {
            get
            {
                return icon;
            }
        }

        //Mesh information access
        protected Vector3 HeadPosition
        {
            get
            {
                return linkedToolController.LocalHeadPosition;
            }
        }

        protected Vector3 InteractionPositionWithoutMirrorLineSnap
        {
            get
            {
                return linkedToolController.LocalInteractionPositionWithoutMirrorLineSnap;
            }
        }

        protected Vector3 InteractionPositionWithMirrorLineSnap
        {
            get
            {
                return linkedToolController.LocalInteractionPositionWithMirrorLineSnap;
            }
        }

        protected int SelectVertex()
        {
            return linkedToolController.SelectVertex(-1);
        }

        protected int SelectVertex(int ignoreVertex)
        {
            return linkedToolController.SelectVertex(ignoreVertex);
        }

        protected Vector3 GetLocalVertexPositionFromIndex(int index)
        {
            return linkedMeshEditor.GetLocalVertexPositionFromIndex(index);
        }

        protected int[] GetClosestVertices(Vector3 position, int count)
        {
            return linkedMeshEditor.GetClosestVertices(position, count);
        }

        protected int[] GetConnectedVertices(int index)
        {
            return linkedMeshEditor.GetConnectedVertices(index);
        }

        public virtual string MultiLineDebugState()
        {
            string returnString = $"Debug of {ToolName} at {Time.time:0.000}:\n"
                + $"• {nameof(IsHeld)} = {IsHeld}\n";

            return returnString;
        }

        protected Vector3[] GetPositionsFromIndexes(int[] vertices)
        {
            Vector3[] positions = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                positions[i] = GetLocalVertexPositionFromIndex(vertices[i]);
            }

            return positions;
        }

        //For override
        public abstract string ToolName { get; }

        public abstract bool IsHeld { get; }
        public virtual bool ForceDifferentUseAndGrab
        {
            get
            {
                return false;
            }
        }

        public abstract void OnActivation();
        public abstract void OnDeactivation();
        public abstract void UpdateWhenActive();

        public abstract void OnPickupDown();
        public abstract void OnDropDown();
        public abstract void OnUseDown();
        public abstract void OnUseUp();
    }

    public enum TriggerFunction
    {
        Grab,
        Select,
        Inverted
    }
}