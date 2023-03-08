using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public abstract class MeshEditTool : UdonSharpBehaviour
    {
        [SerializeField] Sprite Sprite;

        public abstract bool CallUseInsteadOfPickup { get; }

        public abstract string ToolName { get; }

        public Sprite LinkedSprite
        {
            get
            {
                return Sprite;
            }
        }

        public bool isInVR { get; private set; }
        public MeshInteractor LinkedMeshInteractor { get; private set; }
        public MeshController LinkedMeshController { get; private set; }

        public virtual void Setup(MeshInteractor linkedMeshInteractor)
        {
            LinkedMeshInteractor = linkedMeshInteractor;
            LinkedMeshController = linkedMeshInteractor.LinkedMeshController;
            isInVR = Networking.LocalPlayer.IsUserInVR();
        }

        public abstract void UpdateWhenActive();

        public abstract void OnActivation();
        public abstract void OnDeactivation();

        //public abstract void InteractWithVertex(int vertex);
        
        public virtual string MultiLineDebugState()
        {
            string returnString = $"Debug of {ToolName} at {Time.time}:\n";

            returnString += $"{nameof(CallUseInsteadOfPickup)} = {CallUseInsteadOfPickup}\n";

            return returnString;
        }

        public virtual void OnPickupUse()
        {

        }

        public virtual void OnDropUse()
        {

        }

        public virtual void OnUseDown()
        {

        }

        public virtual void OnUseUp()
        {

        }
    }

    public enum TriggerFunction
    {
        Grab,
        Select,
        Inverted
    }
}