using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

namespace Ikari.VertexPainter {
  public class IVPObjectData : MonoBehaviour {
    public Color[] vertexColors = new Color[] { Color.red, Color.green, Color.blue, new Color(0.85f, 0.85f, 0.85f, 1f) };

    public class EditorData {
      //Editor Data
      public bool foldout = false;
      public bool showingVertex = false;
      public bool showingRedColor = true;
      public bool showingGreenColor = true;
      public bool showingBlueColor = true;
      public bool showingAlphaColor = true;
      public bool showingWireframe = true;
    }

    public EditorData editorData = new EditorData();

    //Save Data
    public bool meshCreated = false;
    public bool prefabCreated = false;

    //Instance Mode
    public bool instance = false;
    public string meshName;
    public int instanceID;

    //Original Data
    public Material[] originalMaterials;
  }
}
#endif