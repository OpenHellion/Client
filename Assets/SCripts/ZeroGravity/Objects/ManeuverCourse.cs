using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public class ManeuverCourse : MonoBehaviour
	{
		public enum FeasibilityErrorType
		{
			None = 0,
			Acceleration_Low = 1,
			Acceleration_High = 2,
			FTL_ManeuverIndex = 3,
			FTL_CellFuel = 4,
			FTL_Capacity = 5,
			FTL_Online = 6,
			Course_Impossible = 7,
			ToManyDockedVessels = 8
		}

		public OrbitParameters MyShipOrbit;

		public OrbitParameters StartOrbit = new OrbitParameters();

		public ManeuverTransfer Transfer = new ManeuverTransfer();

		public OrbitParameters EndOrbit;

		public SpaceObjectVessel TargetVessel;

		public LineRenderer ManeuverLine;

		public Color WarpFeasibleColor = Colors.Green;

		public Color WarpNotFeasibleColor = Colors.Red;

		public Transform StartPosition;

		public Transform EndPosition;

		private long nextCourseGUID = 100L;

		public long GUID;

		public string Name;

		public static long NextGUID = 1L;

		public bool Initialized;

		private bool courseLocked;

		public bool IsDragging;

		public double StartTime = 60.0;

		public double EndTime = 240.0;

		private double startTimeSincePeriapsis;

		private double endTimeSincePeriapsis;

		private double TimeToStart;

		private double TimeToTarget;

		private bool isPossible;

		private bool isFeasible;

		public bool ManeuverError;

		public bool WarpCellError;

		public bool PowerError;

		public bool DockingStructureError;

		public double ManeuverDistance;

		public float FuelAmountRequired;

		public Transform DraggingParameter;

		private Vector3 periapsisPosition;

		private double oldRayAngle;

		private double oldTime;

		[CompilerGenerated]
		private static Func<SceneDockingPort, bool> _003C_003Ef__am_0024cache0;

		public Map Map
		{
			get
			{
				return Client.Instance.Map;
			}
		}

		public double ObjectScale
		{
			get
			{
				return Map.Scale / 149597870700.0;
			}
		}

		private Ship myShip
		{
			get
			{
				return Map.MyShip.MainObject as Ship;
			}
		}

		public bool Locked
		{
			get
			{
				return courseLocked;
			}
		}

		public float ManeuverETA
		{
			get
			{
				UpdateManeuver();
				return Transfer.EndEta;
			}
		}

		public bool IsPossibleAndFeasible
		{
			get
			{
				return isPossible && isFeasible;
			}
		}

		public FeasibilityErrorType FeasibilityError { get; private set; }

		public ManeuverCourse()
		{
			GUID = nextCourseGUID++;
		}

		private void Update()
		{
			UpdateManeuver();
		}

		public void Initialize()
		{
			StartOrbit.InitFromPeriapisAndApoapsis(MyShipOrbit.Parent, MyShipOrbit.PeriapsisDistance, MyShipOrbit.ApoapsisDistance, MyShipOrbit.Inclination, MyShipOrbit.ArgumentOfPeriapsis, MyShipOrbit.LongitudeOfAscendingNode, MyShipOrbit.TrueAnomalyAtZeroTime() * (180.0 / System.Math.PI), 0.0);
			StartOrbit.ResetOrbit(Client.Instance.SolarSystem.CurrentTime);
			startTimeSincePeriapsis = StartOrbit.TimeSincePeriapsis;
			endTimeSincePeriapsis = EndOrbit.TimeSincePeriapsis;
			StartTime = 60.0;
			EndTime = 300.0;
		}

		private void UpdateManeuver()
		{
			StartOrbit.ResetOrbit(Client.Instance.SolarSystem.CurrentTime);
			Transfer.StartOrbitAngle = (float)(StartOrbit.TrueAnomalyAtZeroTimePlusTime(startTimeSincePeriapsis + StartTime) * (180.0 / System.Math.PI));
			Transfer.EndOrbitAngle = (float)(EndOrbit.TrueAnomalyAtZeroTimePlusTime(endTimeSincePeriapsis + EndTime) * (180.0 / System.Math.PI));
			TimeToStart = startTimeSincePeriapsis + StartTime - StartOrbit.TimeSincePeriapsis;
			TimeToTarget = endTimeSincePeriapsis + EndTime - EndOrbit.TimeSincePeriapsis;
			Transfer.StartSolarSystemTime = Client.Instance.SolarSystem.CurrentTime + TimeToStart;
			Transfer.EndSolarSystemTime = Client.Instance.SolarSystem.CurrentTime + TimeToTarget;
			Vector3 position = ((StartOrbit.PositionAtTrueAnomaly((double)Transfer.StartOrbitAngle * (System.Math.PI / 180.0), true) + (StartOrbit.Parent.Position - Map.Focus)) * ObjectScale).ToVector3();
			Vector3 position2 = ((EndOrbit.PositionAtTrueAnomaly((double)Transfer.EndOrbitAngle * (System.Math.PI / 180.0), true) + (EndOrbit.Parent.Position - Map.Focus)) * ObjectScale).ToVector3();
			ManeuverLine.SetPosition(0, position);
			ManeuverLine.SetPosition(1, position2);
			StartPosition.position = position;
			EndPosition.position = position2;
			FuelAmountRequired = 0f;
			isPossible = true;
			isFeasible = true;
			FeasibilityError = FeasibilityErrorType.None;
			ManeuverError = false;
			WarpCellError = false;
			PowerError = false;
			DockingStructureError = false;
			float num = (float)(EndTime - StartTime);
			TimeToStart = startTimeSincePeriapsis + StartTime - StartOrbit.TimeSincePeriapsis;
			TimeToTarget = TimeToStart + (double)num;
			if (TimeToStart < 0.0)
			{
				TimeToStart += StartOrbit.OrbitalPeriod;
			}
			if (TimeToTarget < 0.0)
			{
				TimeToTarget += EndOrbit.OrbitalPeriod;
			}
			Transfer.StartEta = (float)TimeToStart;
			Transfer.EndEta = (float)TimeToTarget;
			Transfer.TravelTime = num;
			Transfer.Type = ManeuverType.Warp;
			if (num <= 0f)
			{
				InvalidateTransfer();
				return;
			}
			bool getRelativePosition = StartOrbit.Parent == EndOrbit.Parent;
			Vector3D vector3D = StartOrbit.PositionAfterTime(TimeToStart, getRelativePosition);
			Vector3D vector3D2 = EndOrbit.PositionAfterTime(TimeToTarget, getRelativePosition);
			ManeuverDistance = Vector3D.Distance(vector3D, vector3D2);
			if (myShip.GetCompoundMass() > (double)(myShip.Mass + myShip.FTLEngine.TowingCapacity) || (Transfer.WarpIndex == 0 && myShip.MainVessel.AllDockedVessels.Count > 0))
			{
				isFeasible = false;
				FeasibilityError = FeasibilityErrorType.ToManyDockedVessels;
				DockingStructureError = true;
			}
			else
			{
				DockingStructureError = false;
			}
			if (myShip.SceneID == GameScenes.SceneID.AltCorp_Shuttle_SARA)
			{
				Dictionary<int, SceneDockingPort>.ValueCollection values = myShip.DockingPorts.Values;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CUpdateManeuver_003Em__0;
				}
				SceneDockingPort sceneDockingPort = values.FirstOrDefault(_003C_003Ef__am_0024cache0);
				if (sceneDockingPort != null && sceneDockingPort.DockedToPort != null)
				{
					isFeasible = false;
					FeasibilityError = FeasibilityErrorType.ToManyDockedVessels;
					DockingStructureError = true;
				}
				else
				{
					DockingStructureError = false;
				}
			}
			float num2 = (float)(myShip.GetCompoundMass() / (double)myShip.Mass);
			if (myShip.FTLEngine.WarpsData.Length <= Transfer.WarpIndex)
			{
				isFeasible = false;
				FeasibilityError = FeasibilityErrorType.FTL_ManeuverIndex;
				WarpCellError = true;
			}
			else
			{
				WarpCellError = false;
			}
			WarpData warpData = myShip.FTLEngine.WarpsData[Transfer.WarpIndex];
			FuelAmountRequired = warpData.ActivationCellConsumption + num * warpData.CellConsumption * num2;
			int num3 = 0;
			foreach (KeyValuePair<int, float?> item in myShip.FTLEngine.WarpCellsFuel)
			{
				if (item.Value.HasValue)
				{
					num3++;
				}
			}
			float num4 = 0f;
			foreach (int warpCell in Transfer.WarpCells)
			{
				num4 += ((!myShip.FTLEngine.WarpCellsFuel.ContainsKey(warpCell) || !myShip.FTLEngine.WarpCellsFuel[warpCell].HasValue) ? 0f : myShip.FTLEngine.WarpCellsFuel[warpCell].Value);
			}
			if (num4 < FuelAmountRequired)
			{
				isFeasible = false;
				FeasibilityError = FeasibilityErrorType.FTL_CellFuel;
				WarpCellError = true;
			}
			else
			{
				WarpCellError = false;
			}
			if (warpData.PowerConsumption > myShip.Capacitor.Capacity)
			{
				isFeasible = false;
				FeasibilityError = FeasibilityErrorType.FTL_Capacity;
				PowerError = true;
			}
			else
			{
				PowerError = false;
			}
			double num5 = 4.0 * (vector3D - vector3D2).Magnitude / (double)(num * num);
			if (num5 > (double)warpData.MaxAcceleration)
			{
				isFeasible = false;
				FeasibilityError = FeasibilityErrorType.Acceleration_Low;
				ManeuverError = true;
			}
			else if (num5 < (double)warpData.MinAcceleration)
			{
				isFeasible = false;
				FeasibilityError = FeasibilityErrorType.Acceleration_High;
				ManeuverError = true;
			}
			else
			{
				ManeuverError = false;
			}
			if (!isFeasible)
			{
			}
			if (IsPossibleAndFeasible)
			{
				ManeuverLine.startColor = WarpFeasibleColor;
				ManeuverLine.endColor = WarpFeasibleColor;
			}
			else
			{
				ManeuverLine.startColor = WarpNotFeasibleColor;
				ManeuverLine.endColor = WarpNotFeasibleColor;
			}
		}

		private void InvalidateTransfer()
		{
			isPossible = false;
			ManeuverLine.startColor = WarpNotFeasibleColor;
			ManeuverLine.endColor = WarpNotFeasibleColor;
			FeasibilityError = FeasibilityErrorType.Course_Impossible;
		}

		public void AddStartTime(float time)
		{
			StartTime += time;
		}

		public void AddEndTime(float time)
		{
			EndTime += time;
		}

		public void StopDragging()
		{
			IsDragging = false;
		}

		public void StartDragging(GameObject dragObject, Ray ray)
		{
			DraggingParameter = dragObject.transform;
			Vector3 zero = Vector3.zero;
			Vector3D centerPosition = Vector3D.Zero;
			QuaternionD rotation = QuaternionD.Identity;
			Vector3 zero2 = Vector3.zero;
			if (dragObject == StartPosition.gameObject)
			{
				StartOrbit.GetOrbitPlaneData(out rotation, out centerPosition);
				zero2 = ((StartOrbit.Parent.Position - Map.Focus) * ObjectScale).ToVector3();
				periapsisPosition = ((StartOrbit.PositionAtTrueAnomaly(0.0, false) - Map.Focus) * ObjectScale).ToVector3();
				zero = MathHelper.RayPlaneIntersect(ray, zero2, rotation.ToQuaternion() * Vector3.up);
				oldRayAngle = MathHelper.AngleSigned((zero2 - zero).normalized, (zero2 - periapsisPosition).normalized, rotation.ToQuaternion() * Vector3.up);
				if (oldRayAngle < 0.0)
				{
					oldRayAngle += 360.0;
				}
				if (oldRayAngle > 360.0)
				{
					oldRayAngle -= 360.0;
				}
				oldRayAngle = StartOrbit.CalculatesMeanAnomalyFromTrueAnomaly(oldRayAngle * (System.Math.PI / 180.0)) * (180.0 / System.Math.PI);
				oldTime = oldRayAngle * StartOrbit.OrbitalPeriod / 360.0 - startTimeSincePeriapsis;
			}
			else if (dragObject == EndPosition.gameObject)
			{
				EndOrbit.GetOrbitPlaneData(out rotation, out centerPosition);
				zero2 = ((EndOrbit.Parent.Position - Map.Focus) * ObjectScale).ToVector3();
				periapsisPosition = ((EndOrbit.PositionAtTrueAnomaly(0.0, false) - Map.Focus) * ObjectScale).ToVector3();
				zero = MathHelper.RayPlaneIntersect(ray, zero2, rotation.ToQuaternion() * Vector3.up);
				oldRayAngle = MathHelper.AngleSigned((zero2 - zero).normalized, (zero2 - periapsisPosition).normalized, rotation.ToQuaternion() * Vector3.up);
				if (oldRayAngle < 0.0)
				{
					oldRayAngle += 360.0;
				}
				if (oldRayAngle > 360.0)
				{
					oldRayAngle -= 360.0;
				}
				oldRayAngle = EndOrbit.CalculatesMeanAnomalyFromTrueAnomaly(oldRayAngle * (System.Math.PI / 180.0)) * (180.0 / System.Math.PI);
				oldTime = oldRayAngle * EndOrbit.OrbitalPeriod / 360.0 - endTimeSincePeriapsis;
			}
			IsDragging = true;
		}

		public void Dragging(Ray ray)
		{
			Vector3 zero = Vector3.zero;
			Vector3D centerPosition = Vector3D.Zero;
			QuaternionD rotation = QuaternionD.Identity;
			Vector3 zero2 = Vector3.zero;
			if (DraggingParameter.gameObject == StartPosition.gameObject)
			{
				StartOrbit.GetOrbitPlaneData(out rotation, out centerPosition);
				zero2 = ((StartOrbit.Parent.Position - Map.Focus) * ObjectScale).ToVector3();
				periapsisPosition = ((StartOrbit.PositionAtTrueAnomaly(0.0, false) - Map.Focus) * ObjectScale).ToVector3();
				zero = MathHelper.RayPlaneIntersect(ray, zero2, rotation.ToQuaternion() * Vector3.up);
				double num = MathHelper.AngleSigned((zero2 - zero).normalized, (zero2 - periapsisPosition).normalized, rotation.ToQuaternion() * Vector3.up);
				if (num < 0.0)
				{
					num += 360.0;
				}
				if (num > 360.0)
				{
					num -= 360.0;
				}
				num = StartOrbit.CalculatesMeanAnomalyFromTrueAnomaly(num * (System.Math.PI / 180.0)) * (180.0 / System.Math.PI) - oldRayAngle;
				StartTime = num * StartOrbit.OrbitalPeriod / 360.0 + oldTime;
			}
			else if (DraggingParameter.gameObject == EndPosition.gameObject)
			{
				EndOrbit.GetOrbitPlaneData(out rotation, out centerPosition);
				zero2 = ((EndOrbit.Parent.Position - Map.Focus) * ObjectScale).ToVector3();
				periapsisPosition = ((EndOrbit.PositionAtTrueAnomaly(0.0, false) - Map.Focus) * ObjectScale).ToVector3();
				zero = MathHelper.RayPlaneIntersect(ray, zero2, rotation.ToQuaternion() * Vector3.up);
				double num2 = MathHelper.AngleSigned((zero2 - zero).normalized, (zero2 - periapsisPosition).normalized, rotation.ToQuaternion() * Vector3.up);
				if (num2 < 0.0)
				{
					num2 += 360.0;
				}
				if (num2 > 360.0)
				{
					num2 -= 360.0;
				}
				num2 = EndOrbit.CalculatesMeanAnomalyFromTrueAnomaly(num2 * (System.Math.PI / 180.0)) * (180.0 / System.Math.PI) - oldRayAngle;
				EndTime = num2 * EndOrbit.OrbitalPeriod / 360.0 + oldTime;
			}
		}

		[CompilerGenerated]
		private static bool _003CUpdateManeuver_003Em__0(SceneDockingPort m)
		{
			return m.DockingPortOrder == 1;
		}
	}
}
