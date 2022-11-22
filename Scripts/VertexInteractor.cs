
using System;
using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.SDKBase.Midi;
using VRC.Udon;

[RequireComponent(typeof(VRCPickup))]
public class VertexInteractor : UdonSharpBehaviour
{
    MeshBuilder linkedMeshBuilder;
    VRCPickup attachedPickup;

    public void Setup(MeshBuilder linkedMeshBuilder)
    {
        gameObject.SetActive(true);
        this.linkedMeshBuilder = linkedMeshBuilder;
        attachedPickup = transform.GetComponent<VRCPickup>();
        gameObject.SetActive(false);
    }

    public bool IsHeld
    {
        get
        {
            return attachedPickup.IsHeld;
        }
    }

    public VRCPickup.PickupHand CurrentHand
    {
        get
        {
            return attachedPickup.currentHand;
        }
    }

    public void ForceDropAndDeactivate()
    {
        attachedPickup.Drop();
        gameObject.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public override void OnPickup()
    {
        
    }

    public override void OnDrop()
    {
        gameObject.SetActive(false);
    }

    public override void OnPickupUseDown()
    {
        linkedMeshBuilder.TryToMergeVertex(this);
    }
}
