using UnityEngine;

public class DestroyNote : MonoBehaviour
{

    public GameManager gameManager;
    private ScoreManager scoreManager;

    void Start() {
        scoreManager = (ScoreManager) gameManager.GetComponent("ScoreManager");
    }

    // Start is called before the first frame update
    void OnCollisionEnter2D(Collision2D collisionInfo) {
        if (collisionInfo.collider.gameObject.tag == "Note") {
            scoreManager.ResetCombo();
            gameManager.NoteDestroyed(); 
            Destroy(collisionInfo.gameObject);
        }
    }
}
