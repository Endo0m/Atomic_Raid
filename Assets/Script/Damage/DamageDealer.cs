using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        DealDamage(other.gameObject);
    }

    public void DealDamage(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            Destroy(gameObject); // ”ничтожаем пулю после нанесени€ урона
        }
        else
        {
        }
    }

    public void SetDamageAmount(int newDamageAmount)
    {
        damageAmount = newDamageAmount;
    }
}