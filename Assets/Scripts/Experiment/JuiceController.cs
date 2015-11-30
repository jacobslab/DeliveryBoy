using UnityEngine;
using System.Collections;

public class JuiceController : MonoBehaviour {

	public delegate void JuiceTogglerDelegate(bool isJuicy);
	public JuiceTogglerDelegate ToggleJuice;

	//ENVIRONMENT
	//skybox
	public Material juicySkybox;
	public Material defaultSkybox;

	//arena & models
	public GameObject juicyEnvironment;
	public GameObject defaultEnvironment;

	//soundtrack & sound
		//AudioController deals with juice directly

	//PARTICLES
	//special object particles
	//feedback particles & explosion
	//fireworks and coins in GUI

	//ANIMATIONS
	//treasure chest opening -- taken care of in DefaultObject.cs
	//object small to large -- taken care of in SpawnableObject.cs

	//MAKE SURE THIS HAPPENS BEFORE THE FIRST LOG & OTHER CALCULATIONS.
	//CALLED IN EXPERIMENT --> AWAKE()
	public void Init(){
		ToggleJuice += Toggle;
		ToggleJuice (Config.isJuice);
	}

	public void Toggle (bool isJuice){
		if (!isJuice) {
			SetSkybox (defaultSkybox);
		} else {
			SetSkybox(juicySkybox);
		}
		
		SetEnvironmentJuice(isJuice);
	}

	void SetSkybox(Material skyMat){
		RenderSettings.skybox = skyMat;
	}

	void SetEnvironmentJuice(bool isJuice){
		juicyEnvironment.SetActive (isJuice);
		defaultEnvironment.SetActive (!isJuice);
	}

	public static void PlayParticles(ParticleSystem particles){
		if (Config.isJuice) {
			particles.Stop();
			particles.Play();
		}
	}

}
