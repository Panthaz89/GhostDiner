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

    [SerializeField] public TMP_Text timetext;
    [SerializeField] public AudioSource levelMusic;
    [SerializeField] public AudioSource ChoirMusic;

    public float levelTime;
    private float timeRemaining;
    private bool isGameOver = false;
    private int ChefsKilled = 0;
    private int CustomersLeft = 0;
    private int CustomersKilled = 0;

    private Color originalColor;

    void Start()
    {
        timeRemaining = levelTime;
        originalColor = timetext.color;
        levelMusic.pitch = 0.85f;
    }

    // Update is called once per frame
    void Update()
    {
        float progress = 1f - (timeRemaining / levelTime);

    // Smoothly adjust pitch from 0.85 to 1.10 over time
    if (timeRemaining > 0f){
            levelMusic.pitch = Mathf.Lerp(0.85f, 1.10f, progress);
        }

        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isGameOver = true;
            levelMusic.pitch = -.5f;
            ChoirMusic.Play();
            
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timetext.text = string.Format("{0}:{1:00}", minutes, seconds);


     //Color flashing
  if (timeRemaining <= 60f)
        {
            FlashText();
        }
        else
        {
            // Make sure the text color is normal if above 1 minute
            timetext.color = originalColor;
        }
    }

    void FlashText()
    {
        // Flash between original color and transparent using a sine wave
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * 3f)); // speed = 3 flashes per second
        Color flashColor = Color.red;
        flashColor.a = Mathf.Lerp(0.3f, 1f, alpha); // range of fade (30% - 100%)
        timetext.color = flashColor;
    }
 }

    
