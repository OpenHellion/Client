using UnityEngine;

namespace Ikari.VertexPainter
{
    public class IVPData : ScriptableObject
    {
        public FoldoutsClass Foldouts = new FoldoutsClass();
        public HotkeysClass Hotkeys = new HotkeysClass();

        [SerializeField]
        public SaveMode saveMode = SaveMode.Instance;

        [SerializeField]
        public PaintMode paintMode = PaintMode.Brush;

        [SerializeField]
        public SelectionMode selectionMode = SelectionMode.Selected;

        [SerializeField]
        public LayerMask layerMask = (LayerMask)(int.MaxValue);

        [SerializeField]
        public bool erase;

        [SerializeField]
        public Color primaryColor = Color.red;
        [SerializeField]
        public Color secondaryColor = Color.clear;
        [SerializeField]
        public float brushSize = 1f;
        [SerializeField]
        public float brushHardness = 0.0f;
        [SerializeField]
        public float brushStrength = 1f;
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

        //Foldouts
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
            public bool uninstaller = false;
            [SerializeField]
            public bool questions = true;
            [SerializeField]
            public bool suggestions;
            [SerializeField]
            public bool bugs;
        }

        //Hotkeys
        public class HotkeysClass
        {
            [SerializeField]
            public IVPHotkey paint;
            [SerializeField]
            public IVPHotkey erase;
            [SerializeField]
            public IVPHotkey increaseSize;
            [SerializeField]
            public IVPHotkey decreaseSize;
            [SerializeField]
            public IVPHotkey showVertexColors;
            [SerializeField]
            public IVPHotkey copyVertexColors;
            [SerializeField]
            public IVPHotkey pasteVertexColors;
        }

        //Data
        bool setup = false;

        public void Setup()
        {
            if(setup) { return; }

            CreateHotkeys();
            setup = true;
        }

        void CreateHotkeys()
        {
            Hotkeys.paint = new IVPHotkey(KeyCode.None, EventModifiers.Control);
            Hotkeys.erase = new IVPHotkey(KeyCode.None, EventModifiers.Control | EventModifiers.Shift);
            Hotkeys.increaseSize = new IVPHotkey(KeyCode.KeypadPlus, EventModifiers.Control);
            Hotkeys.decreaseSize = new IVPHotkey(KeyCode.KeypadMinus, EventModifiers.Control);
            Hotkeys.showVertexColors = new IVPHotkey(KeyCode.X, EventModifiers.Control | EventModifiers.Alt);
            Hotkeys.copyVertexColors = new IVPHotkey(KeyCode.C, EventModifiers.Control | EventModifiers.Alt);
            Hotkeys.pasteVertexColors = new IVPHotkey(KeyCode.V, EventModifiers.Control | EventModifiers.Alt);
        }
    } 
}