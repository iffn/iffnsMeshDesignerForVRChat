
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class DefaultShapeProvider : UdonSharpBehaviour
{
    [SerializeField] ObjConterter LinkedObjConverter;

    [SerializeField, TextArea(1, 10)] string Quad;
    [SerializeField, TextArea(1, 10)] string CubeUnmerged;
    [SerializeField, TextArea(1, 10)] string CubeMerged;
    [SerializeField, TextArea(1, 10)] string BreakTest;

    public void ImportQuad()
    {
        LinkedObjConverter.ImportObj(Quad);
    }

    public void ImportCubeUnmerged()
    {
        LinkedObjConverter.ImportObj(CubeUnmerged);
    }

    public void ImportCubeMerged()
    {
        LinkedObjConverter.ImportObj(CubeMerged);
    }

    public void ImportBreakTest()
    {
        LinkedObjConverter.ImportObj(BreakTest);
    }
}
