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
using iView;

public class GazeExample_LegacyGUI : MonoBehaviour {

    //Rectangle of the UI element
    private Rect sizeUIElement;

    //Textures for the Rendering
    [SerializeField]
    private Texture textureUIElementSelected;
    [SerializeField]
    private Texture textureUIElementDeselected;

    private Texture textureUIElementToDraw;

    void Start()
    {
        textureUIElementToDraw = textureUIElementDeselected;
    }
    
    void Update()
    {
        UpdateRectangleOifUIElement();
        CheckInterseptionOfGaze();


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    void OnGUI()
    {
        //Draw the Texture
        GUI.DrawTexture(sizeUIElement, textureUIElementToDraw);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateRectangleOifUIElement()
    {
        //Setup the Rectangle 
        sizeUIElement.width = Screen.height * 0.4f;
        sizeUIElement.height = Screen.height * 0.4f;
        sizeUIElement.x = Screen.width * 0.5f - sizeUIElement.width * 0.5f;
        sizeUIElement.y = Screen.height * 0.5f - sizeUIElement.height * 0.5f;
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckInterseptionOfGaze()
    {
        Vector2 gazePos = SMIGazeController.Instance.GetSample().averagedEye.gazePosInScreenCoords();

        if(sizeUIElement.Contains(gazePos))
        {
            textureUIElementToDraw = textureUIElementSelected;
        }
        else
        {
            textureUIElementToDraw = textureUIElementDeselected;
        }
    }
}
