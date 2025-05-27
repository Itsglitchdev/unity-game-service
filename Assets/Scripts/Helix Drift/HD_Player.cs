using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HD_Player : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip moveClip, loseClip, pointClip;

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab, coinParticlePrefab;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform rotateTransform;
    [SerializeField] private float moveTime = 0.3f;

    [Header("Three Lane System")]
    [SerializeField] private List<float> lanes = new List<float> { -2.45f, -1.75f, -1.05f };

    private float currentRadius;
    private int currentLaneIndex;
    private bool canClick;
    private bool isAlive = true;
    private Coroutine laneChangeCoroutine;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = rotateTransform.localRotation;
        
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        Debug.Log("Resetting Player");
        
        if (laneChangeCoroutine != null)
        {
            StopCoroutine(laneChangeCoroutine);
            laneChangeCoroutine = null;
        }
        
        currentLaneIndex = 0; 
        currentRadius = Mathf.Abs(lanes[currentLaneIndex]); 
        transform.localPosition = Vector3.up * currentRadius;
        rotateTransform.localRotation = initialRotation;
        
        canClick = false;
        isAlive = true;
        
        // Debug.Log($"Player reset to lane {currentLaneIndex}, radius {currentRadius}");
    }

    private void Update()
    {
        if (!HD_GameManager.Instance.isGamePlaying)
        {
            canClick = false;
            return;
        }

        if (!canClick)
        {
            canClick = true;
        }

        if (canClick && isAlive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (HD_SoundManager.Instance != null)
                HD_SoundManager.Instance.PlaySound(moveClip);

            if (laneChangeCoroutine != null)
                StopCoroutine(laneChangeCoroutine);

            laneChangeCoroutine = StartCoroutine(ChangeLane());
        }
    }

    private void FixedUpdate()
    {
        if (!isAlive || !HD_GameManager.Instance.isGamePlaying) return;

        transform.localPosition = Vector3.up * currentRadius;

        float angularSpeed = currentRadius > 0 ? moveSpeed / currentRadius : 0f;
        float rotateValue = angularSpeed * Time.fixedDeltaTime * Mathf.Rad2Deg;

        rotateTransform.Rotate(0, 0, rotateValue);
    }

    private IEnumerator ChangeLane()
    {
        canClick = false;

        int nextLaneIndex = (currentLaneIndex + 1) % lanes.Count;
        float startRadius = currentRadius;
        float endRadius = Mathf.Abs(lanes[nextLaneIndex]);

        float timeElapsed = 0f;

        while (timeElapsed < moveTime)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / moveTime;
            currentRadius = Mathf.Lerp(startRadius, endRadius, t);
            yield return null;
        }

        currentLaneIndex = nextLaneIndex;
        currentRadius = endRadius;
        canClick = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        if (other.CompareTag("Obstacle"))
        {
            Die();
        }
        else if (other.CompareTag("Coin"))
        {
            CollectCoin(other.gameObject);
        }
    }

    private void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        canClick = false;

        if (HD_SoundManager.Instance != null)
            HD_SoundManager.Instance.PlaySound(loseClip);

        if (explosionPrefab != null)
        { 
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f); 
        }

        if (HD_GameManager.Instance != null)
            HD_GameManager.Instance.GameOver();
    }

    private void CollectCoin(GameObject coin)
    {
        if (HD_SoundManager.Instance != null)
            HD_SoundManager.Instance.PlaySound(pointClip);

        if (coinParticlePrefab != null)
        { 
            GameObject particle = Instantiate(coinParticlePrefab, coin.transform.position, Quaternion.identity);
            Destroy(particle, 1f);
        }

        if (HD_GameManager.Instance != null)
            HD_GameManager.Instance.AddScore(10);
        HD_GameManager.Instance.OnCoinCollected();

        Destroy(coin);
    }
}