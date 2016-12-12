using UnityEngine;
using System.Collections;

public class GazeMove : MonoBehaviour {


    private bool gazing = false;
    public float factor = 10f;
    //EXPERIMENT IS A SINGLETON
    private static GazeMove _instance;

    public static GazeMove Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;
        //gazeFollower = GetComponent<Transform>();

    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void MoveSphere(Vector3 normal)
    {
       
            transform.position = normal;
      
    }

    void OnTriggerEnter()
    {
        gazing = true;
    }

    void OnCollisionExit()
    {
        gazing = false;
    }
}
