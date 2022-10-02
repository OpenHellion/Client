using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

[ExecuteInEditMode]
public class SceneNameTag : BaseSceneTrigger, ISceneObject
{
	[SerializeField]
	private int _InSceneID;

	public string NameTagText;

	[NonSerialized]
	public SpaceObjectVessel ParentVessel;

	private Component comp;

	[SerializeField]
	public Material sourceMat;

	private Material matInstance;

	public string fontName;

	public bool Local;

	public override bool CameraMovementAllowed
	{
		get
		{
			return false;
		}
	}

	public override bool ExclusivePlayerLocking
	{
		get
		{
			return true;
		}
	}

	public int InSceneID
	{
		get
		{
			return _InSceneID;
		}
		set
		{
			_InSceneID = value;
		}
	}

	public override SceneTriggerType TriggerType
	{
		get
		{
			return SceneTriggerType.NameTag;
		}
	}

	public override bool IsNearTrigger
	{
		get
		{
			return true;
		}
	}

	public override bool IsInteractable
	{
		get
		{
			return true;
		}
	}

	public override PlayerHandsCheckType PlayerHandsCheck
	{
		get
		{
			return PlayerHandsCheckType.DontCheck;
		}
	}

	public override List<ItemType> PlayerHandsItemType
	{
		get
		{
			return null;
		}
	}

	private void Awake()
	{
		comp = GetComponent<DeferredDecal>();
		if (comp == null)
		{
			comp = GetComponent<MeshRenderer>();
		}
		InstantiateMaterial();
	}

	protected override void Start()
	{
		base.Start();
		SetNameTagText(NameTagText);
	}

	/// <summary>
	/// 	Generates a texture from the specified text, with the font this object is using.
	/// </summary>
	public void GenerateTexture(string text)
	{
		/*
		// This code requires the windows-only System.Common.Drawing library.
		float num = 1f;
		num = ((!(comp is MeshRenderer)) ? (base.transform.lossyScale.x / base.transform.lossyScale.z) : (base.transform.lossyScale.x / base.transform.lossyScale.y));
		Bitmap bitmap = new Bitmap((int)((float)TextureHeight * num), TextureHeight, PixelFormat.Format32bppArgb);
		RectangleF layoutRectangle = new RectangleF(0f, 0f, bitmap.Width, bitmap.Height);
		System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		int height = bitmap.Height;
		FontFamily localFont = Client.Instance.GetLocalFont(fontName);
		System.Drawing.Font font = ((localFont == null) ? new System.Drawing.Font("Arial", height, System.Drawing.FontStyle.Bold) : new System.Drawing.Font(localFont, height, System.Drawing.FontStyle.Bold));
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Center;
		stringFormat.FormatFlags = StringFormatFlags.NoWrap;
		SizeF sizeF = graphics.MeasureString(tx, font);
		float num2 = sizeF.Width / (float)bitmap.Width;
		float num3 = sizeF.Height / (float)bitmap.Height;
		float emSize = ((!(num2 > num3)) ? ((float)height / num3) : ((float)height / num2));
		System.Drawing.Font font2 = ((localFont == null) ? new System.Drawing.Font("Arial", emSize, System.Drawing.FontStyle.Bold) : new System.Drawing.Font(localFont, emSize, System.Drawing.FontStyle.Bold));
		graphics.DrawString(tx, font2, Brushes.White, layoutRectangle, stringFormat);
		graphics.Flush();
		Texture2D texture2D = new Texture2D(bitmap.Width, bitmap.Height);
		ImageConverter imageConverter = new ImageConverter();
		byte[] data = (byte[])imageConverter.ConvertTo(bitmap, typeof(byte[]));
		texture2D.LoadImage(data);
		matInstance.mainTexture = texture2D;*/

		// Broken
		// TODO: Fix this.
		/*TMP_Text textObject = new CustomTMPText(text);

		textObject.font = Client.Instance.GetLocalFont(fontName);
		textObject.fontStyle = FontStyles.Bold;
		textObject.alignment = TextAlignmentOptions.Center;
		textObject.verticalAlignment = VerticalAlignmentOptions.Middle;
		textObject.autoSizeTextContainer = true;
		textObject.enableWordWrapping = false;

		matInstance.mainTexture = textObject.mainTexture;*/
	}

	public void SetNameTagText(string text)
	{
		NameTagText = text;
		GenerateTexture(text);
	}

	public void SetNameTagTextEditor(string text)
	{
		NameTagText = text;
		InstantiateMaterial();
		GenerateTexture(text);
	}

	private void InstantiateMaterial()
	{
		if (matInstance == null && comp != null)
		{
			matInstance = UnityEngine.Object.Instantiate(sourceMat);
			if (comp is MeshRenderer)
			{
				(comp as MeshRenderer).material = matInstance;
			}
			else if (comp is DeferredDecal)
			{
				(comp as DeferredDecal).material = matInstance;
			}
		}
	}

	public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
	{
		if (!base.Interact(player, interactWithOverlappingTriggers))
		{
			return false;
		}
		Client.Instance.InGamePanels.NameTagUI.ToggleNameCanvas(this, true, ParentVessel, _InSceneID);
		return true;
	}

	public override void CancelInteract(MyPlayer player)
	{
		base.CancelInteract(player);
		Client.Instance.InGamePanels.NameTagUI.ToggleNameCanvas(this, false, ParentVessel, _InSceneID);
	}
}

public class CustomTMPText : TMP_Text
{
	public CustomTMPText(string text)
	{
		this.text = text;

		SetAllDirty();
	}
}
