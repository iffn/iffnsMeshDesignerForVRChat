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
        #region InspectorVariables
        [Header("Settings")]
        [SerializeField] bool mirrorActive = true;
        [SerializeField] float mirrorSnap = 0.01f;
        [SerializeField] float maxDesktopInteractionDistance = 1.5f;
        [SerializeField] Vector3 InteractorOffsetVector = Vector3.down;
        [SerializeField] int maxIndicators = 100;

        [Header("Unity assingments")]
        [SerializeField] MeshController linkedMeshController;
        [SerializeField] VertexIndicator VertexInteractorPrefab;
        [SerializeField] LineRenderer LinkedLineRenderer;
        [SerializeField] Transform HelperTransform;
        [SerializeField] MeshRenderer LinkedMeshRenderer;
        [SerializeField] MeshRenderer SymmetryMeshRenderer;
        public MeshFilter ReferenceMesh;
        public GameObject MirrorReferenceMesh;
        public Scaler LinkedScaler;
        #endregion

        #region General variables
        public bool OverUIElement { get; set; } = false; //Property since the Unity Inspector can otherwise assign a wrong variable
        InteractorController linkedInteractionController;
        VertexIndicator[] vertexIndicators = new VertexIndicator[0];
        bool isInVR;
        bool inputDropWorks = false;
        VRCPlayerApi localPlayer;
        public bool setupCalled = false;
        float currentDesktopPickupDistance = 0.5f;
        #endregion

        #region Debug
        double updateFPSForDebug = 0;
        public float LastUpdateTime { get; private set; } = Mathf.NegativeInfinity;
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

                if (mirrorActive && Mathf.Abs(returnValue.x) < vertexInteractionDistance)
                {
                    returnValue.x = 0;
                }

                return returnValue;
            }
        }
        public string DebugState()
        {
            string returnString = "";

            returnString += $"Debug output of {nameof(MeshInteractor)} at {Time.time}:\n";
            returnString += $"{nameof(LastUpdateTime)}: {LastUpdateTime}\n";
            returnString += $"{nameof(CurrentEditTool)}: {((CurrentEditTool != null) ? CurrentEditTool.name : "No tool selected")}\n";
            returnString += $"{nameof(inputDropWorks)}: {inputDropWorks}\n";
            returnString += $"Number of interactors: {vertexIndicators.Length}\n";
            returnString += $"{nameof(primaryHand)}: {primaryHand}\n";
            returnString += $"{nameof(updateFPSForDebug)}: {updateFPSForDebug:0}\n";

            return returnString;
        }
        #endregion

        # region Main funcitons
        public void Setup(InteractorController linkedInteractionIndicator)
        {
            setupCalled = true;

            this.linkedInteractionController = linkedInteractionIndicator;

            LastUpdateTime = Time.time;

            linkedMeshController.Setup();
            
            localPlayer = Networking.LocalPlayer;
            isInVR = localPlayer.IsUserInVR();

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

            //Setup indicators
            if(vertexIndicators == null || vertexIndicators.Length == 0)
            {
                vertexIndicators = new VertexIndicator[maxIndicators];

                for (int i = 0; i < maxIndicators; i++)
                {
                    GameObject newObject = Instantiate(VertexInteractorPrefab.gameObject);

                    newObject.SetActive(false);

                    VertexIndicator indicator = newObject.GetComponent<VertexIndicator>();

                    indicator.Setup(i, transform, vertexInteractionDistance);

                    vertexIndicators[i] = indicator;
                }
            }

            SetIndicatorsFromMesh();
        }

        void Update()
        {
            if (!setupCalled) return;

            LastUpdateTime = Time.time;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            if (currentEditTool) currentEditTool.UpdateWhenActive();

            stopwatch.Stop();

            updateFPSForDebug = 1 / stopwatch.Elapsed.TotalSeconds;
        }
        #endregion

        # region Tools
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
                    foreach (VertexIndicator interactor in vertexIndicators)
                    {
                        interactor.gameObject.SetActive(false);
                    }
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
        #endregion

        # region Information access for tools
        public MeshController LinkedMeshController
        {
            get
            {
                return linkedMeshController;
            }
        }

        public float PlayerHeight
        {
            get
            {
                return (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - localPlayer.GetPosition()).magnitude;
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

        public Vector3 LocalHeadPosition
        {
            get
            {
                return transform.InverseTransformPoint(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
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

                foreach (VertexIndicator vertex in vertexIndicators)
                {
                    vertex.transform.localScale = scale;
                }

                vertexInteractionDistance = value;
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

                        return hand.position + vertexInteractionDistance * (hand.rotation * InteractorOffsetVector.normalized);
                    }
                    else
                    {
                        VRCPlayerApi.TrackingData hand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

                        return hand.position + vertexInteractionDistance * (hand.rotation * InteractorOffsetVector.normalized);
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
        #endregion

        # region Visual functions for tools
        public bool ShowLineRenderer
        {
            set
            {
                LinkedLineRenderer.gameObject.SetActive(value: value);
            }
        }
        public void SetVertexIndicatorState(int index, VertexSelectStates state)
        {
            if (index >= vertexIndicators.Length) return;

            vertexIndicators[index].SelectState = state;
        }
        public void SetLocalLineRendererPositions(Vector3[] positions, bool loop)
        {
            LinkedLineRenderer.loop = loop;

            LinkedLineRenderer.positionCount = positions.Length;

            LinkedLineRenderer.SetPositions(positions);
        }
        #endregion

        #region Edit functions for tools
        public void MoveVertexToLocalPosition(int index, Vector3 localPosition)
        {
            linkedMeshController.SetSingleVertexPosition(index, localPosition);

            if(index < vertexIndicators.Length)
            {
                vertexIndicators[index].transform.localPosition = localPosition;
            }
        }
        #endregion

        #region Managing functions
        public void UpdateMesh(bool updateInteractors)
        {
            linkedMeshController.BuildMeshFromData();
            if (updateInteractors) SetIndicatorsFromMesh();
        }
        public void SetIndicatorsFromMesh()
        {
            if (!InEditMode) return;

            Vector3[] vertices = linkedMeshController.Vertices;

            if(vertices.Length > vertexIndicators.Length)
            {
                for(int i = 0; i< vertexIndicators.Length; i++)
                {
                    vertexIndicators[i].SetInfo(i, vertices[i]);
                    vertexIndicators[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for(int i = 0; i < vertices.Length; i++)
                {
                    vertexIndicators[i].SetInfo(i, vertices[i]);
                    vertexIndicators[i].gameObject.SetActive(true);
                }

                for(int i = vertices.Length; i<vertexIndicators.Length; i++)
                {
                    vertexIndicators[i].gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Else
        public MeshFilter SymmetryMeshFilter
        {
            get
            {
                return SymmetryMeshRenderer.transform.GetComponent<MeshFilter>();
            }
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
        #endregion

        #region VRChat input functions
        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (OverUIElement) return;

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
            if (OverUIElement) return;

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

            if (OverUIElement) return;

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
        #endregion
    }
}