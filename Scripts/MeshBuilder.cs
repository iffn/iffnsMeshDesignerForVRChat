
using System.Linq.Expressions;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{

    public class MeshBuilder : UdonSharpBehaviour
    {
        [Header("Settings")]
        [SerializeField] float minInteractionDistance;
        [SerializeField] bool mirrorActive = true;
        [SerializeField] float mirrorSnap = 0.01f;
        [SerializeField] float maxDesktopInteractionDistance = 1.5f;

        [Header("Unity assingments")]
        [SerializeField] VertexIndicator VertexInteractorPrefab;
        [SerializeField] LineRenderer LinkedLineRenderer;
        [SerializeField] InteractorController LinkedHandIndicator;
        [SerializeField] Transform HelperTransform;
        public Scaler LinkedScaler;
        public MeshFilter SymmetryMeshFilter;

        Vector3[] vertices;

        int[] triangles;

        public bool ManualVertexDrop;

        double updateFPSForDebug;

        public VertexIndicator[] interactorPositions = new VertexIndicator[0];

        bool isInVR;
        bool viveUser = false;

        public bool setupComplete = false;

        float proximityAddTime = 0;

        public int closestVertex = -1;
        public VertexIndicator ClosestVertex
        {
            get
            {
                if (closestVertex == -1) return null;
                return interactorPositions[closestVertex];
            }
        }

        public int secondClosestVertex = -1;
        public VertexIndicator SecondClosestVertex
        {
            get
            {
                if (secondClosestVertex == -1) return null;
                return interactorPositions[secondClosestVertex];
            }
        }

        MeshFilter linkedMeshFilter;
        MeshRenderer linkedMeshRenderer;
        MeshRenderer symmetryMeshRenderer;

        InteractionTypes currentInteractionType = InteractionTypes.Idle;

        float currentDesktopPickupDistance = 0.5f;

        public InteractionTypes CurrentInteractionType
        {
            get
            {
                return currentInteractionType;

            }
            set
            {
                //Unload
                if (closestVertex >= 0) interactorPositions[closestVertex].SelectState = VertexSelectStates.Normal;
                if (secondClosestVertex >= 0) interactorPositions[secondClosestVertex].SelectState = VertexSelectStates.Normal;
                closestVertex = -1;
                secondClosestVertex = -1;

                currentDesktopPickupDistance = (Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position).magnitude;

                switch (currentInteractionType)
                {
                    case InteractionTypes.MoveAndMerge:
                        break;
                    case InteractionTypes.StepAdd:
                        closestVertex = -1;
                        secondClosestVertex = -1;
                        LinkedLineRenderer.gameObject.SetActive(false);
                        break;
                    case InteractionTypes.MoveAndScaleObject:
                        break;
                    case InteractionTypes.AddTriagnle:
                        break;
                    case InteractionTypes.ProximityAdd:
                        closestVertex = -1;
                        secondClosestVertex = -1;
                        LinkedLineRenderer.gameObject.SetActive(false);
                        break;
                    case InteractionTypes.RemoveTriangle:
                        break;
                    case InteractionTypes.Idle:
                        break;
                    case InteractionTypes.RemoveVertex:
                        break;
                    default:
                        break;
                }

                currentInteractionType = value;

                //Load
                switch (value)
                {
                    case InteractionTypes.MoveAndMerge:
                        break;
                    case InteractionTypes.StepAdd:
                        closestVertex = -1;
                        secondClosestVertex = -1;
                        break;
                    case InteractionTypes.MoveAndScaleObject:
                        break;
                    case InteractionTypes.AddTriagnle:
                        closestVertex = -1;
                        secondClosestVertex = -1;
                        break;
                    case InteractionTypes.ProximityAdd:
                        LinkedLineRenderer.gameObject.SetActive(true);
                        break;
                    case InteractionTypes.RemoveTriangle:
                        break;
                    case InteractionTypes.Idle:
                        break;
                    case InteractionTypes.RemoveVertex:
                        break;
                    default:
                        break;
                }
            }
        }

        HandType primaryHand = HandType.RIGHT;

        public HandType PrimaryHand
        {
            get
            {
                return primaryHand;
            }
            set
            {
                primaryHand = value;
            }
        }

        public Vector3 PrimaryHandPosition
        {
            get
            {
                if (isInVR)
                {
                    if (primaryHand == HandType.LEFT)
                    {
                        return Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    }
                    else
                    {
                        return Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    }
                }
                else
                {
                    VRCPlayerApi.TrackingData currentHandData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                    currentDesktopPickupDistance = Mathf.Clamp(currentDesktopPickupDistance, 0, PlayerHeight);

                    return currentHandData.position + currentHandData.rotation * (currentDesktopPickupDistance * Vector3.forward);
                }
            }
        }

        public Quaternion GetPrimaryHandRotation
        {
            get
            {
                if (primaryHand == HandType.LEFT)
                {
                    return Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
                }
                else
                {
                    return Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
                }
            }
        }

        int activeVertex = -1;
        int ActiveVertex
        {
            get
            {
                return activeVertex;
            }
            set
            {
                activeVertex = value;
            }
        }

        bool inEditMode = false;
        public bool InEditMode
        {
            get
            {
                return inEditMode;
            }
            set
            {
                inEditMode = value;

                if (!value)
                {
                    ClearVertexInteractorData();
                }
                else
                {
                    SetInteractorsFromMesh();
                }

                LinkedHandIndicator.gameObject.SetActive(value);

                VertexInteractorScale = VertexInteractorScale; //Refresh scale
            }
        }

        float vertexInteractorScale = 0.02f;
        public float VertexInteractorScale
        {
            get
            {
                return vertexInteractorScale;
            }
            set
            {
                Vector3 scale = value * Vector3.one;

                if (interactorPositions == null)
                {
                    Debug.LogWarning("Somehow null");
                    return;
                }

                foreach (VertexIndicator vertex in interactorPositions)
                {
                    vertex.transform.localScale = scale;
                }

                vertexInteractorScale = value;
            }
        }

        public float PlayerHeight
        {
            get
            {
                return (Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - Networking.LocalPlayer.GetPosition()).magnitude;
            }
        }

        public string LatestDebugText;

        void UpdateDebugText()
        {
            LatestDebugText = "Debug output:\n";
            LatestDebugText += $"Time: {Time.time}\n";
            LatestDebugText += $"{nameof(ActiveVertex)}: {ActiveVertex}\n";
            LatestDebugText += $"Number of interactors: {interactorPositions.Length}\n";
            LatestDebugText += $"{nameof(primaryHand)}: {primaryHand}\n";
            LatestDebugText += $"{nameof(updateFPSForDebug)}: {updateFPSForDebug:0}\n";
            LatestDebugText += $"Mesh vertices: {vertices.Length}\n";
            LatestDebugText += $"Mesh triangles: {triangles.Length}\n";
        }

        public Mesh SharedMesh
        {
            get
            {
                return linkedMeshFilter.sharedMesh;
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

        void ClearVertexInteractorData()
        {
            for (int i = 0; i < interactorPositions.Length; i++)
            {
                Destroy(interactorPositions[i].gameObject);
            }

            interactorPositions = new VertexIndicator[0];
        }

        public Vector3[] verticesDebug;

        void BuildMeshFromData(Vector3[] positions, int[] triangles)
        {
            this.triangles = triangles;
            this.vertices = positions;

            BuildMeshFromData(true);
        }

        void BuildMeshFromData(bool updateInteractorInfo)
        {
            Mesh mesh = linkedMeshFilter.sharedMesh;

            mesh.triangles = new int[0];

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            if (updateInteractorInfo && inEditMode)
            {
                SetInteractorsFromMesh();
            }
        }

        public void UpdateMeshInfoFromMesh()
        {
            Mesh mesh = linkedMeshFilter.sharedMesh;

            vertices = mesh.vertices;
            triangles = mesh.triangles;
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

            if (interactorPositions.Length == vertices.Length)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    interactorPositions[i].SetInfo(i, vertices[i]);
                }
            }
            else
            {
                ClearVertexInteractorData();

                Vector3[] positions = vertices;

                interactorPositions = new VertexIndicator[positions.Length];

                for (int i = 0; i < positions.Length; i++)
                {
                    VertexIndicator currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexIndicator>();

                    currentInteractor.Setup(i, transform, positions[i], this);

                    interactorPositions[i] = currentInteractor;
                }
            }
        }

        void SetSingleVertexPosition(int index, Vector3 localPosition)
        {
            Vector3[] positions = vertices;

            positions[index] = localPosition;

            vertices = positions;

            BuildMeshFromData(false);

            if (inEditMode)
            {
                interactorPositions[index].transform.localPosition = localPosition;
            }
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
            if (LinkedLineRenderer == null)
            {
                correctSetup = false;
                Debug.LogWarning($"Error: {nameof(LinkedLineRenderer)} not assinged");
            }
            if (LinkedHandIndicator == null)
            {
                correctSetup = false;
                Debug.LogWarning($"Error: {nameof(LinkedHandIndicator)} not assinged");
            }

            if (!correctSetup)
            {
                enabled = false;
                return;
            }



            //Setup:
            isInVR = Networking.LocalPlayer.IsUserInVR();

            linkedMeshFilter = transform.GetComponent<MeshFilter>();
            linkedMeshRenderer = transform.GetComponent<MeshRenderer>();

            if (InEditMode)
            {
                SetInteractorsFromMesh();
            }

            LinkedHandIndicator.Setup(this);

            LinkedHandIndicator.gameObject.SetActive(InEditMode);

            if (SymmetryMeshFilter) symmetryMeshRenderer = SymmetryMeshFilter.transform.GetComponent<MeshRenderer>();

            UpdateMeshInfoFromMesh();

            CurrentInteractionType = (InteractionTypes)0;

            //Vive user detection
            string[] controllers = Input.GetJoystickNames();

            viveUser = false;

            foreach (string controller in controllers)
            {
                if (!controller.ToLower().Contains("vive")) continue;

                viveUser = true;
                break;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Setup();
        }

        public void DropVertexAdder()
        {
            LinkedLineRenderer.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateDebugText();

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            if (!isInVR)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    switch (currentInteractionType)
                    {
                        case InteractionTypes.MoveAndMerge:
                            MoveAndMergeUseDesktop();
                            break;
                        case InteractionTypes.StepAdd:
                            StepAddUse();
                            break;
                        case InteractionTypes.MoveAndScaleObject:
                            break;
                        case InteractionTypes.AddTriagnle:
                            TriangleAdditionUse();
                            break;
                        case InteractionTypes.ProximityAdd:
                            UseVertexAdder();
                            break;
                        case InteractionTypes.RemoveTriangle:
                            TriangleRemovalUse();
                            break;
                        case InteractionTypes.Idle:
                            break;
                        case InteractionTypes.RemoveVertex:
                            RemoveVertexUse();
                            break;
                        default:
                            break;
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    switch (currentInteractionType)
                    {
                        case InteractionTypes.MoveAndMerge:
                            activeVertex = -1;
                            break;
                        case InteractionTypes.StepAdd:
                            break;
                        case InteractionTypes.MoveAndScaleObject:
                            break;
                        case InteractionTypes.AddTriagnle:
                            break;
                        case InteractionTypes.ProximityAdd:
                            break;
                        case InteractionTypes.RemoveTriangle:
                            break;
                        case InteractionTypes.Idle:
                            break;
                        case InteractionTypes.RemoveVertex:
                            break;
                        default:
                            break;
                    }
                }
            }

            switch (currentInteractionType)
            {
                case InteractionTypes.MoveAndMerge:
                    MoveAndMergeUpdate();
                    break;
                case InteractionTypes.StepAdd:
                    UpdateStepAdd();
                    break;
                case InteractionTypes.MoveAndScaleObject:
                    break;
                case InteractionTypes.AddTriagnle:
                    break;
                case InteractionTypes.ProximityAdd:
                    UpdateVertexAdder(true);
                    break;
                case InteractionTypes.RemoveTriangle:
                    break;
                case InteractionTypes.Idle:
                    break;
                case InteractionTypes.RemoveVertex:
                    break;
                default:
                    break;
            }

            stopwatch.Stop();

            updateFPSForDebug = 1 / stopwatch.Elapsed.TotalSeconds;
        }

        //Move and merge
        void MoveAndMergeUseDesktop()
        {
            int interactedVertex = SelectVertex();

            if (activeVertex < 0)
            {
                activeVertex = interactedVertex;
            }
            else if (interactedVertex >= 0 && activeVertex != interactedVertex)
            {
                MergeVertices(activeVertex, interactedVertex, true, true, true);

                activeVertex = 0;

                return;
            }
        }

        void UseMergeVertexVR()
        {
            if (activeVertex >= 0)
            {
                int closestVertex = SelectClosestVertexInVR();

                if (closestVertex >= 0) MergeVertices(activeVertex, closestVertex, true, true, true);

                activeVertex = -1;
            }
        }

        void MoveAndMergeUpdate()
        {
            if (ActiveVertex < 0) return;

            if (ActiveVertex >= interactorPositions.Length)
            {
                Debug.LogWarning("Active vertex somehow larger than expected");
                return;
            }

            //Set indicator position
            Transform currentVertex = interactorPositions[ActiveVertex].transform;

            currentVertex.position = PrimaryHandPosition;

            //Snap to mirror
            if (mirrorActive && Mathf.Abs(currentVertex.localPosition.x) < mirrorSnap)
            {
                currentVertex.localPosition = new Vector3(0, currentVertex.localPosition.y, currentVertex.localPosition.z);
            }

            //Apply mesh
            SetSingleVertexPosition(ActiveVertex, currentVertex.localPosition);
        }

        //General Vertex addition
        void UpdateVertexAdder(bool findAttachmentPoints)
        {
            if (findAttachmentPoints)
            {
                closestVertex = -1;
                secondClosestVertex = -1;

                float closestDistance = Mathf.Infinity;
                float secondclosestDistance = Mathf.Infinity;

                Vector3 localHandPosition = transform.InverseTransformPoint(PrimaryHandPosition);

                for (int i = 0; i < interactorPositions.Length; i++)
                {
                    Vector3 currentPosition = vertices[i];

                    float distance = (localHandPosition - currentPosition).magnitude;

                    //Override second
                    if (distance < secondclosestDistance)
                    {
                        secondclosestDistance = distance;
                        secondClosestVertex = i;
                    }

                    //Swap
                    if (secondclosestDistance < closestDistance)
                    {
                        float tempD = secondclosestDistance;
                        int tempV = secondClosestVertex;

                        secondclosestDistance = closestDistance;
                        secondClosestVertex = closestVertex;

                        closestDistance = tempD;
                        closestVertex = tempV;
                    }
                }
            }

            if (closestVertex == -1) return;
            if (secondClosestVertex == -1) return;

            LinkedLineRenderer.SetPosition(0, transform.TransformPoint(vertices[closestVertex]));
            LinkedLineRenderer.SetPosition(1, PrimaryHandPosition);
            LinkedLineRenderer.SetPosition(2, transform.TransformPoint(vertices[secondClosestVertex]));
        }

        public void UseVertexAdder()
        {
            if (closestVertex == -1) return;
            if (secondClosestVertex == -1) return;

            //Vertex
            VertexIndicator[] oldInteractors = interactorPositions;
            Vector3[] oldVertexPositions = vertices;
            interactorPositions = new VertexIndicator[oldInteractors.Length + 1];
            vertices = new Vector3[oldInteractors.Length + 1];

            for (int i = 0; i < oldInteractors.Length; i++)
            {
                interactorPositions[i] = oldInteractors[i];
                vertices[i] = oldVertexPositions[i];
            }

            int newVertexIndex = interactorPositions.Length - 1;

            VertexIndicator currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexIndicator>();

            Vector3 localPosition = transform.InverseTransformPoint(PrimaryHandPosition);

            currentInteractor.Setup(newVertexIndex, transform, localPosition, this);

            interactorPositions[interactorPositions.Length - 1] = currentInteractor;
            vertices[interactorPositions.Length - 1] = localPosition;

            //Triangles
            AddPlayerFacingTriangle(closestVertex, secondClosestVertex, vertices.Length - 1);

            BuildMeshFromData(true);
        }

        //General vertex interaction
        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;
            interactorPositions[closestVertex].SelectState = VertexSelectStates.Selected;
        }

        void DeselectClosestVertex()
        {
            if (closestVertex < 0) return;
            interactorPositions[closestVertex].SelectState = VertexSelectStates.Normal;
            closestVertex = -1;
        }

        void SelectSecondClosesVertex(int vertex)
        {
            secondClosestVertex = vertex;
            interactorPositions[secondClosestVertex].SelectState = VertexSelectStates.Selected;
        }

        void DeselectSecondClosestVertex()
        {
            if (secondClosestVertex < 0) return;
            interactorPositions[secondClosestVertex].SelectState = VertexSelectStates.Normal;
            secondClosestVertex = -1;
        }

        //Step add:
        void UpdateStepAdd()
        {
            if (!isInVR)
            {
                if (Input.GetMouseButtonDown(0)) StepAddUse();
            }

            if (closestVertex >= 0 && secondClosestVertex >= 0)
            {
                UpdateVertexAdder(false);
            }
        }

        void StepAddUse()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex != -1)
            {
                if (closestVertex == -1)
                {
                    SelectClosesVertex(interactedVertex);
                    return;
                }
                else if (closestVertex == interactedVertex)
                {
                    DeselectClosestVertex();
                    return;
                }
                else if (secondClosestVertex == -1)
                {
                    SelectSecondClosesVertex(interactedVertex);
                    return;
                }
                else if (secondClosestVertex == interactedVertex)
                {
                    DeselectSecondClosestVertex();
                    return;
                }
                else
                {
                    AddPlayerFacingTriangle(closestVertex, secondClosestVertex, interactedVertex);
                    BuildMeshFromData(true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
            else
            {
                if (closestVertex == -1 || secondClosestVertex == -1) return;

                UseVertexAdder();

                DeselectClosestVertex();
                DeselectSecondClosestVertex();
            }
        }

        // Remove vertex
        void RemoveVertexUse()
        {
            int interactedVertex = SelectVertex();

            if(closestVertex == -1)
            {
                if(interactedVertex >= 0)
                {
                    closestVertex = interactedVertex;

                    interactorPositions[interactedVertex].SelectState = VertexSelectStates.ReadyToDelete;
                }
            }
            else if (closestVertex == interactedVertex)
            {
                RemoveVertexFromArray(interactedVertex, true, true, true);
            }
            else
            {
                interactorPositions[closestVertex].SelectState = VertexSelectStates.Normal;

                closestVertex = interactedVertex;

                if(interactedVertex >= 0)
                {
                    Debug.Log(interactedVertex);

                    interactorPositions[interactedVertex].SelectState = VertexSelectStates.ReadyToDelete;
                }
            }
        }

        // Triangle addition
        void TriangleAdditionUse()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex != -1)
            {
                if (closestVertex == -1)
                {
                    SelectClosesVertex(interactedVertex);
                    return;
                }
                else if (closestVertex == interactedVertex)
                {
                    DeselectClosestVertex();
                    return;
                }
                else if (secondClosestVertex == -1)
                {
                    SelectSecondClosesVertex(interactedVertex);
                    return;
                }
                else if (secondClosestVertex == interactedVertex)
                {
                    DeselectSecondClosestVertex();
                    return;
                }
                else
                {
                    AddPlayerFacingTriangle(closestVertex, secondClosestVertex, interactedVertex);
                    BuildMeshFromData(true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
        }

        void AddPlayerFacingTriangle(int a, int b, int c)
        {
            Vector3 vecA = vertices[a];
            Vector3 vecB = vertices[b];
            Vector3 vecC = vertices[c];

            Vector3 localHead = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);

            Vector3 normal = Vector3.Cross(vecA - vecB, vecA - vecC);

            float direction = Vector3.Dot(normal, 0.333333f * (vecA + vecB + vecC) - localHead);

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

        //Triangle removal
        void TriangleRemovalUse()
        {
            int interactedVertex = SelectVertex();

            if (interactedVertex != -1)
            {
                if (closestVertex == -1)
                {
                    SelectClosesVertex(interactedVertex);
                    return;
                }
                else if (closestVertex == interactedVertex)
                {
                    DeselectClosestVertex();
                    return;
                }
                else if (secondClosestVertex == -1)
                {
                    SelectSecondClosesVertex(interactedVertex);
                    return;
                }
                else if (secondClosestVertex == interactedVertex)
                {
                    DeselectSecondClosestVertex();
                    return;
                }
                else
                {
                    TryRemoveTriangle(closestVertex, secondClosestVertex, interactedVertex);
                    BuildMeshFromData(true);
                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
        }

        void TryRemoveTriangle(int a, int b, int c)
        {
            if (a == b || a == c || b == c)
            {
                Debug.LogWarning("Error: double triangle found");
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

            for (int i = 0; i < triangles.Length; i++)
            {
                vertexUsed[triangles[i]] = true;
            }

            for (int i = 0; i < vertexUsed.Length; i++)
            {
                if (!vertexUsed[i])
                {
                    RemoveVertexFromArray(i, false, true, false);

                    RemoveUnconnectedVertices();

                    return;
                }
            }
        }

        void RemoveVertexFromArray(int index, bool updateInteractors, bool updateTriangles, bool buildMesh)
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

            if (updateInteractors)
            {
                GameObject.Destroy(interactorPositions[index].gameObject);
                VertexIndicator[] oldVertexInteractors = interactorPositions;
                interactorPositions = new VertexIndicator[oldVertexInteractors.Length - 1];

                newIndex = 0;

                for (int i = 0; i < oldVertexInteractors.Length; i++)
                {
                    if (i == index) continue;

                    interactorPositions[newIndex] = oldVertexInteractors[i];

                    newIndex++;
                }

                if (closestVertex == index) closestVertex = -1;
                else if (closestVertex > index) closestVertex--;

                if (secondClosestVertex == index) secondClosestVertex = -1;
                else if (secondClosestVertex > index) secondClosestVertex--;
            }

            if (updateTriangles)
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

            if (buildMesh)
            {
                RemoveUnconnectedVertices();
                BuildMeshFromData(true);
            }
        }

        public void MergeOverlappingVertices()
        {
            int verticesMerged = 0;

            Debug.Log($"Checking {vertices.Length} for merging");

            //return;

            for (int firstVertex = 0; firstVertex < vertices.Length - 1; firstVertex++)
            {
                Vector3 firstPosition = vertices[firstVertex];

                for (int secondVertex = firstVertex + 1; secondVertex < vertices.Length; secondVertex++)
                {
                    Vector3 secondPosition = vertices[secondVertex];
                    float distance = (firstPosition - secondPosition).magnitude;

                    if (distance < minInteractionDistance)
                    {
                        //Debug.Log($"Merging vertex {firstVertex} with {secondVertex} at distance {distance}" );

                        MergeVertices(firstVertex, secondVertex, false, false, false);
                        secondVertex--;
                        verticesMerged++;
                    }
                }
            }

            Debug.Log($"{verticesMerged} vertices merged");

            BuildMeshFromData(true);
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

        void MergeVertices(int keep, int discard, bool updateMesh, bool updateInteractors, bool removeInvalid)
        {
            //Debug.Log($"Keep: {keep}, discard: {discard}");

            RemoveVertexFromArray(discard, false, false, false);

            int trianglesToBeRemoved = 0;

            //Replace keep with discard
            for (int i = 0; i < triangles.Length; i += 3)
            {
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
                    trianglesToBeRemoved += found - 1;
                }
            }

            /*
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (triangles[i] == discard)
                {
                    triangles[i] = keep;
                }
                if (triangles[i + 1] == discard)
                {
                    triangles[i + 1] = keep;
                }
                if (triangles[i + 2] == discard)
                {
                    triangles[i + 2] = keep;
                }
            }
            */

            //decrement index
            for (int i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] > discard) triangles[i]--;
            }

            //Remove failed triangles
            int trianglesRemoved = 0;
            int trianglesSkipped = 0;

            if (trianglesToBeRemoved > 0)
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

            if (ActiveVertex > discard) ActiveVertex--;

            if (removeInvalid)
            {
                RemoveInvalidTriagnles();
                RemoveUnconnectedVertices();
            }

            if (updateMesh) BuildMeshFromData(vertices, this.triangles);
            if (updateInteractors) SetInteractorsFromMesh();


        }

        int SelectVertex()
        {
            if (isInVR) return SelectClosestVertexInVR();
            else return SelectVertexInDesktop();
        }

        int SelectVertexInDesktop()
        {
            VRCPlayerApi.TrackingData data = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            HelperTransform.transform.SetPositionAndRotation(data.position, data.rotation);

            float closestDistance = maxDesktopInteractionDistance;
            int closestIndex = -1;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == activeVertex) continue;

                Vector3 currentPosition = transform.TransformPoint(vertices[i]);

                Vector3 relativePosition = HelperTransform.InverseTransformPoint(currentPosition);

                float distance = relativePosition.z;

                if (distance > closestDistance) continue;

                relativePosition.z = 0;

                if (relativePosition.magnitude > vertexInteractorScale) continue;

                closestIndex = i;
                closestDistance = distance;
            }

            if(closestIndex >= 0) currentDesktopPickupDistance = closestDistance;

            return closestIndex;
        }

        int SelectClosestVertexInVR()
        {
            Vector3 handPosition = PrimaryHandPosition;

            int closestVertex = -1;
            float closestDistance = minInteractionDistance;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == activeVertex) continue;

                Vector3 currentPosition = vertices[i];
                float distance = (currentPosition - handPosition).magnitude;

                if (distance > closestDistance) continue;

                closestDistance = distance;
                closestVertex = i;
            }

            return closestVertex;
        }

        //Common VR input functions
        void GrabInput(HandType handType)
        {
            if (handType != primaryHand) return; //Currently only one handed

            switch (currentInteractionType)
            {
                case InteractionTypes.MoveAndMerge:
                    if (activeVertex < 0)
                    {
                        activeVertex = SelectClosestVertexInVR();
                    }
                    break;
                case InteractionTypes.StepAdd:
                    break;
                case InteractionTypes.MoveAndScaleObject:
                    break;
                case InteractionTypes.AddTriagnle:
                    break;
                case InteractionTypes.ProximityAdd:
                    //Ignore
                    break;
                case InteractionTypes.RemoveTriangle:
                    break;
                case InteractionTypes.Idle:
                    break;
                case InteractionTypes.RemoveVertex:
                    break;
                default:
                    break;
            }
        }

        void DropInput(HandType handType)
        {
            if (handType != primaryHand) return; //Currently only one handed

            switch (currentInteractionType)
            {
                case InteractionTypes.MoveAndMerge:
                    activeVertex = -1;
                    break;
                case InteractionTypes.StepAdd:
                    break;
                case InteractionTypes.MoveAndScaleObject:
                    break;
                case InteractionTypes.AddTriagnle:
                    break;
                case InteractionTypes.ProximityAdd:
                    break;
                case InteractionTypes.RemoveTriangle:
                    break;
                case InteractionTypes.Idle:
                    break;
                case InteractionTypes.RemoveVertex:
                    break;
                default:
                    break;
            }
        }

        void UseInput(bool value, HandType handType)
        {
            if (handType != primaryHand) return; //Currently only one handed

            if (value)
            {
                switch (currentInteractionType)
                {
                    case InteractionTypes.MoveAndMerge:
                        UseMergeVertexVR();
                        break;
                    case InteractionTypes.StepAdd:
                        StepAddUse();
                        break;
                    case InteractionTypes.MoveAndScaleObject:
                        break;
                    case InteractionTypes.AddTriagnle:
                        TriangleAdditionUse();
                        break;
                    case InteractionTypes.ProximityAdd:
                        UseVertexAdder();
                        break;
                    case InteractionTypes.RemoveTriangle:
                        TriangleRemovalUse();
                        break;
                    case InteractionTypes.Idle:
                        break;
                    case InteractionTypes.RemoveVertex:
                        RemoveVertexUse();
                        break;
                    default:
                        break;
                }
            }
        }

        //VRChat input functions
        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (!isInVR) return;

            if (!viveUser)
            {
                if (value)
                {
                    GrabInput(args.handType);
                }
                else
                {
                    DropInput(args.handType);
                }
            }
            else
            {
                if (activeVertex >= 0)
                {
                    UseInput(value, args.handType);
                }
                else
                {
                    GrabInput(args.handType);
                }
            }
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!isInVR) return;

            if (!viveUser)
            {
                if (value) UseInput(value, args.handType);
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            if (!isInVR) return;

            //InputDrop only called for Vive users
            DropInput(args.handType);
        }
    }

    public enum InteractionTypes
    {
        MoveAndMerge,
        StepAdd,
        MoveAndScaleObject,
        AddTriagnle,
        ProximityAdd,
        RemoveTriangle,
        Idle,
        RemoveVertex
    }

}

