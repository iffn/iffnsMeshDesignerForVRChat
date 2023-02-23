//#define debugLog

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using UdonSharp;
using UnityEngine;
using UnityEngine.iOS;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    [RequireComponent(typeof(MeshController))]
    [RequireComponent(typeof(MeshRenderer))]

    public class MeshInteractor : UdonSharpBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool mirrorActive = true;
        [SerializeField] float mirrorSnap = 0.01f;
        [SerializeField] float maxDesktopInteractionDistance = 1.5f;

        [Header("Unity assingments")]
        [SerializeField] MeshController linkedMeshController;
        [SerializeField] VertexIndicator VertexInteractorPrefab;
        [SerializeField] LineRenderer LinkedLineRenderer;
        [SerializeField] InteractorController LinkedInteractionIndicator;
        [SerializeField] Transform HelperTransform;
        public Scaler LinkedScaler;
        public MeshFilter SymmetryMeshFilter;

        float lastUpdateTime = Mathf.NegativeInfinity;
        double updateFPSForDebug;

        public VertexIndicator[] vertexIndicators = new VertexIndicator[0];

        public int activeVertex = -1;
        public int closestVertex = -1;
        public int secondClosestVertex = -1;

        MeshRenderer linkedMeshRenderer;
        MeshRenderer symmetryMeshRenderer;

        public bool[] vertexIsConnectedToActive;

        bool isInVR;
        bool inputDropWorks = false;
        VRCPlayerApi localPlayer;

        InteractionTypes currentInteractionType = InteractionTypes.Idle;

        float currentDesktopPickupDistance = 0.5f;


        public MeshController LinkedMeshController
        {
            get
            {
                return linkedMeshController;
            }
        }

        bool CheckSetup()
        {
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
            if (LinkedInteractionIndicator == null)
            {
                correctSetup = false;
                Debug.LogWarning($"Error: {nameof(LinkedInteractionIndicator)} not assinged");
            }

            return correctSetup;
        }

        void Setup()
        {
            if (!CheckSetup())
            {
                enabled = false;
                return;
            }

            linkedMeshController.Setup();

            linkedMeshRenderer = transform.GetComponent<MeshRenderer>();

            localPlayer = Networking.LocalPlayer;
            isInVR = localPlayer.IsUserInVR();

            if (InEditMode)
            {
                SetInteractorsFromMesh();
            }

            LinkedInteractionIndicator.Setup(this);

            LinkedInteractionIndicator.gameObject.SetActive(InEditMode);

            if (SymmetryMeshFilter) symmetryMeshRenderer = SymmetryMeshFilter.transform.GetComponent<MeshRenderer>();

            CurrentInteractionType = InteractionTypes.Idle;

            //Vive user detection
            string[] controllers = Input.GetJoystickNames();

            inputDropWorks = false;

            foreach (string controller in controllers)
            {
                if (!controller.ToLower().Contains("vive")) continue;

                inputDropWorks = true;
                break;
            }
        }

        public Vector3 LocalHeadPosition
        {
            get
            {
                return transform.InverseTransformPoint(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
            }
        }

        public InteractionTypes CurrentInteractionType
        {
            get
            {
                return currentInteractionType;

            }
            set
            {
                //Unload
                if (closestVertex >= 0) vertexIndicators[closestVertex].SelectState = VertexSelectStates.Normal;
                if (secondClosestVertex >= 0) vertexIndicators[secondClosestVertex].SelectState = VertexSelectStates.Normal;
                if (activeVertex >= 0) vertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                closestVertex = -1;
                secondClosestVertex = -1;
                activeVertex = -1;

                currentDesktopPickupDistance = (Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position).magnitude;

                LinkedLineRenderer.gameObject.SetActive(false);

                currentInteractionType = value;
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

                LinkedInteractionIndicator.gameObject.SetActive(value);

                LinkedScaler.gameObject.SetActive(value);

                VertexInteractionDistance = VertexInteractionDistance; //Refresh scale
            }
        }

        float vertexInteractionDistance = 0.02f;
        public float VertexInteractionDistance
        {
            get
            {
                return vertexInteractionDistance;
            }
            set
            {
                Vector3 scale = value * Vector3.one;

                if (vertexIndicators == null)
                {
                    Debug.LogWarning("Somehow null");
                    return;
                }

                foreach (VertexIndicator vertex in vertexIndicators)
                {
                    vertex.transform.localScale = scale;
                }

                vertexInteractionDistance = value;
            }
        }

        public float PlayerHeight
        {
            get
            {
                return (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - localPlayer.GetPosition()).magnitude;
            }
        }

        public string DebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(MeshInteractor)} at {Time.time}:\n";
            returnString += $"{nameof(lastUpdateTime)}: {lastUpdateTime}\n";
            returnString += $"{nameof(currentInteractionType)}: {interactionTypeStrings[(int)currentInteractionType]}\n";
            returnString += $"{nameof(activeVertex)}: {activeVertex}\n";
            returnString += $"Number of interactors: {vertexIndicators.Length}\n";
            returnString += $"{nameof(primaryHand)}: {primaryHand}\n";
            returnString += $"{nameof(updateFPSForDebug)}: {updateFPSForDebug:0}\n";

            return returnString;
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
            for (int i = 0; i < vertexIndicators.Length; i++)
            {
                Destroy(vertexIndicators[i].gameObject);
            }

            vertexIndicators = new VertexIndicator[0];
        }

        public Vector3[] verticesDebug;

        void Start()
        {
            Setup();
        }

        void Update()
        {
            lastUpdateTime = Time.time;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            if (!isInVR)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //Left click
                    switch (currentInteractionType)
                    {
                        case InteractionTypes.MoveAndMerge:
                            MoveAndMergeUseDesktop();
                            break;
                        case InteractionTypes.StepAdd:
                            StepAddUse();
                            break;
                        case InteractionTypes.QuadAdd:
                            QuadAddUse();
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
                    //Right click
                    switch (currentInteractionType)
                    {
                        case InteractionTypes.MoveAndMerge:
                            if (activeVertex >= 0) vertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                            activeVertex = -1;
                            break;
                        case InteractionTypes.StepAdd:
                            if (closestVertex >= 0) vertexIndicators[closestVertex].SelectState = VertexSelectStates.Normal;
                            if (closestVertex >= 0) vertexIndicators[closestVertex].SelectState = VertexSelectStates.Normal;
                            closestVertex = -1;
                            secondClosestVertex = -1;
                            break;
                        case InteractionTypes.QuadAdd:
                            LinkedLineRenderer.gameObject.SetActive(false);
                            if (activeVertex >= 0) vertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                            activeVertex = -1;
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
                case InteractionTypes.QuadAdd:
                    UpdateQuadAdd();
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

        public void SetInteractorsFromMesh()
        {
            if (!InEditMode) return;

            Vector3[] vertices = linkedMeshController.Vertices;

            if (vertexIndicators.Length == vertices.Length)
            {
                #if debugLog
                Debug.Log("Just updating positions");
                #endif

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertexIndicators[i].SetInfo(i, vertices[i]);
                }
            }
            else
            {
                #if debugLog
                Debug.Log("Reassigning interactors");
                #endif

                ClearVertexInteractorData();

                Vector3[] positions = vertices;

                vertexIndicators = new VertexIndicator[positions.Length];

                for (int i = 0; i < positions.Length; i++)
                {
                    GameObject newObject = GameObject.Instantiate(VertexInteractorPrefab.gameObject);

                    VertexIndicator currentInteractor = newObject.GetComponent<VertexIndicator>(); //TryGetComponent not exposed in U# (...)

                    currentInteractor.Setup(i, transform, positions[i], vertexInteractionDistance);

                    vertexIndicators[i] = currentInteractor;
                }
            }
        }

        void MoveAndMergeUseDesktop()
        {
            int interactedVertex = SelectVertex();

            if (activeVertex < 0)
            {
                activeVertex = interactedVertex;
            }
            else if (interactedVertex >= 0 && activeVertex != interactedVertex)
            {
                linkedMeshController.MergeVertices(interactedVertex, activeVertex, true);
                UpdateMesh(true);

                //ToDo: Update interactors

                activeVertex = -1;

                return;
            }
        }

        void UseMergeVertexVR()
        {
            if (activeVertex >= 0)
            {
                int closestVertex = SelectClosestVertexInVR();

                if (closestVertex >= 0) linkedMeshController.MergeVertices(closestVertex, activeVertex, true);
                UpdateMesh(true);

                //ToDo: Update interactors

                activeVertex = -1;
            }
        }

        void MoveAndMergeUpdate()
        {
            if (activeVertex < 0) return;

            if (activeVertex >= vertexIndicators.Length)
            {
                Debug.LogWarning("Active vertex somehow larger than expected");
                return;
            }

            //Set indicator position
            Transform currentVertex = vertexIndicators[activeVertex].transform;

            currentVertex.position = PrimaryHandPosition;

            //Snap to mirror
            if (mirrorActive && Mathf.Abs(currentVertex.localPosition.x) < mirrorSnap)
            {
                currentVertex.localPosition = new Vector3(0, currentVertex.localPosition.y, currentVertex.localPosition.z);
            }

            //Apply mesh
            linkedMeshController.SetSingleVertexPosition(activeVertex, currentVertex.localPosition);
            vertexIndicators[activeVertex].transform.localPosition = currentVertex.localPosition;

            UpdateMesh(false);
        }

        //General Vertex addition
        void UpdateVertexAdder(bool findAttachmentPoints)
        {
            Vector3[] vertices = linkedMeshController.Vertices;

            if (findAttachmentPoints)
            {
                closestVertex = -1;
                secondClosestVertex = -1;

                float closestDistance = Mathf.Infinity;
                float secondclosestDistance = Mathf.Infinity;

                Vector3 localHandPosition = transform.InverseTransformPoint(PrimaryHandPosition);

                for (int i = 0; i < vertexIndicators.Length; i++)
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

            if (closestVertex >= 0 && secondClosestVertex > 0)
            {
                LinkedLineRenderer.gameObject.SetActive(true);

                LinkedLineRenderer.positionCount = 3;
                LinkedLineRenderer.loop = true;

                LinkedLineRenderer.SetPosition(0, transform.TransformPoint(vertices[closestVertex]));
                LinkedLineRenderer.SetPosition(1, PrimaryHandPosition);
                LinkedLineRenderer.SetPosition(2, transform.TransformPoint(vertices[secondClosestVertex]));
            }
            else
            {
                LinkedLineRenderer.gameObject.SetActive(false);
            }
        }

        public void UseVertexAdder()
        {
            if (closestVertex == -1) return;
            if (secondClosestVertex == -1) return;

            //Indicator
            VertexIndicator[] oldInteractors = vertexIndicators;
            vertexIndicators = new VertexIndicator[oldInteractors.Length + 1];

            for (int i = 0; i < oldInteractors.Length; i++)
            {
                vertexIndicators[i] = oldInteractors[i];
            }

            int newVertexIndex = vertexIndicators.Length - 1;

            VertexIndicator currentInteractor = GameObject.Instantiate(VertexInteractorPrefab.gameObject).GetComponent<VertexIndicator>();

            Vector3 localPosition = transform.InverseTransformPoint(PrimaryHandPosition);

            currentInteractor.Setup(newVertexIndex, transform, localPosition, vertexInteractionDistance);

            vertexIndicators[vertexIndicators.Length - 1] = currentInteractor;

            //Mesh
            linkedMeshController.AddVertex(localPosition, closestVertex, secondClosestVertex, LocalHeadPosition);
            UpdateMesh(true);
        }

        //General vertex interaction
        void SelectClosesVertex(int vertex)
        {
            closestVertex = vertex;
            vertexIndicators[closestVertex].SelectState = VertexSelectStates.Selected;
        }

        void DeselectClosestVertex()
        {
            if (closestVertex < 0) return;
            vertexIndicators[closestVertex].SelectState = VertexSelectStates.Normal;
            closestVertex = -1;
        }

        void SelectSecondClosesVertex(int vertex)
        {
            secondClosestVertex = vertex;
            vertexIndicators[secondClosestVertex].SelectState = VertexSelectStates.Selected;
        }

        void DeselectSecondClosestVertex()
        {
            if (secondClosestVertex < 0) return;
            vertexIndicators[secondClosestVertex].SelectState = VertexSelectStates.Normal;
            secondClosestVertex = -1;
        }

        public void UpdateMesh(bool updateInteractors)
        {
            linkedMeshController.BuildMeshFromData();
            if (updateInteractors) SetInteractorsFromMesh();
        }

        //Step add:
        void UpdateStepAdd()
        {
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
                    linkedMeshController.AddPlayerFacingTriangle(closestVertex, secondClosestVertex, interactedVertex, LocalHeadPosition);
                    UpdateMesh(true);
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

        //Quad add
        void UpdateQuadAdd()
        {
            if (activeVertex < 0) return;

            Vector3[] vertices = LinkedMeshController.Vertices;

            closestVertex = -1;
            secondClosestVertex = -1;

            float closestDistance = Mathf.Infinity;
            float secondclosestDistance = Mathf.Infinity;

            Vector3 localHandPosition = transform.InverseTransformPoint(PrimaryHandPosition);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == activeVertex) continue;
                if (!vertexIsConnectedToActive[i]) continue;

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

            if(closestVertex >= 0 && secondClosestVertex >= 0)
            {
                LinkedLineRenderer.gameObject.SetActive(true);

                LinkedLineRenderer.positionCount = 5;
                LinkedLineRenderer.loop = false;

                LinkedLineRenderer.SetPosition(0, transform.TransformPoint(vertices[closestVertex]));
                LinkedLineRenderer.SetPosition(1, PrimaryHandPosition);
                LinkedLineRenderer.SetPosition(2, transform.TransformPoint(vertices[activeVertex]));
                LinkedLineRenderer.SetPosition(3, PrimaryHandPosition);
                LinkedLineRenderer.SetPosition(4, transform.TransformPoint(vertices[secondClosestVertex]));
            }
        }

        void QuadAddUse()
        {
            int interactedVertex = SelectVertex();

            if (activeVertex < 0)
            {
                //Select initial vertex
                activeVertex = interactedVertex;
                if(activeVertex >= 0) vertexIndicators[activeVertex].SelectState = VertexSelectStates.Selected;

                //Setup connection array
                int[] triangles = LinkedMeshController.Triangles;

                vertexIsConnectedToActive = new bool[LinkedMeshController.Vertices.Length];

                for (int i = 0; i < triangles.Length; i+= 3)
                {
                    int a = triangles[i];
                    int b = triangles[i + 1];
                    int c = triangles[i + 2];

                    if (a == activeVertex || b == activeVertex || c == activeVertex)
                    {
                        vertexIsConnectedToActive[a] = true;
                        vertexIsConnectedToActive[b] = true;
                        vertexIsConnectedToActive[c] = true;
                    }
                }

                return;
            }

            if (activeVertex == interactedVertex)
            {
                //Deselect vertex if alreadyselected
                LinkedLineRenderer.gameObject.SetActive(false);
                vertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                activeVertex = -1;
                return;
            }

            if (closestVertex >= 0 && secondClosestVertex >= 0)
            {
                LinkedMeshController.AddQuad(activeVertex, closestVertex, secondClosestVertex, transform.InverseTransformPoint(PrimaryHandPosition), LocalHeadPosition);
                
                vertexIndicators[activeVertex].SelectState = VertexSelectStates.Normal;
                activeVertex = -1;

                UpdateMesh(true);
            }
        }

        // Remove vertex
        void RemoveVertexUse()
        {
            int interactedVertex = SelectVertex();

            if (closestVertex == -1)
            {
                if (interactedVertex >= 0)
                {
                    closestVertex = interactedVertex;

                    vertexIndicators[interactedVertex].SelectState = VertexSelectStates.ReadyToDelete;
                }
            }
            else if (closestVertex == interactedVertex)
            {
                linkedMeshController.RemoveVertexClean(interactedVertex);
                UpdateMesh(true);
                closestVertex = -1;
            }
            else
            {
                vertexIndicators[closestVertex].SelectState = VertexSelectStates.Normal;

                closestVertex = interactedVertex;

                if (interactedVertex >= 0)
                {
                    vertexIndicators[interactedVertex].SelectState = VertexSelectStates.ReadyToDelete;
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
                    linkedMeshController.AddPlayerFacingTriangle(closestVertex, secondClosestVertex, interactedVertex, LocalHeadPosition);
                    UpdateMesh(false);

                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
        }

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
                    linkedMeshController.TryRemoveTriangle(closestVertex, secondClosestVertex, interactedVertex);
                    UpdateMesh(true);

                    DeselectClosestVertex();
                    DeselectSecondClosestVertex();
                }
            }
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

            Vector3[] vertices = linkedMeshController.Vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == activeVertex) continue;

                Vector3 currentPosition = transform.TransformPoint(vertices[i]);

                Vector3 relativePosition = HelperTransform.InverseTransformPoint(currentPosition);

                float distance = relativePosition.z;

                if (distance > closestDistance) continue;

                relativePosition.z = 0;

                if (relativePosition.magnitude > vertexInteractionDistance) continue;

                closestIndex = i;
                closestDistance = distance;
            }

            if (closestIndex >= 0) currentDesktopPickupDistance = closestDistance;

            return closestIndex;
        }

        int SelectClosestVertexInVR()
        {
            Vector3 handPosition = transform.InverseTransformPoint(PrimaryHandPosition);

            int closestVertex = -1;
            float closestDistance = vertexInteractionDistance;

            Vector3[] vertices = linkedMeshController.Vertices;

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
            #if debugLog
            Debug.Log("Grab input");
            #endif

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
                case InteractionTypes.QuadAdd:
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
            #if debugLog
            Debug.Log("Drop input");
            #endif

            if (handType != primaryHand) return; //Currently only one handed

            switch (currentInteractionType)
            {
                case InteractionTypes.MoveAndMerge:
                    activeVertex = -1;
                    break;
                case InteractionTypes.StepAdd:
                    break;
                case InteractionTypes.QuadAdd:
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
            #if debugLog
            Debug.Log("Use input");
            #endif

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
                    case InteractionTypes.QuadAdd:
                        QuadAddUse();
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

            if (!inputDropWorks)
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
                if (!value) return;

                switch (CurrentInteractionType)
                {
                    case InteractionTypes.MoveAndMerge:
                        if (activeVertex >= 0)
                        {
                            UseInput(value, args.handType);
                        }
                        else
                        {
                            GrabInput(args.handType);
                        }
                        break;
                    case InteractionTypes.StepAdd:
                        break;
                    case InteractionTypes.QuadAdd:
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

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!isInVR) return;

            if (!inputDropWorks)
            {
                if (value) UseInput(value, args.handType);
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            if (!isInVR) return;

            if (!value) return;

            inputDropWorks = true;

            //InputDrop only called for Vive users
            DropInput(args.handType);
        }

        readonly string[] interactionTypeStrings = new string[] {
            "MoveAndMerge",
            "StepAdd",
            "QuadAdd",
            "MoveAndScaleObject",
            "AddTriagnle",
            "ProximityAdd",
            "RemoveTriangle",
            "Idle",
            "RemoveVertex"};
    }

    public enum InteractionTypes
    {
        MoveAndMerge,
        StepAdd,
        QuadAdd,
        MoveAndScaleObject,
        AddTriagnle,
        ProximityAdd,
        RemoveTriangle,
        Idle,
        RemoveVertex
    }
}