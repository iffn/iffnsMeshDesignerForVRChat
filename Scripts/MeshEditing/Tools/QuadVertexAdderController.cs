using UdonSharp;
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

        int[] connectedVertices;
        Vector3[] connectedVertexPositions;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(activeVertex)} = {activeVertex}\n";
            returnString += $"{nameof(closestVertex)} = {closestVertex}\n";
            returnString += $"{nameof(secondClosestVertex)} = {secondClosestVertex}\n";
            returnString += $"{nameof(activeVertexPosition)} = {activeVertexPosition}\n";

            if(connectedVertices != null) returnString += $"{nameof(connectedVertices)}.length = {connectedVertices.Length}\n";
            else returnString += $"{nameof(connectedVertices)} = null\n";

            if (connectedVertexPositions != null) returnString += $"{nameof(connectedVertexPositions)}.length = {connectedVertexPositions.Length}\n";
            else returnString += $"{nameof(connectedVertexPositions)} = null\n";

            return returnString;
        }

        public override void OnActivation()
        {
            activeVertex = -1;
            closestVertex= -1;
            secondClosestVertex = -1;
            LinkedInteractionInterface.ShowLineRenderer = false;
        }

        public override void OnDeactivation()
        {
            if (activeVertex >= 0) LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);
            LinkedInteractionInterface.ShowLineRenderer = false;
        }

        public override void UpdateWhenActive()
        {
            if (activeVertex == -1) return;

            closestVertex = -1;
            secondClosestVertex = -1;

            float closestDistance = Mathf.Infinity;
            float secondclosestDistance = Mathf.Infinity;

            Vector3 localHandPosition = InteractionPositionWithMirrorLineSnap;

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

            LinkedInteractionInterface.SetLineRendererPositions(
                new Vector3[] { connectedVertexPositions[closestVertex], localHandPosition, activeVertexPosition, localHandPosition, connectedVertexPositions[secondClosestVertex] }
                , false);
        }

        void Deselect()
        {
            if (activeVertex >= 0) LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);

            LinkedInteractionInterface.ShowLineRenderer = false;

            activeVertex = -1;
            closestVertex = -1;
            secondClosestVertex = -1;
        }

        public override void OnUseDown()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex != -1)
            {
                if(activeVertex == -1)
                {
                    //Select current
                    activeVertex = interactedVertex;
                    activeVertexPosition = GetLocalVertexPositionFromIndex(activeVertex);
                    LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Selected);
                    LinkedInteractionInterface.ShowLineRenderer = true;

                    connectedVertices = GetConnectedVertices(activeVertex);
                    connectedVertexPositions = new Vector3[connectedVertices.Length];

                    connectedVertexPositions = GetPositionsFromIndexes(connectedVertices);

                }
                else if (activeVertex == interactedVertex)
                {
                    //Deselect current
                    Deselect();
                }
                else if(closestVertex >= 0 && secondClosestVertex >= 0)
                {
                    //Merge with vertex

                    Vector3 headPosition = HeadPosition;

                    if (interactedVertex != connectedVertices[closestVertex])
                    {
                        LinkedInteractionInterface.AddPointFacingTriangle(activeVertex, interactedVertex, closestVertex, headPosition, false);
                    }

                    if(interactedVertex != connectedVertices[secondClosestVertex])
                    {
                        LinkedInteractionInterface.AddPointFacingTriangle(activeVertex, interactedVertex, secondClosestVertex, headPosition, false);
                    }

                    LinkedInteractionInterface.UpdateMeshFromData();

                    Deselect();
                }
            }
            else
            {
                if (closestVertex >= 0 && secondClosestVertex >= 0)
                {
                    //Add new vertex
                    LinkedInteractionInterface.AddVertex(InteractionPositionWithMirrorLineSnap, new int[] { activeVertex, closestVertex, secondClosestVertex }, true);
                }

                Deselect();
            }
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