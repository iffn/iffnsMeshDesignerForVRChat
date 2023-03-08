using System.Diagnostics;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class StepVertexAdderController : MeshEditTool
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
                return "Step add vertex";
            }
        }

        int closestVertex = -1;
        int secondClosestVertex = -1;

        Vector3 closestVertexPosition = Vector3.zero;
        Vector3 secondClosestVertexPosition = Vector3.zero;

        Vector3 localHandPosition;

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
            if (closestVertex == -1 || secondClosestVertex == -1) return;

            localHandPosition = LinkedMeshInteractor.LocalInteractionPositionWithMirror;

            LinkedMeshInteractor.SetLocalLineRendererPositions(
                new Vector3[] { localHandPosition, closestVertexPosition, secondClosestVertexPosition }
                , true);
        }

        public override void OnUseDown()
        {
            int interactedVertex = LinkedMeshInteractor.SelectVertex();

            if (interactedVertex != -1)
            {
                if (closestVertex == -1)
                {
                    SelectClosesVertex(interactedVertex);
                }
                else if (closestVertex == interactedVertex)
                {
                    DeselectClosestVertex();
                }
                else if (secondClosestVertex == -1)
                {
                    SelectSecondClosesVertex(interactedVertex);
                }
                else if (secondClosestVertex == interactedVertex)
                {
                    DeselectSecondClosestVertex();
                }
                else
                {
                    //Add triangle instead of new vertex
                    LinkedMeshController.AddPlayerFacingTriangle(closestVertex, secondClosestVertex, interactedVertex, LinkedMeshInteractor.LocalHeadPosition);
                    LinkedMeshInteractor.UpdateMesh(true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
            else
            {
                if (closestVertex == -1 || secondClosestVertex == -1) return;

                //Add new vertex
                LinkedMeshController.AddVertex(localHandPosition, closestVertex, secondClosestVertex, LinkedMeshInteractor.LocalHeadPosition);
                LinkedMeshInteractor.UpdateMesh(true);

                DeselectClosestVertex();
                DeselectSecondClosestVertex();
            }

            LinkedMeshInteractor.ShowLineRenderer = (closestVertex >= 0 && secondClosestVertex >= 0);
        }

        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;
            closestVertexPosition = LinkedMeshController.Vertices[vertex];
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
            secondClosestVertexPosition = LinkedMeshController.Vertices[vertex];
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