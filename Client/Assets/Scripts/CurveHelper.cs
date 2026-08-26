using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Curve Helper", menuName = "Helpers/Curve Helper")]
public class CurveHelper : ScriptableObject
{
	public List<AnimationCurve> Curves;
}
