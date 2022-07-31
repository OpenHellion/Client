using UnityEngine;

namespace Ikari.VertexPainter
{
	public class IVPData : ScriptableObject
	{
		public class FoldoutsClass
		{
			[SerializeField]
			public bool objectProperties = true;

			[SerializeField]
			public bool objectsSelected = true;

			[SerializeField]
			public bool color = true;

			[SerializeField]
			public bool tool = true;

			[SerializeField]
			public bool paint = true;

			[SerializeField]
			public bool gizmos = true;

			[SerializeField]
			public bool hotkeys = true;

			[SerializeField]
			public bool uninstaller;

			[SerializeField]
			public bool questions = true;

			[SerializeField]
			public bool suggestions;

			[SerializeField]
			public bool bugs;
		}

		public class HotkeysClass
		{
			[SerializeField]
			public Hotkey paint = new Hotkey(KeyCode.None, EventModifiers.Control);

			[SerializeField]
			public Hotkey erase = new Hotkey(KeyCode.None, EventModifiers.Shift | EventModifiers.Control);

			[SerializeField]
			public Hotkey increaseSize = new Hotkey(KeyCode.KeypadPlus, EventModifiers.Control);

			[SerializeField]
			public Hotkey decreaseSize = new Hotkey(KeyCode.KeypadMinus, EventModifiers.Control);

			[SerializeField]
			public Hotkey showVertexColors = new Hotkey(KeyCode.X, EventModifiers.Control | EventModifiers.Alt);

			[SerializeField]
			public Hotkey copyVertexColors = new Hotkey(KeyCode.C, EventModifiers.Control | EventModifiers.Alt);

			[SerializeField]
			public Hotkey pasteVertexColors = new Hotkey(KeyCode.V, EventModifiers.Control | EventModifiers.Alt);
		}

		public class Hotkey
		{
			public Event data = new Event();

			public bool enabled;

			public Hotkey(KeyCode keyCode, EventModifiers modifiers)
			{
				data.modifiers = modifiers;
				data.keyCode = keyCode;
			}
		}

		public FoldoutsClass Foldouts = new FoldoutsClass();

		public HotkeysClass Hotkeys = new HotkeysClass();

		[SerializeField]
		public SaveMode saveMode;

		[SerializeField]
		public PaintMode paintMode;

		[SerializeField]
		public SelectionMode selectionMode = SelectionMode.Selected;

		[SerializeField]
		public bool erase;

		[SerializeField]
		public Color primaryColor = Color.red;

		[SerializeField]
		public Color secondaryColor = Color.clear;

		[SerializeField]
		public float brushSize = 1f;

		[SerializeField]
		public float brushStrenght = 1f;

		[SerializeField]
		public float brushAngleLimit = 180f;

		[SerializeField]
		public float bucketSize;

		[SerializeField]
		public bool rChannel = true;

		[SerializeField]
		public bool gChannel = true;

		[SerializeField]
		public bool bChannel = true;

		[SerializeField]
		public bool aChannel = true;

		[SerializeField]
		public Color handleColor = Color.yellow;

		[SerializeField]
		public Color outlineHandleColor = Color.grey;

		[SerializeField]
		public bool solidHandle;

		[SerializeField]
		public bool drawHandleOutline;

		[SerializeField]
		public bool drawHandleAngle;
	}
}
