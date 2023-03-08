using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class StepTriangleRemoverController : MeshEditTool
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

        public override void Setup(MeshInteractor linkedMeshInteractor)
        {
            base.Setup(linkedMeshInteractor);
        }

        public override void OnActivation()
        {
            closestVertex = -1;
            secondClosestVertex = -1;
            LinkedMeshInteractor.ShowLineRenderer = false;
        }

        public override void OnDeactivation()
        {

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
                    LinkedMeshController.TryRemoveTriangle(closestVertex, secondClosestVertex, interactedVertex);
                    LinkedMeshInteractor.UpdateMesh(true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
        }
        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;
            LinkedMeshInteractor.VertexIndicators[closestVertex].SelectState = VertexSelectStates.Selected;
        }

        void DeselectClosestVertex()
        {
            if (closestVertex < 0) return;
            LinkedMeshInteractor.VertexIndicators[closestVertex].SelectState = VertexSelectStates.Normal;
            closestVertex = -1;
        }

        void SelectSecondClosesVertex(int vertex)
        {
            secondClosestVertex = vertex;
            LinkedMeshInteractor.VertexIndicators[secondClosestVertex].SelectState = VertexSelectStates.Selected;
        }

        void DeselectSecondClosestVertex()
        {
            if (secondClosestVertex < 0) return;
            LinkedMeshInteractor.VertexIndicators[secondClosestVertex].SelectState = VertexSelectStates.Normal;
            secondClosestVertex = -1;
        }
    }
}