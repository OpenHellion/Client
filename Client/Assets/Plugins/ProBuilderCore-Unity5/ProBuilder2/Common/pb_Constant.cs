using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Constant
	{
		public const string PRODUCT_NAME = "ProBuilder";

		public static readonly HideFlags EDITOR_OBJECT_HIDE_FLAGS = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

		public const float MAX_POINT_DISTANCE_FROM_CONTROL = 20f;

		private static Material _defaultMaterial = null;

		private static Material _facePickerMaterial;

		private static Material _vertexPickerMaterial;

		private static Shader _selectionPickerShader = null;

		private static Material _UnityDefaultDiffuse = null;

		private static Material _UnlitVertexColorMaterial;

		public const char DEGREE_SYMBOL = '°';

		public const char CMD_SUPER = '⌘';

		public const char CMD_SHIFT = '⇧';

		public const char CMD_OPTION = '⌥';

		public const char CMD_ALT = '⎇';

		public const char CMD_DELETE = '⌫';

		public const string pbDefaultEditLevel = "pbDefaultEditLevel";

		public const string pbDefaultSelectionMode = "pbDefaultSelectionMode";

		public const string pbHandleAlignment = "pbHandleAlignment";

		public const string pbVertexColorTool = "pbVertexColorTool";

		public const string pbToolbarLocation = "pbToolbarLocation";

		public const string pbDefaultEntity = "pbDefaultEntity";

		public const string pbExtrudeMethod = "pbExtrudeMethod";

		public const string pbDefaultFaceColor = "pbDefaultFaceColor";

		public const string pbDefaultEdgeColor = "pbDefaultEdgeColor";

		public const string pbDefaultSelectedVertexColor = "pbDefaultSelectedVertexColor";

		public const string pbDefaultVertexColor = "pbDefaultVertexColor";

		public const string pbDefaultOpenInDockableWindow = "pbDefaultOpenInDockableWindow";

		public const string pbEditorPrefVersion = "pbEditorPrefVersion";

		public const string pbEditorShortcutsVersion = "pbEditorShortcutsVersion";

		public const string pbDefaultCollider = "pbDefaultCollider";

		public const string pbForceConvex = "pbForceConvex";

		public const string pbVertexColorPrefs = "pbVertexColorPrefs";

		public const string pbShowEditorNotifications = "pbShowEditorNotifications";

		public const string pbDragCheckLimit = "pbDragCheckLimit";

		public const string pbForceVertexPivot = "pbForceVertexPivot";

		public const string pbForceGridPivot = "pbForceGridPivot";

		public const string pbManifoldEdgeExtrusion = "pbManifoldEdgeExtrusion";

		public const string pbPerimeterEdgeBridgeOnly = "pbPerimeterEdgeBridgeOnly";

		public const string pbPBOSelectionOnly = "pbPBOSelectionOnly";

		public const string pbCloseShapeWindow = "pbCloseShapeWindow";

		public const string pbUVEditorFloating = "pbUVEditorFloating";

		public const string pbUVMaterialPreview = "pbUVMaterialPreview";

		public const string pbShowSceneToolbar = "pbShowSceneToolbar";

		public const string pbNormalizeUVsOnPlanarProjection = "pbNormalizeUVsOnPlanarProjection";

		public const string pbStripProBuilderOnBuild = "pbStripProBuilderOnBuild";

		public const string pbDisableAutoUV2Generation = "pbDisableAutoUV2Generation";

		public const string pbShowSceneInfo = "pbShowSceneInfo";

		public const string pbEnableBackfaceSelection = "pbEnableBackfaceSelection";

		public const string pbVertexPaletteDockable = "pbVertexPaletteDockable";

		public const string pbExtrudeAsGroup = "pbExtrudeAsGroup";

		public const string pbUniqueModeShortcuts = "pbUniqueModeShortcuts";

		public const string pbMaterialEditorFloating = "pbMaterialEditorFloating";

		public const string pbShapeWindowFloating = "pbShapeWindowFloating";

		public const string pbIconGUI = "pbIconGUI";

		public const string pbShiftOnlyTooltips = "pbShiftOnlyTooltips";

		public const string pbDrawAxisLines = "pbDrawAxisLines";

		public const string pbCollapseVertexToFirst = "pbCollapseVertexToFirst";

		public const string pbMeshesAreAssets = "pbMeshesAreAssets";

		public const string pbElementSelectIsHamFisted = "pbElementSelectIsHamFisted";

		public const string pbFillHoleSelectsEntirePath = "pbFillHoleSelectsEntirePath";

		public const string pbDetachToNewObject = "pbDetachToNewObject";

		[Obsolete("Use pb_MeshImporter::quads")]
		public const string pbPreserveFaces = "pbPreserveFaces";

		public const string pbDragSelectWholeElement = "pbDragSelectWholeElement";

		public const string pbDragSelectMode = "pbDragSelectMode";

		public const string pbShadowCastingMode = "pbShadowCastingMode";

		public const string pbEnableExperimental = "pbEnableExperimental";

		public const string pbCheckForProBuilderUpdates = "pbCheckForProBuilderUpdates";

		public const string pbManageLightmappingStaticFlag = "pbManageLightmappingStaticFlag";

		public const string pbVertexHandleSize = "pbVertexHandleSize";

		public const string pbUVGridSnapValue = "pbUVGridSnapValue";

		public const string pbUVWeldDistance = "pbUVWeldDistance";

		public const string pbWeldDistance = "pbWeldDistance";

		public const string pbExtrudeDistance = "pbExtrudeDistance";

		public const string pbBevelAmount = "pbBevelAmount";

		public const string pbEdgeSubdivisions = "pbEdgeSubdivisions";

		public const string pbDefaultShortcuts = "pbDefaultShortcuts";

		public const string pbDefaultMaterial = "pbDefaultMaterial";

		public const string pbCurrentMaterialPalette = "pbCurrentMaterialPalette";

		public const string pbGrowSelectionUsingAngle = "pbGrowSelectionUsingAngle";

		public const string pbGrowSelectionAngle = "pbGrowSelectionAngle";

		public const string pbGrowSelectionAngleIterative = "pbGrowSelectionAngleIterative";

		public const string pbShowDetail = "pbShowDetail";

		public const string pbShowOccluder = "pbShowOccluder";

		public const string pbShowMover = "pbShowMover";

		public const string pbShowCollider = "pbShowCollider";

		public const string pbShowTrigger = "pbShowTrigger";

		public const string pbShowNoDraw = "pbShowNoDraw";

		public static readonly Rect RectZero = new Rect(0f, 0f, 0f, 0f);

		public static Color ProBuilderBlue = new Color(0f, 0.682f, 0.937f, 1f);

		public static Color ProBuilderLightGray = new Color(0.35f, 0.35f, 0.35f, 0.4f);

		public static Color ProBuilderDarkGray = new Color(0.1f, 0.1f, 0.1f, 0.3f);

		public const int MENU_ABOUT = 0;

		public const int MENU_EDITOR = 100;

		public const int MENU_SELECTION = 200;

		public const int MENU_GEOMETRY = 200;

		public const int MENU_ACTIONS = 300;

		public const int MENU_MATERIAL_COLORS = 400;

		public const int MENU_VERTEX_COLORS = 400;

		public const int MENU_REPAIR = 600;

		public const int MENU_MISC = 600;

		public const int MENU_EXPORT = 800;

		public static Vector3[] VERTICES_CUBE = new Vector3[8]
		{
			new Vector3(-0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, -0.5f),
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(-0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f)
		};

		public static int[] TRIANGLES_CUBE = new int[24]
		{
			0, 1, 4, 5, 1, 2, 5, 6, 2, 3,
			6, 7, 3, 0, 7, 4, 4, 5, 7, 6,
			3, 2, 0, 1
		};

		public const int MAX_VERTEX_COUNT = 65000;

		public static Material DefaultMaterial
		{
			get
			{
				if (_defaultMaterial == null)
				{
					_defaultMaterial = (Material)Resources.Load("Materials/Default_Prototype", typeof(Material));
					if (_defaultMaterial == null)
					{
						_defaultMaterial = UnityDefaultDiffuse;
					}
				}
				return _defaultMaterial;
			}
		}

		public static Material FacePickerMaterial
		{
			get
			{
				if (_facePickerMaterial == null)
				{
					_facePickerMaterial = Resources.Load<Material>("Materials/FacePicker");
					if (_facePickerMaterial == null)
					{
						_facePickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/FacePicker"));
					}
					else
					{
						_facePickerMaterial.shader = Shader.Find("Hidden/ProBuilder/FacePicker");
					}
				}
				return _facePickerMaterial;
			}
		}

		public static Material VertexPickerMaterial
		{
			get
			{
				if (_vertexPickerMaterial == null)
				{
					_vertexPickerMaterial = Resources.Load<Material>("Materials/VertexPicker");
					if (_vertexPickerMaterial == null)
					{
						_vertexPickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/VertexPicker"));
					}
					else
					{
						_vertexPickerMaterial.shader = Shader.Find("Hidden/ProBuilder/VertexPicker");
					}
				}
				return _vertexPickerMaterial;
			}
		}

		public static Shader SelectionPickerShader
		{
			get
			{
				if (_selectionPickerShader == null)
				{
					_selectionPickerShader = Shader.Find("Hidden/ProBuilder/SelectionPicker");
				}
				return _selectionPickerShader;
			}
		}

		public static Material TriggerMaterial => (Material)Resources.Load("Materials/Trigger", typeof(Material));

		public static Material ColliderMaterial => (Material)Resources.Load("Materials/Collider", typeof(Material));

		public static Material NoDrawMaterial => (Material)Resources.Load("Materials/NoDraw", typeof(Material));

		public static Material UnityDefaultDiffuse
		{
			get
			{
				if (_UnityDefaultDiffuse == null)
				{
					GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					_UnityDefaultDiffuse = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
				return _UnityDefaultDiffuse;
			}
		}

		public static Material UnlitVertexColor
		{
			get
			{
				if (_UnlitVertexColorMaterial == null)
				{
					_UnlitVertexColorMaterial = (Material)Resources.Load("Materials/UnlitVertexColor", typeof(Material));
				}
				return _UnlitVertexColorMaterial;
			}
		}
	}
}
