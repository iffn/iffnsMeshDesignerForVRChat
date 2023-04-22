
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class ObjConterter : BaseMeshConverter
    {
        Vector3[] verticesFromLastImport = new Vector3[0];
        int[] trianglesFromLastImport = new int[0];

        //Inherited functions


        public override Vector3[] VerticesFromLastImport
        {
            get
            {
                return verticesFromLastImport;
            }
        }

        public override int[] TrianglesFromLastImport
        {
            get
            {
                return trianglesFromLastImport;
            }
        }

        public override string Title
        {
            get
            {
                return ".obj";
            }
        }

        public override string ExportMesh(Vector3[] vertices, int[] triangles)
        {
            string returnString = "";

            returnString += $"o New mesh\n";

            foreach (Vector3 vertex in vertices)
            {
                string x = vertex.x.ToString("0.00000");
                string y = vertex.y.ToString("0.00000");
                string z = vertex.z.ToString("0.00000");

                returnString += $"v {x} {y} {z}\n";
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                returnString += $"f {triangles[i] + 1} {triangles[i + 1] + 1} {triangles[i + 2] + 1}\n";
            }

            return returnString;
        }

        public override bool ImportMeshIfValidAndSaveData(string text)
        {
            string[] lines = text.Split('\n');

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

            verticesFromLastImport = new Vector3[vertexCount];
            trianglesFromLastImport = new int[triangleCount * 3];

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

                        verticesFromLastImport = new Vector3[0];
                        trianglesFromLastImport = new int[0];

                        return false;
                    }

                    verticesFromLastImport[vertexIndex].x = float.Parse(components[0]);
                    verticesFromLastImport[vertexIndex].y = float.Parse(components[1]);
                    verticesFromLastImport[vertexIndex].z = float.Parse(components[2]);

                    vertexIndex++;

                    continue;
                }
                if (line.StartsWith("f "))
                {
                    string[] components = line.Substring(2).Split(' ');

                    if (components.Length != 3)
                    {
                        Debug.LogWarning($"Error: {line} could not be converted to a triangle");

                        verticesFromLastImport = new Vector3[0];
                        trianglesFromLastImport = new int[0];

                        return false;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        if (components[i].Contains("/"))
                        {
                            components[i] = components[i].Substring(0, components[i].IndexOf("/"));
                        }
                    }

                    trianglesFromLastImport[triangleIndex] = int.Parse(components[0]) - 1;
                    trianglesFromLastImport[triangleIndex + 1] = int.Parse(components[1]) - 1;
                    trianglesFromLastImport[triangleIndex + 2] = int.Parse(components[2]) - 1;

                    triangleIndex += 3;

                    continue;
                }
            }

            if (verticesFromLastImport.Length <= 3
                || trianglesFromLastImport.Length <= 3)
            {
                verticesFromLastImport = new Vector3[0];
                trianglesFromLastImport = new int[0];
                return false;
            }

            return true;
        }

        public override void InputFieldUpdated(string text)
        {
            
        }
    }
}