using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f;

    private float direction = 1f; // Default direction is right

    void Update()
    {
        // Move the bullet horizontally
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);

        // Destroy the bullet if it goes off-screen
        if (!GetComponent<Renderer>().isVisible)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
     public void SetDirection(float newDirection)
    {
        direction = newDirection;
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * newDirection;
        transform.localScale = newScale;
    }

} 