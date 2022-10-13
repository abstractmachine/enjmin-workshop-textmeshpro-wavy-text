using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Wavy : MonoBehaviour
{

	// la valeur alpha principale
	[Range(0.0f, 1.0f)]
	public float alpha = 1.0f;

	// la vitesse de changement
	[Range(0.0f, 5.0f)]
	public float vitesse = 0.5f;

	// la différences entre une lettre et une autre
	[Range(1.0f, 10.0f)]
	public float spread = 1.0f;

	// la différences entre une lettre et une autre
	[Range(0.0f, 5.0f)]
	public float radius = 1.0f;


	// des variables pour mondifier le TextMeshPro
	TMP_Text textMesh;
	TMP_TextInfo textInfo;
	TMP_MeshInfo[] cachedMeshInfo;


	void Start()
	{
		// Chercher dans ce GameObject le component TextMesh
		textMesh = GetComponent<TMP_Text>();
	}


	void Update()
	{

		BeginMeshTransform();

		// aller character par character
		for (int characterIndex = 0; characterIndex < textInfo.characterCount; characterIndex++)
		{
			// Only change the vertex color if the text element is visible.
			if (!textInfo.characterInfo[characterIndex].isVisible)
			{
				continue;
			}

			// pour écarter plus les mouvements entre chaque lettre
			float sineOffset = characterIndex * spread;
			// générer une pulsation
			float sine = Mathf.Sin((Time.time + sineOffset) * vitesse);
			float cosine = Mathf.Cos((Time.time + sineOffset) * vitesse);

			Vector3 translate = new Vector3(sine*radius, cosine*radius, 0);
			float angle = sine * cosine * radius * 45;
			ModifyPosition(characterIndex, translate, angle);

			float characterAlpha = ((sine + 1.0f) / 2.0f) * alpha;
			Color color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, characterAlpha);
			ModifyColor(characterIndex, color);
		}
		// for(textInfo.characterCount)

		EndMeshTransform();
	}
	// Update()








	void BeginMeshTransform()
	{
		// Force the text object to update right away so we can have geometry to modify right from the start.
		textMesh.ForceMeshUpdate();
		// chercher l'info de cet component TextMesh
		textInfo = textMesh.textInfo;
		// Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
		cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
	}


	void EndMeshTransform()
	{
		// Push changes into meshes
		for (int i = 0; i < textInfo.meshInfo.Length; i++)
		{
			textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
			textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
		}
		// nous avons modifié des valeurs dans le mesh
		textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
	}



	void ModifyColor(int characterIndex, Color color)
	{
		// Get the index of the material used by the current character.
		int materialIndex = textInfo.characterInfo[characterIndex].materialReferenceIndex;
		// Get the index of the first vertex used by this text element.
		int vertexIndex = textInfo.characterInfo[characterIndex].vertexIndex;

		// create this color
		Color32[] newVertexColors = textInfo.meshInfo[materialIndex].colors32;
		Color32 finalColor = new Color32((byte)(color.r*255), (byte)(color.g*255), (byte)(color.b*255), (byte)(color.a * 255));

		newVertexColors[vertexIndex + 0] = finalColor;
		newVertexColors[vertexIndex + 1] = finalColor;
		newVertexColors[vertexIndex + 2] = finalColor;
		newVertexColors[vertexIndex + 3] = finalColor;
	}



	void ModifyPosition(int characterIndex, Vector3 translate, float angle)
	{
		// Get the index of the material used by the current character.
		int materialIndex = textInfo.characterInfo[characterIndex].materialReferenceIndex;
		// Get the index of the first vertex used by this text element.
		int vertexIndex = textInfo.characterInfo[characterIndex].vertexIndex;
		// Get the cached vertices of the mesh used by this text element (character or sprite).
		Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
		Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

		Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;
		Vector3 offset = charMidBasline;

		destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
		destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
		destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
		destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

		Matrix4x4 matrix = Matrix4x4.TRS(translate, Quaternion.Euler(0, 0, angle), Vector3.one);

		destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
		destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
		destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
		destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

		destinationVertices[vertexIndex + 0] += offset;
		destinationVertices[vertexIndex + 1] += offset;
		destinationVertices[vertexIndex + 2] += offset;
		destinationVertices[vertexIndex + 3] += offset;

		// Get the vertex colors of the mesh used by this text element (character or sprite).
	}


}
// class Wavy