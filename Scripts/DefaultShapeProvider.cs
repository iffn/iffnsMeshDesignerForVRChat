
using iffnsStuff.iffnsVRCStuff.MeshBuilder;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class DefaultShapeProvider : UdonSharpBehaviour
{
    [SerializeField] MeshConverterController LinkedMeshConverterController;

    [SerializeField, TextArea(1, 10)] string Quad;
    [SerializeField, TextArea(1, 10)] string CubeUnmerged;
    [SerializeField, TextArea(1, 10)] string CubeMerged;
    [SerializeField, TextArea(1, 10)] string BreakTest;

    public void ImportQuad()
    {
        LinkedMeshConverterController.ImportObj(Quad);
    }

    public void ImportCubeUnmerged()
    {
        LinkedMeshConverterController.ImportObj(CubeUnmerged);
    }

    public void ImportCubeMerged()
    {
        LinkedMeshConverterController.ImportObj(CubeMerged);
    }

    public void ImportBreakTest()
    {
        LinkedMeshConverterController.ImportObj(BreakTest);
    }
}
