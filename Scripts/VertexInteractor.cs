
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VertexInteractor : UdonSharpBehaviour
{
    public int index;
    MeshBuilder linkedMeshBuilder;
    Collider attachedCollider;

    public void Setup(int index, Transform parent, Vector3 localPosition, MeshBuilder linkedMeshBuilder)
    {
        this.index = index;
        transform.parent = parent;
        transform.localPosition = localPosition;
        gameObject.SetActive(true);
        this.linkedMeshBuilder = linkedMeshBuilder;
        attachedCollider = transform.GetComponent<Collider>();
        transform.localScale = linkedMeshBuilder.VertexInteractorScale * Vector3.one;
    }

    public bool ColliderState
    {
        set
        {
            attachedCollider.enabled = value;
        }
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
