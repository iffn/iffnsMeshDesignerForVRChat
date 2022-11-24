# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MakeMeshUnique : EditorWindow
{
    [MenuItem("Tools/iffnsStuff/MeshBuilder/MakeMeshUnique")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MakeMeshUnique));
    }

    MeshBuilder currentBuilder;

    void OnGUI()
    {
        currentBuilder = EditorGUILayout.ObjectField(
           obj: currentBuilder,
           objType: typeof(MeshBuilder),
           true) as MeshBuilder;

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
        MeshFilter currentMeshFilter = currentBuilder.transform.GetComponent<MeshFilter>();

        Mesh currentMesh = currentMeshFilter.sharedMesh;

        Mesh newMesh = Instantiate(currentMesh);

        currentMeshFilter.sharedMesh = currentMesh;

        if(currentBuilder.SymmetryMeshFilter != null)
        {
            currentBuilder.SymmetryMeshFilter.sharedMesh = newMesh;
        }
    }
}

#endif