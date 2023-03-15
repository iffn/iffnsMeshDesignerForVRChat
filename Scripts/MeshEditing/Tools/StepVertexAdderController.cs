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

        public override string MultiLineDebugState()
        {
            string returnString = base.MultiLineDebugState();

            returnString += $"{nameof(closestVertex)} = {closestVertex}\n";
            returnString += $"{nameof(secondClosestVertex)} = {secondClosestVertex}\n";
            returnString += $"{nameof(closestVertexPosition)} = {closestVertexPosition}\n";
            returnString += $"{nameof(secondClosestVertexPosition)} = {secondClosestVertexPosition}\n";

            return returnString;
        }

        public override void OnActivation()
        {
            closestVertex = -1;
            secondClosestVertex = -1;
        }

        public override void OnDeactivation()
        {
            DeselectClosestVertex();
            DeselectSecondClosestVertex();
        }

        public override void UpdateWhenActive()
        {
            if (closestVertex == -1 || secondClosestVertex == -1) return;

            LinkedInteractionProvider.SetLineRendererPositions(
                new Vector3[] { InteractionPositionWithMirrorLineSnap, closestVertexPosition, secondClosestVertexPosition }
                , true);
        }

        public override void OnUseDown()
        {
            int interactedVertex = SelectVertex();

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
                    LinkedInteractionProvider.AddPointFacingTriangle(closestVertex, secondClosestVertex, interactedVertex, HeadPosition, true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
            else
            {
                if (closestVertex == -1 || secondClosestVertex == -1) return;

                //Add new vertex
                LinkedInteractionProvider.AddVertex(InteractionPositionWithMirrorLineSnap, new int[] { closestVertex, secondClosestVertex }, true);

                DeselectClosestVertex();
                DeselectSecondClosestVertex();
            }

            LinkedInteractionProvider.ShowLineRenderer = (closestVertex >= 0 && secondClosestVertex >= 0);
        }

        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;
            closestVertexPosition = GetLocalVertexPositionFromIndex(vertex);
            LinkedInteractionProvider.SetVertexSelectState(closestVertex, VertexSelectStates.Selected);
        }

        void DeselectClosestVertex()
        {
            if (closestVertex < 0) return;
            LinkedInteractionProvider.SetVertexSelectState(closestVertex, VertexSelectStates.Normal);
            closestVertex = -1;
        }

        void SelectSecondClosesVertex(int vertex)
        {
            secondClosestVertex = vertex;
            secondClosestVertexPosition = GetLocalVertexPositionFromIndex(vertex);
            LinkedInteractionProvider.SetVertexSelectState(secondClosestVertex, VertexSelectStates.Selected);
        }

        void DeselectSecondClosestVertex()
        {
            if (secondClosestVertex < 0) return;
            LinkedInteractionProvider.SetVertexSelectState(secondClosestVertex, VertexSelectStates.Normal);
            secondClosestVertex = -1;
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