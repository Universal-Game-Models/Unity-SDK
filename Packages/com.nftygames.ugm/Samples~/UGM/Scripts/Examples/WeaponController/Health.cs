using UnityEngine;
using UnityEngine.Events;

namespace UGM.Examples.WeaponController
{
    public class Health : MonoBehaviour
    {
        public int maxHealth = 100;
        public int health;
        public float regenDelay = 2f; // Time delay after which regeneration starts
        public float regenSpeed = 50f; // Speed at which health regenerates

        public UnityEvent<int> onHealthChanged = new UnityEvent<int>();
        public UnityEvent<float> onHealthRatioChanged = new UnityEvent<float>();

        private float lastChangeTime; // Time of the last health change
        private bool isRegenerating; // Flag to track if regeneration is in progress
        private float accumulatedTime; // Accumulated time for health regeneration

        private void Start()
        {
            health = maxHealth;
            onHealthChanged.AddListener((int newHealth) => onHealthRatioChanged.Invoke((float)newHealth / (float)maxHealth));
            lastChangeTime = Time.time;
            isRegenerating = false;
            accumulatedTime = 0f;
        }

        private void Update()
        {
            if (!isRegenerating && Time.time - lastChangeTime >= regenDelay && health < maxHealth)
            {
                isRegenerating = true;
                accumulatedTime = 0f;
            }

            if (isRegenerating)
            {
                RegenerateHealth();
            }
        }

        private void RegenerateHealth()
        {
            accumulatedTime += Time.deltaTime;

            while (accumulatedTime >= (1f / regenSpeed))
            {
                health = Mathf.Clamp(health + 1, 0, maxHealth);
                onHealthChanged.Invoke(health);

                if (health >= maxHealth)
                {
                    health = maxHealth;
                    isRegenerating = false;
                    break;
                }

                accumulatedTime -= 1f / regenSpeed;
            }
        }

        public void ChangeHealth(int amount)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealth);
            onHealthChanged.Invoke(health);
            lastChangeTime = Time.time;

            if (isRegenerating)
            {
                isRegenerating = false;
            }
        }
    }
}
