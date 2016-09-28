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
using UnityEngine.EventSystems;
using System.Collections.Generic;
using iView;

namespace iView_Example
{
    public class GazeExample_2DGUI : MonoBehaviour
    {
        private float selectedScaleFactor = 2.1f;

        //Safe the OldSelectedObject
        private GazeSelectableItem oldSelection;

        #region UnityFunction

        /// <summary>
        /// Update the Scene
        /// </summary>
        void Update()
        {
            RayCastFromGazeTo2DCanvas();

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit(); 
            }
        }
        #endregion

        #region PrivateFunction
        
        private void RayCastFromGazeTo2DCanvas()
        {
            //Create A PointerEvent for a Screenspace Canvas
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = SMIGazeController.Instance.GetSample().averagedEye.gazePosInUnityScreenCoords();

            //Safe the Raycast
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                if (oldSelection == null)
                {
                    oldSelection = raycastResults[0].gameObject.GetComponent<GazeSelectableItem>();
                }

                else if (raycastResults[0].gameObject != oldSelection)
                {
                    oldSelection.GetComponent<GazeSelectableItem>().OnGazeExit();
                    oldSelection = raycastResults[0].gameObject.GetComponent<GazeSelectableItem>();
                }

                GazeSelectableItem item = raycastResults[0].gameObject.GetComponent<GazeSelectableItem>();

                if (item)
                {
                    item.OnGazeEnter(selectedScaleFactor);
                }
            }
            else
            {
                if (oldSelection != null)
                {
                    oldSelection.GetComponent<GazeSelectableItem>().OnGazeExit();
                    oldSelection = null;
                }
            }
        }
        #endregion
    }

}
