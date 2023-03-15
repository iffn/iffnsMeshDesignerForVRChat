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

        public override void OnActivation()
        {
            activeVertex= -1;
        }

        public override void OnDeactivation()
        {

        }

        public override void UpdateWhenActive()
        {
            
        }

        public override void OnUseDown()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex != -1)
            {
                if(activeVertex == -1)
                {
                    //Select vertex
                    LinkedInteractionProvider.SetVertexSelectState(interactedVertex, VertexSelectStates.ReadyToDelete);
                    activeVertex = interactedVertex;
                }
                else if(interactedVertex != activeVertex)
                {
                    //Reselect vertex
                    LinkedInteractionProvider.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);
                    activeVertex = interactedVertex;
                    LinkedInteractionProvider.SetVertexSelectState(interactedVertex, VertexSelectStates.ReadyToDelete);
                }
                else
                {
                    //Remove vertex
                    LinkedInteractionProvider.SetVertexSelectState(interactedVertex, VertexSelectStates.Normal);
                    LinkedInteractionProvider.RemoveVertex(activeVertex);
                }
            }
            else
            {
                if(activeVertex >= 0)
                {
                    //Deselect vertex
                    LinkedInteractionProvider.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);

                    activeVertex = -1;
                }
                
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