
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using iffnsStuff.iffnsVRCStuff.MeshBuilder;

public class MeshBuilderInterface : UdonSharpBehaviour
{
    //Unity setup
    [Header("Unity assingments")]
    [SerializeField] Toggle UseWireframeMaterialToggle;
    [SerializeField] Toggle EditMeshToggle;
    [SerializeField] Toggle SymmetryModeToggle;
    [SerializeField] Toggle ShowInteractionLocationToggle;
    [SerializeField] Toggle ShowScalingIndicatorToggle;
    [SerializeField] Slider interactionDistanceSlider;
    [SerializeField] Material WireframeMaterial;
    [SerializeField] Material NonWireframeMaterial;
    [SerializeField] ObjConterter LinkedObjConverter;
    [SerializeField] GameObject[] VROnlyObjects;
    [SerializeField] GameObject RequestOwnershipButton;
    [SerializeField] TMPro.TextMeshProUGUI debugText;
    [SerializeField] InteractorController LinkedInteractorController;

    MeshInteractor linkedMeshInteractor;
    MeshSyncController linkedSyncController;

    bool isInVR;
    bool setupCalled = false;

    public void Setup(MeshInteractor linkedMeshInteractor, MeshSyncController linkedSyncController)
    {
        this.linkedMeshInteractor = linkedMeshInteractor;
        this.linkedSyncController = linkedSyncController;

        RequestOwnershipButton.SetActive(linkedSyncController);

        setupCalled = true;

        isInVR = Networking.LocalPlayer.IsUserInVR();

        if (!isInVR)
        {
            foreach(GameObject obj in VROnlyObjects)
            {
                obj.SetActive(false);
            }
        }


        LinkedObjConverter.Setup(this.linkedMeshInteractor, linkedSyncController);
    }

    private void Update()
    {
        if (!setupCalled) return;

        string debugString = "";

        if (linkedMeshInteractor) debugString += linkedMeshInteractor.DebugState() + "\n";
        else debugString += $"{nameof(linkedMeshInteractor)} = null\n";

        if (linkedMeshInteractor && linkedMeshInteractor.LinkedMeshController) debugString += linkedMeshInteractor.LinkedMeshController.DebugState() + "\n";
        else debugString += $"{nameof(linkedMeshInteractor.LinkedMeshController)} = null\n";

        if (linkedSyncController) debugString += linkedSyncController.DebugState() + "\n";
        else debugString += $"{nameof(linkedSyncController)} = null\n";

        debugString += $"{LinkedInteractorController.MultiLineDebugState()}\n";

        debugText.text = debugString;
    }

    public bool Ownership
    {
        set
        {
            RequestOwnershipButton.SetActive(!value);

            if (!value)
            {
                InEditMode = false;
            }
        }
    }

    public bool InEditMode
    {
        set
        {
            EditMeshToggle.isOn = value; //Rest should be called automatically
        }
    }

    //Toggle calls
    public void ToggleUseWireframeMaterial()
    {
        if (UseWireframeMaterialToggle.isOn)
        {
            linkedMeshInteractor.AttachedMaterial = WireframeMaterial;
        }
        else
        {
            linkedMeshInteractor.AttachedMaterial = NonWireframeMaterial;
        }
    }

    public void ResetScale()
    {
        linkedMeshInteractor.LinkedScaler.ResetScale();
    }

    public void MergeOverlappingVertices()
    {
        linkedMeshInteractor.LinkedMeshController.MergeOverlappingVertices(0.001f);
        linkedMeshInteractor.UpdateMesh(true);
    }

    bool skipEvent = false;

    public void ToggleEditMesh()
    {
        if (skipEvent) return;

        if(linkedMeshInteractor == null)
        {
            Debug.LogWarning("Error: LinkedMeshBuilder is null");
            return;
        }

        if(EditMeshToggle == null)
        {
            Debug.LogWarning("Error: EditMeshToggle is null");
            return;
        }

        if (EditMeshToggle.isOn)
        {
            if (!linkedSyncController || linkedSyncController.IsOwner)
            {
                linkedMeshInteractor.InEditMode = true;
            }
            else
            {
                skipEvent = true;
                EditMeshToggle.isOn = false;
                skipEvent = false;
            }
        }
        else
        {
            linkedMeshInteractor.InEditMode = false;
        }
    }

    public void ToggleSymmetryMode()
    {
        linkedMeshInteractor.SymmetryMode = SymmetryModeToggle.isOn;
    }

    public void ToggleShowScalingIndicator()
    {
        //ToDo
    }

    public void UpdateInteractionDistance()
    {
        linkedMeshInteractor.VertexInteractionDistance = interactionDistanceSlider.value;
    }

    public void InderactorSizeX1o25()
    {
        linkedMeshInteractor.VertexInteractionDistance *= 1.25f;
    }

    public void InderactorSizeX0o8()
    {
        linkedMeshInteractor.VertexInteractionDistance *= 0.8f;
    }

    public void RequestOwnership()
    {
        if (!linkedSyncController)
        {
            RequestOwnershipButton.gameObject.SetActive(false);
            return;
        }

        linkedSyncController.RequestOwnership();
    }
}
