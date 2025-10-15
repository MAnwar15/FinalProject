using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TextMeshProUGUI stateText;
    public TextMeshProUGUI tipsText;

    bool gameOver = false;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        UpdateStateUI("Playing: Hide & Escape");
        if (tipsText != null)
            tipsText.text = "Throw rocks to distract enemies. Avoid their sight cones.";
    }

    public void OnPlayerKilled()
    {
        if (gameOver) return;
        gameOver = true;
        UpdateStateUI("You were caught! ");
        Debug.Log("[GameManager] Player killed - game over");
        // additional: disable locomotion, show menu, etc.
    }

    public void OnPlayerEscaped()
    {
        if (gameOver) return;
        gameOver = true;
        UpdateStateUI("You escaped! ");
        Debug.Log("[GameManager] Player escaped - win");
    }

    void UpdateStateUI(string msg)
    {
        if (stateText != null) stateText.text = msg;
    }
}
