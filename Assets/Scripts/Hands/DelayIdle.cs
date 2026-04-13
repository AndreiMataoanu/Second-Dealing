using System.Collections;
using UnityEngine;

namespace Hands
{
    public class DelayIdle : MonoBehaviour
    {
        [SerializeField] private float delayTime = 2f;
        private Animator animator;
        private bool calledCoroutine;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            DelayIdleAnimation();
        }

        private void DelayIdleAnimation()
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Armature_Hand|Idle_Neutral")) return;
            StartCoroutine(SetInMotion());
        }

        private IEnumerator SetInMotion()
        {
            if (calledCoroutine) yield return null;
            calledCoroutine = true;
            animator.SetBool("inMotion", true);
            
            yield return new WaitForSeconds(delayTime);
            animator.SetBool("inMotion", false);
            calledCoroutine = false;
        }
    }
}
