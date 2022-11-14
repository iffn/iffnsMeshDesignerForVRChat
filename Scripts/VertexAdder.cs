
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(VRCPickup))]
public class VertexAdder : UdonSharpBehaviour
{
    public VRCPickup attachedPickup;
    MeshBuilder linkedMeshBuilder;

    public bool IsHeld
    {
        get
        {
            if(attachedPickup == null)
            {
                attachedPickup = GetComponent<VRCPickup>();
            }

            return attachedPickup.IsHeld;
        }
    }

    public void Setup(MeshBuilder linkedMeshBuilder)
    {
        attachedPickup = transform.GetComponent<VRCPickup>();

        this.linkedMeshBuilder = linkedMeshBuilder;
    }

    public void UpdateIdlePosition()
    {
        if (!IsHeld)
        {
            VRCPlayerApi.TrackingData head = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            transform.position = head.position + head.rotation * new Vector3(-0.2f, -0.1f, 0);
        }
    }

    public void PickMeUp()
    {
        //Interact();
    }

    void Start()
    {
        //Use setup instead
    }

    private void Update()
    {
        
    }

    public override void OnPickup()
    {
        linkedMeshBuilder.PickupVertexAdder();
    }

    public override void OnDrop()
    {
        linkedMeshBuilder.DropVertexAdder();
    }

    public override void OnPickupUseDown()
    {
        linkedMeshBuilder.UseVertexAdder();
    }

    public void ForceDropIfHeld()
    {
        if(attachedPickup == null)
        {
            attachedPickup = transform.GetComponent<VRCPickup>();
            Debug.LogWarning("Pickup somehow not attached");
        }

        attachedPickup.Drop();
    }
}
