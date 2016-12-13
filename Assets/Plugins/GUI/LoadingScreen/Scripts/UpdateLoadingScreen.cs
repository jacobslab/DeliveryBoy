// -----------------------------------------------------------------------
//
// (c) Copyright 1997-2015, SensoMotoric Instruments GmbH
// 
// Permission  is  hereby granted,  free  of  charge,  to any  person  or
// organization  obtaining  a  copy  of  the  software  and  accompanying
// documentation  covered  by  this  license  (the  "Software")  to  use,
// reproduce,  display, distribute, execute,  and transmit  the Software,
// and  to  prepare derivative  works  of  the  Software, and  to  permit
// third-parties to whom the Software  is furnished to do so, all subject
// to the following:
// 
// The  copyright notices  in  the Software  and  this entire  statement,
// including the above license  grant, this restriction and the following
// disclaimer, must be  included in all copies of  the Software, in whole
// or  in part, and  all derivative  works of  the Software,  unless such
// copies   or   derivative   works   are   solely   in   the   form   of
// machine-executable  object   code  generated  by   a  source  language
// processor.
// 
// THE  SOFTWARE IS  PROVIDED  "AS  IS", WITHOUT  WARRANTY  OF ANY  KIND,
// EXPRESS OR  IMPLIED, INCLUDING  BUT NOT LIMITED  TO THE  WARRANTIES OF
// MERCHANTABILITY,   FITNESS  FOR  A   PARTICULAR  PURPOSE,   TITLE  AND
// NON-INFRINGEMENT. IN  NO EVENT SHALL  THE COPYRIGHT HOLDERS  OR ANYONE
// DISTRIBUTING  THE  SOFTWARE  BE   LIABLE  FOR  ANY  DAMAGES  OR  OTHER
// LIABILITY, WHETHER  IN CONTRACT, TORT OR OTHERWISE,  ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE  SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// -----------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using iView;

public class UpdateLoadingScreen : MonoBehaviour {

    Text loadingScreenText;
    LoadingIcon loadingWheel;

    private MaskableGraphic[] loadingScreenElements;

    private bool isFinishedRoutineStarted = false;
    private Color fontColor;
    private Color destinationColorText;

    private bool isDone = false;
    
    
    void Start()
    {
        //Safe all Children
        loadingScreenElements = GetComponentsInChildren<MaskableGraphic>();

        //Get the TextComponent
        loadingScreenText = GetComponentInChildren<Text>();

        //Get the LoadingIcon
        loadingWheel = GetComponentInChildren<LoadingIcon>();

        //Safe the Color
        fontColor = loadingScreenText.color;
        destinationColorText = fontColor;
    }

    void Update()
    {
        updateTextColor();

        //fade out the Screen
        if (isDone)
        {
            foreach (MaskableGraphic item in loadingScreenElements)
            {
                item.color = Color.Lerp(item.color, Color.clear, Time.deltaTime * 8f);
                destinationColorText = Color.clear;
                loadingWheel.FadeOut();
            }
        }


        if (SMIGazeController.Instance.GetIsStartingOver() == true && !isFinishedRoutineStarted)
        {
            isFinishedRoutineStarted = true;
            setFinishedMode(SMIGazeController.Instance.GetErrorID());
        }
    }

    private void updateTextColor()
    {
        if (loadingScreenText.color != destinationColorText)
        {
            loadingScreenText.color = Color.Lerp(loadingScreenText.color, destinationColorText, Time.deltaTime * 5);
        }
    }

    private void setFinishedMode(int errorID)
    {
        StartCoroutine(showResult(errorID));
    }

    private IEnumerator showResult(int errorID)
    {
        destinationColorText = Color.clear;
        yield return new WaitForSeconds(0.5f);
        destinationColorText = fontColor;

        if (errorID == 1)
        {
            loadingScreenText.text = "Setup finished";
            loadingWheel.SetSucessIcon();
    
            yield return new WaitForSeconds(1f);
        }

        else
        {
            loadingScreenText.text = "Setup failed";
            loadingWheel.SetFailedIcon();

            yield return new WaitForSeconds(5f);
        }

        isDone = true;
    }

}
