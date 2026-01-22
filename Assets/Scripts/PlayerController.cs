using UnityEngine;


namespace PhotonTest
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private PhotonTest.Player player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            PlayerMove();
            KeyInput();
        }

        void KeyInput()
        {
            if (Input.GetKeyDown(KeyCode.J)) Interaction();
            if (Input.GetKeyDown(KeyCode.K)) ThrowOrPickUp();
        }

        void PlayerMove()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

        }

        void Interaction()
        {
        
        }

        void ThrowOrPickUp()
        {

        }
    }

}
