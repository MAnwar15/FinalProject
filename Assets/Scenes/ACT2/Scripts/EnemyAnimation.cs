using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // مثال: العدو يمشي دايماً
        anim.SetBool("isWalking", true);
    }
}
