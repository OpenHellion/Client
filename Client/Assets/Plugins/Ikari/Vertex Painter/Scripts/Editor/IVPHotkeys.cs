using UnityEngine;
using System.Collections;
using Ikari.VertexPainter;

public class IVPHotkeys : MonoBehaviour {

  public static void DoHotkeys() {

  }

  void PaintHotkey() {
    if (IVPController.IVPData.Hotkeys.paint.enabled) {
      if (Event.current.modifiers == IVPController.IVPData.Hotkeys.paint.data.modifiers) {
        IVPVariables.Interaction.Painting = true;
        IVPVariables.Data.EditedObjects.Clear();
        IVPVariables.Raycast.Raycasting = true;
      } else {
        IVPVariables.Interaction.Painting = false;
        IVPVariables.Raycast.Raycasting = false;
        IVPVariables.Raycast.ResetHit();
      }
    }
  }

  void CopyVertexColors() {
    if (IVPController.IVPData.Hotkeys.copyVertexColors.enabled) {

    }
  }
}
