using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class SingleVertexRemoverController : MeshEditTool
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
                return "Remove single vertex";
            }
        }

        int activeVertex = -1;

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(activeVertex)} = {activeVertex}\n";

            return returnString;
        }

        public override void Setup(MeshInteractor linkedMeshInteractor)
        {
            base.Setup(linkedMeshInteractor);
        }

        public override void OnActivation()
        {
            activeVertex= -1;
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
                if(activeVertex == -1)
                {
                    //Select vertex
                    LinkedMeshInteractor.SetVertexIndicatorState(interactedVertex, VertexSelectStates.ReadyToDelete);
                    activeVertex = interactedVertex;
                }
                else if(interactedVertex != activeVertex)
                {
                    //Reselect vertex
                    LinkedMeshInteractor.SetVertexIndicatorState(activeVertex, VertexSelectStates.Normal);
                    activeVertex = interactedVertex;
                    LinkedMeshInteractor.SetVertexIndicatorState(interactedVertex, VertexSelectStates.ReadyToDelete);
                }
                else
                {
                    //Remove vertex
                    LinkedMeshInteractor.SetVertexIndicatorState(interactedVertex, VertexSelectStates.Normal);
                    LinkedMeshController.RemoveVertexClean(activeVertex);
                    LinkedMeshInteractor.UpdateMesh(true);
                }
            }
            else
            {
                if(activeVertex >= 0)
                {
                    //Deselect vertex
                    LinkedMeshInteractor.SetVertexIndicatorState(activeVertex, VertexSelectStates.Normal);

                    activeVertex = -1;
                }
                
            }
        }
    }
}