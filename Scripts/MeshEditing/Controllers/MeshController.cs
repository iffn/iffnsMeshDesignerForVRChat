//#define debugLog

using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    //[RequireComponent(typeof(MeshFilter))]
    public class MeshController : UdonSharpBehaviour
    {
        MeshEditor linkedMeshEditor;
        MeshSyncController linkedSyncController;
        Mesh linkedMesh;

        bool setupCalled = false;
        float lastUpdateTime = Mathf.NegativeInfinity;

        public float LastUpdateTime
        {
            get
            {
                return lastUpdateTime;
            }
        }

        public string DebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(MeshController)} at {Time.time}:\n";
            returnString += $"{nameof(lastUpdateTime)}: {lastUpdateTime}\n";
            returnString += $"Vertices: {linkedMesh.vertices.Length}\n";
            returnString += $"Triangles: {linkedMesh.triangles.Length}\n";

            return returnString;
        }

        private void Update()
        {
            if (!setupCalled) return;
            lastUpdateTime = Time.time;
        }

        public void Setup(MeshEditor linkedMeshEditor, MeshSyncController linkedSyncController, Mesh linkedMesh)
        {
            this.linkedMeshEditor= linkedMeshEditor;
            this.linkedSyncController= linkedSyncController;

            lastUpdateTime = Time.time;

            this.linkedMesh = linkedMesh;
            setupCalled = true;
        }

        public Vector3[] Vertices
        {
            get
            {
                return linkedMesh.vertices;
            }
        }

        public int[] Triangles
        {
            get
            {
                return linkedMesh.triangles;
            }
        }

        public void SetData(Vector3[] vertices, int[] triangles, UdonSharpBehaviour sender)
        {
            //Build mesh from data
            linkedMesh.triangles = new int[0];

            linkedMesh.vertices = vertices;
            linkedMesh.triangles = triangles;

            linkedMesh.RecalculateNormals();
            linkedMesh.RecalculateTangents();
            linkedMesh.RecalculateBounds();

            //Inform listeners
            if (sender != linkedMeshEditor) linkedMeshEditor.UpdateFromMesh();
            if (sender != linkedSyncController) linkedSyncController.Sync();
        }
    }
}