using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Basket : MonoBehaviour
{
    public TMP_Text scoreText;
    int curScore = 0;
    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = curScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            other.GetComponent<Ball>().Despawn();
            curScore++;
            scoreText.text = curScore.ToString();
        }
    }
}
