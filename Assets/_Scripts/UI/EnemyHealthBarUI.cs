using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private Image healthFillImage;

    [Header("Ajustes de visibilidad")]
    [SerializeField] private float visibleDuration = 2f;

    private float timer = 0f;
    private bool isVisible = false;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponentInParent<Enemy>();
        healthCanvas.enabled = false;
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.OnGetHit.AddListener(UpdateHealthUI);
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.OnGetHit.RemoveListener(UpdateHealthUI);
        }
    }

    private void Update()
    {
        if (isVisible)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                healthCanvas.enabled = false;
                isVisible = false;
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (enemy == null || healthFillImage == null || healthCanvas == null) return;

        // Mostrar canvas
        healthCanvas.enabled = true;
        isVisible = true;
        timer = visibleDuration;

        // Actualizar fill
        float fillAmount = (float)enemy.Health / enemy.EnemyData.MaxHealth;
        healthFillImage.fillAmount = fillAmount;
    }
}
