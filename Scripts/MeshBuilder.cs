
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Encodings;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.ClientSim;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class MeshBuilder : UdonSharpBehaviour
{
    [SerializeField] VertexAdder LinkedVertexAdder;
    [SerializeField] LineRenderer LinkedLineRenderer;
    [SerializeField] float DesktopVertexSpeed = 0.2f;
    [SerializeField] Transform VertexPositionIndicatorPrefab;
    [SerializeField] VertexInteractor leftVertexPickup;
    [SerializeField] VertexInteractor rightVertexPickup;
    
    [SerializeField] MeshFilter SymmetryMeshFilter;


    int closestVertexIndex = 0;
    int secondClosestVertexIndex = 0;

    int currentLeftIndex = 0;
    int currentRightIndex = 0;

    Transform[] vertexIndicators = new Transform[0];

    bool isInVR;

    readonly char newLine = '\n';

    MeshFilter linkedMeshFilter;
    MeshRenderer linkedMeshRenderer;
    MeshRenderer symmetryMeshRenderer;

    public bool setupComplete = false;

    float vertexInteractorScale = 0.01f;
    public float VertexInteractorScale
    {
        get
        {
            return vertexInteractorScale;
        }
        set
        {
            vertexInteractorScale = value;

            Vector3 scale = value * Vector3.one;

            foreach(Transform vertex in vertexIndicators)
            {
                vertex.transform.localScale = scale;
            }
        }
    }

    bool inEditMode = true;
    public bool InEditMode
    {
        get
        {
            return inEditMode;
        }
        set
        {
            inEditMode = value;

            LinkedVertexAdder.gameObject.SetActive(value);
            LinkedVertexAdder.ForceDropIfHeld();

            if (value)
            {
                SetIndicatorsFromMesh();
            }
            else
            {
                ClearIndicators();
            }
        }
    }

    public bool SymmetryMode
    {
        set
        {
            if (!SymmetryMeshFilter) return;

            SymmetryMeshFilter.transform.gameObject.SetActive(value);
        }
    }

    public string ObjString
    {
        get
        {
            string returnString = "";

            returnString += $"o New mesh{newLine}";
            
            foreach(Vector3 vertex in linkedMeshFilter.sharedMesh.vertices)
            {
                string x = vertex.x.ToString("0.000");
                string y = vertex.z.ToString("0.000");
                string z = vertex.y.ToString("0.000");

                returnString += $"v {x} {y} {z}{newLine}";
            }

            int[] triangles = linkedMeshFilter.sharedMesh.triangles;

            for (int i = 0; i<triangles.Length; i+= 3)
            {
                returnString += $"f {triangles[i] + 1} {triangles[i+1] + 1} {triangles[i+2] + 1}{newLine}";
            }

            return returnString;
        }
        set
        {
            string[] lines = value.Split(newLine);

            int vertexCount = 0;
            int triangleCount = 0;

            foreach(string line in lines)
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
            int [] triangles = new int[triangleCount * 3];

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

                    string a = components[0].Substring(0, components[0].IndexOf("/"));
                    string b = components[1].Substring(0, components[1].IndexOf("/"));
                    string c = components[2].Substring(0, components[2].IndexOf("/"));

                    triangles[triangleIndex] = int.Parse(a) - 1;
                    triangles[triangleIndex + 1] = int.Parse(b) - 1;
                    triangles[triangleIndex + 2] = int.Parse(c) - 1;

                    triangleIndex += 3;

                    continue;
                }
            }

            SetupElementsAndMeshFromData(vertices, triangles);
        }
    }


    public Material AttachedMaterial
    {
        get
        {
            return linkedMeshRenderer.sharedMaterial;
        }
        set
        {
            linkedMeshRenderer.sharedMaterial = value;
            if (symmetryMeshRenderer) symmetryMeshRenderer.sharedMaterial = value;
        }
    }

    void ClearIndicators()
    {
        for(int i = 0; i<vertexIndicators.Length; i++)
        {
            GameObject.Destroy(vertexIndicators[i].gameObject);

            vertexIndicators[i] = null;
        }

        vertexIndicators = new Transform[0];
    }

    public Vector3[] verticesDebug;

    void BuildMeshFromData(Vector3[] positions, int[] triangles)
    {
        Mesh mesh = linkedMeshFilter.sharedMesh;

        mesh.triangles = new int[0];

        mesh.vertices = positions;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    void SetupElementsAndMeshFromData(Vector3[] positions, int[] triangles)
    {
        BuildMeshFromData(positions, triangles);

        if (inEditMode)
        {
            SetIndicatorsFromMesh();
        }
    }

    void SetSingleVertexPosition(int index, Vector3 localPosition)
    {
        Vector3[] positions = linkedMeshFilter.sharedMesh.vertices;

        positions[index] = localPosition;

        linkedMeshFilter.sharedMesh.vertices = positions;

        if (inEditMode)
        {
            vertexIndicators[index].transform.localPosition = localPosition;
        }
    }

    void SetIndicatorsFromMesh()
    {
        ClearIndicators();

        Vector3[] positions = linkedMeshFilter.sharedMesh.vertices;

        vertexIndicators = new Transform[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            Transform currentIndicator = GameObject.Instantiate(VertexPositionIndicatorPrefab.gameObject).GetComponent<Transform>();

            vertexIndicators[i] = currentIndicator;

            currentIndicator.parent = transform;

            currentIndicator.transform.localPosition = positions[i];
        }
    }

    public void TryToMergeVertex(VertexInteractor interactor)
    {
        Vector3[] vertexPositions = linkedMeshFilter.sharedMesh.vertices;

        int currentVertexIndex = (interactor.CurrentHand == VRC_Pickup.PickupHand.Left) ? currentLeftIndex : currentRightIndex;

        Vector3 heldVertexPosition = vertexPositions[currentVertexIndex];

        int closestVertex = -1;
        float closestDistanceSquare = Mathf.Infinity;

        for (int i = 0; i < vertexPositions.Length; i++)
        {
            if (i == currentVertexIndex) continue;

            float distance = (vertexPositions[i] - heldVertexPosition).sqrMagnitude;

            if (distance < closestDistanceSquare)
            {
                closestDistanceSquare = distance;
                closestVertex = i;
            }
        }

        if (closestVertex == -1) return;

        if (closestDistanceSquare < vertexInteractorScale * vertexInteractorScale)
        {
            MergeVertices(keep: currentVertexIndex, discard: closestVertex);

            SetIndicatorsFromMesh();

            interactor.ForceDropAndDeactivate();
        }
    }

    void SetupBasicMesh()
    {
        Vector3[] vertexPositions = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0)
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        SetupElementsAndMeshFromData(vertexPositions, triangles);
    }

    void Setup()
    {
        //Check setup:
        bool correctSetup = true;

        if (leftVertexPickup == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(leftVertexPickup)} not assinged");
        }
        if (rightVertexPickup == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(rightVertexPickup)} not assinged");
        }
        if (LinkedVertexAdder == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(LinkedVertexAdder)} not assinged");
        }
        if (LinkedLineRenderer == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(LinkedLineRenderer)} not assinged");
        }

        if (!correctSetup)
        {
            enabled = false;
            return;
        }

        //Setup:
        LinkedVertexAdder.Setup(this);

        isInVR = Networking.LocalPlayer.IsUserInVR();

        linkedMeshFilter = transform.GetComponent<MeshFilter>();
        linkedMeshRenderer = transform.GetComponent<MeshRenderer>();

        leftVertexPickup.Setup(this);
        rightVertexPickup.Setup(this);

        if (InEditMode)
        {
            SetIndicatorsFromMesh();
        }

        if(SymmetryMeshFilter) symmetryMeshRenderer = SymmetryMeshFilter.transform.GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    public void PickupVertexAdder()
    {
        LinkedLineRenderer.gameObject.SetActive(true);
    }

    public void DropVertexAdder()
    {
        LinkedLineRenderer.gameObject.SetActive(false);
    }

    public void UseVertexAdder()
    {
        //Vertex
        Transform[] oldVertexIndicators = vertexIndicators;
        Vector3[] oldVertexPositions = linkedMeshFilter.sharedMesh.vertices;

        vertexIndicators = new Transform[oldVertexPositions.Length + 1];
        Vector3[] vertexPositions = new Vector3[oldVertexPositions.Length + 1];

        for (int i = 0; i < oldVertexPositions.Length; i++)
        {
            vertexIndicators[i] = vertexIndicators[i];
            vertexPositions[i] = oldVertexPositions[i];
        }

        int newVertexIndex = vertexIndicators.Length - 1;

        Transform currentInteractor = GameObject.Instantiate(VertexPositionIndicatorPrefab.gameObject).GetComponent<Transform>();

        vertexIndicators[vertexIndicators.Length - 1] = currentInteractor;
        vertexPositions[vertexIndicators.Length - 1] = currentInteractor.transform.localPosition;

        //Triangles
        int[] oldTriangles = linkedMeshFilter.sharedMesh.triangles;

        int[] triangles = new int[oldTriangles.Length + 3];

        for (int i = 0; i < oldTriangles.Length; i++)
        {
            triangles[i] = oldTriangles[i];
        }

        Vector3 normal = Vector3.Cross(LinkedVertexAdder.transform.position - vertexIndicators[closestVertexIndex].transform.position, LinkedVertexAdder.transform.position - vertexIndicators[secondClosestVertexIndex].transform.position);

        float direction = Vector3.Dot(normal, LinkedVertexAdder.transform.position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);

        if(direction > 0)
        {
            triangles[triangles.Length - 3] = closestVertexIndex;
            triangles[triangles.Length - 2] = newVertexIndex;
            triangles[triangles.Length - 1] = secondClosestVertexIndex;
        }
        else
        {
            triangles[triangles.Length - 3] = closestVertexIndex;
            triangles[triangles.Length - 2] = secondClosestVertexIndex;
            triangles[triangles.Length - 1] = newVertexIndex;
        }

        BuildMeshFromData(vertexPositions, triangles);
    }

    [SerializeField] bool mirrorActive = true;
    [SerializeField] float mirrorSnap = 0.01f;


    public double updateFPS;

    // Update is called once per frame
    void Update()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        stopwatch.Start();

        if (!inEditMode) return;

        if (isInVR)
        {
            VRUpdate();
        }
        else
        {
            DesktopUpdate();
        }

        updateFPS = 1 / stopwatch.Elapsed.TotalSeconds;
    }

    Vector3 desktopInteractPosition
    {
        get
        {
            VRCPlayerApi.TrackingData head = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            return head.position + head.rotation * (0.5f * Vector3.forward);
        }
    }

    void DesktopUpdate()
    {
        if (LinkedVertexAdder.IsHeld)
        {
            //No update needed
        }
        else if (!leftVertexPickup.IsHeld)
        {
            int closestLeftVertex = -1;
            float closestLeftDistanceSquared = 0.2f * 0.2f; //minDistance
            Vector3[] vertexPositions = linkedMeshFilter.sharedMesh.vertices;

            float pickupDistanceSquared = vertexInteractorScale * vertexInteractorScale;

            VRCPlayerApi.TrackingData head = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            //Use closest from point
            Vector3 localInteractPoint = transform.InverseTransformPoint(desktopInteractPosition);

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                float currentDistance = (vertexPositions[i] - localInteractPoint).sqrMagnitude;

                if (currentDistance < closestLeftDistanceSquared)
                {
                    closestLeftVertex = i;
                    closestLeftDistanceSquared = currentDistance;
                }
            }

            /*
            //Use intercept:
            Debug.Log(head.rotation * (head.position - vertexPositions[0]));

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                Vector3 localHeadPosition = head.rotation * (head.position - vertexPositions[i]);

                if(localHeadPosition.x * localHeadPosition.x + localHeadPosition.y * localHeadPosition.y < pickupDistanceSquared)
                {
                    float currentDistance = localHeadPosition.z;

                    if (closestLeftDistance < currentDistance)
                    {
                        closestLeftVertex = i;
                        closestLeftDistance = currentDistance;
                    }
                }
            }
            */

            if(closestLeftVertex != -1)
            {
                leftVertexPickup.transform.localPosition = vertexPositions[closestLeftVertex];
                leftVertexPickup.gameObject.SetActive(true);

                currentLeftIndex = closestLeftVertex;
            }
            else
            {
                leftVertexPickup.gameObject.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LinkedVertexAdder.transform.position = desktopInteractPosition;
            }

        }
        else
        {
            leftVertexPickup.transform.position = desktopInteractPosition;

            SetSingleVertexPosition(currentLeftIndex, leftVertexPickup.transform.localPosition);
        }
    }

    void VRUpdate()
    {
        LinkedVertexAdder.UpdateIdlePosition();

        Vector3[] vertexPositions = linkedMeshFilter.sharedMesh.vertices;

        int closestLeftVertex = -1;
        float closestLeftSquareDistance = Mathf.Infinity;

        int closestRightVertex = -1;
        float closestRightSquareDistance = Mathf.Infinity;

        Vector3 localLeftHandPosition = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position);
        Vector3 localRightHandPosition = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);

        if (LinkedVertexAdder.CurrentHand == VRC_Pickup.PickupHand.Left)
        {
            //No update needed
        }
        else if (!leftVertexPickup.IsHeld)
        {
            for (int i = 0; i < vertexPositions.Length; i++)
            {
                float currentDistance = (localLeftHandPosition - vertexPositions[i]).sqrMagnitude;

                if ( currentDistance > closestLeftSquareDistance)
                {
                    closestLeftVertex = i;
                    closestLeftSquareDistance = currentDistance;
                }
            }
        }
        else
        {
            if (mirrorActive && Mathf.Abs(localLeftHandPosition.x) < mirrorSnap)
            {
                Vector3 newPosition = new Vector3(0, localLeftHandPosition.y, localLeftHandPosition.z);

                SetSingleVertexPosition(currentLeftIndex, localLeftHandPosition);
            }
            else
            {
                SetSingleVertexPosition(currentLeftIndex, localLeftHandPosition);
            }
        }

        if (LinkedVertexAdder.CurrentHand == VRC_Pickup.PickupHand.Right)
        {
            //No update needed
        }
        else if (!rightVertexPickup.IsHeld)
        {
            for (int i = 0; i < vertexPositions.Length; i++)
            {
                float currentDistance = (localRightHandPosition - vertexPositions[i]).sqrMagnitude;

                if (currentDistance < closestRightSquareDistance)
                {
                    closestRightVertex = i;
                    closestRightSquareDistance = currentDistance;
                }
            }
        }
        else
        {
            if (mirrorActive && Mathf.Abs(localRightHandPosition.x) < mirrorSnap)
            {
                Vector3 newPosition = new Vector3(0, localRightHandPosition.y, localRightHandPosition.z);

                SetSingleVertexPosition(currentRightIndex, localRightHandPosition);
            }
            else
            {
                SetSingleVertexPosition(currentRightIndex, localRightHandPosition);
            }
        }

        if(!leftVertexPickup.IsHeld && rightVertexPickup.IsHeld)
        {
            if (closestLeftVertex == closestRightVertex)
            {
                if (closestLeftSquareDistance < closestRightSquareDistance)
                {
                    if (closestLeftSquareDistance < vertexInteractorScale * vertexInteractorScale)
                    {
                        leftVertexPickup.gameObject.SetActive(true);
                        rightVertexPickup.gameObject.SetActive(false);
                        leftVertexPickup.transform.localPosition = vertexPositions[closestLeftVertex];
                        currentLeftIndex = closestLeftVertex;
                    }
                }
                else
                {
                    if (closestRightSquareDistance < vertexInteractorScale * vertexInteractorScale)
                    {
                        rightVertexPickup.gameObject.SetActive(true);
                        leftVertexPickup.gameObject.SetActive(false);
                        rightVertexPickup.transform.localPosition = vertexPositions[closestRightVertex];
                        currentRightIndex = closestRightVertex;
                    }
                }
            }
            else
            {
                if (closestLeftSquareDistance < vertexInteractorScale * vertexInteractorScale)
                {
                    leftVertexPickup.gameObject.SetActive(true);
                    leftVertexPickup.transform.localPosition = vertexPositions[closestLeftVertex];
                    currentLeftIndex = closestLeftVertex;
                }

                if (closestRightSquareDistance < vertexInteractorScale * vertexInteractorScale)
                {
                    rightVertexPickup.gameObject.SetActive(true);
                    rightVertexPickup.transform.localPosition = vertexPositions[closestLeftVertex];
                    currentRightIndex = closestRightVertex;
                }
            }
        }
    }

    void RemoveVertexFromArray(int index, bool updateTriangles)
    {
        GameObject.Destroy(vertexIndicators[index].gameObject);

        Transform[] oldVertexPositions = vertexIndicators;
        vertexIndicators = new Transform[oldVertexPositions.Length - 1];

        int newIndex = 0;

        for (int i = 0; i < oldVertexPositions.Length; i++)
        {
            if (i == index) continue;

            vertexIndicators[newIndex++] = oldVertexPositions[i];
        }

        int[] triangles = linkedMeshFilter.sharedMesh.triangles;

        if (updateTriangles)
        {
            int trianglesToBeRemoved = 0;

            foreach(int triangle in triangles)
            {
                if (triangle == index) trianglesToBeRemoved++;
            }

            int[] oldTriangles = triangles;
            triangles = new int[triangles.Length - trianglesToBeRemoved * 3];

            for (int i = 0; i<triangles.Length; i+=3)
            {
                int a = oldTriangles[i];
                int b = oldTriangles[i + 1];
                int c = oldTriangles[i + 2];

                if(a != index && b != index && c!= index)
                {
                    triangles[i] = a;
                    triangles[i + 1] = b;
                    triangles[i + 2] = c;
                }
            }
        }

        linkedMeshFilter.sharedMesh.triangles = triangles;
    }

    public void MergeOverlappingVertices()
    {
        int verticesMerged = 0;

        for(int i = 0; i<vertexIndicators.Length - 1; i++)
        {
            Vector3 firstPosition = vertexIndicators[i].transform.position;

            for(int j = i + 1; j<vertexIndicators.Length; j++)
            {
                Vector3 secondPosition = vertexIndicators[i + 1].transform.position;
                float distacne = (firstPosition - secondPosition).magnitude;

                if (distacne < vertexInteractorScale)
                {
                    MergeVertices(i, j);
                    verticesMerged++;
                }
            }
        }

        Debug.Log($"{verticesMerged} vertices merged");

        SetIndicatorsFromMesh();
    }

    void MergeVertices(int keep, int discard)
    {
        RemoveVertexFromArray(discard, false);

        int trianglesToBeRemoved = 0;

        int[] triangles = linkedMeshFilter.sharedMesh.triangles;

        //Replace keep with discard
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int found = 0;

            if (triangles[i] == discard)
            {
                triangles[i] = keep;
                found++;
            }
            if (triangles[i + 1] == discard)
            {
                triangles[i + 1] = keep;
                found++;
            }
            if (triangles[i + 2] == discard)
            {
                triangles[i + 2] = keep;
                found++;
            }

            if(found > 0)
            {
                trianglesToBeRemoved += found - 1;
            }
        }

        //decrement index
        for (int i = 0; i < triangles.Length; i++)
        {
            if (triangles[i] > discard) triangles[i]--;
        }

        //Remove failed triangles
        int trianglesRemoved = 0;

        if(trianglesToBeRemoved > 0)
        {
            int[] oldTriangles = triangles;
            triangles = new int[triangles.Length - trianglesToBeRemoved * 3];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = oldTriangles[i];
                int b = oldTriangles[i + 1];
                int c = oldTriangles[i + 2];

                if (a != b && b != c && c != a)
                {
                    triangles[i] = a - trianglesRemoved * 3;
                    triangles[i + 1] = trianglesRemoved * 3;
                    triangles[i + 2] = trianglesRemoved * 3;
                }
                else
                {
                    trianglesRemoved++;
                }
            }
        }

        linkedMeshFilter.sharedMesh.triangles = triangles; ;
    }
}
