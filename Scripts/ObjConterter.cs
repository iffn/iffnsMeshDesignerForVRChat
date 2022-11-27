
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class ObjConterter : UdonSharpBehaviour
{
    [SerializeField] InputField LinkedInputField;
    
    MeshBuilder LinkedMeshBuilder;

    readonly char newLine = '\n';

    public void Setup(MeshBuilder linkedMeshBuilder)
    {
        this.LinkedMeshBuilder = linkedMeshBuilder;
    }

    private void Start()
    {
        //Use Setup instead
    }

    public void ImportObj()
    {
        Debug.Log("Import with limit set to " + LinkedInputField.characterLimit);

        SetMeshFromObjString(LinkedMeshBuilder.SharedMesh, LinkedInputField.text);

        LinkedMeshBuilder.UpdateMeshInfoFromMesh();

        LinkedMeshBuilder.SetInteractorsFromMesh();
    }

    public void ExportObj()
    {
        Debug.Log("Export");
        LinkedInputField.text = GetObjStringFromMesh(LinkedMeshBuilder.SharedMesh);
    }

    string GetObjStringFromMesh(Mesh mesh)
    {
        string returnString = "";

        returnString += $"o New mesh{newLine}";

        foreach (Vector3 vertex in mesh.vertices)
        {
            string x = vertex.x.ToString("0.00000");
            string y = vertex.y.ToString("0.00000");
            string z = vertex.z.ToString("0.00000");

            returnString += $"v {x} {y} {z}{newLine}";
        }

        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            returnString += $"f {triangles[i] + 1} {triangles[i + 1] + 1} {triangles[i + 2] + 1}{newLine}";
        }

        return returnString;
    }

    void SetMeshFromObjString(Mesh mesh, string objString)
    {
        string[] lines = objString.Split(newLine);

        int vertexCount = 0;
        int triangleCount = 0;

        foreach (string line in lines)
        {
            if (line.StartsWith("v "))
            {
                vertexCount++;
                continue;
            }
            if (line.StartsWith("f "))
            {
                triangleCount++;
                continue;
            }
        }

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount * 3];

        int vertexIndex = 0;
        int triangleIndex = 0;

        foreach (string line in lines)
        {
            if (line.StartsWith("v "))
            {
                string[] components = line.Substring(2).Split(' ');

                if (components.Length != 3)
                {
                    Debug.LogWarning($"Error: {line} could not be converted to a vertex position");

                    return;
                }

                vertices[vertexIndex].x = float.Parse(components[0]);
                vertices[vertexIndex].y = float.Parse(components[1]);
                vertices[vertexIndex].z = float.Parse(components[2]);

                vertexIndex++;

                continue;
            }
            if (line.StartsWith("f "))
            {
                string[] components = line.Substring(2).Split(' ');

                if (components.Length != 3)
                {
                    Debug.LogWarning($"Error: {line} could not be converted to a triangle");
                    return;
                }

                for(int i = 0; i<3; i++)
                {
                    if (components[i].Contains("/"))
                    {
                        components[i] = components[i].Substring(0, components[i].IndexOf("/"));
                    }
                }

                triangles[triangleIndex] = int.Parse(components[0]) - 1;
                triangles[triangleIndex + 1] = int.Parse(components[1]) - 1;
                triangles[triangleIndex + 2] = int.Parse(components[2]) - 1;

                triangleIndex += 3;

                continue;
            }
        }

        if(vertices.Length <= 3
            || triangles.Length <= 3)
        {
            return;
        }

        if(vertices.Length > mesh.vertices.Length)
        {
            //More vertices -> first set vertices
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }
        else
        {
            //Less vertices -> first set triangles
            mesh.triangles = triangles;
            mesh.vertices = vertices;
        }

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }
}
