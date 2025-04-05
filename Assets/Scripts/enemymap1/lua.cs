using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lua : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject,3f);
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(-5f, rb.linearVelocity.y);
    }
}
