using JetBrains.Annotations;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MoveAndMergeController : MeshEditTool
    {
        public override bool CallUseInsteadOfPickup
        {
            get
            {
                return activeVertex >= 0;
            }
        }

        public override string ToolName
        {
            get
            {
                return "Move and Merge";
            }
        }

        int activeVertex = -1;

        public override void Setup(MeshInteractor linkedMeshInteractor)
        {
            base.Setup(linkedMeshInteractor);
        }

        public override void OnActivation()
        {
            activeVertex = -1;
            LinkedMeshInteractor.ShowLineRenderer = false;
        }

        public override void OnDeactivation()
        {

        }

        public override void UpdateWhenActive()
        {
            if (!CallUseInsteadOfPickup) return;

            Vector3 localPosition = LinkedMeshInteractor.LocalInteractionPositionWithMirror;

            LinkedMeshInteractor.MoveVertexToLocalPosition(activeVertex, localPosition);

            LinkedMeshInteractor.UpdateMesh(false);
        }

        public override void OnPickupUse()
        {
            activeVertex = LinkedMeshInteractor.SelectVertex();
        }

        public override void OnDropUse()
        {
            activeVertex = -1;
        }

        public override void OnUseDown()
        {
            int interactedVertex = LinkedMeshInteractor.SelectVertex();

            if (interactedVertex == activeVertex)
            {
                //Select again = drop
                activeVertex = -1;
            }
            else
            {
                //Select different one = Merge
                LinkedMeshController.MergeVertices(interactedVertex, activeVertex, true);

                LinkedMeshInteractor.UpdateMesh(true);

                activeVertex = -1;
            }
        }
    }
}