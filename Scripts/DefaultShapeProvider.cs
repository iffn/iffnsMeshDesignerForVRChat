using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshDesigner
{
    public class DefaultShapeProvider : UdonSharpBehaviour
    {
        [SerializeField] MeshConverterController LinkedMeshConverterController;
        [SerializeField] BaseMeshConverter LinkedConverter;

        [SerializeField, TextArea(1, 10)] string ImportText;

        public void Import()
        {
            LinkedMeshConverterController.ImportData(ImportText, LinkedConverter);
        }
    }
}