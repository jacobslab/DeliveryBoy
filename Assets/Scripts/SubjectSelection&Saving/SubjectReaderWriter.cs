using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class SubjectReaderWriter : MonoBehaviour {

	public static Dictionary<string,Subject> subjectDict;

	//I/O
	StreamWriter fileWriter;
	StreamReader fileReader;

	//subject file
	string subjectFile = "TextFiles/Subjects.txt";
	

	//SINGLETON
	private static SubjectReaderWriter _instance;
	
	public static SubjectReaderWriter Instance{
		get{
			return _instance;
		}
	}
	
	void Awake(){
		DontDestroyOnLoad(transform.gameObject); //allow subject controller to be passed from scene to scene
		
		if (_instance != null) {
			Debug.Log("Instance already exists!");
			Destroy(transform.gameObject);
			return;
		}
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		subjectDict = new Dictionary<string,Subject>();
	}

	public void ReadSubjects() {
		subjectDict = new Dictionary<string,Subject>();

		fileReader = new StreamReader ( subjectFile );

		subjectDict.Clear();

		string line = fileReader.ReadLine();
		
		while (line != null) {
			string[] splitLine = line.Split(',');

			//should be split into 3: name, score #, session #
			if(splitLine.Length == 3){

				Subject newSubject = new Subject();
				//name
				newSubject.name = splitLine[0];

				//score
				string[] splitScore = splitLine[1].Split(' ');
				newSubject.score = int.Parse(splitScore[1]);

				//session
				string[] splitSession = splitLine[2].Split(' ');
				//newSubject.blocks = int.Parse(splitSession[1]);
				newSubject.trials = 0; //JUST START IT OVER?! TODO: new save system?


				//add to the subject list!
				subjectDict.Add(newSubject.name, newSubject);

			}
			line = fileReader.ReadLine();
		}

		fileReader.Close();
	}

	public void WriteNewSubject( string subjectName){
		fileWriter = new StreamWriter ( subjectFile, true ); //will append to existing file instead of overwriting it
		
		//write subject to file
		fileWriter.WriteLine(subjectName + ",score 0,block 0");
		
		//flush & close the file
		fileWriter.Flush();
		fileWriter.Close();
	}

	//have to re-write entire file -- highly non-trivial to edit a single line of the file
	//written when scenes are loaded
	public void RecordSubjects(){
		if( ExperimentSettings.currentSubject != null && !ExperimentSettings.isReplay && ExperimentSettings.isLogging){

			fileWriter = new StreamWriter ( subjectFile, false ); //will overwrite file
		
			//write subjects to file
			foreach(KeyValuePair<string, Subject> entry in SubjectReaderWriter.subjectDict)
			{
				fileWriter.WriteLine(entry.Value.name + ",score " + entry.Value.score + ",block "+ entry.Value.trials);
			}
			
			//flush & close the file
			fileWriter.Flush();
			fileWriter.Close();

		}
	}

}
