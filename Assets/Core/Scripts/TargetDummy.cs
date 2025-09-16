using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    public GameObject ui;
    public RectTransform container;
    public TextMeshProUGUI dps;

    private Unit unit;
    private bool sessionInProgress = false;
    private float damageStartedAt;
    private float lastDamagedAt;
    private float damageDone = 0;
    private const float RESET_TIME = 5;

    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.events.OnUnitDamaged.AddListener(OnDamageTaken);
    }

    public void OnDamageTaken(GameEvents.OnUnitDamagedInfo info)
    {
        if (info.damagedUnit == unit)
        {
            if (!sessionInProgress)
            {
                sessionInProgress = true;
                damageStartedAt = Time.time;
            }
            lastDamagedAt = Time.time;
            damageDone += info.damage;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        if (Time.time > lastDamagedAt + RESET_TIME)
            sessionInProgress = false;

        if (!sessionInProgress)
            damageDone = 0;

        dps.text = "Damage: " + damageDone + "\n" + "DPS: " + Mathf.RoundToInt(damageDone / (Time.time - damageStartedAt));
    }

    private void UpdateHealthBar()
    {
        ui.gameObject.SetActive(true);

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        screenPoint.x /= ui.transform.localScale.x;
        screenPoint.y /= ui.transform.localScale.y;
        container.anchoredPosition = screenPoint;
    }
}
