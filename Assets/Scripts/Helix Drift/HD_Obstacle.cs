using UnityEngine;

public class HD_Obstacle : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform rotateTransform;
    
    private Quaternion initialRotation;

    private void Start()
    {
        if (rotateTransform != null)
            initialRotation = rotateTransform.localRotation;
        else
            initialRotation = transform.localRotation;
    }

    void FixedUpdate()
    {   
        if (HD_GameManager.Instance == null || !HD_GameManager.Instance.isGamePlaying) return;
        
        if (rotateTransform != null)
            rotateTransform.Rotate(0, 0, speed * Time.fixedDeltaTime);
        else
            transform.Rotate(0, 0, speed * Time.fixedDeltaTime);
    }
    
    public void ResetObstacle()
    {
        // Debug.Log($"Resetting obstacle: {gameObject.name}");
        
        if (rotateTransform != null)
            rotateTransform.localRotation = initialRotation;
        else
            transform.localRotation = initialRotation;
    }
}