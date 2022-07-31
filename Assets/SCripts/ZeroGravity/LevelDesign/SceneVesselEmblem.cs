using System.Collections.Generic;
using ThreeEyedGames;
using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class SceneVesselEmblem : MonoBehaviour
	{
		public static Dictionary<string, Texture> Textures;

		public static Dictionary<string, Sprite> Sprites;

		public string EmblemId;

		private Texture2D EmptyEmblem;

		private Decalicious decal;

		private Material matInstance;

		private void Awake()
		{
		}

		private void Start()
		{
			EmptyEmblem = Resources.Load("emptyEmblem") as Texture2D;
			SetEmblem((!EmblemId.IsNullOrEmpty()) ? EmblemId : string.Empty);
		}

		public void SetEmblem(string emblemId, bool fromResources = false)
		{
			EmblemId = emblemId;
			decal = GetComponent<Decalicious>();
			matInstance = Object.Instantiate(decal.Material);
			decal.Material = matInstance;
			if (decal != null && matInstance != null)
			{
				Texture value = null;
				if (EmblemId == string.Empty)
				{
					value = EmptyEmblem;
				}
				else if (fromResources)
				{
					value = Resources.Load("Emblems/" + emblemId) as Texture;
				}
				else
				{
					Textures.TryGetValue(emblemId, out value);
				}
				if (value == null)
				{
					value = EmptyEmblem;
				}
				matInstance.mainTexture = value;
				if (matInstance.HasProperty("_SpecularTex"))
				{
					matInstance.SetTexture("_SpecularTex", value);
				}
				if (matInstance.HasProperty("_MaskTex"))
				{
				}
				if (matInstance.HasProperty("_SmoothnessTex"))
				{
					matInstance.SetTexture("_SmoothnessTex", value);
				}
				if (matInstance.HasProperty("_SpecularMultiplier"))
				{
					matInstance.SetColor("_SpecularMultiplier", Color.white);
				}
			}
		}

		private void OnDestroy()
		{
			if (matInstance != null)
			{
				Object.Destroy(matInstance);
			}
		}
	}
}
