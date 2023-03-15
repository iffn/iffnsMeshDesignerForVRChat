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
        public override bool CallUseInsteadOfPickup
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
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(activeVertex)} = {activeVertex}\n";

            return returnString;
        }

        public override void OnActivation()
        {
            activeVertex = -1;
        }

        public override void OnDeactivation()
        {
            activeVertex = -1;
        }

        public override void UpdateWhenActive()
        {
            if (!CallUseInsteadOfPickup) return;

            Vector3 localPosition = InteractionPositionWithMirrorLineSnap;

            LinkedInteractionProvider.MoveVertexToPosition(activeVertex, localPosition, true);
        }

        public override void OnPickupUse()
        {
            activeVertex = SelectVertex();
        }

        public override void OnDropUse()
        {
            activeVertex = -1;
        }

        public override void OnUseDown()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex == activeVertex)
            {
                //Select again = drop
                activeVertex = -1;
            }
            else
            {
                //Select different one = Merge
                LinkedInteractionProvider.MergeVertices(interactedVertex, activeVertex, true);

                activeVertex = -1;
            }
        }

        public override void OnUseUp()
        {
            
        }
    }
}