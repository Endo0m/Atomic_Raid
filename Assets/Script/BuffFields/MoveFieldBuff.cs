using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFieldBuff : MonoBehaviour
{
    [SerializeField] private Vector3 _direction;

    [SerializeField] private int _speed;    

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        if(transform.position.z < -5f )
        {
            Destroy(gameObject);
        }
    }
}
