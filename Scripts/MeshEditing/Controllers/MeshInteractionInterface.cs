using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class MeshInteractionInterface : UdonSharpBehaviour
    {
        //Note: This should be an interface of MeshEditor, but U# does not support interfaces yet

        //Inspector variables
        [Header("Unity assingments")]
        [SerializeField] LineRenderer LinkedLineRenderer;
        
        MeshEditor linkedMeshEditor;

        public void Setup(MeshEditor linkedMeshEditor)
        {
            this.linkedMeshEditor = linkedMeshEditor;
        }

        //View
        public bool ShowLineRenderer
        {
            set
            {
                LinkedLineRenderer.gameObject.SetActive(value: value);
            }
        }

        public void SetLineRendererPositions(Vector3[] positions, bool loop)
        {
            LinkedLineRenderer.loop = loop;

            LinkedLineRenderer.positionCount = positions.Length;

            LinkedLineRenderer.SetPositions(positions);
        }

        public void SetVertexSelectState(int index, VertexSelectStates state)
        {
            linkedMeshEditor.SetVertexSelectStatesInteraction(index, state);
        }

        public void ResetInteractorStates()
        {
            linkedMeshEditor.ResetInteractorStatesInteraction();
        }

        //Edit
        public void MoveVertexToPosition(int vertex, Vector3 position, bool applyData)
        {
            linkedMeshEditor.MoveVertexToPositionInteraction(vertex, position, applyData);
        }

        public void RemoveVertex(int vertex, bool applyData)
        {
            linkedMeshEditor.RemoveVertexInteraction(vertex, applyData);
        }

        public void MergeVertices(int keep, int discard, bool applyData)
        {
            linkedMeshEditor.MergeVerticesInteraction(keep, discard, applyData);
        }

        public void AddVertex(Vector3 position, int[] connectedVertices, bool applyData)
        {
            linkedMeshEditor.AddVertexInteraction(position, connectedVertices, applyData);
        }

        public void AddPointFacingTriangle(int vertexA, int vertexB, int vertexC, Vector3 facingPosition, bool applyData)
        {
            linkedMeshEditor.AddPointFacingTriangleInteraction(vertexA, vertexB, vertexC, facingPosition, applyData);
        }

        public void RemoveTriangle(int vertexA, int vertexB, int vertexC, bool applyData)
        {
            linkedMeshEditor.RemoveTriangleInteraction(vertexA, vertexB, vertexC, applyData);
        }

        public void ApplyMeshData()
        {
            linkedMeshEditor.ApplyMeshDataInteraction();
        }
    }
}