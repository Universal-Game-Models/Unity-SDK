using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int health;
    public UnityEvent<int> onHealthChanged = new UnityEvent<int>();
    public UnityEvent<float> onHealthRatioChanged = new UnityEvent<float>();

    private void Start()
    {
        health = maxHealth;
        onHealthChanged.AddListener((int newHealth) => onHealthRatioChanged.Invoke((float)newHealth / (float)maxHealth));
    }

    public void ChangeHealth(int amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        onHealthChanged.Invoke(health);
    }
}