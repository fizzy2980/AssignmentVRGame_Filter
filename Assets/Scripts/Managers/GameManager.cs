using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private TMP_Text ScoreTitle;
    [SerializeField] private TMP_Text Score;
    [SerializeField] private TMP_Text Bonus;
    [SerializeField] private TMP_Text LeftBullet;
    [SerializeField] private GameObject ScorePanel;
    [SerializeField] private Pistol Pistol;

    private int CurrentScore;
    public int WinScore;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy extra instances
            return;
        }

        DontDestroyOnLoad(gameObject); // Keep this instance across scenes
    }

    void Start()
    {
        ScoreTitle.text = "Score To Win : " + WinScore.ToString();
    }

    public void UpdateScore(int score)
    {
        CurrentScore += score;
        Score.text = "Score : " + CurrentScore.ToString();

        if (CurrentScore > WinScore)
        {
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        ScorePanel.SetActive(true);
        int CurrentAmmo = Pistol.gunData.currentAmmo;
        int Magazine = Pistol.gunData.magazineSize;
        int TotalBulletLeftInGun = CurrentAmmo + Magazine;
        int CalculateBonus = TotalBulletLeftInGun * 100;

        LeftBullet.text = "Bullet Left : "  + TotalBulletLeftInGun.ToString();
        Bonus.text = "Bonus : " + CalculateBonus.ToString();
        Score.text = "Total score : " + CurrentScore + CalculateBonus;


    }
}
