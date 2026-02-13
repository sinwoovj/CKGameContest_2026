using UnityEngine;

namespace Shurub
{
    public class Portal : MonoBehaviour
    {
        public enum Direction
        {
            None,
            Horizontal,
            Vertical
        }

        [SerializeField] private Direction direction;
        [SerializeField] private Vector3 warpPosition;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (warpPosition == null)
            {
                return;
            }

            if (collision.collider.TryGetComponentInChildren(out PlayerController playerController))
            {
                if (direction == Direction.None)
                {
                    playerController.WarpTo(warpPosition);
                    return;
                }

                if (direction == Direction.Horizontal)
                {
                    playerController.WarpTo(new Vector3(playerController.transform.position.x, warpPosition.y, 0f));
                    return;
                }

                if (direction == Direction.Vertical)
                {
                    playerController.WarpTo(new Vector3(warpPosition.x, playerController.transform.position.y, 0f));
                    return;
                }
            }
        }
    }
}
