using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MeshEditor : UdonSharpBehaviour
    {
        [Header("Unity assingments")]
        [SerializeField] Transform HelperTransform;
        [SerializeField] VertexIndicator VertexIndicatorTemplate;

        //Runtime variables
        MeshController linkedMeshController;
        ToolController linkedToolController;

        Vector3[] vertices = new Vector3[0];
        int[] triangles = new int[0];

        VertexIndicator[] vertexIndicators = new VertexIndicator[100];
        public VertexIndicator[] VertexIndicatorsForResetting
        {
            get
            {
                return vertexIndicators;
            }
        }

        bool inEditMode;
        public bool InEditMode
        {
            get
            {
                return inEditMode;
            }
            set
            {
                inEditMode = value;

                if (value)
                {
                    UpdateFromMesh();

                    UpdateVertexIndicatorsInEditMode();
                }
                else
                {
                    foreach (VertexIndicator indicator in vertexIndicators)
                    {
                        indicator.gameObject.SetActive(false);
                    }
                }
            }
        }

        public string MultiLineDebugState()
        {
            string returnString = $"Debug of {nameof(MeshEditor)} at {Time.time}:\n";

            returnString += $"• {nameof(inEditMode)}: {inEditMode}\n";
            returnString += $"• {nameof(vertices)}.length: {vertices.Length}\n";
            returnString += $"• {nameof(triangles)}.length: {triangles.Length}\n";

            return returnString;
        }

        public void Setup(MeshController linkedMeshController, ToolController linkedToolController, Transform meshTransform)
        {
            this.linkedMeshController = linkedMeshController;
            this.linkedToolController = linkedToolController;

            for(int i = 0; i < vertexIndicators.Length; i++)
            {
                GameObject newObject = GameObject.Instantiate(VertexIndicatorTemplate.gameObject);

                newObject.SetActive(false);

                VertexIndicator indicator = newObject.transform.GetComponent<VertexIndicator>();

                indicator.Setup(i, meshTransform, linkedToolController.VertexInteractionDistance);

                vertexIndicators[i] = indicator;
            }
        }

        public void Setup(MeshController linkedMeshController, ToolController linkedToolController, Transform meshTransform, VertexIndicator[] vertexIndicators)
        {
            this.linkedMeshController = linkedMeshController;
            this.linkedToolController = linkedToolController;

            this.vertexIndicators = vertexIndicators;

            foreach(VertexIndicator indicator in vertexIndicators)
            {
                indicator.transform.parent = meshTransform;
            }
        }

        Vector3 LocalTriangleFacingPointWhenGenrating
        {
            get
            {
                return linkedToolController.LocalHeadPosition;
            }
        }

        public void UpdateFromMesh()
        {
            if (!inEditMode) return;

            vertices = linkedMeshController.Vertices;
            triangles = linkedMeshController.Triangles;

            UpdateVertexIndicatorsInEditMode();
        }

        void UpdateVertexIndicatorsInEditMode()
        {
            if(vertices.Length >= vertexIndicators.Length)
            {
                for(int i = 0; i<vertexIndicators.Length; i++)
                {
                    vertexIndicators[i].transform.localPosition = vertices[i];
                    vertexIndicators[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertexIndicators[i].transform.localPosition = vertices[i];
                    vertexIndicators[i].gameObject.SetActive(true);
                }

                for (int i = vertices.Length; i < vertexIndicators.Length; i++)
                {
                    vertexIndicators[i].gameObject.SetActive(false);
                }
            }
        }

        //Settings
        public float VertexIndicatorRadius
        {
            set
            {
                foreach(VertexIndicator indicator in vertexIndicators)
                {
                    indicator.Radius = value;
                }
            }
        }

        #region Interaction provider
        //Access
        public int GetClosestVertexInRadius(Vector3 position, float radius)
        {
            int closestVertex = -1;
            float closestDistance = radius;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 currentPosition = vertices[i];
                float distance = (currentPosition - position).magnitude;

                if (distance > closestDistance) continue;

                closestDistance = distance;
                closestVertex = i;
            }

            return closestVertex;
        }

        public int GetClosestVectorInCylinder(Vector3 origin, Quaternion heading, float radius, float maxDistance)
        {
            HelperTransform.parent = transform;
            HelperTransform.localPosition = origin;
            HelperTransform.localRotation = heading;

            float closestDistance = maxDistance;
            int closestIndex = -1;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 currentPosition = transform.TransformPoint(vertices[i]);

                Vector3 relativePosition = HelperTransform.InverseTransformPoint(currentPosition);

                float distance = relativePosition.z;

                if (distance > closestDistance) continue;

                relativePosition.z = 0;

                if (relativePosition.magnitude > radius) continue;

                closestIndex = i;
                closestDistance = distance;
            }

            return closestIndex;
        }

        public Vector3 GetLocalVertexPositionFromIndex(int index)
        {
            if(index < 0 || index >= vertices.Length) return Vector3.zero;

            return vertices[index];
        }

        public int[] GetClosestVertices(Vector3 position, int count)
        {

            if(count <= 0) return new int[0];

            int[] returnValue;
            float[] distances;

            if (count < vertices.Length)
            {
                returnValue = new int[count];
                distances = new float[count];
            }
            else
            {
                count = vertices.Length;

                returnValue = new int[vertices.Length];
                distances = new float[vertices.Length];
            }

            for(int i = 0; i < count; i++)
            {
                distances[i] = Mathf.Infinity;
            }

            int lastIndex = returnValue.Length - 1;

            for(int i = 0; i < vertices.Length; i++)
            {
                float distance = (position - vertices[i]).sqrMagnitude;

                if (distance > distances[lastIndex]) continue;

                returnValue[lastIndex] = i;
                distances[lastIndex] = distance;

                for (int j = returnValue.Length - 1; j > 0; j--)
                {
                    ///*if (i == 0 && j == returnValue.Length - 1)*/ Debug.Log($"{distances[j - 1]} < {distances[j]} ");
                    if (distances[j - 1] < distances[j]) break;

                    int saveIndex = returnValue[j];
                    float saveDistance = distances[j];

                    returnValue[j] = returnValue[j - 1];
                    distances[j] = distances[j - 1];

                    returnValue[j - 1] = saveIndex;
                    distances[j - 1] = saveDistance;
                }
            }

            return returnValue;
        }

        public int[] GetConnectedVertices(int index)
        {
            //Create connected array
            bool[] connected = new bool[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                if (a == index || b == index || c == index)
                {
                    connected[a] = true;
                    connected[b] = true;
                    connected[c] = true;
                }
            }

            //Count connected vertices
            int count = 0;

            for (int i = 0; i < connected.Length; i++)
            {
                if (connected[i]) count++;
            }

            //Create array
            int[] returnValues = new int[count - 1];

            //Fill array with connected vertices
            int counter = 0;

            for (int i = 0; i < connected.Length; i++)
            {
                if (i == index) continue;

                if (connected[i]) returnValues[counter++] = i;
            }

            //Return value
            return returnValues;
        }

        //View
        public void SetVertexSelectStatesInteraction(int index, VertexSelectStates state)
        {
            if (index >= vertexIndicators.Length) return;

            vertexIndicators[index].SelectState = state;
        }

        public void ResetInteractorStatesInteraction()
        {
            foreach (VertexIndicator interactor in vertexIndicators)
            {
                interactor.SelectState = VertexSelectStates.Normal;
            }
        }

        //Edit
        public void MoveVertexToPositionInteraction(int vertex, Vector3 position, bool updateMesh)
        {
            vertices[vertex] = position;

            //if(vertex <= vertexIndicators.Length) vertexIndicators[vertex].transform.localPosition = position;

            if (updateMesh) UpdateMeshFromDataInteraction();
        }

        public void MergeVerticesInteraction(int keep, int discard, bool updateMesh)
        {
            MergeVertices(keep, discard, true);

            if (updateMesh) UpdateMeshFromDataInteraction();
        }

        public void RemoveVertexInteraction(int vertex)
        {
            RemoveVertexWithoutCleaning(vertex, true);

            RemoveUnconnectedVertices();
        }

        public void AddVertexInteraction(Vector3 position, int[] connectedVertices, bool updateMesh)
        {
            if(connectedVertices == null || connectedVertices.Length < 2) return;

            Vector3[] oldVertexPositions = vertices;
            vertices = new Vector3[vertices.Length + 1];

            for (int i = 0; i < oldVertexPositions.Length; i++)
            {
                vertices[i] = oldVertexPositions[i];
            }

            vertices[vertices.Length - 1] = position;

            Vector3 localFacingPoint = LocalTriangleFacingPointWhenGenrating;

            for(int i = 0; i<connectedVertices.Length - 1; i++)
            {
                AddPointFacingTriangle(vertices.Length - 1, connectedVertices[i], connectedVertices[i + 1], localFacingPoint);
            }

            if (updateMesh) UpdateMeshFromDataInteraction();
        }

        public void AddPointFacingTriangleInteraction(int vertexA, int vertexB, int vertexC, Vector3 facingPosition, bool updateMesh)
        {
             AddPointFacingTriangle(vertexA, vertexB, vertexC, facingPosition);

            if (updateMesh) UpdateMeshFromDataInteraction();
        }

        public void RemoveTriangleInteraction(int vertexA, int vertexB, int vertexC, bool updateMesh)
        {
            TryRemoveTriangle(vertexA, vertexB, vertexC);

            if (updateMesh) UpdateMeshFromDataInteraction();
        }

        public void UpdateMeshFromDataInteraction()
        {
            linkedMeshController.SetData(vertices, triangles, this);

            UpdateVertexIndicatorsInEditMode();
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

            UpdateMeshFromDataInteraction();
        }
        #endregion

        #region Mesh editing
        public void AddPointFacingTriangle(int a, int b, int c, Vector3 localFacingPosition)
        {
            Debug.Log($"Adding triagnle {a}, {b}, {c}");

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

            foreach (int index in triangles)
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

        void MergeVertices(int keep, int discard, bool removeInvalid)
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
        #endregion
    }
}