using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score {

    private float score;

    public Score()
    {
        score = 40.0f;
    }

	public void increaseScore(float scoreChange)
    {
        score += scoreChange;
    }

    public void decreaseScore(float scoreChange)
    {
        score -= scoreChange;
        if (score < 0) score = 0;
    }

    public float getScore()
    {
        return score;
    }
}
