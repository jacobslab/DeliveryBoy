using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AdvancedCA : MonoBehaviour {
	#region Variables
	private Shader curShader;
	public float DispersionAmount = 1.0f;
	public enum ColorSet { RedBlue = 0 , RedGreen = 1 };
	public ColorSet Colors;
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
		
		//Find
		curShader = Shader.Find("Custom/AdvancedCA");
		
	}
	
	void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if(curShader != null)
		{
			material.SetFloat("_Amount", DispersionAmount);
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
		if(ColorSet.RedBlue == Colors)
		{
			Shader.EnableKeyword("REDBLUE");
			Shader.DisableKeyword("REDGREEN");
		}
		else if(ColorSet.RedGreen == Colors)
		{
			Shader.EnableKeyword("REDGREEN");
			Shader.DisableKeyword("REDBLUE");
		}
	}
	
	void OnDisable ()
	{
		if(curMaterial)
		{
			DestroyImmediate(curMaterial);	
		}
		
	}
	
	
} 