
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
    [SerializeField] Material WireframeMaterial;
    [SerializeField] ObjConterter LinkedObjConverter;
    [SerializeField] TMPro.TextMeshProUGUI debugText;

    Material defaultMaterial;
    MeshInteractor linkedMeshInteractor;

    bool isInVR;

    public void Setup(MeshInteractor linkedMeshInteractor)
    {
        isInVR = Networking.LocalPlayer.IsUserInVR();

        this.linkedMeshInteractor = linkedMeshInteractor;

        LinkedObjConverter.Setup(this.linkedMeshInteractor);
    }

    private void Update()
    {
        string debugString = "";

        if (linkedMeshInteractor)
        {
            debugString += linkedMeshInteractor.DebugState();
            debugString += "\n";
            debugString += linkedMeshInteractor.LinkedMeshController.DebugState();
        }
        else
        {
            debugString = "Setup not completed at " + Time.time;
        }

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
            defaultMaterial = linkedMeshInteractor.AttachedMaterial;
            linkedMeshInteractor.AttachedMaterial = WireframeMaterial;
        }
        else
        {
            if (defaultMaterial == null)
            {
                Debug.LogWarning("Error: Default material of mesh is not set yet. Make sure you don't start with it enabled");
            }
            else
            {
                linkedMeshInteractor.AttachedMaterial = defaultMaterial;
            }
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

    public void InderactorSizeX1o25()
    {
        linkedMeshInteractor.VertexInteractionDistance *= 1.25f;
    }

    public void InderactorSizeX0o8()
    {
        linkedMeshInteractor.VertexInteractionDistance *= 0.8f;
    }
}
