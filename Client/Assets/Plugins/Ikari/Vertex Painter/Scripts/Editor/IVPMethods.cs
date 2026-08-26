using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

public class IVPMethods : MonoBehaviour {
  public static class Path {
    public static string GetEditorPath(ScriptableObject editorScript) {
      MonoScript script = MonoScript.FromScriptableObject(editorScript);
      return AssetDatabase.GetAssetPath(script).Replace(string.Concat(script.name, ".cs"), "");
    }
  }

  public static class Reflection {
    public static string GetFieldName<TFieldSource>
      (Expression<Func<TFieldSource, object>> expression) {
      var lambda = expression as LambdaExpression;
      MemberExpression memberExpression;
      if (lambda.Body is UnaryExpression) {
        var unaryExpression = lambda.Body as UnaryExpression;
        memberExpression = unaryExpression.Operand as MemberExpression;
      } else {
        memberExpression = lambda.Body as MemberExpression;
      }

      Debug.Assert(memberExpression != null, "Please provide a lambda expression like 'n => n.PropertyName'");

      if (memberExpression != null) {
        var fieldInfo = memberExpression.Member as FieldInfo;

        return fieldInfo.Name;
      }

      return null;
    }
  }
}
