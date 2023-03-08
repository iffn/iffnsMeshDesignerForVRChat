# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MakeMeshUnique : EditorWindow
    {
        [MenuItem("Tools/iffnsStuff/MeshBuilder/MakeMeshUnique")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MakeMeshUnique));
        }

        MeshInteractor currentBuilder;

        void OnGUI()
        {
            currentBuilder = EditorGUILayout.ObjectField(
               obj: currentBuilder,
               objType: typeof(MeshInteractor),
               true) as MeshInteractor;

            if (currentBuilder != null)
            {
                if (GUILayout.Button("Make current mesh unique"))
                {
                    MakeCurrentMeshUnique();
                }
            }
        }

        void MakeCurrentMeshUnique()
        {
            MeshFilter currentMeshFilter = currentBuilder.LinkedMeshController.MeshFilter;

            Mesh currentMesh = currentMeshFilter.sharedMesh;

            Mesh newMesh = new Mesh
            {
                vertices = currentMesh.vertices,
                triangles = currentMesh.triangles,
                uv = currentMesh.uv,
                uv2 = currentMesh.uv2,
                uv3 = currentMesh.uv3,
                uv4 = currentMesh.uv4,
                uv5 = currentMesh.uv5,
                uv6 = currentMesh.uv6,
                uv7 = currentMesh.uv7,
                uv8 = currentMesh.uv8,
                normals = currentMesh.normals,
                tangents = currentMesh.tangents,
                bounds = currentMesh.bounds
            };

            currentMeshFilter.mesh = null;
            currentMeshFilter.mesh = newMesh;

            if (currentBuilder.SymmetryMeshFilter != null)
            {
                currentBuilder.SymmetryMeshFilter.mesh = newMesh;
            }
        }
    }
}

#endif