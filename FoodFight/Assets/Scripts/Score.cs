using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour {

    private float score;

    /*
     * Score works as follows:
     * It is a percentile value that is a float so ranges between 0-100
     * 20% = 1 star
     * 40% = 2 stars
     * 60% = 3 stars
     * 80% = 4 stars
     * 100% = 5 stars
     */

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
