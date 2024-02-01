using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WhackZombie : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    private Animator Animator;
    [SerializeField]
    private float AnimationSpeed;
    [SerializeField]
    private float Uptime;

    [Header("Events")]
    public UnityEvent OnMiss;

    public bool IsUp;
    public bool CanBeHit;
    public bool GameOngoing;

    private Coroutine Up;

    public WhackZombie SetUptime(float Time)
    {
        Uptime = Time;
        return this;
    }

    public void MoveUp()
    {
        Up = StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        Animator.SetFloat("AnimSpeed", 1 / AnimationSpeed);

        // Move up
        Animator.Play("Show");
        yield return new WaitForSeconds(AnimationSpeed);

        // Keep up until timer expires
        IsUp = true;
        CanBeHit = true;
        yield return new WaitForSeconds(Uptime);

        // Move down
        IsUp = false;
        CanBeHit = false;
        Animator.Play("Hide");
        yield return new WaitForSeconds(AnimationSpeed);

        // Strike if not hit
        if (GameOngoing)
        {
            OnMiss.Invoke();
        }
    }

    public void OnHit()
    {
        IsUp = false;
        CanBeHit = false;
        Animator.Play("Hit");
        StopCoroutine(Up);
    }
}