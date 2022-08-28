using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Linq.Expressions;
using Ikari.VertexPainter;
using System.Reflection;

public class IVPInterface : EditorWindow {
  public static float DrawFloatField(string labelName, float value) {
    EditorGUI.BeginChangeCheck();
    float tempValue = 0;
    tempValue = EditorGUILayout.FloatField(string.Concat(labelName, ": "), value);
    if (EditorGUI.EndChangeCheck()) {
      Undo.RecordObject(IVPController.IVPData, labelName);
      value = tempValue;
      EditorUtility.SetDirty(IVPController.IVPData);
    }
    return value;
  }

  public static float DrawSlider(float value, float minValue, float maxValue, string undoName) {
    EditorGUI.BeginChangeCheck();
    float tempValue = 0;
    tempValue = GUILayout.HorizontalSlider(value, minValue, maxValue);
    if (EditorGUI.EndChangeCheck()) {
      Undo.RecordObject(IVPController.IVPData, undoName);
      value = tempValue;
      EditorUtility.SetDirty(IVPController.IVPData);
    }
    return value;
  }

  public static void DrawColorToggle(string variableName, string undoName, string labelName, bool value, Color color) {
    if ((bool)IVPController.IVPData.GetType().GetField(variableName).GetValue(IVPController.IVPData)) {
      GUI.color = color;
    }
    if (GUILayout.Button(labelName, EditorStyles.toggle, GUILayout.ExpandWidth(false))) {
      value = !(bool)IVPController.IVPData.GetType().GetField(variableName).GetValue(IVPController.IVPData);
      Undo.RecordObject(IVPController.IVPData, undoName);
      IVPController.IVPData.GetType().GetField(variableName).SetValue(IVPController.IVPData, value);
      EditorUtility.SetDirty(IVPController.IVPData);
    }
    GUI.color = Color.white;
  }

  public static void DrawToggle(string variableName, string undoName, string labelName, bool value) {
    if (GUILayout.Button(labelName, EditorStyles.toggle, GUILayout.ExpandWidth(false))) {
      value = !(bool)IVPController.IVPData.GetType().GetField(variableName).GetValue(IVPController.IVPData);
      Undo.RecordObject(IVPController.IVPData, undoName);
      IVPController.IVPData.GetType().GetField(variableName).SetValue(IVPController.IVPData, value);
      EditorUtility.SetDirty(IVPController.IVPData);
    }
  }

  public static string DrawSearchBar(string text) {
    GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
    text = GUILayout.TextField(text, "ToolbarSeachTextField");
    if (GUILayout.Button("", "ToolbarSeachCancelButton")) {
      text = "";
      GUI.FocusControl(null);
    }
    GUILayout.EndHorizontal();
    GUILayout.Space(0);
    return text;
  }
}
