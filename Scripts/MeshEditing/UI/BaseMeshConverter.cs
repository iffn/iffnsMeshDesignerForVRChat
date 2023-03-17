using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public abstract class BaseMeshConverter : UdonSharpBehaviour
    {
        public abstract string Title { get; }
        public abstract void InputFieldUpdated(string text);

        public abstract bool ImportMeshIfValidAndSaveData(string text);

        public abstract Vector3[] VerticesFromLastImport { get; }

        public abstract int[] TrianglesFromLastImport { get; }

        public abstract string ExportMesh(Vector3[] vertices, int[] triangles);
    }
}