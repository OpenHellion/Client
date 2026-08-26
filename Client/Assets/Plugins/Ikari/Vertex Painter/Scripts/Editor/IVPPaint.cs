using UnityEngine;
using System.Collections;
using UnityEditor;
using Ikari.VertexPainter;
using System.Linq;

public class IVPPaint : MonoBehaviour {
  //Paint Variables
  static float posMagnitude;
  static Vector3 pos;
  static Color vColor = Color.white;
  static Vector3[] meshVertex;
  static Color[] meshColors;

  public static void DoPaint() {
    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && !Event.current.alt && IVPVariables.Raycast.IsHitting()) {
      if (IVPVariables.Raycast.HitDataMain.collider == null)
        return;

      for (int i = 0; i < IVPVariables.Raycast.HitData.Length; i++) {
        RaycastHit hitData = IVPVariables.Raycast.HitData[i];
        if (CanPaint(hitData)) {
          PrePaint(hitData);
          BrushPaint(hitData);
          BucketPaint(hitData);
        }
      }
    }
  }

  static void PrePaint(RaycastHit hitData) {
    //Get Data from the object.
    if (IVPVariables.Data.ActualObject == hitData.collider.gameObject) { return; }
    MeshFilter meshFilter = hitData.collider.gameObject.GetComponent<MeshFilter>();
    SkinnedMeshRenderer skinMeshRenderer = hitData.collider.gameObject.GetComponent<SkinnedMeshRenderer>();
    IVPObjectData objectProperties = hitData.collider.gameObject.GetComponent<IVPObjectData>();

    //Add Object Properties if it's missing
    if (objectProperties == null) {
      objectProperties = hitData.collider.gameObject.AddComponent<IVPObjectData>();
      objectProperties.instanceID = objectProperties.GetInstanceID();
    }

    //Instance
    if (IVPController.IVPData.saveMode == SaveMode.Instance) {
      Mesh meshInstance = null;

      if (meshFilter != null) {
        if (!objectProperties.instance || objectProperties.instance && objectProperties.instanceID != objectProperties.GetInstanceID()) {
          meshFilter.sharedMesh = Instantiate(meshFilter.sharedMesh) as Mesh;
          meshInstance = meshFilter.sharedMesh;
          meshInstance.name = meshFilter.sharedMesh.name;
          objectProperties.instance = true;
        } else {
          meshInstance = meshFilter.sharedMesh;
        }

        IVPVariables.Data.ActualMesh = meshInstance;
      } else if (skinMeshRenderer != null) {
        if (!objectProperties.instance || objectProperties.instance && objectProperties.instanceID != objectProperties.GetInstanceID()) {
          meshInstance = Instantiate(skinMeshRenderer.sharedMesh) as Mesh;
          meshInstance.name = skinMeshRenderer.sharedMesh.name;
          objectProperties.instance = true;
        } else {
          meshInstance = skinMeshRenderer.sharedMesh;
        }
      }

      objectProperties.instanceID = objectProperties.GetInstanceID();
      objectProperties.instance = true;
    }

    //Asset
    if (IVPController.IVPData.saveMode == SaveMode.Asset) {
      Mesh meshClone = null;

      if (meshFilter != null) {
        meshClone = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
      } else if (skinMeshRenderer != null) {
        meshClone = Mesh.Instantiate(skinMeshRenderer.sharedMesh) as Mesh;
      }

      meshClone.name = meshFilter.sharedMesh.name;
      objectProperties.instanceID = objectProperties.GetInstanceID();
      objectProperties.instance = true;
    }
  }

  static void BrushPaint(RaycastHit hitData) {
    if (IVPController.IVPData.paintMode != PaintMode.Brush) { return; }

    meshVertex = IVPVariables.Data.ActualMesh.vertices;
    meshColors = IVPVariables.Data.ActualMesh.colors;

    if (IVPVariables.Data.ActualMesh.colors.Length != IVPVariables.Data.ActualMesh.vertices.Length) {
      IVPVariables.Data.ActualMesh.colors = new Color[IVPVariables.Data.ActualMesh.vertexCount];
      meshColors = IVPVariables.Data.ActualMesh.colors;
    }

    pos = hitData.collider.transform.InverseTransformPoint(IVPVariables.Raycast.HitDataMain.point);

    if (Vector3.Angle(IVPVariables.Raycast.HitDataMain.normal, Vector3.up) <= IVPController.IVPData.brushAngleLimit) {
      for (int i = 0; i < meshVertex.Length; i++) {
        posMagnitude = (meshVertex[i] - pos).magnitude * hitData.collider.transform.localScale.magnitude;

        float distFade = 1 - (posMagnitude / (IVPController.IVPData.brushSize * 2));
        if (distFade < 0.0f)
          continue;

        float distFadePow = Mathf.Lerp(4, 0.1f, IVPController.IVPData.brushHardness);
        float hardnessFade = Mathf.Pow(distFade, distFadePow);

        vColor = Color.Lerp(
          meshColors[i],
          IVPController.IVPData.primaryColor,
          IVPController.IVPData.brushStrength * hardnessFade);
        if (IVPController.IVPData.rChannel)
          meshColors[i].r = vColor.r;
        if (IVPController.IVPData.gChannel)
          meshColors[i].g = vColor.g;
        if (IVPController.IVPData.bChannel)
          meshColors[i].b = vColor.b;
        if (IVPController.IVPData.aChannel)
          meshColors[i].a = vColor.a;
      }
      Undo.RegisterCompleteObjectUndo(IVPVariables.Data.ActualMesh, "Vertex Paint");
      IVPVariables.Data.ActualMesh.colors = meshColors;
      EditorUtility.SetDirty(IVPVariables.Data.ActualMesh);
    }
  }

  static void BucketPaint(RaycastHit hitData) {
    if (IVPController.IVPData.paintMode != PaintMode.Bucket) { return; }

    meshVertex = IVPVariables.Data.ActualMesh.vertices;
    meshColors = IVPVariables.Data.ActualMesh.colors;

    if (IVPVariables.Data.ActualMesh.colors.Length != IVPVariables.Data.ActualMesh.vertexCount) {
      IVPVariables.Data.ActualMesh.colors = new Color[IVPVariables.Data.ActualMesh.vertexCount];
      meshColors = IVPVariables.Data.ActualMesh.colors;
    }

    pos = hitData.collider.transform.InverseTransformPoint(IVPVariables.Raycast.HitDataMain.point);

    if (Vector3.Angle(IVPVariables.Raycast.HitDataMain.normal, Vector3.up) <= IVPController.IVPData.brushAngleLimit) {
      for (int i = 0; i < meshVertex.Length; i++) {
        posMagnitude = (meshVertex[i] - pos).magnitude;

        if (posMagnitude > IVPController.IVPData.bucketSize) continue;

        if (meshColors[i] == Color.red && IVPController.IVPData.rChannel) {
          for (int r = 0; r < meshColors.Length; r++) {
            if (meshColors[r] == Color.red) {
              Color newColor = Color.Lerp(meshColors[r], IVPController.IVPData.primaryColor, 1);
              meshColors[r] = newColor;
            }
          }
        }

        if (meshColors[i] == Color.green && IVPController.IVPData.gChannel) {
          for (int g = 0; g < meshColors.Length; g++) {
            if (meshColors[g] == Color.green) {
              Color newColor = Color.Lerp(meshColors[g], IVPController.IVPData.primaryColor, 1);
              meshColors[g] = newColor;
            }
          }
        }

        if (meshColors[i] == Color.blue && IVPController.IVPData.bChannel) {
          for (int b = 0; b < meshColors.Length; b++) {
            if (meshColors[b] == Color.blue) {
              Color newColor = Color.Lerp(meshColors[b], IVPController.IVPData.primaryColor, 1);
              meshColors[b] = newColor;
            }
          }
        }

        if (meshColors[i] == Color.clear && IVPController.IVPData.aChannel) {
          for (int a = 0; a < meshColors.Length; a++) {
            if (meshColors[a].a == 0) {
              Color newColor = Color.Lerp(meshColors[a], IVPController.IVPData.primaryColor, 1);
              meshColors[a] = newColor;
            }
          }
        }
      }
      IVPVariables.Data.ActualMesh.colors = meshColors;
    }
  }

  static bool CanPaint(RaycastHit hitData) {
    if (IVPController.IVPData.selectionMode == Ikari.VertexPainter.SelectionMode.All) {
      return true;
    } else if (IVPController.IVPData.selectionMode == Ikari.VertexPainter.SelectionMode.Selected) {
      for (int i = 0; i < IVPVariables.Data.SelectedObjects.Length; i++) {
        if (IVPVariables.Data.SelectedObjects[i] == hitData.collider.gameObject) {
          return true;
        }
      }
      return false;
    } else {
      return false;
    }
  }
}