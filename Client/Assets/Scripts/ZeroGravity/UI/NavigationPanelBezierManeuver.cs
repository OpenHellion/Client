namespace ZeroGravity.UI
{
	public class NavigationPanelBezierManeuver
	{
		public delegate void OnChangeDelegate(NavigationPanelBezierManeuver maneuver);

		public OnChangeDelegate OnChange;

		public NavigationPanelObject Object;

		public float StartPos;

		public float EndPos;

		public float Scale;
	}
}
