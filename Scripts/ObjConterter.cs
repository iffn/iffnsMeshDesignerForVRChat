
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ObjConterter : UdonSharpBehaviour
{
    [SerializeField] InputField LinkedInputField;
    [SerializeField] MeshBuilder LinkedMeshBuilder;

    private void Start()
    {
        //LinkedInputField.characterLimit = 0;
    }

    public void ImportObj()
    {
        Debug.Log("Import with limit set to " + LinkedInputField.characterLimit);

        LinkedMeshBuilder.ObjString = LinkedInputField.text;
    }

    public void ExportObj()
    {
        Debug.Log("Export");
        LinkedInputField.text = LinkedMeshBuilder.ObjString;
    }
}
