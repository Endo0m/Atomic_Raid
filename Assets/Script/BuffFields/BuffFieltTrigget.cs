 using UnityEngine;

/// <summary>
/// ����� BuffFielTrigger
/// ��������� �� ������ ������� ����� ����� �
/// � ������� ����� ����� ������������
/// </summary>



public class BuffFieltTrigget : MonoBehaviour
{
    [SerializeField] BuffList _buffList;
    private BuffSoundManager buffSoundManager;

    private void Start()
    {
        buffSoundManager = FindObjectOfType<BuffSoundManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BuffFieldsController buffController = other.GetComponent<BuffFieldsController>();
            if (buffController != null && buffController.CanApplyBuff(_buffList))
            {
                buffController.StartBuff(_buffList.ToString());
                if (buffSoundManager != null)
                {
                    buffSoundManager.PlayBuffSound(_buffList);
                }
                Destroy(gameObject);
            }
        }
    }
}