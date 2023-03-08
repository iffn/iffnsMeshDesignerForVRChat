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

        int closestVertex = -1;
        int secondClosestVertex = -1;
        Vector3 localHandPosition;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(closestVertex)} = {closestVertex}\n";
            returnString += $"{nameof(secondClosestVertex)} = {secondClosestVertex}\n";
            returnString += $"{nameof(localHandPosition)} = {localHandPosition}\n";

            return returnString;
        }

        public override void Setup(MeshInteractor linkedMeshInteractor)
        {
            base.Setup(linkedMeshInteractor);
        }

        public override void OnActivation()
        {
            closestVertex = -1;
            secondClosestVertex = -1;
            LinkedMeshInteractor.ShowLineRenderer = true;
        }

        public override void OnDeactivation()
        {

        }

        public override void UpdateWhenActive()
        {
            Vector3[] vertices = LinkedMeshController.Vertices;

            closestVertex = -1;
            secondClosestVertex = -1;

            float closestDistance = Mathf.Infinity;
            float secondclosestDistance = Mathf.Infinity;

            localHandPosition = LinkedMeshInteractor.LocalInteractionPositionWithMirror;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 currentPosition = vertices[i];

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

            LinkedMeshInteractor.SetLocalLineRendererPositions(new Vector3[] { localHandPosition, vertices[closestVertex], vertices[secondClosestVertex] }, true);
        }

        public override void OnUseDown()
        {
            if (closestVertex == -1) return;
            if (secondClosestVertex == -1) return;

            LinkedMeshController.AddVertex(localHandPosition, closestVertex, secondClosestVertex, LinkedMeshInteractor.LocalHeadPosition);
            LinkedMeshInteractor.UpdateMesh(true);
        }
    }
}