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
                    VertexIndicators[interactedVertex].SelectState = VertexSelectStates.ReadyToDelete;
                    activeVertex = interactedVertex;
                }
                else if(interactedVertex != activeVertex)
                {
                    //Reselect vertex
                    VertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                    activeVertex = interactedVertex;
                    VertexIndicators[interactedVertex].SelectState = VertexSelectStates.ReadyToDelete;
                }
                else
                {
                    //Remove vertex
                    VertexIndicators[interactedVertex].SelectState = VertexSelectStates.Normal;
                    LinkedMeshController.RemoveVertexClean(activeVertex);
                    LinkedMeshInteractor.UpdateMesh(true);
                }
            }
            else
            {
                if(activeVertex >= 0)
                {
                    //Deselect vertex
                    if(activeVertex < LinkedMeshInteractor.vertexIndicators.Length)
                    {
                        VertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                    }

                    activeVertex = -1;
                }
                
            }
        }
    }
}