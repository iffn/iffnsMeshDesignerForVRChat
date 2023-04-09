using JetBrains.Annotations;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MoveAndMergeController : MeshEditTool
    {
        public override bool IsHeld
        {
            get
            {
                return activeVertex >= 0;
            }
        }

        public override string ToolName
        {
            get
            {
                return "Move and Merge";
            }
        }

        int activeVertex = -1;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState()
                + $"• {nameof(activeVertex)} = {activeVertex}\n";

            return returnString;
        }

        public override void OnActivation()
        {
            activeVertex = -1;
        }

        public override void OnDeactivation()
        {
            if (activeVertex < 0) return;
            LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);
            activeVertex = -1;
        }

        public override void UpdateWhenActive()
        {
            if (!IsHeld) return;

            Vector3 localPosition = InteractionPositionWithMirrorLineSnap;

            LinkedInteractionInterface.MoveVertexToPosition(activeVertex, localPosition, true);
        }

        public override void OnPickupDown()
        {
            activeVertex = SelectVertex(activeVertex);
        }

        public override void OnDropDown()
        {
            activeVertex = -1;
        }

        public override void OnUseDown()
        {
            int interactedVertex = SelectVertex(activeVertex);

            if (interactedVertex < 0) return;

            if (interactedVertex == activeVertex)
            {
                //Select again = drop
                activeVertex = -1;
            }
            else
            {
                //Select different one = Merge
                LinkedInteractionInterface.MergeVertices(interactedVertex, activeVertex, true);

                activeVertex = -1;
            }
        }

        public override void OnUseUp()
        {
            
        }
    }
}