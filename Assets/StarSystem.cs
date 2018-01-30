using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSystem : MonoBehaviour
{

    public RectTransform starCover;
    public UnityEngine.UI.Text numberText;

    private const float MAX_SCORE = 5.0f;
    private const float numberDifferenceDisplayTime = 2f;

    private int timesScored = 0;
    private float score = 0f;

    public void ReportScore(float goodness)
    {

        float previousAverage = 0;
        if (timesScored != 0)
            previousAverage = score / timesScored;

        timesScored++;
        score += goodness * MAX_SCORE;
        float newAverage = score / timesScored;

        UpdateCover(newAverage / MAX_SCORE);
        StartCoroutine(ShowDifference(newAverage, previousAverage));
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
