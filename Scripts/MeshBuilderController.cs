using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.MeshBuilder
{
    public class MeshBuilderController : UdonSharpBehaviour
    {
        /*
        ToDo:
        - Add spawn ability:
        -- Create desktop interface
        -- Create VR interface
        -- Enable editing by default

        - Implement reset mode:
        -- Design reset message on interface
        -- Link reset ability
        -- Make sure the obj converter is save from resets
        -- Test reset ability

        - Testing:
        -- Test in Desktop
        -- Test in VR
        -- Test with friends
        */

        [Header("Unity assingments")]
        [SerializeField] MeshInteractor LinkedMeshInteractor;
        [SerializeField] MeshController LinkedMeshController;
        [SerializeField] MeshController MeshControllerPrefab;
        [SerializeField] MeshBuilderInterface LinkedBuilderInterface;
        [SerializeField] InteractorController LinkeInteractorController;

        [SerializeField] GameObject WarningText;
        [SerializeField] GameObject ResetButton;
        
        GameObject newInteractorObject = null;

        bool spawned = false;
        bool IsUserInVR;
        VRCPlayerApi player;

        public void SpawnMeshContorller()
        {
            LinkedMeshInteractor.gameObject.SetActive(true);
            LinkedBuilderInterface.gameObject.SetActive(true);

            LinkeInteractorController.SwitchToMoveText();

            SetupElements();

            MoveConttrollerToPlayer();

            LinkedBuilderInterface.InEditMode = true;

            spawned = true;
        }

        public void MoveConttrollerToPlayer()
        {
            transform.SetPositionAndRotation(player.GetPosition(), player.GetRotation());
        }

        public void ResetControllers()
        {
            WarningText.SetActive(false);
            ResetButton.SetActive(false);

            LinkedMeshInteractor.ClearVertexInteractorData();

            Transform referenceTransform = LinkedMeshInteractor.transform;

            newInteractorObject = GameObject.Instantiate(MeshControllerPrefab.gameObject, referenceTransform.position, referenceTransform.rotation, referenceTransform.parent);
            
            GameObject.Destroy(LinkedMeshInteractor.gameObject);

            LinkedMeshInteractor = newInteractorObject.GetComponent<MeshInteractor>();

            SetupElements();

            LinkedBuilderInterface.ToggleEditMesh();
            LinkeInteractorController.CurrentInteractionType = InteractionTypes.Idle;
        }

        void SetupElements()
        {
            //Null check
            if (!LinkedMeshInteractor) return;

            //Setup
            LinkedMeshController = LinkedMeshInteractor.LinkedMeshController;

            LinkedMeshInteractor.Setup(LinkeInteractorController);

            LinkedBuilderInterface.Setup(LinkedMeshInteractor);

            LinkeInteractorController.Setup(LinkedMeshInteractor);
        }

        public int state = -1;

        void CheckIfStillRunning()
        {
            state = 0;

            if (!spawned) return;
            
            state++;

            if (!LinkedMeshInteractor)
            {
                LinkedMeshInteractor = newInteractorObject.GetComponent<MeshInteractor>();

                SetupElements();
            }

            state++;

            float time = Time.time - 1;

            if (LinkedMeshInteractor.LastUpdateTime > time && LinkedMeshController.LastUpdateTime > time)
            {
                WarningText.SetActive(false);
                ResetButton.SetActive(false);

                return;
            }

            state++;

            WarningText.SetActive(true);
            ResetButton.SetActive(true);
        }

        void UpdateDesktopInputs()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!spawned)
                {
                    SpawnMeshContorller();
                }
                else
                {
                    MoveConttrollerToPlayer();
                }
            }
        }

        private void Start()
        {
            player = Networking.LocalPlayer;
            IsUserInVR = player.IsUserInVR();

            LinkeInteractorController.Setup(LinkedMeshInteractor);
        }

        private void Update()
        {
            CheckIfStillRunning();

            if(!IsUserInVR) UpdateDesktopInputs();
        }
    }
}