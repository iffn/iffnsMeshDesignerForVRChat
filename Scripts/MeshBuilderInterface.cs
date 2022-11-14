
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;

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

    [SerializeField] Transform LeftHandInteractionIndicator;
    [SerializeField] Transform RightHandInteractionIndicator;

    Material defaultMaterial;

    bool isInVR;

    void Start()
    {
        bool correctSetup = true;

        if (LeftHandInteractionIndicator == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(LeftHandInteractionIndicator)} not assinged");
        }
        if (RightHandInteractionIndicator == null)
        {
            correctSetup = false;
            Debug.LogWarning($"Error: {nameof(RightHandInteractionIndicator)} not assinged");
        }

        RightHandInteractionIndicator.gameObject.SetActive(isInVR && ShowInteractionLocationToggle.isOn);
        LeftHandInteractionIndicator.gameObject.SetActive(isInVR && ShowInteractionLocationToggle.isOn);

        ToggleEditMesh();
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

    public void ToggleEditMesh()
    {
        LinkedMeshBuilder.InEditMode = EditMeshToggle.isOn;
    }

    public void ToggleSymmetryMode()
    {
        LinkedMeshBuilder.SymmetryMode = SymmetryModeToggle.isOn;
    }

    public void ToggleShowInteractionLocation()
    {
        RightHandInteractionIndicator.gameObject.SetActive(isInVR && ShowInteractionLocationToggle.isOn);
        LeftHandInteractionIndicator.gameObject.SetActive(isInVR && ShowInteractionLocationToggle.isOn);
    }

    public void JoinOverlappingVertices()
    {
        LinkedMeshBuilder.MergeOverlappingVertices();
    }

    public void ToggleShowScalingIndicator()
    {
        //ToDo
    }

    public void InderactorSizeX1o25()
    {
        LinkedMeshBuilder.vertexInteractorScale *= 1.25f;
    }

    public void InderactorSizeX0o8()
    {
        LinkedMeshBuilder.vertexInteractorScale *= 0.8f;
    }
}
