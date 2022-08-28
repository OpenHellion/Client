using UnityEngine;
using UnityEditor;
using Ikari.VertexPainter;

[CustomEditor(typeof(IVPObjectData))]
public class IVPObjectDataEditor : Editor {

  IVPObjectData script;

  void OnEnable() {
    script = (IVPObjectData)target;
  }

  public override void OnInspectorGUI() {
    if (GUILayout.Button("Create Prefab")) {
      IVPSave.CreatePrefabFromInstance(script.gameObject);
    }
  }

  static public void UninstallIt() {

    IVPObjectData[] objects = FindObjectsOfType(typeof(IVPObjectData)) as IVPObjectData[];
    foreach (IVPObjectData op in objects) {
      DestroyImmediate(op);
    }
  }

}