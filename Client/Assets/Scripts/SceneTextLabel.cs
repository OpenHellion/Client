using UnityEngine;

[ExecuteInEditMode]
public class SceneTextLabel : MonoBehaviour
{
	public string Text;

	public int TextureHeight = 200;

	private MeshRenderer comp;

	[SerializeField] public Material sourceMat;

	private Material matInstance;

	public string fontName;

	private void Awake()
	{
		comp = GetComponent<MeshRenderer>();

		InstantiateMaterial();
	}

	private void Start()
	{
		SetNameTagText(Text);
	}

	/// <summary>
	/// 	Generates a texture from the specified text, with the font this object is using.
	/// 	Duplicate of SceneNameTag.GetLocalFont.
	/// </summary>
	public void GenerateTexture(string text)
	{
		Debug.LogError("Tried to generate texture, but method is deprecated.");
	}

	public void SetNameTagText(string text)
	{
		Text = text;
		GenerateTexture(text);
	}

	public void SetLabelTextEditor(string text)
	{
		Text = text;
		InstantiateMaterial();
		GenerateTexture(text);
	}

	private void InstantiateMaterial()
	{
		if (matInstance == null && comp != null)
		{
			matInstance = Instantiate(sourceMat);
			comp.material = matInstance;
		}
	}
}
