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

namespace iView_Example
{

    public class GazeExample_3DInteraction : MonoBehaviour
    {


        [SerializeField]
        private float selectedScaleFactor = 2f;

        //Safe the OldSelectedObject
        private GameObject oldSelection;


        void Update()
        {
            Raycast3DScene();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

        }

        private void Raycast3DScene()
        {
            Ray rayGaze = Camera.main.ScreenPointToRay(SMIGazeController.Instance.GetSample().averagedEye.gazePosInUnityScreenCoords());
            RaycastHit hit; 

            //Raycast from the Gazeposition on the Screen
            if(Physics.Raycast(rayGaze,out hit))
            {

                GazeSelectableItem item = hit.collider.gameObject.GetComponent<GazeSelectableItem>();

                if(item!= null)
                {
                    if(oldSelection == null)
                    {
                        oldSelection = hit.collider.gameObject;
                    }

                    else if(hit.collider.gameObject != oldSelection)
                    {
                        oldSelection.GetComponent<GazeSelectableItem>().OnGazeExit();
                        oldSelection = hit.collider.gameObject;
                    }

                    oldSelection.GetComponent<GazeSelectableItem>().OnGazeEnter(selectedScaleFactor);
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

                // no result --> Check if there is an older Selection saved
            else
            {
                if (oldSelection != null)
                {
                    oldSelection.GetComponent<GazeSelectableItem>().OnGazeExit();
                    oldSelection = null;
                }
            }
        }
    }
}