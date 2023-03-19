using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
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
        public void MoveVertexToPosition(int vertex, Vector3 position, bool updateMesh)
        {
            linkedMeshEditor.MoveVertexToPositionInteraction(vertex, position, updateMesh);
        }

        public void RemoveVertex(int vertex)
        {
            linkedMeshEditor.RemoveVertexInteraction(vertex);
        }

        public void MergeVertices(int keep, int discard, bool updateMesh)
        {
            linkedMeshEditor.MergeVerticesInteraction(keep, discard, updateMesh);
        }

        public void AddVertex(Vector3 position, int[] connectedVertices, bool updateMesh)
        {
            linkedMeshEditor.AddVertexInteraction(position, connectedVertices, updateMesh);
        }

        public void AddPointFacingTriangle(int vertexA, int vertexB, int vertexC, Vector3 facingPosition, bool updateMesh)
        {
            linkedMeshEditor.AddPointFacingTriangleInteraction(vertexA, vertexB, vertexC, facingPosition, updateMesh);
        }

        public void RemoveTriangle(int vertexA, int vertexB, int vertexC, bool updateMesh)
        {
            linkedMeshEditor.RemoveTriangleInteraction(vertexA, vertexB, vertexC, updateMesh);
        }

        public void UpdateMeshFromData()
        {
            linkedMeshEditor.UpdateMeshFromDataInteraction();
        }
    }
}