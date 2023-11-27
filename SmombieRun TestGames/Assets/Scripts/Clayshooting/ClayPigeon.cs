using System.Collections;
using UnityEngine;
using Enums.ClayshootingEnums;
using UnityEngine.Events;

public class ClayPigeon : MonoBehaviour
{
    private const float MaxAnglePos = 90f, MaxAngleNeg = -250;
    public UnityEvent OnMiss;
    // Use this for initialization
    private void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("OutOfBounds"))
        {
            OnMiss.Invoke();
            Destroy(gameObject);
        }
    }

    public void Launch(SpawnSide Side, Clayshooting BaseCallback)
    {
        Rigidbody2D ClayRigid = GetComponent<Rigidbody2D>();

        float Angle = 10;
        switch (Side)
        {
            case SpawnSide.LEFT:
                {
                    Angle = Random.Range(10, 60);
                    break;
                }
            case SpawnSide.RIGHT:
                {
                    Angle = Random.Range(-190, -250);
                    break;
                }
        }
        Vector3 AngleForce = GetVectorFromAngle(Angle);
        float Power = Random.Range(30, 45);
        ClayRigid.AddForce(Power * AngleForce, ForceMode2D.Impulse);
    }

    private Vector3 GetVectorFromAngle(float Angle)
    {
        float xcomponent = Mathf.Cos(Angle * Mathf.PI / 180);
        float ycomponent = Mathf.Sin(Angle * Mathf.PI / 180);
        return new Vector3(xcomponent, ycomponent, 0);
    }
}