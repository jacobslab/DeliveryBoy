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

public class LoadingIcon : MonoBehaviour
{
    bool isRotationActive = true;

    private Color destinationColor;

    [SerializeField]
    private Sprite sucessIcon;
    [SerializeField]
    private Sprite failedIcon;

    private Image iconInstance;

    void Start()
    {
        iconInstance = GetComponent<Image>();
        destinationColor = Color.white;
    }

    void Update()
    {

        if (iconInstance.color != destinationColor)
        {
            iconInstance.color = Color.Lerp(iconInstance.color, destinationColor, Time.deltaTime * 8);
        }

        if (isRotationActive)
        {
            gameObject.transform.Rotate(Vector3.forward, 8f);
        }

    }

    /// <summary>
    /// Fade the Icon to Alpha
    /// </summary>
    public void FadeOut()
    {
        destinationColor = Color.clear;
    }

    /// <summary>
    /// Fade the Icon to White
    /// </summary>
    public void FadeIn()
    {
        destinationColor = Color.white;
    }

    /// <summary>
    /// Set an Failed Icon
    /// </summary>
    public void SetFailedIcon()
    {
        StartCoroutine(showError());
    }

    /// <summary>
    /// Set an SucessIcon
    /// </summary>
    public void SetSucessIcon()
    {
        StartCoroutine(showSucess());
    }

    private IEnumerator showSucess()
    {
        FadeOut();
        yield return new WaitForSeconds(0.25f);
        FadeIn();
        isRotationActive = false;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        iconInstance.sprite = sucessIcon;

    }

    private IEnumerator showError()
    {
        FadeOut();
        yield return new WaitForSeconds(0.25f);
        FadeIn();
        isRotationActive = false;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        iconInstance.sprite = failedIcon;
    }

}
