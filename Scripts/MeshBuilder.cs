﻿
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

    VertexInteractor[] interactorPositions = new VertexInteractor[0];

    bool isInVR;

    MeshFilter linkedMeshFilter;
    MeshRenderer linkedMeshRenderer;
    MeshRenderer symmetryMeshRenderer;

    public bool setupComplete = false;

    public float vertexInteractorScale
    {
        get
        {
            return interactorPositions[0].transform.localScale.x;
        }
        set
        {
            Vector3 scale = value * Vector3.one;

            foreach(VertexInteractor vertex in interactorPositions)
            {
                vertex.transform.localScale = scale;
            }
        }
    }

    public Mesh SharedMesh
    {
        get
        {
            return linkedMeshFilter.sharedMesh;
        }
    }

    public Vector3[] MeshVertices
    {
        get
        {
            return linkedMeshFilter.sharedMesh.vertices;
        }
        set
        {
            linkedMeshFilter.sharedMesh.vertices = value;
        }
    }

    public int[] MeshTriangles
    {
        get
        {
            return linkedMeshFilter.sharedMesh.triangles;
        }
        set
        {
            linkedMeshFilter.sharedMesh.triangles = value;
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

            foreach(VertexInteractor interactor in interactorPositions)
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
        for(int i = 0; i<interactorPositions.Length; i++)
        {
            VertexInteractor interactor = interactorPositions[i];

            GameObject.Destroy(interactorPositions[i].gameObject);

            interactorPositions[i] = null;
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

    void UpdateMeshData()
    {
        Mesh mesh = linkedMeshFilter.sharedMesh;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    void SetElementsAndMeshFromData()
    {
        UpdateMeshData();

        if (inEditMode)
        {
            SetInteractorsFromMesh();
        }
    }

    void SetElementsAndMeshFromData(Vector3[] positions, int[] triangles)
    {
        BuildMeshFromData(positions, triangles);

        if (inEditMode)
        {
            SetInteractorsFromMesh();
        }
    }

    public void SetInteractorsFromMesh()
    {
        if (!InEditMode) return;

        ClearData();

        Vector3[] positions = MeshVertices;

        interactorPositions = new VertexInteractor[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            VertexInteractor currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexInteractor>();

            currentInteractor.Setup(i, transform, positions[i], this);

            interactorPositions[i] = currentInteractor;
        }
    }

    void SetSingleVertexPosition(int index, Vector3 localPosition)
    {
        Vector3[] positions = MeshVertices;

        positions[index] = localPosition;

        MeshVertices = positions;

        if (inEditMode)
        {
            interactorPositions[index].transform.localPosition = localPosition;
        }
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

        SetElementsAndMeshFromData(vertexPositions, triangles);
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
        
        SetInteractorsFromMesh();

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
        VertexInteractor[] oldInteractors = interactorPositions;
        Vector3[] oldVertexPositions = MeshVertices;
        interactorPositions = new VertexInteractor[oldInteractors.Length + 1];
        Vector3[] newVertexPositions = new Vector3[oldInteractors.Length + 1];

        for (int i = 0; i < oldInteractors.Length; i++)
        {
            interactorPositions[i] = oldInteractors[i];
            newVertexPositions[i] = oldVertexPositions[i];
        }

        int newVertexIndex = interactorPositions.Length - 1;

        VertexInteractor currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexInteractor>();

        Vector3 localPosition = transform.InverseTransformPoint(LinkedVertexAdder.transform.position);

        currentInteractor.Setup(newVertexIndex, transform, localPosition, this);

        interactorPositions[interactorPositions.Length - 1] = currentInteractor;
        newVertexPositions[interactorPositions.Length - 1] = localPosition;

        //Triangles
        int[] oldTriangles = MeshTriangles;

        int[] newTriangles = new int[oldTriangles.Length + 3];

        for (int i = 0; i < oldTriangles.Length; i++)
        {
            newTriangles[i] = oldTriangles[i];
        }

        Vector3 normal = Vector3.Cross(LinkedVertexAdder.transform.position - closestVertex.transform.position, LinkedVertexAdder.transform.position - secondClosestVertex.transform.position);

        float direction = Vector3.Dot(normal, LinkedVertexAdder.transform.position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);

        if(direction > 0)
        {
            newTriangles[newTriangles.Length - 3] = closestVertex.index;
            newTriangles[newTriangles.Length - 2] = newVertexIndex;
            newTriangles[newTriangles.Length - 1] = secondClosestVertex.index;
        }
        else
        {
            newTriangles[newTriangles.Length - 3] = closestVertex.index;
            newTriangles[newTriangles.Length - 2] = secondClosestVertex.index;
            newTriangles[newTriangles.Length - 1] = newVertexIndex;
        }

        BuildMeshFromData(newVertexPositions, newTriangles);
    }

    [SerializeField] bool mirrorActive = true;
    [SerializeField] float mirrorSnap = 0.01f;

    void PCUpdate()
    {

    }

    void VRUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isInVR)
        {
            LinkedVertexAdder.UpdateIdlePosition();

            if(Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryHandTrigger") > 0.9 || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryHandTrigger") > 0.9)
            {
                DropFunction();
            }
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

                Transform currentVertex = interactorPositions[activeVertex].transform;

                currentVertex.position = newPosition;

                if (mirrorActive && Mathf.Abs(currentVertex.localPosition.x) < mirrorSnap)
                {
                    currentVertex.localPosition = new Vector3(0, currentVertex.localPosition.y, currentVertex.localPosition.z);
                }

                SetSingleVertexPosition(activeVertex, currentVertex.localPosition);
            }
            else
            {
                Transform currentVertex = interactorPositions[activeVertex].transform;

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

                SetSingleVertexPosition(activeVertex, currentVertex.localPosition);
            }

        }
        else if(LinkedVertexAdder.IsHeld)
        {
            closestVertex = null;
            secondClosestVertex = null;

            float closestDistance = 0;
            float secondclosestDistance = 0;

            for(int i = 0; i<interactorPositions.Length; i++)
            {
                VertexInteractor currentVertex = interactorPositions[i];

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
        foreach(int index in MeshTriangles)
        {
            interactorPositions[index].index = index; //ToFix: Size missmatch error
        }
    }

    void RemoveVertexFromArray(int index, bool updateTriangles)
    {
        GameObject.Destroy(interactorPositions[index].gameObject);

        VertexInteractor[] oldVertexInteractors = interactorPositions;
        interactorPositions = new VertexInteractor[oldVertexInteractors.Length - 1];
        Vector3[] oldVertexPositons = MeshVertices;
        Vector3[] newVertexPositions = new Vector3[oldVertexInteractors.Length - 1];

        int newIndex = 0;

        for (int i = 0; i < oldVertexInteractors.Length; i++)
        {
            if (i == index) continue;

            interactorPositions[newIndex] = oldVertexInteractors[i];
            newVertexPositions[newIndex] = oldVertexPositons[i];

            newIndex++;
        }

        if (updateTriangles)
        {
            int trianglesToBeRemoved = 0;

            foreach(int triangle in MeshTriangles)
            {
                if (triangle == index) trianglesToBeRemoved++;
            }

            int[] oldTriangles = MeshTriangles;
            int[] newTriangles = new int[oldTriangles.Length - trianglesToBeRemoved * 3];

            for (int i = 0; i<newTriangles.Length; i+=3)
            {
                int a = oldTriangles[i];
                int b = oldTriangles[i + 1];
                int c = oldTriangles[i + 2];

                if(a != index && b != index && c!= index)
                {
                    newTriangles[i] = a;
                    newTriangles[i + 1] = b;
                    newTriangles[i + 2] = c;
                }
            }

            MeshTriangles = newTriangles;

            UpdateVertexIndexFromTriangles();
        }
    }

    public void MergeOverlappingVertices()
    {
        int verticesMerged = 0;

        for(int i = 0; i<interactorPositions.Length - 1; i++)
        {
            Vector3 firstPosition = interactorPositions[i].transform.position;

            for(int j = i + 1; j<interactorPositions.Length; j++)
            {
                Vector3 secondPosition = interactorPositions[i + 1].transform.position;
                float distacne = (firstPosition - secondPosition).magnitude;

                if (distacne < maxVertexMergeDistance)
                {
                    MergeVertices(i, j, false);
                    verticesMerged++;
                }
            }
        }

        Debug.Log($"{verticesMerged} vertices merged");

        UpdateMeshData();
    }

    void MergeVertices(int keep, int discard, bool updateInteractors)
    {
        RemoveVertexFromArray(discard, false);

        int trianglesToBeRemoved = 0;

        int[] triangles = MeshTriangles;

        //Replace keep with discard
        for (int i = 0; i < MeshTriangles.Length; i += 3)
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

        MeshTriangles = triangles;

        if(updateInteractors) UpdateVertexIndexFromTriangles();
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if(!inEditMode) return;

        if (activeVertex < 0) return;

        int closestVertex = -1;
        float closestDistance = Mathf.Infinity;

        for(int i = 0; i<interactorPositions.Length; i++)
        {
            if (i == activeVertex) continue;

            float distance = (interactorPositions[i].transform.position - interactorPositions[activeVertex].transform.position).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertex = i;
            }
        }

        if (closestVertex == -1) return;

        if (closestDistance < interactorPositions[activeVertex].transform.localScale.x)
        {
            MergeVertices(keep: activeVertex, discard: closestVertex, updateInteractors: true);

            SetElementsAndMeshFromData();

            activeVertex = -1;
        }
    }

    void DropFunction()
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

    public override void InputDrop(bool value, UdonInputEventArgs args)
    {
        //For VR: Call drop function in update loop instead -> Index and Quest 2 don't call this function

        if(!isInVR && value == true)
        {
            DropFunction();
        }
    }
}
