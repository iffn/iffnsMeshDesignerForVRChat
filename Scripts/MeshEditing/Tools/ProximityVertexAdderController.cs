using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class ProximityVertexAdderController : MeshEditTool
    {
        public override bool IsHeld
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

        int[] vertices = new int[2];

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState()
                + $"• {nameof(vertices)} = {GetIntArrayString(vertices)}\n"
                + $"• Vertex index length = {vertices.Length}\n";

            return returnString;
        }

        public override void OnActivation()
        {
            vertices = new int[0];
            LinkedInteractionInterface.ShowLineRenderer = true;
        }

        public override void OnDeactivation()
        {
            LinkedInteractionInterface.ShowLineRenderer = false;
        }

        public override void UpdateWhenActive()
        {
            Vector3 interactionPosition = InteractionPositionWithMirrorLineSnap;

            vertices = GetClosestVertices(interactionPosition, 2);

            if (vertices.Length != 2) return;

            Vector3[] foundPositons = GetPositionsFromIndexes(vertices);
            Vector3[] positions = new Vector3[foundPositons.Length + 1];

            for(int i = 0; i < foundPositons.Length; i++)
            {
                positions[i] = foundPositons[i];
            }

            positions[positions.Length - 1] = interactionPosition;

            LinkedInteractionInterface.SetLineRendererPositions(positions, true);
        }

        public override void OnUseDown()
        {
            if(vertices.Length != 2) return;

            LinkedInteractionInterface.AddVertex(InteractionPositionWithMirrorLineSnap, vertices, true);

            vertices = new int[0];
        }

        public override void OnPickupDown()
        {
            
        }

        public override void OnDropDown()
        {
            
        }

        public override void OnUseUp()
        {
            
        }

        string GetIntArrayString(int[] array) //Should be static
        {
            if (array == null) return "null";
            if (array.Length == 0) return "[]";

            string returnString = "[";

            foreach (int element in array)
            {
                returnString += element + ", ";
            }

            returnString = returnString.Substring(0, returnString.Length - 2) + "]";

            return returnString;
        }
    }
}