using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransferGate : MonoBehaviour {

    public string scenePath = "";
    public Vector3 transferPotition = Vector3.zero;
    public Vector3 direction = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        AvatarComponent component = other.GetComponent<AvatarComponent>();
        if (component != null && component.objectType == CEntityType.Player && component.status != eEntityStatus.Death)
        {
            if (scenePath == "")
            {
                VRInputManager.Instance.playerComponent.movementController.StopMove();
                VRInputManager.Instance.playerComponent.navMeshAgent.Warp(transferPotition);
                VRInputManager.Instance.playerComponent.transform.rotation = Quaternion.Euler(direction);
            }
            else
            {
                VRInputManager.Instance.playerComponent.movementController.StopMove();
                var sceneLoader = SceneManager.LoadSceneAsync(scenePath);
                SceneManager.sceneLoaded += onSceneLoaded;
            }

        }
    }

    public void onSceneLoaded(Scene scene, LoadSceneMode model)
    {
        VRInputManager.Instance.playerComponent.navMeshAgent.Warp(transferPotition);
        VRInputManager.Instance.playerComponent.transform.rotation = Quaternion.Euler(direction);
        SceneManager.sceneLoaded -= onSceneLoaded;
    }
}
