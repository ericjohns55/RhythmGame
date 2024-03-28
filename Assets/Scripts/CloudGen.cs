using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates clouds in the background of the scene for decoration. Cloud spawn interval
/// can be changed within the editor
/// </summary>
public class CloudGen : MonoBehaviour
{
    [SerializeField] GameObject cloudPrefab;
    [SerializeField] float spawnInterval;
    [SerializeField] GameObject endPoint;
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        InvokeRepeating("SpawnCloud", 0f, spawnInterval);
    }

    void SpawnCloud()
    {
        GameObject cloud = Instantiate(cloudPrefab, transform);
        cloud.name = "cloud";

        startPos.y = Random.Range(startPos.y - 1f, startPos.y + 1f);
        float scale = Random.Range(0.8f, 1.2f);
        cloud.transform.localScale = new Vector2(scale, scale);

        cloud.transform.position = startPos;

        // Start floating movement
        cloud.GetComponent<CloudMovement>().StartFloating(Random.Range(0.5f, 1.5f), endPoint.transform.position.x);
    }
}
