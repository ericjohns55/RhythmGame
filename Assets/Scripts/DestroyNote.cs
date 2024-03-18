using UnityEngine;

public class DestroyNote : MonoBehaviour
{
    // Start is called before the first frame update
    void OnCollisionEnter2D(Collision2D collisionInfo) {
        if (collisionInfo.collider.gameObject.tag == "Note") {
            Destroy(collisionInfo.gameObject);
        }
    }
}
