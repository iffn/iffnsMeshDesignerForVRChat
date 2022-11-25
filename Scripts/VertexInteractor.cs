
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VertexInteractor : UdonSharpBehaviour
{
    MeshBuilder linkedMeshBuilder;
    Collider attachedCollider;
    MeshRenderer attachedRenderer;

    [SerializeField] Material defaultColor;
    [SerializeField] Material interactColor;
    [SerializeField] Material removeColor;

    VertexSelectStates selectState = VertexSelectStates.Normal;

    int index;
    public int Index
    {
        get
        {
            return index;
        }
        set
        {
            index = value;
            InteractionText = value.ToString();
        }
    }

    public void SetInfo(int index, Vector3 localPosition)
    {
        this.index = index;
        transform.localPosition = localPosition;
    }

    public void Setup(int index, Transform parent, Vector3 localPosition, MeshBuilder linkedMeshBuilder)
    {
        this.Index = index;
        transform.parent = parent;
        transform.localPosition = localPosition;
        gameObject.SetActive(true);
        this.linkedMeshBuilder = linkedMeshBuilder;
        attachedCollider = transform.GetComponent<Collider>();
        transform.localScale = linkedMeshBuilder.VertexInteractorScale * Vector3.one;

        attachedRenderer = transform.GetComponent<MeshRenderer>();
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

    public VertexSelectStates SetSelectState
    {
        set
        {
            switch (selectState)
            {
                case VertexSelectStates.Normal:
                    attachedRenderer.material = defaultColor;
                    InteractionText = value.ToString();
                    break;
                case VertexSelectStates.ReadyToDelete:
                    attachedRenderer.material = removeColor;
                    InteractionText = $"Remove {index}?";
                    break;
                case VertexSelectStates.Selected:
                    attachedRenderer.material = defaultColor;
                    InteractionText = value.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public override void Interact()
    {
        linkedMeshBuilder.InteractWithVertex(this);
    }
}

public enum VertexSelectStates
{
    Normal,
    ReadyToDelete,
    Selected
}
