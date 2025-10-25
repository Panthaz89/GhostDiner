using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManger : MonoBehaviour
{
    // Start is called before the first frame update
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    [SerializeField]public TMP_Text timetext;

    public float levelTime = 300f;
    private float timeRemaining;
    private bool isGameOver = false;
    private int ChefsKilled = 0;
    private int CustomersLeft = 0;
    private int CustomersKilled = 0;

    void Start()
    {
        timeRemaining = levelTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isGameOver = true;
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timetext.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}
