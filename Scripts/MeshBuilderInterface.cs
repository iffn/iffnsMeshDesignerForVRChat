
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
    [SerializeField] TMPro.TextMeshProUGUI debugText;
    [SerializeField] InteractorController LinkedInteractorController;

    MeshInteractor linkedMeshInteractor;

    bool isInVR;
    bool setupCalled = false;

    public void Setup(MeshInteractor linkedMeshInteractor)
    {
        setupCalled = true;

        isInVR = Networking.LocalPlayer.IsUserInVR();

        if (!isInVR)
        {
            foreach(GameObject obj in VROnlyObjects)
            {
                obj.SetActive(false);
            }
        }

        this.linkedMeshInteractor = linkedMeshInteractor;

        LinkedObjConverter.Setup(this.linkedMeshInteractor);
    }

    private void Update()
    {
        if (!setupCalled) return;

        string debugString = "";

        if (linkedMeshInteractor && linkedMeshInteractor.LinkedMeshController)
        {
            debugString += linkedMeshInteractor.DebugState();
            debugString += "\n";
            debugString += linkedMeshInteractor.LinkedMeshController.DebugState();
        }
        else
        {
            debugString = $"Setup not completed at {Time.time}:\n";

            debugString += $"{nameof(linkedMeshInteractor)} is null: {linkedMeshInteractor == null}\n";
            if(linkedMeshInteractor) debugString += $"{nameof(linkedMeshInteractor.LinkedMeshController)} is null: {linkedMeshInteractor.LinkedMeshController == null}\n";
        }

        debugString += LinkedInteractorController.MultiLineDebugState();

        debugText.text = debugString;
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

    public void ToggleEditMesh()
    {
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

        linkedMeshInteractor.InEditMode = EditMeshToggle.isOn;
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
        linkedMeshInteractor.interactionDistance = interactionDistanceSlider.value;
    }

    public void InderactorSizeX1o25()
    {
        linkedMeshInteractor.VertexInteractionDistance *= 1.25f;
    }

    public void InderactorSizeX0o8()
    {
        linkedMeshInteractor.VertexInteractionDistance *= 0.8f;
    }

}
