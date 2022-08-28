using UnityEngine;
using System.Collections;
using UnityEditor;
using Ikari.VertexPainter;

public class IVPGizmos : MonoBehaviour {
  public static void DrawHandle() {
    SceneView.RepaintAll();

    //if (IVPVariables.Raycast.HitData == null)
    //  return;
    var hitData = IVPVariables.Raycast.HitDataMain;
    if (IVPController.IVPData.solidHandle) {
      Handles.color = IVPController.IVPData.handleColor;
      Handles.DrawSolidDisc(hitData.point, hitData.normal, IVPController.IVPData.brushSize);
      Handles.color = IVPController.IVPData.handleColor;
      Handles.DrawSolidDisc(hitData.point, hitData.normal, IVPController.IVPData.brushSize);
    } else {
      Handles.color = IVPController.IVPData.handleColor;
      Handles.DrawWireDisc(hitData.point, hitData.normal, IVPController.IVPData.brushSize);
      Handles.color = IVPController.IVPData.handleColor;
      Handles.DrawWireDisc(hitData.point, hitData.normal, IVPController.IVPData.brushSize);
    }

    if (IVPController.IVPData.drawHandleOutline) {
      Handles.color = IVPController.IVPData.outlineHandleColor;
      Handles.DrawWireDisc(hitData.point, hitData.normal, IVPController.IVPData.brushSize - 0.005f);
    }

    if (IVPController.IVPData.drawHandleAngle) {
      Handles.Label(new Vector3(hitData.point.x - 0.12f, hitData.point.y + 0.25f, hitData.point.z), Vector3.Angle(hitData.normal, Vector3.up).ToString("#.##"), EditorStyles.largeLabel);
    }
  }
}
