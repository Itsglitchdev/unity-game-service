using UnityEngine;
using System.Collections.Generic;

public class HD_Coin : MonoBehaviour
{
    [SerializeField] private Transform _centerTranform;
    [SerializeField] private List<float> _spawnPosX = new List<float>() { -2.45f, -1.75f, -1.05f };

    private void Awake()
    {
        transform.localPosition = Vector3.right * _spawnPosX[Random.Range(0,_spawnPosX.Count)];
        _centerTranform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 37) * 10f);
    }

}
