using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSystem : MonoBehaviour
{

    public RectTransform starCover;
    public UnityEngine.UI.Text numberText;

    private const float MAX_SCORE = 5.0f;
    private const int RECENT_AVERAGE_SIZE = 12;
    private const float numberDifferenceDisplayTime = 3f;

    private List<float> scores = new List<float>();

    public bool ReportScore(float goodness)
    {
        float previousAverage = RecentAverage();
        scores.Add(goodness * MAX_SCORE);
        float newAverage = RecentAverage();

        UpdateCover(newAverage / MAX_SCORE);
        StartCoroutine(ShowDifference(newAverage, previousAverage));

        return newAverage > previousAverage;
    }

    public float CumulativeRating()
    {
        return Sum(scores) / scores.Count;
    }

    private float RecentAverage()
    {
        if (scores.Count == 0)
            return 0f;
        else if (scores.Count < RECENT_AVERAGE_SIZE)
            return Sum(scores) / scores.Count;
        else
            return Sum(scores.GetRange(scores.Count - RECENT_AVERAGE_SIZE, RECENT_AVERAGE_SIZE)) / RECENT_AVERAGE_SIZE;
            
    }

    private float Sum(List<float> sumMe)
    {
        float sum = 0;
        for (int i = 0; i < sumMe.Count; i++)
        {
            Debug.Log(i.ToString() + ": " + sumMe[i].ToString());
            sum += sumMe[i];
        }
        return sum;
    }

	private void UpdateCover(float percentCover)
    {
        starCover.anchorMin = new Vector2(starCover.anchorMin.x, percentCover);
    }

    private IEnumerator ShowDifference(float newAverage, float previousAverage)
    {
        float changeInScore = newAverage - previousAverage;
        string numberScore = numberText.text;
        numberText.text = numberScore + "\n" + changeInScore.ToString("+#.##;-#.##");
        yield return new WaitForSeconds(numberDifferenceDisplayTime);
        numberText.text = newAverage.ToString("#.##");

    }
}
