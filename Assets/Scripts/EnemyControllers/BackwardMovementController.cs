using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackwardMovementController : MonoBehaviour
{
    public float speed;
    private Rigidbody _rigidbody;

    public void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody != null)
        {
            _rigidbody.AddForce(Vector3.back * speed, ForceMode.VelocityChange);
        }
    }

    public void Update()
    {
        // If object has rigidbody then the engine moves it based using velocity 
        if (_rigidbody != null)
        {
            return;
        }

        transform.position += Vector3.back * Time.deltaTime * speed;
    }
}
