// CinemachineFollowPlayer.cs
using UnityEngine;
using Cinemachine;

public class CinemachineFollowPlayer : MonoBehaviour
{
    [Tooltip("Arrastra aquí tu Virtual Camera de Cinemachine")]
    public CinemachineVirtualCamera virtualCamera;

    [Tooltip("Tag que debe tener el prefab de Player (p. ej. \"Player\").")]
    public string playerTag = "Player";

    private bool isFollowing = false;

    void Update()
    {
        if (isFollowing || virtualCamera == null) return;

        // Intentamos encontrar al jugador por su tag
        GameObject playerGO = GameObject.FindWithTag(playerTag);
        if (playerGO != null)
        {
            Transform playerTransform = playerGO.transform;

            // Asignamos el Follow y LookAt al jugador
            virtualCamera.Follow = playerTransform;
            virtualCamera.LookAt = playerTransform;

            // Posicionar la cámara inmediatamente en la posición del jugador
            Vector3 snapPos = playerTransform.position;
            snapPos.z = virtualCamera.transform.position.z;
            virtualCamera.transform.position = snapPos;

            isFollowing = true;
        }
    }
}
