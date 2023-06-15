using UnityEngine;
using UnityEngine.EventSystems;

namespace UGM.Examples.WeaponController
{
    public class Weapon : MonoBehaviour
    {
        public string enemyTag = "Enemy";
        protected int damage;
        protected bool isAttacking;
        protected Animator animator;

        protected void Awake()
        {
            animator = GetComponentInParent<Animator>();
        }

        public virtual void Attack() 
        {
            isAttacking = true;
        }
        public virtual void OnHit(GameObject other)
        {
            //If the other has the tag "Enemy" and has a Health component
            if (other.gameObject.tag == enemyTag)
            {
                var health = other.gameObject.GetComponent<Health>();
                if (health)
                {
                    health.ChangeHealth(-damage);
                    Debug.Log("Dealt " + damage + " damage to " + other.name);
                }
            }
        }
        public virtual void StopAttacking()
        {
            //Force stop a currently running attack
            isAttacking = false;
        }

        protected virtual void Update()
        {
            //Mouse button clicked when not over UI
            if (!isAttacking && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Attack();
            }
            if (isAttacking && Input.GetMouseButtonUp(0))
            {
                StopAttacking();
            }      
        }
    }
}