using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class SingleVertexRemoverController : MeshEditTool
    {
        public override bool IsHeld
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
            string returnString = base.MultiLineDebugState()
                + $"• {nameof(activeVertex)} = {activeVertex}\n";

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
                    LinkedInteractionInterface.SetVertexSelectState(interactedVertex, VertexSelectStates.ReadyToDelete);
                    activeVertex = interactedVertex;
                }
                else if(interactedVertex != activeVertex)
                {
                    //Reselect vertex
                    LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);
                    activeVertex = interactedVertex;
                    LinkedInteractionInterface.SetVertexSelectState(interactedVertex, VertexSelectStates.ReadyToDelete);
                }
                else
                {
                    //Remove vertex
                    LinkedInteractionInterface.SetVertexSelectState(interactedVertex, VertexSelectStates.Normal);
                    LinkedInteractionInterface.RemoveVertex(activeVertex, true);
                }
            }
            else
            {
                if(activeVertex >= 0)
                {
                    //Deselect vertex
                    LinkedInteractionInterface.SetVertexSelectState(activeVertex, VertexSelectStates.Normal);

                    activeVertex = -1;
                }
                
            }
        }

        public override void OnPickupDown()
        {
            
        }

        public override void OnDropDown()
        {
            
        }

        public override void OnUseUp()
        {
            
        }
    }
}