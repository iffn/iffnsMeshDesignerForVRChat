using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MeshInteractionProvider : UdonSharpBehaviour
    {
        //ToDo: Rename provider to interface
        //Note: This should be an interface of MeshEditor, but U# does not support interfaces yet

        //Inspector variables
        [Header("Unity assingments")]
        [SerializeField] MeshEditor LinkedMeshEditor;
        [SerializeField] LineRenderer LinkedLineRenderer;

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
            LinkedMeshEditor.SetVertexSelectStates(index, state);
        }

        public void ResetInteractorStates()
        {
            LinkedMeshEditor.ResetInteractorStates();
        }

        //Edit
        public void MoveVertexToPosition(int vertex, Vector3 position, bool updateMesh)
        {
            LinkedMeshEditor.MoveVertexToPosition(vertex, position, updateMesh);
        }

        public void RemoveVertex(int vertex)
        {
            LinkedMeshEditor.RemoveVertex(vertex);
        }

        public void MergeVertices(int keep, int discard, bool updateMesh)
        {
            LinkedMeshEditor.MergeVertices(keep, discard, updateMesh);
        }

        public void AddVertex(Vector3 position, int[] connectedVertices, bool updateMesh)
        {
            LinkedMeshEditor.AddVertex(position, connectedVertices, updateMesh);
        }

        public void AddPointFacingTriangle(int vertexA, int vertexB, int vertexC, Vector3 facingPosition, bool updateMesh)
        {
            LinkedMeshEditor.AddPointFacingTriangle(vertexA, vertexB, vertexC, facingPosition, updateMesh);
        }

        public void RemoveTriangle(int vertexA, int vertexB, int vertexC, bool updateMesh)
        {
            LinkedMeshEditor.RemoveTriangle(vertexA, vertexB, vertexC, updateMesh);
        }

        public void UpdateMeshFromData()
        {
            LinkedMeshEditor.UpdateMeshFromData();
        }
    }
}