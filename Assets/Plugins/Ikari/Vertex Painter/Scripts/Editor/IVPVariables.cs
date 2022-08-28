using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IVPVariables : MonoBehaviour {

  public static class Interaction {
    public static bool Painting;
  }

  public static class Raycast {
    public static bool Raycasting;
    public static Ray Ray;
    static RaycastHit[] hitData;
    static RaycastHit hitDataMain;
    public static RaycastHit[] HitData {
      get {
        if (hitData == null)
          hitData = new RaycastHit[0];
        return hitData;
      }
      set {
        hitData = value;
      }
    }

    public static RaycastHit HitDataMain {
      get {
        return hitDataMain;
      }
      set {
        hitDataMain = value;
      }
    }

    public static bool IsHitting() {
      if (HitData.Length > 0) {
        return true;
      } else {
        return false;
      }
    }


    public static void ResetHit() {
      HitData = new RaycastHit[0];
      HitDataMain = new RaycastHit();
    }
  }

  public static class Data {
    public static GameObject ActualObject;
    public static Mesh ActualMesh;
    public static GameObject[] SelectedObjects;
    public static List<GameObject> EditedObjects = new List<GameObject>();
    public static List<Color32> ColorClipboard = new List<Color32>();
  }

  public static class Gizmo {

  }
}
