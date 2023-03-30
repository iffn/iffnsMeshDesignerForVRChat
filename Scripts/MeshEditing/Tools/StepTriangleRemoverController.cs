using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class StepTriangleRemoverController : MeshEditTool
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
                return "Step remove triangle";
            }
        }

        int closestVertex = -1;
        int secondClosestVertex = -1;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState()
                + $"• {nameof(closestVertex)} = {closestVertex}\n"
                + $"• {nameof(secondClosestVertex)} = {secondClosestVertex}\n";

            return returnString;
        }

        public override void OnActivation()
        {
            closestVertex = -1;
            secondClosestVertex = -1;
        }

        public override void OnDeactivation()
        {
            DeselectClosestVertex();
            DeselectSecondClosestVertex();
        }

        public override void UpdateWhenActive()
        {

        }

        public override void OnUseDown()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex != -1)
            {
                if (closestVertex == -1)
                {
                    SelectClosesVertex(interactedVertex);
                    return;
                }
                else if (closestVertex == interactedVertex)
                {
                    DeselectClosestVertex();
                    return;
                }
                else if (secondClosestVertex == -1)
                {
                    SelectSecondClosesVertex(interactedVertex);
                    return;
                }
                else if (secondClosestVertex == interactedVertex)
                {
                    DeselectSecondClosestVertex();
                    return;
                }
                else
                {
                    LinkedInteractionInterface.RemoveTriangle(closestVertex, secondClosestVertex, interactedVertex, true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
        }
        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;
            LinkedInteractionInterface.SetVertexSelectState(closestVertex, VertexSelectStates.Selected);
        }

        void DeselectClosestVertex()
        {
            if (closestVertex < 0) return;
            LinkedInteractionInterface.SetVertexSelectState(closestVertex, VertexSelectStates.Normal);
            closestVertex = -1;
        }

        void SelectSecondClosesVertex(int vertex)
        {
            secondClosestVertex = vertex;
            LinkedInteractionInterface.SetVertexSelectState(secondClosestVertex, VertexSelectStates.Selected);
        }

        void DeselectSecondClosestVertex()
        {
            if (secondClosestVertex < 0) return;
            LinkedInteractionInterface.SetVertexSelectState(secondClosestVertex, VertexSelectStates.Normal);
            secondClosestVertex = -1;
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
    }
}