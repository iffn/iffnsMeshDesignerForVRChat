
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
    [SerializeField] MeshBuilder LinkedMeshBuilder;

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

        LinkedObjConverter.Setup(LinkedMeshBuilder);
    }

    private void Update()
    {
        debugText.text = LinkedMeshBuilder.DebugState();
    }

    //Toggle calls
    public void ToggleUseWireframeMaterial()
    {
        if (UseWireframeMaterialToggle.isOn)
        {
            defaultMaterial = LinkedMeshBuilder.AttachedMaterial;
            LinkedMeshBuilder.AttachedMaterial = WireframeMaterial;
        }
        else
        {
            if (defaultMaterial == null)
            {
                Debug.LogWarning("Error: Default material of mesh is not set yet. Make sure you don't start with it enabled");
            }
            else
            {
                LinkedMeshBuilder.AttachedMaterial = defaultMaterial;
            }
        }
    }

    public void ResetScale()
    {
        LinkedMeshBuilder.LinkedScaler.ResetScale();
    }

    public void MergeOverlappingVertices()
    {
        LinkedMeshBuilder.MergeOverlappingVertices();
    }

    public void ToggleEditMesh()
    {
        LinkedMeshBuilder.InEditMode = EditMeshToggle.isOn;
    }

    public void ToggleSymmetryMode()
    {
        LinkedMeshBuilder.SymmetryMode = SymmetryModeToggle.isOn;
    }

    public void ToggleShowScalingIndicator()
    {
        //ToDo
    }

    public void InderactorSizeX1o25()
    {
        LinkedMeshBuilder.VertexInteractorScale *= 1.25f;
    }

    public void InderactorSizeX0o8()
    {
        LinkedMeshBuilder.VertexInteractorScale *= 0.8f;
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
