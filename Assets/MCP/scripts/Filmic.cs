using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Filmic : MonoBehaviour {
	#region Variables
	public Shader curShader;
	public float ShoulderStrength = 0.15f;
	public float LinearStrength = 0.55f;
	public float LinearAngle = 0.1f;
	public float ToeStrength = 0.2f;
	public float ToeNumerator = 0.02f;
	public float ToeDenominator = 0.7f;
	public float Weight = 10.2f;
	private Material curMaterial;
	#endregion
	
	#region Properties
	Material material
	{
		get
		{
			if(curMaterial == null)
			{
				curMaterial = new Material(curShader);
				curMaterial.hideFlags = HideFlags.HideAndDontSave;	
			}
			return curMaterial;
		}
	}
	#endregion
	// Use this for initialization
	void Start () 
	{
		if(!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
	}
	
	void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if(curShader != null)
		{
			material.SetFloat("_A", ShoulderStrength);
			material.SetFloat("_B", LinearStrength);
			material.SetFloat("_C", LinearAngle);
			material.SetFloat("_D", ToeStrength);
			material.SetFloat("_E", ToeNumerator);
			material.SetFloat("_F", ToeDenominator);
			material.SetFloat("_W", Weight);
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);	
		}
		
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	
	void OnDisable ()
	{
		if(curMaterial)
		{
			DestroyImmediate(curMaterial);	
		}
		
	}
	
	
}