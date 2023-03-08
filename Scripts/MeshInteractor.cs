//#define debugLog

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using ClipperLib;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    //[RequireComponent(typeof(MeshController))]
    //[RequireComponent(typeof(MeshRenderer))]

    public class MeshInteractor : UdonSharpBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool mirrorActive = true;
        [SerializeField] float mirrorSnap = 0.01f;
        [SerializeField] float maxDesktopInteractionDistance = 1.5f;
        [SerializeField] Vector3 InteractorOffsetVector = Vector3.down;

        [Header("Unity assingments")]
        [SerializeField] MeshController linkedMeshController;
        [SerializeField] VertexIndicator VertexInteractorPrefab;
        [SerializeField] LineRenderer LinkedLineRenderer;
        [SerializeField] Transform HelperTransform;
        [SerializeField] MeshRenderer LinkedMeshRenderer;
        [SerializeField] MeshRenderer SymmetryMeshRenderer;
        public MeshFilter ReferenceMesh;
        public GameObject MirrorReferenceMesh;

        public bool overUIElement = false;

        MeshEditTool currentEditTool;
        public MeshEditTool CurrentEditTool
        {
            get
            {
                return currentEditTool;
            }
            set
            {
                //Unload
                if (currentEditTool) currentEditTool.OnDeactivation();

                //Reassign
                currentEditTool = value;

                //Load
                if (currentEditTool) currentEditTool.OnActivation();
            }
        }

        public bool MirrorActive
        {
            get
            {
                return mirrorActive;
            }
        }

        public Scaler LinkedScaler;
        //public MeshFilter SymmetryMeshFilter;

        [HideInInspector] public float interactionDistance;

        float lastUpdateTime = Mathf.NegativeInfinity;

        public float LastUpdateTime
        {
            get
            {
                return lastUpdateTime;
            }
        }

        double updateFPSForDebug = 0;

        public VertexIndicator[] vertexIndicators = new VertexIndicator[0];

        InteractorController linkedInteractionController;
        

        public bool[] vertexIsConnectedToActive;

        bool isInVR;
        bool inputDropWorks = false;
        VRCPlayerApi localPlayer;

        float currentDesktopPickupDistance = 0.5f;

        public MeshFilter SymmetryMeshFilter
        {
            get
            {
                return SymmetryMeshRenderer.transform.GetComponent<MeshFilter>();
            }
        }

        public MeshController LinkedMeshController
        {
            get
            {
                return linkedMeshController;
            }
        }

        public bool CheckSetup()
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

            return correctSetup;
        }

        public bool setupCalled = false;

        public void TestFunction()
        {
            Debug.Log($"Test function called");
        }

        public void Setup(InteractorController linkedInteractionIndicator)
        {
            setupCalled = true;

            this.linkedInteractionController = linkedInteractionIndicator;

            lastUpdateTime = Time.time;

            linkedMeshController.Setup();
            
            localPlayer = Networking.LocalPlayer;
            isInVR = localPlayer.IsUserInVR();

            //Also do auto constructor functions:
            vertexIndicators = new VertexIndicator[0];

            if (!linkedMeshController)
            {
                Debug.LogWarning($"Error: {nameof(linkedMeshController)} in {nameof(MeshInteractor)} somehow not assigned");
            }

            LinkedScaler.gameObject.SetActive(inEditMode);

            //Setup mesh
            if (InEditMode)
            {
                SetIndicatorsFromMesh();
            }

            //Vive user detection
            inputDropWorks = !isInVR;

            string[] controllers = Input.GetJoystickNames();

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

        public Vector3 InteractionPosition
        {
            get
            {
                if (isInVR)
                {
                    if (primaryHand == HandType.LEFT)
                    {
                        VRCPlayerApi.TrackingData hand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);

                        return hand.position + interactionDistance * (hand.rotation * InteractorOffsetVector.normalized);
                    }
                    else
                    {
                        VRCPlayerApi.TrackingData hand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

                        return hand.position + interactionDistance * (hand.rotation * InteractorOffsetVector.normalized);
                    }
                }
                else
                {
                    VRCPlayerApi.TrackingData currentHandData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                    currentDesktopPickupDistance = Mathf.Clamp(currentDesktopPickupDistance, 0, PlayerHeight);

                    return currentHandData.position + currentHandData.rotation * (currentDesktopPickupDistance * Vector3.forward);
                }
            }
        }

        public Vector3 LocalInteractionPosition
        {
            get
            {
                return transform.InverseTransformPoint(InteractionPosition);
            }
        }

        public Vector3 LocalInteractionPositionWithMirror
        {
            get
            {
                Vector3 returnValue = LocalInteractionPosition;

                if (mirrorActive && Mathf.Abs(returnValue.x) < interactionDistance)
                {
                    returnValue.x = 0;
                }

                return returnValue;
            }
        }

        public Quaternion GetPrimaryHandRotation
        {
            get
            {
                if (primaryHand == HandType.LEFT)
                {
                    return localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
                }
                else
                {
                    return localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
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
                    SetIndicatorsFromMesh();
                }

                linkedInteractionController.InEditMode = value;

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
            returnString += $"{nameof(CurrentEditTool)}: {((CurrentEditTool != null) ? CurrentEditTool.name : "No tool selected")}\n";
            returnString += $"{nameof(inputDropWorks)}: {inputDropWorks}\n";
            returnString += $"Number of interactors: {vertexIndicators.Length}\n";
            returnString += $"{nameof(primaryHand)}: {primaryHand}\n";
            returnString += $"{nameof(updateFPSForDebug)}: {updateFPSForDebug:0}\n";

            return returnString;
        }

        public bool SymmetryMode
        {
            set
            {
                if (!SymmetryMeshRenderer) return;

                SymmetryMeshRenderer.transform.gameObject.SetActive(value);
            }
        }

        public Material AttachedMaterial
        {
            get
            {
                return LinkedMeshRenderer.sharedMaterial;
            }
            set
            {
                LinkedMeshRenderer.sharedMaterial = value;
                if (SymmetryMeshRenderer) SymmetryMeshRenderer.sharedMaterial = value;
            }
        }

        public void ClearVertexInteractorData()
        {
            for (int i = 0; i < vertexIndicators.Length; i++)
            {
                Destroy(vertexIndicators[i].gameObject);
            }

            vertexIndicators = new VertexIndicator[0];
        }

        public Vector3[] verticesDebug;

        void Update()
        {
            if (!setupCalled) return;

            lastUpdateTime = Time.time;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            if(currentEditTool) currentEditTool.UpdateWhenActive();

            stopwatch.Stop();

            updateFPSForDebug = 1 / stopwatch.Elapsed.TotalSeconds;
        }

        public void SetIndicatorsFromMesh()
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

                    if(currentInteractor == null)
                    {
                        Debug.LogWarning($"Error: {nameof(VertexIndicator)} was not found on instantiated object");
                        GameObject.Destroy(newObject);
                        continue;
                    }

                    currentInteractor.Setup(i, transform, positions[i], vertexInteractionDistance);

                    vertexIndicators[i] = currentInteractor;
                }
            }
        }

        public void MoveVertexToLocalPosition(int index, Vector3 localPosition)
        {
            Transform currentVertex = vertexIndicators[index].transform;

            currentVertex.localPosition = localPosition;

            linkedMeshController.SetSingleVertexPosition(index, currentVertex.localPosition);
        }

        public bool ShowLineRenderer
        {
            set
            {
                LinkedLineRenderer.gameObject.SetActive(value: value);
            }
        }

        public void SetLocalLineRendererPositions(Vector3[] positions, bool loop)
        {
            LinkedLineRenderer.loop = loop;

            LinkedLineRenderer.positionCount = positions.Length;

            LinkedLineRenderer.SetPositions(positions);
        }

        public void UpdateMesh(bool updateInteractors)
        {
            linkedMeshController.BuildMeshFromData();
            if (updateInteractors) SetIndicatorsFromMesh();
        }


        public int SelectVertex()
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
            Vector3 handPosition = transform.InverseTransformPoint(InteractionPosition);

            int closestVertex = -1;
            float closestDistance = vertexInteractionDistance;

            Vector3[] vertices = linkedMeshController.Vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
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

            if (!currentEditTool) return;
        }

        void DropInput(HandType handType, bool activeInput)
        {
            #if debugLog
            Debug.Log("Drop input");
            #endif

            if (handType != primaryHand) return; //Currently only one handed
        }

        void UseInput(bool value, HandType handType)
        {
            #if debugLog
            Debug.Log("Use input");
            #endif

            if (handType != primaryHand) return; //Currently only one handed

            if (value)
            {
                
            }
        }

        //VRChat input functions
        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (overUIElement) return;

            if (!currentEditTool) return;

            if (args.handType != primaryHand) return; //Currently only one handed

            if (!inputDropWorks)
            {
                if (value)
                {
                    currentEditTool.OnPickupUse();
                }
                else
                {
                    currentEditTool.OnDropUse();
                }
            }
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (overUIElement) return;

            if (!currentEditTool) return;

            if (args.handType != primaryHand) return; //Currently only one handed

            if (!inputDropWorks)
            {
                if (value)
                {
                    currentEditTool.OnUseDown();
                }
                else
                {
                    currentEditTool.OnUseUp();
                }
            }
            else
            {
                if (currentEditTool.CallUseInsteadOfPickup)
                {
                    if (value)
                    {
                        currentEditTool.OnUseDown();
                    }
                    else
                    {
                        currentEditTool.OnUseUp();
                    }
                }
                else
                {
                    if (value)
                    {
                        currentEditTool.OnPickupUse();
                    }
                    else
                    {
                        currentEditTool.OnDropUse();
                    }
                }
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            inputDropWorks = true;

            if (overUIElement) return;

            if (!currentEditTool) return;

            if (args.handType != primaryHand) return; //Currently only one handed

            if (value)
            {
                if (currentEditTool.CallUseInsteadOfPickup)
                {
                    currentEditTool.OnPickupUse();
                }
                else
                {
                    currentEditTool.OnDropUse();
                }
            }
            else
            {
                currentEditTool.OnDropUse();
            }
        }
    }
}