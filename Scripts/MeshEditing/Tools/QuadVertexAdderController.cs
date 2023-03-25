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
        int closestVertexInConnectedArray = -1;
        int secondClosestVertexInConnectedArray = -1;

        Vector3 activeVertexPosition = Vector3.zero;

        int[] connectedVertices;
        Vector3[] connectedVertexPositions;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(activeVertex)} = {activeVertex}\n";
            returnString += $"{nameof(closestVertexInConnectedArray)} = {closestVertexInConnectedArray}\n";
            returnString += $"{nameof(secondClosestVertexInConnectedArray)} = {secondClosestVertexInConnectedArray}\n";
            returnString += $"{nameof(activeVertexPosition)} = {activeVertexPosition}\n";

            returnString += $"• {nameof(connectedVertices)} = {GetIntArrayString(connectedVertices)}\n";

            if (connectedVertexPositions != null) returnString += $"{nameof(connectedVertexPositions)}.length = {connectedVertexPositions.Length}\n";
            else returnString += $"{nameof(connectedVertexPositions)} = null\n";

            return returnString;
        }

        public override void OnActivation()
        {
            activeVertex = -1;
            closestVertexInConnectedArray= -1;
            secondClosestVertexInConnectedArray = -1;
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

            closestVertexInConnectedArray = -1;
            secondClosestVertexInConnectedArray = -1;

            float closestDistance = Mathf.Infinity;
            float secondclosestDistance = Mathf.Infinity;

            Vector3 localHandPosition = InteractionPositionWithMirrorLineSnap;

            for (int i = 0; i < connectedVertices.Length; i++)
            {
                Vector3 currentPosition = connectedVertexPositions[i];

                float distance = (localHandPosition - currentPosition).magnitude;

                //Override second
                if (distance < secondclosestDistance)
                {
                    secondclosestDistance = distance;
                    secondClosestVertexInConnectedArray = i;
                }

                //Swap
                if (secondclosestDistance < closestDistance)
                {
                    float tempD = secondclosestDistance;
                    int tempV = secondClosestVertexInConnectedArray;

                    secondclosestDistance = closestDistance;
                    secondClosestVertexInConnectedArray = closestVertexInConnectedArray;

                    closestDistance = tempD;
                    closestVertexInConnectedArray = tempV;
                }
            }

            LinkedInteractionInterface.SetLineRendererPositions(
                new Vector3[] { connectedVertexPositions[closestVertexInConnectedArray], localHandPosition, activeVertexPosition, localHandPosition, connectedVertexPositions[secondClosestVertexInConnectedArray] }
                , false);
        }

        void Deselect()
        {
            if (activeVertex >= 0) LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);

            LinkedInteractionInterface.ShowLineRenderer = false;

            activeVertex = -1;
            closestVertexInConnectedArray = -1;
            secondClosestVertexInConnectedArray = -1;
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
                else if(closestVertexInConnectedArray >= 0 && secondClosestVertexInConnectedArray >= 0)
                {
                    //Merge with vertex

                    Vector3 headPosition = HeadPosition;

                    if (interactedVertex != connectedVertices[closestVertexInConnectedArray])
                    {
                        LinkedInteractionInterface.AddPointFacingTriangle(activeVertex, interactedVertex, connectedVertices[closestVertexInConnectedArray], headPosition, false);
                    }

                    if(interactedVertex != connectedVertices[secondClosestVertexInConnectedArray])
                    {
                        LinkedInteractionInterface.AddPointFacingTriangle(activeVertex, interactedVertex, connectedVertices[secondClosestVertexInConnectedArray], headPosition, false);
                    }

                    LinkedInteractionInterface.UpdateMeshFromData();

                    Deselect();
                }
            }
            else
            {
                if (closestVertexInConnectedArray >= 0 && secondClosestVertexInConnectedArray >= 0)
                {
                    //Add new vertex
                    LinkedInteractionInterface.AddVertex(InteractionPositionWithMirrorLineSnap, new int[] { connectedVertices[closestVertexInConnectedArray], activeVertex, connectedVertices[secondClosestVertexInConnectedArray] }, true);
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