
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VertexInteractor : UdonSharpBehaviour
{
    public int index;
    MeshBuilder linkedMeshBuilder;

    public void Setup(int index, Transform parent, Vector3 localPosition, MeshBuilder linkedMeshBuilder)
    {
        this.index = index;
        transform.parent = parent;
        transform.localPosition = localPosition;
        gameObject.SetActive(true);
        this.linkedMeshBuilder = linkedMeshBuilder;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public override void Interact()
    {
        linkedMeshBuilder.InteractWithVertex(this);
    }
}
