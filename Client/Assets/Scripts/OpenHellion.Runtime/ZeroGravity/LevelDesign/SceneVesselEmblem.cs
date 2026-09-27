using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ZeroGravity.LevelDesign
{
	public class SceneVesselEmblem : MonoBehaviour
	{
		public static Dictionary<string, Texture> Textures;

		public static Dictionary<string, Sprite> Sprites;

		public string EmblemId;

		private Texture2D EmptyEmblem;

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
			DecalProjector decal = GetComponentInChildren<DecalProjector>();
			matInstance = Instantiate(decal.material);
			decal.material = matInstance;
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
			matInstance.SetTexture("_SpecularMap", value);
			matInstance.SetColor("_SpecularColor", Color.white);
		}

		private void OnDestroy()
		{
			if (matInstance != null)
			{
				Destroy(matInstance);
			}
		}
	}
}
