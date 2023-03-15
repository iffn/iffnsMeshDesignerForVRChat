using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class ProximityVertexAdderController : MeshEditTool
    {
        public override bool CallUseInsteadOfPickup
        {
            get
            {
                return true;
            }
        }

        public override string ToolName
        {
            get
            {
                return "Proximity add vertex";
            }
        }

        int[] vertices = new int[0];

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"Vertex index length = {vertices.Length}\n";

            return returnString;
        }

        public override void OnActivation()
        {
            vertices = new int[0];
            LinkedInteractionProvider.ShowLineRenderer = true;
        }

        public override void OnDeactivation()
        {
            

            LinkedInteractionProvider.ShowLineRenderer = false;
        }

        public override void UpdateWhenActive()
        {
            vertices = GetClosestVertices(InteractionPositionWithMirrorLineSnap, 2);

            if (vertices.Length != 2) return;

            LinkedInteractionProvider.SetLineRendererPositions(GetPositionsFromIndexes(vertices), true);
        }

        public override void OnUseDown()
        {
            if(vertices.Length != 2) return;

            LinkedInteractionProvider.AddVertex(InteractionPositionWithMirrorLineSnap, vertices, true);

            vertices = new int[0];
        }

        public override void OnPickupUse()
        {
            
        }

        public override void OnDropUse()
        {
            
        }

        public override void OnUseUp()
        {
            
        }
    }
}