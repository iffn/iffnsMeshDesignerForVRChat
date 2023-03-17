using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MeshConverterController : UdonSharpBehaviour
    {
        [SerializeField] Toggle ShowReferenceMeshTroggle;
        [SerializeField] Toggle MirrorReferenceMeshTroggle;
        [SerializeField] InputField LinkedInputField;
        [SerializeField] Dropdown LinkedControllerSelectorDropdown;
        [SerializeField] Transform SpecificConverterUIHolder;
        [SerializeField] ObjConterter LinkedObjConverter;
        [SerializeField] BaseMeshConverter[] LinkedImporters;
        [SerializeField] MeshController LinkedMeshController;

        void Setup()
        {
            /*
            LinkedControllerSelectorDropdown.ClearOptions();

            string[] options = new string[LinkedImporters.Length];

            for(int i = 0; i < options.Length; i++)
            {
                options[i] = LinkedImporters[i].Title;
            }

            LinkedControllerSelectorDropdown.AddOptions(options);
            */
        }

        public void SetInputText(string text)
        {

        }

        public void ImportObj(string objText)
        {
            bool worked = LinkedObjConverter.ImportMeshIfValidAndSaveData(objText);

            if (!worked) return;

            LinkedMeshController.SetData(LinkedObjConverter.VerticesFromLastImport, LinkedObjConverter.TrianglesFromLastImport);
        }

        //VRChat funcitons
        public void ClearInputField()
        {
            LinkedInputField.text = "";
        }

        public void ExportData()
        {

        }

        public void ImportData()
        {

        }

        public void JoinOverlappingVertices()
        {

        }

        public void ImportDataForReference()
        {

        }

        public void ToggleUpdate()
        {

        }

        public void InputFieldUpdated()
        {

        }
    }
}