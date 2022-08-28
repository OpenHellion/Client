using UnityEngine;
using System.Collections;
using System.Linq;

public class IVPEdit : MonoBehaviour {

  public static void CopyVertexColors() {
    for (int i = 0; i < IVPVariables.Data.SelectedObjects.Length; i++) {
      IVPVariables.Data.ColorClipboard = IVPVariables.Data.SelectedObjects[i].GetComponent<MeshFilter>().sharedMesh.colors32.ToList();
    }
  }
}
