using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public Sprite[] cloudSprites;
    private float _speed;
    private float _endPosX;

    private void Start()
    {
        if (cloudSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, cloudSprites.Length);
            GetComponent<SpriteRenderer>().sprite = cloudSprites[randomIndex];
        } else {
            Debug.LogWarning("No cloud sprites in the sprite array");
        }
    }


    public void StartFloating(float speed, float endPosX)
    {
        _speed = speed;
        _endPosX = endPosX;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime * _speed);
        
        if(transform.position.x > _endPosX)
        {
            Destroy(gameObject);
        }
    }
}
