//#define debugLog

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    //[RequireComponent(typeof(MeshFilter))]
    public class MeshController : UdonSharpBehaviour
    {
        [Header("Debug info")]
        Vector3[] vertices = new Vector3[0];
        int[] triangles = new int[0];

        bool setupCalled = false;
        float lastUpdateTime = Mathf.NegativeInfinity;

        public float LastUpdateTime
        {
            get
            {
                return lastUpdateTime;
            }
        }

        MeshFilter linkedMeshFilter;

        public string DebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(MeshController)} at {Time.time}:\n";
            returnString += $"{nameof(lastUpdateTime)}: {lastUpdateTime}\n";
            returnString += $"Vertices: {vertices.Length}\n";
            returnString += $"Triangles: {triangles.Length}\n";

            return returnString;
        }

        private void Update()
        {
            if (!setupCalled) return;
            lastUpdateTime = Time.time;
        }

        public void Setup()
        {
            setupCalled = true;
            lastUpdateTime = Time.time;

            linkedMeshFilter = transform.GetComponent<MeshFilter>();

            UpdateMeshInfoFromMesh();
        }

        public Vector3[] Vertices
        {
            get
            {
                return vertices;
            }
        }

        public int[] Triangles
        {
            get
            {
                return triangles;
            }
        }

        public MeshFilter MeshFilter
        {
            get
            {
                return linkedMeshFilter;
            }
        }

        public Mesh SharedMesh
        {
            get
            {
                return linkedMeshFilter.sharedMesh;
            }
        }

        public int[] GetConnectedVertices(int index)
        {
            //Create connected array
            bool[] connected = new bool[vertices.Length];

            for(int i = 0; i<triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                if(a == index || b == index || c == index)
                {
                    connected[a] = true;
                    connected[b] = true;
                    connected[c] = true;
                }
            }

            //Count connected vertices
            int count = 0;

            for(int i = 0; i<connected.Length; i++)
            {
                if (connected[i]) count++;
            }

            //Create array
            int[] returnValues = new int[count - 1];

            //Fill array with connected vertices
            int counter = 0;

            for(int i = 0; i< connected.Length; i++)
            {
                if (i == index) continue;

                if (connected[i]) returnValues[counter++] = i;
            }

            //Return value
            return returnValues;
        }

        void BuildMeshFromData(Vector3[] positions, int[] triangles)
        {
            this.triangles = triangles;
            this.vertices = positions;

            BuildMeshFromData();
        }

        public void BuildMeshFromData()
        {
            Mesh mesh = linkedMeshFilter.sharedMesh;

            mesh.triangles = new int[0];

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        public void UpdateMeshInfoFromMesh()
        {
            Mesh mesh = linkedMeshFilter.sharedMesh;

            vertices = mesh.vertices;
            triangles = mesh.triangles;
        }

        public void SetSingleVertexPosition(int index, Vector3 localPosition)
        {
            Vector3[] positions = vertices;

            positions[index] = localPosition;

            vertices = positions;
        }

        public void AddVertex(Vector3 localPosition, int connectedIndex1, int connectedIndex2, Vector3 localFacingPosition)
        {
            Vector3[] oldVertexPositions = vertices;
            vertices = new Vector3[vertices.Length + 1];

            for (int i = 0; i < oldVertexPositions.Length; i++)
            {
                vertices[i] = oldVertexPositions[i];
            }

            vertices[vertices.Length - 1] = localPosition;

            AddPlayerFacingTriangle(vertices.Length - 1, connectedIndex1, connectedIndex2, localFacingPosition);
        }

        public void AddQuad(int basePoint, int firstSidePoint, int secondSidePoint, Vector3 additionPoint, Vector3 localFacingPosition)
        {
            AddVertex(additionPoint, basePoint, firstSidePoint, localFacingPosition);
            AddPlayerFacingTriangle(basePoint, secondSidePoint, vertices.Length - 1, localFacingPosition);
        }

        public void AddPlayerFacingTriangle(int a, int b, int c, Vector3 localFacingPosition)
        {
            Vector3 vecA = vertices[a];
            Vector3 vecB = vertices[b];
            Vector3 vecC = vertices[c];

            Vector3 normal = Vector3.Cross(vecA - vecB, vecA - vecC);

            float direction = Vector3.Dot(normal, 0.333333f * (vecA + vecB + vecC) - localFacingPosition);

            int[] oldTriangles = triangles;

            triangles = new int[oldTriangles.Length + 3];

            for (int i = 0; i < oldTriangles.Length; i++)
            {
                triangles[i] = oldTriangles[i];
            }

            if (direction < 0)
            {
                triangles[triangles.Length - 3] = a;
                triangles[triangles.Length - 2] = b;
                triangles[triangles.Length - 1] = c;
            }
            else
            {
                triangles[triangles.Length - 3] = a;
                triangles[triangles.Length - 2] = c;
                triangles[triangles.Length - 1] = b;
            }
        }

        public void TryRemoveTriangle(int a, int b, int c)
        {
            if (a == b || a == c || b == c)
            {
                Debug.LogWarning("Error: double triangle found");
                return;
            }

            if (triangles.Length < 3)
            {
                Debug.LogWarning("Error: traingle length somehow 0");
                return;
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int ta = triangles[i];
                int tb = triangles[i + 1];
                int tc = triangles[i + 2];

                bool found = (ta == a || tb == a || tc == a) &&
                             (ta == b || tb == b || tc == b) &&
                             (ta == c || tb == c || tc == c);

                if (!found) continue;

                #if debugLog
                Debug.Log($"Found triangle {ta},{tb},{tc}");
                #endif

                int[] oldTriangles = triangles;

                triangles = new int[oldTriangles.Length - 3];

                int indexAddition = 0;

                for (int j = 0; j < triangles.Length; j += 3)
                {
                    if (j == i)
                    {
                        indexAddition += 3;
                    }

                    triangles[j] = oldTriangles[j + indexAddition];
                    triangles[j + 1] = oldTriangles[j + indexAddition + 1];
                    triangles[j + 2] = oldTriangles[j + indexAddition + 2];
                }

                RemoveUnconnectedVertices();

                return;
            }

            Debug.LogWarning($"Error: triangle {a}, {b}, {c} not found");
        }

        [RecursiveMethod]
        void RemoveUnconnectedVertices()
        {
            bool[] vertexUsed = new bool[vertices.Length];

            if (vertexUsed[0] == true)
            {
                //Should never be called

                Debug.LogWarning("Unlike normal C#, U# apparently sets the default boolean value to true");

                for (int i = 0; i < vertexUsed.Length; i++)
                {
                    vertexUsed[i] = false;
                }
            }

            foreach(int index in triangles)
            {
                vertexUsed[index] = true;
            }

            for (int i = 0; i < vertexUsed.Length; i++)
            {
                if (!vertexUsed[i])
                {
                    RemoveVertexWithoutCleaning(i, false);
                    //Recursion needed since index changes after removing vertex i
                    RemoveUnconnectedVertices();

                    return;
                }
            }
        }

        void RemoveVertexWithoutCleaning(int index, bool removeAffectedTriangles)
        {
            Vector3[] oldVertexPositons = vertices;
            Vector3[] newVertexPositions = new Vector3[oldVertexPositons.Length - 1];

            int newIndex = 0;

            for (int i = 0; i < oldVertexPositons.Length; i++)
            {
                if (i == index) continue;

                newVertexPositions[newIndex] = oldVertexPositons[i];

                newIndex++;
            }

            vertices = newVertexPositions;

            //Remove affected triangles
            if (removeAffectedTriangles)
            {
                int trianglesToBeRemoved = 0;

                foreach (int triangle in this.triangles)
                {
                    if (triangle == index) trianglesToBeRemoved++;
                }

                int[] oldTriangles = this.triangles;
                triangles = new int[oldTriangles.Length - trianglesToBeRemoved * 3];

                int offset = 0;

                for (int i = 0; i < oldTriangles.Length; i += 3)
                {
                    int a = oldTriangles[i];
                    int b = oldTriangles[i + 1];
                    int c = oldTriangles[i + 2];

                    if (a != index && b != index && c != index)
                    {
                        if (a > index) a--;
                        if (b > index) b--;
                        if (c > index) c--;

                        triangles[i - offset] = a;
                        triangles[i + 1 - offset] = b;
                        triangles[i + 2 - offset] = c;
                    }
                    else
                    {
                        offset += 3;
                    }
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length; i++)
                {
                    if (triangles[i] > index)
                    {
                        triangles[i]--;
                    }
                }
            }
        }

        public void RemoveVertexClean(int index)
        {
            RemoveVertexWithoutCleaning(index, true);

            RemoveUnconnectedVertices();
        }

        public void MergeOverlappingVertices(float threshold)
        {
            int verticesMerged = 0;

            #if debugLog
            Debug.Log($"Checking {vertices.Length} for merging");
            #endif

            //return;

            for (int firstVertex = 0; firstVertex < vertices.Length - 1; firstVertex++)
            {
                Vector3 firstPosition = vertices[firstVertex];

                for (int secondVertex = firstVertex + 1; secondVertex < vertices.Length; secondVertex++)
                {
                    Vector3 secondPosition = vertices[secondVertex];
                    float distance = (firstPosition - secondPosition).magnitude;

                    if (distance < threshold)
                    {
                        //Debug.Log($"Merging vertex {firstVertex} with {secondVertex} at distance {distance}" );

                        MergeVertices(firstVertex, secondVertex, false);
                        secondVertex--;
                        verticesMerged++;
                    }
                }
            }

            Debug.Log($"{verticesMerged} vertices merged");

            BuildMeshFromData();
        }

        void RemoveInvalidTriagnles()
        {
            int trianglesToBeRemoved = 0;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                if (a == b || a == c || b == c)
                {
                    trianglesToBeRemoved++;
                }
            }

            int[] oldTriangles = triangles;
            triangles = new int[triangles.Length - trianglesToBeRemoved * 3];

            int offset = 0;

            for (int i = 0; i < oldTriangles.Length; i += 3)
            {
                int a = oldTriangles[i];
                int b = oldTriangles[i + 1];
                int c = oldTriangles[i + 2];

                if (a == b || a == c || b == c)
                {
                    offset += 3;
                }
                else
                {
                    triangles[i - offset] = a;
                    triangles[i + 1 - offset] = b;
                    triangles[i + 2 - offset] = c;
                }
            }
        }

        public void MergeVertices(int keep, int discard, bool removeInvalid)
        {
            //Remove vertex:
            Vector3[] oldVertexPositons = this.vertices;
            vertices = new Vector3[oldVertexPositons.Length - 1];

            int newIndex = 0;

            for (int i = 0; i < oldVertexPositons.Length; i++)
            {
                if (i == discard) continue;

                vertices[newIndex] = oldVertexPositons[i];

                newIndex++;
            }

            int trianglesToBeRemoved = 0;

            //Replace keep with discard and count invalid triangles:
            for (int i = 0; i < triangles.Length; i += 3)
            {
                #if debugLog
                Debug.Log($"Checking triangle = {i} with values {triangles[i]}, {triangles[i + 1]}, {triangles[i + 2]}");
                #endif

                int found = 0;

                if (triangles[i] == discard || triangles[i] == keep)
                {
                    triangles[i] = keep;

                    found++;
                }
                if (triangles[i + 1] == discard || triangles[i + 1] == keep)
                {
                    triangles[i + 1] = keep;
                    found++;
                }
                if (triangles[i + 2] == discard || triangles[i + 2] == keep)
                {
                    triangles[i + 2] = keep;
                    found++;
                }

                //When triangles are being destroyed
                if (found > 1)
                {
                    #if debugLog
                    Debug.Log($"Triaggle to be removed = {i} with values {triangles[i]}, {triangles[i+1]}, {triangles[i+2]}");
                    #endif

                    trianglesToBeRemoved += 1;
                }
            }

            //decrement index
            for (int i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] > discard) triangles[i]--;
            }

            Debug.Log("trianglesToBeRemoved = " + trianglesToBeRemoved);

            //Remove failed triangles
            if (trianglesToBeRemoved > 0)
            {
                int trianglesRemoved = 0;
                int trianglesSkipped = 0;

                int[] oldTriangles = triangles;
                triangles = new int[triangles.Length - trianglesToBeRemoved * 3];

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int a = oldTriangles[i];
                    int b = oldTriangles[i + 1];
                    int c = oldTriangles[i + 2];

                    if (a != b && b != c && c != a)
                    {
                        triangles[i - trianglesSkipped] = a; // Subtract from index instead of value???????
                        triangles[i + 1 - trianglesSkipped] = b;
                        triangles[i + 2 - trianglesSkipped] = c;
                    }
                    else
                    {
                        trianglesRemoved++;
                        trianglesSkipped += 3;
                    }
                }
            }

            if (removeInvalid)
            {
                RemoveInvalidTriagnles();
                RemoveUnconnectedVertices();
            }
        }
    }
}