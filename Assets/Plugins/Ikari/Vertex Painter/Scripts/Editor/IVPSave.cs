using UnityEngine;
using System.Collections;
using UnityEditor;
using Ikari.VertexPainter;

public class IVPSave : MonoBehaviour {

  public static void CreatePrefabFromInstance(GameObject go) {
    string pathOfPrefab = CreatePrefab(go);
    string pathOfMesh = CreateMeshAsset(go);
    LinkMeshToPrefab(pathOfPrefab, pathOfMesh);
  }

  static string CreatePrefab(GameObject go) {
    string filePath = EditorUtility.SaveFilePanelInProject("Save Prefab", "", "prefab", "");

    if (!string.IsNullOrEmpty(filePath)) {
      GameObject prefabToOverwrite = AssetDatabase.LoadMainAssetAtPath(filePath) as GameObject;

      if (prefabToOverwrite != null) {
        PrefabUtility.ReplacePrefab(prefabToOverwrite, go);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      } else {
        PrefabUtility.CreatePrefab(filePath, go);
      }
    }

    //Set original materials
    GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject));
    prefab.GetComponent<Renderer>().sharedMaterials = prefab.GetComponent<IVPObjectData>().originalMaterials;

    return filePath;
  }

  static string CreateMeshAsset(GameObject go) {
    string filePath = EditorUtility.SaveFilePanelInProject("Save Mesh", "", "asset", "");

    if (!string.IsNullOrEmpty(filePath)) {
      Mesh editedMesh = go.GetComponent<MeshFilter>().sharedMesh;
      Mesh meshToOverwrite = AssetDatabase.LoadMainAssetAtPath(filePath) as Mesh;

      if (meshToOverwrite != null) {
        string guid = new System.Guid().ToString();
        string tempPath = filePath.Replace(".asset", guid);

        AssetDatabase.CreateAsset(editedMesh, tempPath);
        FileUtil.ReplaceFile(tempPath, filePath);
        AssetDatabase.DeleteAsset(tempPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      } else {
        AssetDatabase.CreateAsset(editedMesh, filePath);
      }
    }
    return filePath;
  }

  static void LinkMeshToPrefab(string pathOfPrefab, string pathOfMesh) {
    GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(pathOfPrefab, typeof(GameObject));
    Mesh mesh = AssetDatabase.LoadAssetAtPath(pathOfMesh, typeof(Mesh)) as Mesh;

    prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
  }

  /*public static void ExportToDae(Mesh mesh, string pathOfFile)
  {
      Grendgine_Collada collada = Grendgine_Collada.Grendgine_Load_File(pathOfFile);
      string colorsAsString = "";

      if (collada.Library_Geometries.Geometry[0].Mesh.Source[0].ID == "Col")
      {
          for (int i = 0; i < mesh.colors32.Length; i++)
          {
              colorsAsString += mesh.colors32[i].r + " ";
              colorsAsString += mesh.colors32[i].g + " ";
              colorsAsString += mesh.colors32[i].b + " ";
          }

          collada.Library_Geometries.Geometry[0].Mesh.Source[0].Float_Array.Value_As_String = colorsAsString;
      }
  }*/

  public static void Save() {
    if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
      if (IVPController.IVPData.saveMode == SaveMode.Instance) {
        for (int i = 0; i < IVPVariables.Data.EditedObjects.Count; i++) {
          EditorUtility.SetDirty(IVPVariables.Data.EditedObjects[i]);
        }
      }

      if (IVPController.IVPData.saveMode == SaveMode.Asset) {
        for (int i = 0; i < IVPVariables.Data.EditedObjects.Count; i++) {
          if (string.IsNullOrEmpty(IVPVariables.Data.EditedObjects[i].GetComponent<IVPObjectData>().meshName)) {
            var filePath = EditorUtility.SaveFilePanelInProject("Save Mesh as Asset", "", "asset", "");

            if (!string.IsNullOrEmpty(filePath)) {
              var editedObject = IVPVariables.Data.EditedObjects[i];
              var editedMesh = editedObject.GetComponent<MeshFilter>().sharedMesh;
              Mesh createdMesh = AssetDatabase.LoadMainAssetAtPath(filePath) as Mesh;
              if (createdMesh != null) {

                AssetDatabase.CreateAsset(editedMesh, filePath.Replace(".asset", "temporalFileIkariVertex132.asset"));
                FileUtil.ReplaceFile(filePath.Replace(".asset", "temporalFileIkariVertex132.asset"), filePath);
                AssetDatabase.DeleteAsset(filePath.Replace(".asset", "temporalFileIkariVertex132.asset"));
                editedObject.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath(filePath, typeof(Mesh)) as Mesh;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
              } else {
                AssetDatabase.CreateAsset(editedMesh, filePath);
              }
              editedObject.GetComponent<IVPObjectData>().meshCreated = true;
              int pos = filePath.LastIndexOf("/") + 1;
              editedObject.GetComponent<IVPObjectData>().meshName = filePath.Substring(pos, filePath.Length - pos);
              AssetDatabase.LoadAssetAtPath(filePath, typeof(Mesh)).name = filePath.Substring(pos, filePath.Length - pos).Replace(".asset", "");
            }

          }
        }
      }
      Resources.UnloadUnusedAssets();
    }
  }
}
