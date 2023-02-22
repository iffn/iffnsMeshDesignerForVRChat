
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
    [SerializeField] MeshInteractor LinkedMeshInteractor;

    [SerializeField] Toggle UseWireframeMaterialToggle;
    [SerializeField] Toggle EditMeshToggle;
    [SerializeField] Toggle SymmetryModeToggle;
    [SerializeField] Toggle ShowInteractionLocationToggle;
    [SerializeField] Toggle ShowScalingIndicatorToggle;

    [SerializeField] Material WireframeMaterial;

    [SerializeField] ObjConterter LinkedObjConverter;

    [SerializeField] TMPro.TextMeshProUGUI debugText;

    Material defaultMaterial;

    bool isInVR;

    void Start()
    {
        bool correctSetup = true;

        if (!correctSetup)
        {
            gameObject.SetActive(false);
            return;
        }

        ToggleEditMesh();

        LinkedObjConverter.Setup(LinkedMeshInteractor);
    }

    private void Update()
    {
        string debugString = "";

        debugString += LinkedMeshInteractor.DebugState();
        debugString += "\n";
        debugString += LinkedMeshInteractor.LinkedMeshController.DebugState();

        debugText.text = debugString;
    }

    //Toggle calls
    public void ToggleUseWireframeMaterial()
    {
        if (UseWireframeMaterialToggle.isOn)
        {
            defaultMaterial = LinkedMeshInteractor.AttachedMaterial;
            LinkedMeshInteractor.AttachedMaterial = WireframeMaterial;
        }
        else
        {
            if (defaultMaterial == null)
            {
                Debug.LogWarning("Error: Default material of mesh is not set yet. Make sure you don't start with it enabled");
            }
            else
            {
                LinkedMeshInteractor.AttachedMaterial = defaultMaterial;
            }
        }
    }

    public void ResetScale()
    {
        LinkedMeshInteractor.LinkedScaler.ResetScale();
    }

    public void MergeOverlappingVertices()
    {
        LinkedMeshInteractor.LinkedMeshController.MergeOverlappingVertices(0.001f);
        LinkedMeshInteractor.UpdateMesh(true);
    }

    public void ToggleEditMesh()
    {
        if(LinkedMeshInteractor == null)
        {
            Debug.LogWarning("Error: LinkedMeshBuilder is null");
            return;
        }

        if(EditMeshToggle == null)
        {
            Debug.LogWarning("Error: EditMeshToggle is null");
            return;
        }

        LinkedMeshInteractor.InEditMode = EditMeshToggle.isOn;
    }

    public void ToggleSymmetryMode()
    {
        LinkedMeshInteractor.SymmetryMode = SymmetryModeToggle.isOn;
    }

    public void ToggleShowScalingIndicator()
    {
        //ToDo
    }

    public void InderactorSizeX1o25()
    {
        LinkedMeshInteractor.VertexInteractionDistance *= 1.25f;
    }

    public void InderactorSizeX0o8()
    {
        LinkedMeshInteractor.VertexInteractionDistance *= 0.8f;
    }

    /*
    public void IndicatorSizeX1o25()
    {
        LinkedMeshBuilder.IndicatorScale *= 1.25f;
    }

    public void IndicatorSizeX0o8()
    {
        LinkedMeshBuilder.IndicatorScale *= 0.8f;
    }
    */
}
