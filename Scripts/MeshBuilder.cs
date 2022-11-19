
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
    [SerializeField] VertexInteractor VertexInteractorPrefab;
    [SerializeField] VertexAdder LinkedVertexAdder;
    [SerializeField] LineRenderer LinkedLineRenderer;
    [SerializeField] float DesktopVertexSpeed = 0.2f;
    
    [SerializeField] MeshFilter SymmetryMeshFilter;

    const float maxVertexMergeDistance = 0.001f;

    VRCPickup.PickupHand currentHand;
    Vector3 handOffset;

    VertexInteractor closestVertex = null;
    VertexInteractor secondClosestVertex = null;

    int activeVertex = -1;

    VertexInteractor[] vertexPositions = new VertexInteractor[0];
    public int[] triangles;

    bool isInVR;

    readonly char newLine = '\n';

    MeshFilter linkedMeshFilter;
    MeshRenderer linkedMeshRenderer;
    MeshRenderer symmetryMeshRenderer;

    public bool setupComplete = false;

    public float vertexInteractorScale
    {
        get
        {
            return vertexPositions[0].transform.localScale.x;
        }
        set
        {
            Vector3 scale = value * Vector3.one;

            foreach(VertexInteractor vertex in vertexPositions)
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

            foreach(VertexInteractor interactor in vertexPositions)
            {
                interactor.gameObject.SetActive(value);
            }

            LinkedVertexAdder.gameObject.SetActive(value);
            LinkedVertexAdder.ForceDropIfHeld();
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
            triangles = new int[triangleCount * 3];

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

    void ClearData()
    {
        for(int i = 0; i<vertexPositions.Length; i++)
        {
            VertexInteractor interactor = vertexPositions[i];

            GameObject.Destroy(vertexPositions[i].gameObject);

            vertexPositions[i] = null;
        }
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

    void BuildMeshFromElements()
    {
        Vector3[] vertices = new Vector3[vertexPositions.Length];

        for (int i = 0; i < vertexPositions.Length; i++)
        {
            vertices[i] = vertexPositions[i].transform.localPosition;
        }

        BuildMeshFromData(vertices, triangles);
    }

    void SetupElementsAndMeshFromData(Vector3[] positions, int[] triangles)
    {
        this.triangles = triangles;

        linkedMeshFilter.sharedMesh.triangles = triangles;

        ClearData();

        vertexPositions = new VertexInteractor[positions.Length];

        for(int i = 0; i<positions.Length; i++)
        {
            VertexInteractor currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexInteractor>();

            currentInteractor.Setup(i, transform, positions[i], this);

            vertexPositions[i] = currentInteractor;
        }

        BuildMeshFromElements();
    }

    void SetupElementsFromMesh()
    {
        ClearData();

        Vector3[] positions = linkedMeshFilter.sharedMesh.vertices;

        vertexPositions = new VertexInteractor[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            VertexInteractor currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexInteractor>();

            currentInteractor.Setup(i, transform, positions[i], this);

            vertexPositions[i] = currentInteractor;
        }

        triangles = linkedMeshFilter.sharedMesh.triangles;
    }

    public void InteractWithVertex(VertexInteractor interactedVertex)
    {
        activeVertex = interactedVertex.index;

        if (isInVR)
        {
            VRCPlayerApi.TrackingData leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            VRCPlayerApi.TrackingData rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

            Vector3 vertexPosition = interactedVertex.transform.position;

            if ((leftHand.position - vertexPosition).magnitude < (rightHand.position - vertexPosition).magnitude)
            {
                currentHand = VRCPickup.PickupHand.Left;

                interactedVertex.transform.SetPositionAndRotation(leftHand.position, leftHand.rotation);
            }
            else
            {
                currentHand = VRCPickup.PickupHand.Right;

                interactedVertex.transform.SetPositionAndRotation(rightHand.position, rightHand.rotation);
            }

            //handOffset = interactedVertex.transform.InverseTransformDirection(vertexPosition);
            handOffset = Vector3.zero;

            interactedVertex.transform.position = vertexPosition;
        }
        else
        {
            Networking.LocalPlayer.Immobilize(true);
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

        if (VertexInteractorPrefab == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(VertexInteractorPrefab)} not assinged");
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
        
        SetupElementsFromMesh();

        if(SymmetryMeshFilter) symmetryMeshRenderer = SymmetryMeshFilter.transform.GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    public void PickupVertexAdder()
    {
        if(activeVertex >= 0)
        {
            LinkedVertexAdder.ForceDropIfHeld();
            return;
        }

        LinkedLineRenderer.gameObject.SetActive(true);

        
    }

    public void DropVertexAdder()
    {
        LinkedLineRenderer.gameObject.SetActive(false);
    }

    public void UseVertexAdder()
    {
        //Vertex
        VertexInteractor[] oldVertexPositions = vertexPositions;
        vertexPositions = new VertexInteractor[oldVertexPositions.Length + 1];

        for (int i = 0; i < oldVertexPositions.Length; i++)
        {
            vertexPositions[i] = oldVertexPositions[i];
        }

        int newVertexIndex = vertexPositions.Length - 1;

        VertexInteractor currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexInteractor>();

        currentInteractor.Setup(newVertexIndex, transform, transform.InverseTransformPoint(LinkedVertexAdder.transform.position), this);

        vertexPositions[vertexPositions.Length - 1] = currentInteractor;

        //Triangles
        int[] oldTriangles = this.triangles;

        triangles = new int[oldTriangles.Length + 3];

        for (int i = 0; i < oldTriangles.Length; i++)
        {
            triangles[i] = oldTriangles[i];
        }

        Vector3 normal = Vector3.Cross(LinkedVertexAdder.transform.position - closestVertex.transform.position, LinkedVertexAdder.transform.position - secondClosestVertex.transform.position);

        float direction = Vector3.Dot(normal, LinkedVertexAdder.transform.position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);

        if(direction > 0)
        {
            triangles[triangles.Length - 3] = closestVertex.index;
            triangles[triangles.Length - 2] = newVertexIndex;
            triangles[triangles.Length - 1] = secondClosestVertex.index;
        }
        else
        {
            triangles[triangles.Length - 3] = closestVertex.index;
            triangles[triangles.Length - 2] = secondClosestVertex.index;
            triangles[triangles.Length - 1] = newVertexIndex;
        }

        BuildMeshFromElements();
    }

    [SerializeField] bool mirrorActive = true;
    [SerializeField] float mirrorSnap = 0.01f;

    // Update is called once per frame
    void Update()
    {
        if (isInVR)
        {
            LinkedVertexAdder.UpdateIdlePosition();
        }

        if (activeVertex >= 0)
        {
            if (isInVR)
            {
                VRCPlayerApi.TrackingData currentHandData;

                switch (currentHand)
                {
                    case VRC_Pickup.PickupHand.None:
                        activeVertex = -1;
                        return;
                        break;
                    case VRC_Pickup.PickupHand.Left:
                        currentHandData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                        break;
                    case VRC_Pickup.PickupHand.Right:
                        currentHandData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                        break;
                    default:
                        currentHandData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                        break;
                }

                Vector3 newPosition = currentHandData.position + currentHandData.rotation * handOffset;

                Transform vertex = vertexPositions[activeVertex].transform;

                vertex.position = newPosition;

                if (mirrorActive && Mathf.Abs(vertex.localPosition.x) < mirrorSnap)
                {
                    vertex.localPosition = new Vector3(0, vertex.localPosition.y, vertex.localPosition.z);
                }
            }
            else
            {
                Transform currentVertex = vertexPositions[activeVertex].transform;

                if (Input.GetKey(KeyCode.W))
                {
                    currentVertex.transform.localPosition += Vector3.forward * Time.deltaTime * DesktopVertexSpeed;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    Vector3 newPosition = Time.deltaTime * DesktopVertexSpeed * Vector3.left + currentVertex.transform.localPosition;

                    if (mirrorActive && Mathf.Abs(newPosition.x) < mirrorSnap)
                    {
                        newPosition.x = 0;
                    }

                    currentVertex.transform.localPosition = newPosition;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    currentVertex.transform.localPosition += Vector3.back * Time.deltaTime * DesktopVertexSpeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    Vector3 newPosition = Time.deltaTime * DesktopVertexSpeed * Vector3.right + currentVertex.transform.localPosition;

                    if (mirrorActive && Mathf.Abs(newPosition.x) < mirrorSnap)
                    {
                        newPosition.x = 0;
                    }

                    currentVertex.transform.localPosition = newPosition;
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currentVertex.transform.localPosition += Vector3.up * Time.deltaTime * DesktopVertexSpeed;
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    currentVertex.transform.localPosition += Vector3.down * Time.deltaTime * DesktopVertexSpeed;
                }

                if (Input.GetMouseButton(1))
                {
                    Networking.LocalPlayer.Immobilize(false);
                }

                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    RemoveVertexFromArray(activeVertex, true);
                    activeVertex = -1;
                }
            }

            BuildMeshFromElements();
        }
        else if(LinkedVertexAdder.IsHeld)
        {
            closestVertex = null;
            secondClosestVertex = null;

            float closestDistance = 0;
            float secondclosestDistance = 0;

            for(int i = 0; i<vertexPositions.Length; i++)
            {
                VertexInteractor currentVertex = vertexPositions[i];

                //Fill first
                if(closestVertex == null)
                {
                    closestVertex = currentVertex;
                    closestDistance = GetDistanceToVertexAdder(currentVertex);
                    continue;
                }

                //Fill or replace second
                if(secondClosestVertex == null)
                {
                    secondclosestDistance = GetDistanceToVertexAdder(currentVertex);
                    secondClosestVertex = currentVertex;
                }
                else
                {
                    float distance = GetDistanceToVertexAdder(currentVertex);

                    if(distance < secondclosestDistance)
                    {
                        secondclosestDistance = distance;
                        secondClosestVertex = currentVertex;
                    }
                }

                //Reorder if not correct
                if (secondclosestDistance < closestDistance)
                {
                    float temp = secondclosestDistance;
                    VertexInteractor tempVertex = secondClosestVertex;

                    secondclosestDistance = closestDistance;
                    secondClosestVertex = closestVertex;

                    closestDistance = temp;
                    closestVertex = tempVertex;
                }
            }

            if (secondClosestVertex == null) return;

            LinkedLineRenderer.SetPosition(0, closestVertex.transform.position);
            LinkedLineRenderer.SetPosition(1, LinkedVertexAdder.transform.position);
            LinkedLineRenderer.SetPosition(2, secondClosestVertex.transform.position);
        }
    }

    float GetDistanceToVertexAdder(VertexInteractor a)
    {
        return (a.transform.position - LinkedVertexAdder.transform.position).magnitude;
    }

    void UpdateVertexIndexFromTriangles()
    {
        foreach(int index in triangles)
        {
            vertexPositions[index].index = index; //ToFix: Size missmatch error
        }
    }

    void RemoveVertexFromArray(int index, bool updateTriangles)
    {
        GameObject.Destroy(vertexPositions[index].gameObject);

        VertexInteractor[] oldVertexPositions = vertexPositions;
        vertexPositions = new VertexInteractor[oldVertexPositions.Length - 1];

        int newIndex = 0;

        for (int i = 0; i < oldVertexPositions.Length; i++)
        {
            if (i == index) continue;

            vertexPositions[newIndex++] = oldVertexPositions[i];
        }


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

            UpdateVertexIndexFromTriangles();
        }
    }

    public void MergeOverlappingVertices()
    {
        int verticesMerged = 0;

        for(int i = 0; i<vertexPositions.Length - 1; i++)
        {
            Vector3 firstPosition = vertexPositions[i].transform.position;

            for(int j = i + 1; j<vertexPositions.Length; j++)
            {
                Vector3 secondPosition = vertexPositions[i + 1].transform.position;
                float distacne = (firstPosition - secondPosition).magnitude;

                if (distacne < maxVertexMergeDistance)
                {
                    MergeVertices(i, j);
                    verticesMerged++;
                }
            }
        }

        Debug.Log($"{verticesMerged} vertices merged");

        BuildMeshFromElements();
    }

    void MergeVertices(int keep, int discard)
    {
        RemoveVertexFromArray(discard, false);

        int trianglesToBeRemoved = 0;

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

        if (activeVertex > discard) activeVertex--;

        UpdateVertexIndexFromTriangles();
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if(!inEditMode) return;

        if (activeVertex < 0) return;

        int closestVertex = -1;
        float closestDistance = Mathf.Infinity;

        for(int i = 0; i<vertexPositions.Length; i++)
        {
            if (i == activeVertex) continue;

            float distance = (vertexPositions[i].transform.position - vertexPositions[activeVertex].transform.position).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertex = i;
            }
        }

        if (closestVertex == -1) return;

        if (closestDistance < vertexPositions[activeVertex].transform.localScale.x)
        {
            MergeVertices(keep: activeVertex, discard: closestVertex);

            BuildMeshFromElements();

            activeVertex = -1;
        }
    }

    public override void InputDrop(bool value, UdonInputEventArgs args)
    {
        if (!inEditMode) return;

        if (activeVertex < 0) return;

        activeVertex = -1;
        if (isInVR)
        {

        }
        else
        {
            Networking.LocalPlayer.Immobilize(false);
        }
    }
}
