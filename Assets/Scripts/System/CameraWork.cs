using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Shurub
{
    public class CameraWork : MonoBehaviourPun
    {
        [SerializeField]
        private Vector3 offset = new Vector3(0f, 0f, -10f);

        [SerializeField]
        private float smoothSpeed = 5f;

        Transform cameraTransform;
        bool isFollowing;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            if (!photonView.IsMine)
                return;

            TryStartFollowing();
        }

        void LateUpdate()
        {
            if (!photonView.IsMine || !isFollowing || cameraTransform == null)
                return;

            Follow();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!photonView.IsMine)
                return;

            // ¾À ¹Ù²ð ¶§¸¶´Ù ´Ù½Ã ¿¬°á
            TryStartFollowing();
        }

        void TryStartFollowing()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            cameraTransform = cam.transform;
            isFollowing = true;
            Cut();
        }

        void Follow()
        {
            Vector3 targetPos = transform.position + offset;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPos,
                smoothSpeed * Time.deltaTime
            );
        }

        void Cut()
        {
            cameraTransform.position = transform.position + offset;
        }
    }
}