#define debugLog

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
        + Add spawn ability:
        ++ Create desktop interface
        ++ Create VR interface
        ++ Enable editing by default

        - Implement reset mode:
        ++ Design reset message on interface
        ++ Link reset ability
        ++ Make sure the obj converter is save from resets
        ++ Test reset ability

        - Testing:
        -- Test in Desktop
        -- Test in VR
        -- Test with friends
        */

        [Header("Unity assingments")]
        [SerializeField] MeshInteractor LinkedMeshInteractor;
        [SerializeField] MeshController LinkedMeshController;
        [SerializeField] MeshInteractor MeshControllerPrefab;
        [SerializeField] MeshBuilderInterface LinkedBuilderInterface;
        [SerializeField] InteractorController LinkedInteractorController;
        [SerializeField] GameObject[] VROnlyObjects;
        [SerializeField] GameObject[] DesktopOnlyObjects;

        [SerializeField] GameObject WarningText;
        [SerializeField] GameObject ResetButton;
        
        GameObject newInteractorObject = null;

        bool spawned = false;
        bool IsUserInVR;
        VRCPlayerApi player;

        public void SpawnMeshContorller()
        {
            LinkedInteractorController.SwitchToMoveText();

            LinkedMeshInteractor.gameObject.SetActive(true);
            LinkedBuilderInterface.gameObject.SetActive(true);

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

            newInteractorObject.SetActive(true);

            GameObject.Destroy(LinkedMeshInteractor.gameObject);

            LinkedMeshInteractor = newInteractorObject.GetComponent<MeshInteractor>();

            SetupElements();

            LinkedBuilderInterface.ToggleEditMesh();
            LinkedInteractorController.CurrentInteractionType = InteractionTypes.Idle;
        }

        void SetupElements()
        {
            //Null check
            if (!LinkedMeshInteractor)
            {
                Debug.LogWarning($"Error: {nameof(LinkedMeshInteractor)} is somehow null");

                return;
            }

            //Setup
            LinkedMeshController = LinkedMeshInteractor.LinkedMeshController;
            
            LinkedMeshInteractor.Setup(LinkedInteractorController);

            LinkedBuilderInterface.Setup(LinkedMeshInteractor);

            LinkedInteractorController.Setup(LinkedMeshInteractor);
        }

        void CheckIfStillRunning()
        {
            if (!spawned) return;
            
            if (!LinkedMeshInteractor)
            {
                LinkedMeshInteractor = newInteractorObject.GetComponent<MeshInteractor>();

                SetupElements();
            }

            float time = Time.time - 1;

            if (LinkedMeshInteractor.LastUpdateTime > time && LinkedMeshController.LastUpdateTime > time)
            {
                WarningText.SetActive(false);
                ResetButton.SetActive(false);

                return;
            }

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

            LinkedInteractorController.Setup(LinkedMeshInteractor);

            SetupElements();

            LinkedMeshInteractor.gameObject.SetActive(false);
            LinkedBuilderInterface.gameObject.SetActive(false);

            foreach(GameObject obj in VROnlyObjects)
            {
                obj.SetActive(IsUserInVR);
            }

            foreach (GameObject obj in DesktopOnlyObjects)
            {
                obj.SetActive(!IsUserInVR);
            }
        }

        private void Update()
        {
            CheckIfStillRunning();

            if(!IsUserInVR) UpdateDesktopInputs();

            //Debug.Log("Main controller still running");
        }
    }
}