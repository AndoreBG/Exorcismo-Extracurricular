using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Space]
    [Header("== Componentes ==")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject health_0;
    [SerializeField] private GameObject health_1;
    [SerializeField] private GameObject health_2;
    [SerializeField] private GameObject health_3;
    [SerializeField] private GameObject health_4;
    [SerializeField] private GameObject health_5;

    void Start()
    {
        InitializeComponents();
    }

    void Update()
    {
        
    }

    void InitializeComponents()
    {
        healthBar.SetActive(true);
        health_0.SetActive(true);
        health_1.SetActive(false);
        health_2.SetActive(false);
        health_3.SetActive(false);
        health_4.SetActive(false);
        health_5.SetActive(false);
    }
}
