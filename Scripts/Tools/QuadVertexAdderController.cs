﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class QuadVertexAdderController : MeshEditTool
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
                return "Quad add vertex";
            }
        }

        int activeVertex = -1;
        int closestVertex = -1;
        int secondClosestVertex = -1;

        Vector3 activeVertexPosition = Vector3.zero;

        Vector3 localHandPosition;

        int[] connectedVertices;
        Vector3[] connectedVertexPositions;

        public override void Setup(MeshInteractor linkedMeshInteractor)
        {
            base.Setup(linkedMeshInteractor);
        }

        public override void OnActivation()
        {
            activeVertex = -1;
            closestVertex= -1;
            secondClosestVertex = -1;
            LinkedMeshInteractor.ShowLineRenderer = false;
        }

        public override void OnDeactivation()
        {

        }

        public override void UpdateWhenActive()
        {
            if (activeVertex == -1) return;

            closestVertex = -1;
            secondClosestVertex = -1;

            float closestDistance = Mathf.Infinity;
            float secondclosestDistance = Mathf.Infinity;

            localHandPosition = LinkedMeshInteractor.LocalInteractionPositionWithMirror;

            for (int i = 0; i < connectedVertices.Length; i++)
            {
                if (i == activeVertex) continue;

                Vector3 currentPosition = connectedVertexPositions[i];

                float distance = (localHandPosition - currentPosition).magnitude;

                //Override second
                if (distance < secondclosestDistance)
                {
                    secondclosestDistance = distance;
                    secondClosestVertex = i;
                }

                //Swap
                if (secondclosestDistance < closestDistance)
                {
                    float tempD = secondclosestDistance;
                    int tempV = secondClosestVertex;

                    secondclosestDistance = closestDistance;
                    secondClosestVertex = closestVertex;

                    closestDistance = tempD;
                    closestVertex = tempV;
                }
            }

            localHandPosition = LinkedMeshInteractor.LocalInteractionPositionWithMirror;

            LinkedMeshInteractor.SetLocalLineRendererPositions(
                new Vector3[] { connectedVertexPositions[closestVertex], localHandPosition, activeVertexPosition, localHandPosition, connectedVertexPositions[secondClosestVertex] }
                , false);
        }

        void Deselect()
        {
            if (activeVertex >= 0) LinkedMeshInteractor.VertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;

            LinkedMeshInteractor.ShowLineRenderer = false;

            activeVertex = -1;
            closestVertex = -1;
            secondClosestVertex = -1;
        }

        public override void OnUseDown()
        {
            int interactedVertex = LinkedMeshInteractor.SelectVertex();

            if (interactedVertex != -1)
            {
                if(activeVertex == -1)
                {
                    //Select current
                    activeVertex = interactedVertex;
                    activeVertexPosition = LinkedMeshController.Vertices[activeVertex];
                    LinkedMeshInteractor.VertexIndicators[activeVertex].SelectState = VertexSelectStates.Selected;
                    LinkedMeshInteractor.ShowLineRenderer = true;

                    connectedVertices = LinkedMeshController.GetConnectedVertices(interactedVertex);
                    connectedVertexPositions = new Vector3[connectedVertices.Length];

                    for(int i = 0; i <connectedVertices.Length; i++)
                    {
                        connectedVertexPositions[i] = LinkedMeshController.Vertices[connectedVertices[i]];
                    }

                }
                else if (activeVertex == interactedVertex)
                {
                    //Deselect current
                    Deselect();
                }
                else if(closestVertex >= 0 && secondClosestVertex >= 0)
                {
                    //Merge with vertex

                    Vector3 localHeadPosition = LinkedMeshInteractor.LocalHeadPosition;

                    if(interactedVertex != connectedVertices[closestVertex])
                    {
                        LinkedMeshController.AddPlayerFacingTriangle(activeVertex, connectedVertices[closestVertex], interactedVertex, localHeadPosition);
                    }

                    if(interactedVertex != connectedVertices[secondClosestVertex])
                    {
                        LinkedMeshController.AddPlayerFacingTriangle(activeVertex, connectedVertices[secondClosestVertex], interactedVertex, localHeadPosition);
                    }

                    Deselect();

                    LinkedMeshInteractor.UpdateMesh(false);
                }
            }
            else
            {
                if (closestVertex >= 0 && secondClosestVertex >= 0)
                {
                    //Add new vertex
                    LinkedMeshController.AddVertex(localHandPosition, connectedVertices[closestVertex], activeVertex, LinkedMeshInteractor.LocalHeadPosition);
                    LinkedMeshController.AddPlayerFacingTriangle(LinkedMeshController.Vertices.Length - 1, connectedVertices[secondClosestVertex], activeVertex, LinkedMeshInteractor.LocalHeadPosition);
                }
                
                LinkedMeshInteractor.UpdateMesh(true);

                Deselect();
            }

            
        }
    }
}