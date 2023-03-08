using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class StepTriangleAdderController : MeshEditTool
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
                return "Step add triangle";
            }
        }

        int closestVertex = -1;
        int secondClosestVertex = -1;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(closestVertex)} = {closestVertex}\n";
            returnString += $"{nameof(secondClosestVertex)} = {secondClosestVertex}\n";

            return returnString;
        }

        public override void Setup(MeshInteractor linkedMeshInteractor)
        {
            base.Setup(linkedMeshInteractor);
        }

        public override void OnActivation()
        {
            closestVertex= -1;
            secondClosestVertex= -1;
            LinkedMeshInteractor.ShowLineRenderer = false;
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
            int interactedVertex = LinkedMeshInteractor.SelectVertex();

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
                    LinkedMeshController.AddPlayerFacingTriangle(closestVertex, secondClosestVertex, interactedVertex, LinkedMeshInteractor.LocalHeadPosition);
                    LinkedMeshInteractor.UpdateMesh(true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
        }
        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;

            LinkedMeshInteractor.SetVertexIndicatorState(closestVertex, VertexSelectStates.Selected);
        }

        void DeselectClosestVertex()
        {
            if (closestVertex < 0) return;
            LinkedMeshInteractor.SetVertexIndicatorState(closestVertex, VertexSelectStates.Normal);
            closestVertex = -1;
        }

        void SelectSecondClosesVertex(int vertex)
        {
            secondClosestVertex = vertex;
            LinkedMeshInteractor.SetVertexIndicatorState(secondClosestVertex, VertexSelectStates.Selected);
        }

        void DeselectSecondClosestVertex()
        {
            if (secondClosestVertex < 0) return;
            LinkedMeshInteractor.SetVertexIndicatorState(secondClosestVertex, VertexSelectStates.Normal);
            secondClosestVertex = -1;
        }
    }
}