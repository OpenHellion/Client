using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;
using Debug = System.Diagnostics.Debug;

namespace ZeroGravity
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class Localization
	{
		public enum StandardInteractionTip
		{
			None = 0,
			Rifles = 1,
			Helmets = 2,
			Jetpacks = 3,
			Suits = 4,
			Ammo = 5,
			Handguns = 6,
			Grenades = 7,
			Canisters = 8,
			Gravity = 9,
			ExitCryo = 10,
			EnterCryo = 11,
			Recycler = 12,
			Piloting = 13,
			Turret = 14,
			Medpack = 15,
			Small = 16,
			Medium = 17,
			Large = 18,
			FireExtinguisher = 19,
			BasketballHoop = 20,
			Poster = 21,
			HackingDescription = 22,
			ResearchTable = 23,
			Undock = 24,
			Docking = 25
		}

		public static Dictionary<int, string> LocalizationFiles;

		private static string defaultValues;

		public static string[] PreloadText;

		public static string Preload01;

		public static string Preload02;

		public static string Preload03;

		public static string Preload04;

		public static string Preload05;

		public static string Preload06;

		public static string Preload07;

		public static string Preload08;

		public static string Preload09;

		public static string Preload10;

		public static string Preload11;

		public static string Preload12;

		public static string Preload13;

		public static string Preload14;

		public static string Preload15;

		public static string Preload16;

		public static string Preload17;

		public static string Preload18;

		public static string Preload19;

		public static string Preload20;

		public static string Preload21;

		public static string Preload22;

		public static string Preload23;

		public static string Preload24;

		public static string Preload25;

		public static string Preload26;

		public static string Preload27;

		public static string Preload28;

		public static string Preload29;

		public static string Preload30;

		public static string Preload31;

		public static string Preload32;

		public static string Preload33;

		public static string Preload34;

		public static string Preload35;

		public static string Preload36;

		public static string Preload37;

		public static string Preload38;

		public static string Preload39;

		public static string Preload40;

		public static string Preload41;

		public static string Preload42;

		public static string Preload43;

		public static string Preload44;

		public static string Preload45;

		public static string Preload46;

		public static string Preload47;

		public static string Preload48;

		public static string ConnectingToMain;

		public static string ConnectingToGame;

		public static string Connect;

		public static string Play;

		public static string PlaySP;

		public static string Options;

		public static string Quit;

		public static string Apply;

		public static string Disclaimer;

		public static string Welcome;

		public static string Understand;

		public static string ReadMore;

		public static string LatestNews;

		public static string AdvancedVideoSettings;

		public static string MouseSettings;

		public static string KeyboardSettings;

		public static string AltKey;

		public static string AmbientOcclusion;

		public static string AntiAliasing;

		public static string Audio;

		public static string AutoStabilization;

		public static string Back;

		public static string BasicVideoSettings;

		public static string BasicAudioSettings;

		public static string Bloom;

		public static string Cancel;

		public static string Character;

		public static string ChromaticAberration;

		public static string Community;

		public static string Confirm;

		public static string Controls;

		public static string CreateCharacter;

		public static string CurrentServer;

		public static string PleaseSelectServer;

		public static string Default;

		public static string EADisclaimer;

		public static string EnterCustomBoxName;

		public static string EnterPassword;

		public static string Exit;

		public static string EyeAdaptation;

		public static string F1ForHelp;

		public static string Favorites;

		public static string FullScreen;

		public static string Full;

		public static string GameSettings;

		public static string GeneralSettings;

		public static string Glossary;

		public static string HeadBobStrength;

		public static string HideTips;

		public static string HideTutorial;

		public static string Interact;

		public static string Key;

		public static string Loading;

		public static string Logout;

		public static string MainMenu;

		public static string MasterVolume;

		public static string MotionBlur;

		public static string Official;

		public static string Ping;

		public static string Players;

		public static string PreAplha;

		public static string Quality;

		public static string Refresh;

		public static string Resolution;

		public static string Respawn;

		public static string Resume;

		public static string Save;

		public static string Search;

		public static string ServerBrowser;

		public static string Server;

		public static string Settings;

		public static string Shadows;

		public static string ShipSettings;

		public static string ShowCrosshair;

		public static string TextureQuality;

		public static string Throwing;

		public static string Use;

		public static string Username;

		public static string VSync;

		public static string Video;

		public static string VoiceVolume;

		public static string LanguageSettings;

		public static string ChooseLanguage;

		public static string ReportServer;

		public static string ServerSettings;

		public static string PlayerSettings;

		public static string GlobalSettings;

		public static string Autosave;

		public static string NewGame;

		public static string NewGameDescription;

		public static string SinglePlayer;

		public static string SinglePlayerModeDescription;

		public static string DeleteSaveGame;

		public static string AreYouSureDeleteSaveGame;

		public static string Multiplayer;

		public static string JoinDiscord;

		public static string Movement;

		public static string Actions;

		public static string Ship;

		public static string Suit;

		public static string Communications;

		public static string QuickActions;

		public static string Male;

		public static string Female;

		public static string ChooseSpawnPoint;

		public static string ChooseStartingPoint;

		public static string Continue;

		public static string FreshStart;

		public static string LatencyProblems;

		public static string Rubberbanding;

		public static string ServerStuck;

		public static string DisconnectedFromServer;

		public static string Other;

		public static string SendReport;

		public static string ReportSent;

		public static string ReportFailed;

		public static string FreshStartConfrimTitle;

		public static string FreshStartConfrimText;

		public static string Forward;

		public static string Backward;

		public static string Left;

		public static string Right;

		public static string LeanRight;

		public static string LeanLeft;

		public static string RotationClockwise;

		public static string RotationAnticlockwise;

		public static string Jump;

		public static string Crouch;

		public static string Sprint;

		public static string Up;

		public static string Down;

		public static string Grab;

		public static string FreeLook;

		public static string Inventory;

		public static string ExitPanel;

		public static string PrimaryMouseButton;

		public static string SecondaryMouseButton;

		public static string ThirdMouseButton;

		public static string DropThrow;

		public static string InteractTakeInHands;

		public static string EquipItem;

		public static string ItemOptions;

		public static string Reload;

		public static string ChangeStance;

		public static string Stabilization;

		public static string LeftMouse;

		public static string RightMouse;

		public static string MiddleMouse;

		public static string ToggleVisor;

		public static string ToggleLights;

		public static string ToggleJetpack;

		public static string MatchVelocityControl;

		public static string MatchVelocity;

		public static string WarpDrive;

		public static string TargetUp;

		public static string TargetDown;

		public static string FilterLeft;

		public static string FilterRight;

		public static string ChangeDockingPort;

		public static string LightsToggle;

		public static string Free;

		public static string InProgress;

		public static string ExitGame;

		public static string Game;

		public static string ConnectionError;

		public static string SystemError;

		public static string Disabled;

		public static string InvalidSystemSpesifications;

		public static string ConnectionToGameBroken;

		public static string DeleteCharacter;

		public static string DuplicatedControl;

		public static string DuplicateControlMessage;

		public static string VersionError;

		public static string VersionErrorMessage;

		public static string AreYouSureLogout;

		public static string AreYouSureExitGame;

		public static string AreYouSureDeleteCharacter;

		public static string AreYouSureRespawn;

		public static string AreYouSureLoad;

		public static string AreYouSureYouWantToSave;

		public static string TryAgainLater;

		public static string SpawnErrorTitle;

		public static string SpawnErrorMessage;

		public static string InvertMouse;

		public static string InvertMouseWhileDriving;

		public static string Sensitivity;

		public static string ResetControls;

		public static string ResetControlsMessage;

		public static string ClientVersion;

		public static string Yes;

		public static string No;

		public static string Selected;

		public static string DistressSignal;

		public static string ArrivalTime;

		public static string EngineToggle;

		public static string EngineThrustUp;

		public static string EngineThrustDown;

		public static string InventoryFull;

		public static string Melee;

		public static string Chat;

		public static string HoldToLoot;

		public static string PressToInteract;

		public static string HoldToEquip;

		public static string PressToWarp;

		public static string Talk;

		public static string Active;

		public static string Standby;

		public static string Malfunction;

		public static string Missing;

		public static string Ready;

		public static string Radio;

		public static string Scanning;

		public static string All;

		public static string SaveGameSettings;

		public static string SaveQualitySettings;

		public static string UnnamedVessel;

		public static string Reverse;

		public static string Quick1;

		public static string Quick2;

		public static string Quick3;

		public static string Quick4;

		public static string Name;

		public static string InfoScreen;

		public static string Scan;

		public static string ZoomOut;

		public static string MyShip;

		public static string HomeStation;

		public static string AddCustomOrbit;

		public static string RemoveOrbit;

		public static string SelectedMapObject;

		public static string WarpTo;

		public static string CustomOrbit;

		public static string FtlManeuver;

		public static string Warp1;

		public static string Warp2;

		public static string Warp3;

		public static string CellsSelected;

		public static string WarpDistance;

		public static string PowerConsumption;

		public static string ManeuverStatus;

		public static string ManeuverTimeAdjustment;

		public static string ActivationTime;

		public static string InitializeNavigation;

		public static string AuthorizedVessels;

		public static string UnstableOrbit;

		public static string Inclination;

		public static string ArgumentOfPeriapsis;

		public static string LongitudeOfAscendingNode;

		public static string Periapsis;

		public static string Apoapsis;

		public static string PositionOnOrbit;

		public static string OrbitalPeriod;

		public static string Stage;

		public static string WarpSettings;

		public static string SelectManeuver;

		public static string SignalAmplification;

		public static string Signature;

		public static string Register;

		public static string Unregister;

		public static string SetAsPoint;

		public static string InviteFriend;

		public static string Authorized;

		public static string Locked;

		public static string Unlocked;

		public static string Registered;

		public static string CryoChamber;

		public static string InvitePending;

		public static string InviteSent;

		public static string RegisterToAccess;

		public static string SpawnPointNotSet;

		public static string SpawnPointSet;

		public static string SelectFriend;

		public static string ActionRequired;

		public static string AreYouSureCryo;

		public static string DangerCryo;

		public static string EnvironmentalMonitor;

		public static string UnbreathableAtmosphere;

		public static string Gravity;

		public static string Bar;

		public static string AirQuality;

		public static string AirFiltering;

		public static string PressureRegulation;

		public static string RePressurize;

		public static string Depressurize;

		public static string Pressure;

		public static string InnerDoor;

		public static string OuterDoor;

		public static string AirTank;

		public static string WarningArilock;

		public static string Raw;

		public static string Refined;

		public static string Refine;

		public static string Crafting;

		public static string ReifineAmount;

		public static string VentAmount;

		public static string TransferArmount;

		public static string ActiveSystems;

		public static string NothingConnectedToSlot;

		public static string NoOtherCargoAvailable;

		public static string NoRafineryAvailable;

		public static string Propellant;

		public static string Unload;

		public static string Vent;

		public static string VentDescription;

		public static string Cargo;

		public static string Refining;

		public static string Slot;

		public static string CargoHeading;

		public static string EnergyConsumption;

		public static string ProcessingTime;

		public static string Empty;

		public static string CraftingTime;

		public static string CancelCrafting;

		public static string CancelCraftingDescription;

		public static string CancelCraftingWarning;

		public static string AccelerationHigh;

		public static string AccelerationLow;

		public static string CourseImpossible;

		public static string FTLCapacity;

		public static string FTLCellFuel;

		public static string FTLManeuverIndex;

		public static string FTLOffline;

		public static string FTLReady;

		public static string FTLMalfunction;

		public static string ToManyDockedVessels;

		public static string ManeuverEnds;

		public static string ManeuverInterrupted;

		public static string ConfirmManeuver;

		public static string ManeuverInitiated;

		public static string AlignShip;

		public static string PowerOutput;

		public static string SystemParts;

		public static string Optimal;

		public static string FusionReactor;

		public static string DeuteriumTank;

		public static string SolarPanels;

		public static string Capacitor;

		public static string Consumption;

		public static string PowerSupplyScreen;

		public static string LifeSupportPanelLabel;

		public static string LifeSupportSystem;

		public static string NoAirGenerator;

		public static string NoAirFilter;

		public static string AirGenerator;

		public static string OxygenTank;

		public static string NitrogenTank;

		public static string AirFilter;

		public static string TurnOn;

		public static string TurnOff;

		public static string Airlock;

		public static string SecurityTerminal;

		public static string Claim;

		public static string AddCrewMember;

		public static string ShipCrew;

		public static string AuthorizedPersonnelList;

		public static string CommandingOfficer;

		public static string Crew;

		public static string Resign;

		public static string EnterCustomShipName;

		public static string AreYouSureResign;

		public static string Promote;

		public static string Remove;

		public static string AreYouSurePromote;

		public static string RefuelingStation;

		public static string RCS;

		public static string Engine;

		public static string RechargeStation;

		public static string LowFuel;

		public static string NoFuel;

		public static string Distance;

		public static string ControlChangeDockingPort;

		public static string Module;

		public static string AvailbaleDockingPorts;

		public static string RCSFuelLevel;

		public static string ModulesInRange;

		public static string AvailableModules;

		public static string TargetedModule;

		public static string AvailablePorts;

		public static string DirectionalSpeed;

		public static string Power;

		public static string Fuel;

		public static string Oxygen;

		public static string Capacity;

		public static string ExtPressure;

		public static string Jetpack;

		public static string SuitPower;

		public static string JetpackOffline;

		public static string OxygenLow;

		public static string PressToOpenJournal;

		public static string PressToToggleRcs;

		public static string PressToToggleFlashlight;

		public static string PressToToggleTargeting;

		public static string OpenJournalForMoreDetails;

		public static string NewQuestAvailable;

		public static string HelmetRadar;

		public static string HelmetOnSpeed;

		public static string HelmetOffSpeed;

		public static string CurrentVessel;

		public static string Selection;

		public static string Offline;

		public static string Online;

		public static string Cooldown;

		public static string Powerup;

		public static string None;

		public static string Air;

		public static string Ice;

		public static string Regolith;

		public static string DryIce;

		public static string Nitrates;

		public static string Hydrogen;

		public static string Helium3;

		public static string Nitro;

		public static string Nitrogen;

		public static string CarbonFibers;

		public static string Alloys;

		public static string Circuits;

		public static string Reserved;

		public static string Tut_Undefined;

		public static string Tut_1AdditionalInfo;

		public static string Tut_1;

		public static string Tut_2;

		public static string Tut_3;

		public static string Tut_4;

		public static string Tut_5;

		public static string Tut_6;

		public static string Tut_7;

		public static string CanisterIsEmpty;

		public static string ResourcesAreAlreadyFull;

		public static string CODNone;

		public static string CODPressure;

		public static string CODFrost;

		public static string CODHeat;

		public static string CODImpact;

		public static string CODShot;

		public static string CODSuffocate;

		public static string CODSuicide;

		public static string CODShipwrecked;

		public static string CODShredded;

		public static string CODExplosion;

		public static string CODSpaceExposure;

		public static string CODVesselDacay;

		public static string CODVesselGrenadeExplosion;

		public static string CODVesselProximityExplosion;

		public static string CODVesselSmallDebrisHit;

		public static string CODVesselLargeDebrisHit;

		public static string CODVesselSelfDestruct;

		public static string PressAnyKeyToContinue;

		public static string WeaponModKey;

		public static Dictionary<int, string> TutorialText;

		public static string[] SystemChatName;

		public static string[] SystemChat;

		public static string System;

		public static string AutomatedDistress;

		public static string RescueShipWillArriveIn;

		public static string RescueShipEnRoute;

		public static string AnotherShipInRange;

		public static string RescueShipArrived;

		public static string AltairRifle;

		public static string MilitaryAssaultRifle;

		public static string MilitarySniperRifle;

		public static string MilitaryHandGun01;

		public static string MilitaryHandGun02;

		public static string AltairRifleAmmo;

		public static string MilitaryAssaultRifleAmmo;

		public static string MilitarySniperRifleAmmo;

		public static string MilitaryHandGunAmmo01;

		public static string AltairPressurisedSuit;

		public static string AltairEVASuit;

		public static string AltairPressurisedHelmet;

		public static string AltairEVAHelmet;

		public static string AltairPressurisedJetpack;

		public static string AltairEVAJetpack;

		public static string MachineryPart;

		public static string AltairHandDrill;

		public static string AltairHandDrillBattery;

		public static string AltairHandDrillCanister;

		public static string AltairResourceContainer;

		public static string AltairRefinedCanister;

		public static string AltairCrowbar;

		public static string AltairGlowStick;

		public static string AltairMedpackSmall;

		public static string AltairMedpackBig;

		public static string AltairDisposableHackingTool;

		public static string AltairHandheldAsteroidScanningTool;

		public static string LogItem;

		public static string GenericItem;

		public static string APGrenade;

		public static string EMPGrenade;

		public static string PortableTurret;

		public static string RepairTool;

		public static string SoeSuit;

		public static string SoeHelmet;

		public static string SoeJetpack;

		public static string AegisAssaultRifle;

		public static string AltairRifleDescription;

		public static string MilitaryAssaultRifleDescription;

		public static string MilitarySniperRifleDescription;

		public static string MilitaryHandGun01Description;

		public static string MilitaryHandGun02Description;

		public static string AltairRifleAmmoDescription;

		public static string MilitaryAssaultRifleAmmoDescription;

		public static string MilitarySniperRifleAmmoDescription;

		public static string MilitaryHandGunAmmo01Description;

		public static string AltairPressurisedSuitDescription;

		public static string AltairEVASuitDescription;

		public static string AltairPressurisedHelmetDescription;

		public static string AltairEVAHelmetDescription;

		public static string AltairPressurisedJetpackDescription;

		public static string AltairEVAJetpackDescription;

		public static string MachineryPartDescription;

		public static string AltairHandDrillDescription;

		public static string AltairHandDrillBatteryDescription;

		public static string AltairHandDrillCanisterDescription;

		public static string AltairResourceContainerDescription;

		public static string AltairRefinedCanisterDescription;

		public static string AltairCrowbarDescription;

		public static string AltairGlowStickDescription;

		public static string AltairMedpackSmallDescription;

		public static string AltairMedpackBigDescription;

		public static string AltairDisposableHackingToolDescription;

		public static string AltairHandheldAsteroidScanningToolDescription;

		public static string LogItemDescription;

		public static string GenericItemDescription;

		public static string APGrenadeDescription;

		public static string EMPGrenadeDescription;

		public static string PortableTurretDescription;

		public static string SoeSuitDescription;

		public static string SoeHelmetDescription;

		public static string SoeJetpackDescription;

		public static string AegisAssaultRifleDescription;

		public static string Flag;

		public static string BasketBall;

		public static string BookHolder;

		public static string Hoop;

		public static string LavaLamp;

		public static string PlantRing;

		public static string PlantZikaLeaf;

		public static string PlantCanister;

		public static string Poster;

		public static string PosterBethyr;

		public static string PosterBurner;

		public static string PosterEverest;

		public static string PosterHellion;

		public static string PosterTurret;

		public static string PosterCrewQuarters;

		public static string PosterSonsOfEarth;

		public static string TeslaBall;

		public static string Picture;

		public static string AltCorp_Cup;

		public static string CoffeeMachine;

		public static string BrokenArmature;

		public static string ShatteredPlating;

		public static string FriedElectronics;

		public static string DamagedTransmiter;

		public static string RupturedInsulation;

		public static string BurnedPDU;

		public static string DiamondCore;

		public static string FlagDescription;

		public static string BasketBallDescription;

		public static string BookHolderDescription;

		public static string HoopDescription;

		public static string LavaLampDescription;

		public static string PlantRingDescription;

		public static string PlantZikaLeafDescription;

		public static string PlantCanisterDescription;

		public static string PosterDescription;

		public static string TeslaBallDescription;

		public static string PictureDescription;

		public static string AltCorp_CupDescription;

		public static string CoffeeMachineDescription;

		public static string ScrapDescription;

		public static string DiamondCoreDescription;

		public static string Fuse;

		public static string ServoMotor;

		public static string SolarPanel;

		public static string ExternalAirVent;

		public static string AirProcessingController;

		public static string CarbonFilters;

		public static string AirFilterUnit;

		public static string PressureRegulator;

		public static string RadarSignalAmplifier;

		public static string CoreContainmentFieldGenerator;

		public static string ResourceInjector;

		public static string ThermonuclearCatalyst;

		public static string ExternalDeuteriumExhaust;

		public static string PowerCollector;

		public static string PowerDiffuser;

		public static string GrapheneNanotubes;

		public static string PowerDisipator;

		public static string NaniteCore;

		public static string EmShieldGenerator;

		public static string HighEnergyLaser;

		public static string RcsThrusters;

		public static string SingularityCellDetonator;

		public static string WarpFieldGenerator;

		public static string HighEnergyConverter;

		public static string SingularityContainmentField;

		public static string WarpInductor;

		public static string WarpCell;

		public static string EMFieldController;

		public static string MilitaryNaniteCore;

		public static string ThermonuclearCatalystDescription;

		public static string ResourceInjectorDescription;

		public static string CoreContainmentFieldGeneratorDescription;

		public static string EMFieldControllerDescription;

		public static string ServoMotorDescription;

		public static string AirProcessingControllerDescription;

		public static string CarbonFiltersDescription;

		public static string AirFilterUnitDescription;

		public static string PlasmaAcceleratorDescription;

		public static string HighEnergyLaserDescription;

		public static string SingularityCellDetonatorDescription;

		public static string WarpCellDescription;

		public static string PressureRegulatorDescription;

		public static string NaniteCoreDescription;

		public static string MilitaryNaniteCoreDescription;

		public static string EngineStatus;

		public static string ENG;

		public static string FTL;

		public static string Contacts;

		public static string RadarRange;

		public static string Matched;

		public static string ClusterOfVessels;

		public static string Available;

		public static string ETA;

		public static string DrivingTips;

		public static string Stabilize;

		public static string PilotingNotActive;

		public static string CloseDoor;

		public static string OpenDoor;

		public static string AreYouSureAirlock;

		public static string DangerAirlock;

		public static string Stop;

		public static string RemoveOutfit;

		public static string AirOutput;

		public static string ChangeShipName;

		public static string Temperature;

		public static string VesselStatus;

		public static string GravityFail;

		public static string Defective;

		public static string Breach;

		public static string Fire;

		public static string FireHazard;

		public static string GravityMalfunction;

		public static string Failure;

		public static string Hull;

		public static string RepairPointMessageRoom;

		public static string RepairPointMessageVesselRoom;

		public static string Light;

		public static string EmergencyLight;

		public static string Fabricator;

		public static string Refinery;

		public static string Systems;

		public static string Health;

		public static string FuelLevels;

		public static string FireExtinguisherDescription;

		public static string FireExtingusher;

		public static string NoItemAttachedToCargo;

		public static string NoRefineryAvailable;

		public static string AmountToTransfer;

		public static string AllVessels;

		public static string AttachPoint;

		public static string AirlockPressure;

		public static string DoorControl;

		public static string DistressCallActive;

		public static string Disconnected;

		public static string UnavailableFromInGameMenu;

		public static string CheckRcsUtilityAccess;

		public static string SelfDestruct;

		public static string Activate;

		public static string ConnectingToInvite;

		public static string ShutDown;

		public static string Deploy;

		public static string Retract;

		public static string AreYouSureSelfDestruct;

		public static string ChangeShipEmblem;

		public static string RefiningTime;

		public static string InDebrisField;

		public static string CapacitorsTotal;

		public static string Tier;

		public static string Output;

		public static string ResourcesConsumption;

		public static string PowerCapacity;

		public static string PowerUpTime;

		public static string CoolDownTime;

		public static string InsertPartToImprove;

		public static string NoLifeSupportConnected;

		public static string NoPowerSupplyConnected;

		public static string TotalOutput;

		public static string TotalConsumption;

		public static string TotalCapacity;

		public static string UnauthorizedAccess;

		public static string Attached;

		public static string TransferResources;

		public static string BaseConsumption;

		public static string Canister;

		public static string Generator;

		public static string RequiredResources;

		public static string Jettison;

		public static string InsertItem;

		public static string From;

		public static string To;

		public static string Craft;

		public static string Danger;

		public static string ChooseAnItemToCraft;

		public static string RemoveItem;

		public static string ChooseAnItem;

		public static string NotEnoughResources;

		public static string NoPower;

		public static string RecyclingOutput;

		public static string VolumetricLighting;

		public static string NoAirTankAvailable;

		public static string UnableToDepressurize;

		public static string UnableToPressurize;

		public static string AirCapacity;

		public static string ConnectedLifeSupportSystems;

		public static string ConnectedPowerSupplySystems;

		public static string ConnectedVessels;

		public static string SunExposure;

		public static string ConnectedCargos;

		public static string VesselSystems;

		public static string NoFabricatorAvailable;

		public static string TransferFrom;

		public static string TransferTo;

		public static string NoSunExposure;

		public static string ModuleVolume;

		public static string FilteringRate;

		public static string DragResourcesForCrafting;

		public static string JoinHellionOnDiscord;

		public static string AirLockcontrols;

		public static string LifeSupportInfo;

		public static string AirGeneratorDescription;

		public static string AirFilterDescription;

		public static string AirTankDescription;

		public static string VolumeDescription;

		public static string PressurizeDescription;

		public static string DepressurizeDescription;

		public static string VentActionDescription;

		public static string PowerSupplySystem;

		public static string PowerSupplyInfo;

		public static string CapacitorDescription;

		public static string SolarPanelDescription;

		public static string FusionReactorDescription;

		public static string CurrentVesselConsumtion;

		public static string ConnectedVesselDescription;

		public static string ToggleBaseConsumption;

		public static string Ammo;

		public static string Suits;

		public static string Rifles;

		public static string Handguns;

		public static string Grenades;

		public static string Helmets;

		public static string ToggleGravity;

		public static string AirTankNotConnected;

		public static string ExitCryo;

		public static string EnterCryo;

		public static string RecyclerSlot;

		public static string ResearchSlot;

		public static string Piloting;

		public static string ServerRestartIn;

		public static string EngineNotAvailable;

		public static string CargoFull;

		public static string SaveGame;

		public static string LoadGame;

		public static string Console;

		public static string Items;

		public static string Modules;

		public static string CommandList;

		public static string Medpack;

		public static string SmallItems;

		public static string MediumItems;

		public static string LargeItems;

		public static string BasketballHoop;

		public static string AmbienceVolume;

		public static string Consumable;

		public static string Hands;

		public static string Helmet;

		public static string Outfit;

		public static string Primary;

		public static string Secondary;

		public static string Utility;

		public static string Tool;

		public static string ResearchRequired;

		public static string AllTiersUnlocked;

		public static string FurtherResearchRequired;

		public static string Radiation;

		public static string SystemFailiure;

		public static string Researching;

		public static string Recycling;

		public static string Weapons;

		public static string Tools;

		public static string Parts;

		public static string Medical;

		public static string General;

		public static string Magazines;

		public static string Containers;

		public static string CellConsumption;

		public static string ZeroGravityMovement;

		public static string Rotation;

		public static string SlotFor;

		public static string MultipleItems;

		public static string CargoAttachPoint;

		public static string PowerRechargeStation;

		public static string DockingPanel;

		public static string CryoPanel;

		public static string CargoPanel;

		public static string LifeSupportPanel;

		public static string PowerSupplyPanel;

		public static string AirlockPanel;

		public static string NavigationPanel;

		public static string DockingPortController;

		public static string MessageTerminal;

		public static string HackingDescription;

		public static string PullToUndock;

		public static string CapacitorTitle;

		public static string PowerOutputTitle;

		public static string PowerConsumptionTitle;

		public static string ChangeRateTitle;

		public static string AirTankTitle;

		public static string ServoMotorTitle;

		public static string ResourceInjectorTitle;

		public static string CarbonFilterTitle;

		public static string CatalystTitle;

		public static string CapacitorTooltip;

		public static string SunExposureTitle;

		public static string ChangeRateOxygenTitle;

		public static string ChangeRateNitorgenTitle;

		public static string HeliumChangeRateTitle;

		public static string SunExposureRateTitle;

		public static string PowerOutputTooltip;

		public static string PowerConsumptionTooltip;

		public static string ChangeRateTooltip;

		public static string AirTankTooltip;

		public static string VesselsOutputTooltip;

		public static string SelectedVesselTooltip;

		public static string ServoMotorTooltip;

		public static string ResourceInjectorTooltip;

		public static string CarbonFilterTooltip;

		public static string CatalystTooltip;

		public static string SunExposureTooltip;

		public static string ManeuverTimeTitle;

		public static string ManeuverTimeTooltip;

		public static string WarpPowerTitle;

		public static string WarpPowerTooltip;

		public static string WarpCellsTitle;

		public static string WarpCellsTooltip;

		public static string DockedVesselsTitle;

		public static string DockedVesselsTooltip;

		public static string ScanButtonTitle;

		public static string ScanButtonTooltip;

		public static string VesselPowerTitle;

		public static string VesselPowerTooltip;

		public static string RentYourOwnServer;

		public static string VesselPowerOffline;

		public static string DoorOpenWarningTitle;

		public static string DoorOpenWarningTooltip;

		public static string Warning;

		public static string ChangeRateOxygenTooltip;

		public static string ChangeRateNitrogenTooltip;

		public static string HeliumChangeRateTootlip;

		public static string SunExposureRateTootlip;

		public static string InGameDescription;

		public static string OrbitingNear;

		public static string WarpingNear;

		public static string FloatingFreelyNear;

		public static string TeleportingFromDiscord;

		public static string GravityInfluenceRadius;

		public static string Radius;

		public static string MultipleObjects;

		public static string ObjectsInGroup;

		public static string DefaultMapAsteroidDescription;

		public static string DefauldMapCelestialDescription;

		public static string CustomOrbitDescription;

		public static string AltCorp_Shuttle_SARA;

		public static string AltCorp_Shuttle_CECA;

		public static string AltCorp_CorridorModule;

		public static string AltCorp_CorridorIntersectionModule;

		public static string AltCorp_Corridor45TurnModule;

		public static string AltCorp_Corridor45TurnRightModule;

		public static string AltCorp_CorridorVertical;

		public static string ALtCorp_PowerSupply_Module;

		public static string AltCorp_LifeSupportModule;

		public static string AltCorp_Cargo_Module;

		public static string AltCorp_Command_Module;

		public static string AltCorp_StartingModule;

		public static string AltCorp_AirLock;

		public static string AltCorp_DockableContainer;

		public static string AltCorp_CrewQuarters_Module;

		public static string AltCorp_SolarPowerModule;

		public static string AltCorp_FabricatorModule;

		public static string SmallOutpost;

		public static string MediumOutpost;

		public static string LargeOutpost;

		public static string SmallStation;

		public static string MediumStation;

		public static string LargeStation;

		public static string AlreadyEquipped;

		public static string EquipSuitFirst;

		public static string Error;

		public static string UnableToStartSPGame;

		public static string Hellion_SpExeIsMissing;

		public static string RemoveOutfitTooltip;

		public static string Load;

		public static string LootAll;

		public static string Journal;

		public static string Blueprints;

		public static string NoSuitEquipped;

		public static string NoLogAvailable;

		public static string ToggleTracking;

		public static string Tracking;

		public static string ProximityLoot;

		public static string Drop;

		public static string Loot;

		public static string TakeAll;

		public static string ShowContainerSlots;

		public static string NoMagazine;

		public static string FireMode;

		public static string AutoFireMode;

		public static string SingleFireMode;

		public static string FireType;

		public static string ArmorPiercingFireType;

		public static string FragmentationFireType;

		public static string IncediaryFireType;

		public static string NormalFireType;

		public static string PleaseAssignAllControls;

		public static string DataPrivacySettings;

		public static string Edit;

		public static string ServerInfo;

		public static string ExternalBrowserPage;

		public static string Refill;

		public static string Roll;

		public static string HideTipsFromMenu;

		public static string ChangeTargetPort;

		public static string ChangeTarget;

		public static string ToggleEngine;

		public static string MatchTargetsVelocity;

		public static string CollisionWarning;

		public static string OffSpeedAssistant;

		public static string ChangeRadarRange;

		public static string OffTarget;

		public static string NoTargetModulesInRange;

		public static string CreateCharacterLore;

		public static string RcsCancelManeuver;

		public static string QuestTerminalHint;

		public static string ToBeContinued;

		public static string WrongSavegameVersion;

		public static string Battery;

		public static string BatteryMissing;

		public static string OutOfRange;

		public static string NotScannable;

		public static string ScanningRange;

		public static string Low;

		public static string Medium;

		public static string High;

		public static string Unknown;

		public static string Armor;

		public static string UsesLeft;

		public static string UnknownObject;

		public static string Equipment;

		public static string ResourcesLabel;

		public static string Networking;

		public static string Sent;

		public static string Received;

		public static string Reset;

		public static string TowingDisabled;

		public static string NoDockedVessels;

		public static string WarpSignature;

		public static string WarpSignatureDescription;

		public static string UnidentifiedObject;

		public static string UnidentifiedObjectDescription;

		public static string NoPart;

		public static string PossibleContacts;

		public static string RadarSignature;

		public static string SpawnMiningDescription;

		public static string SpawnFreeRoamDescription;

		public static string SpawnPremadeStationDescription;

		public static string SpawnDoomedDescription;

		public static string FreeRoamSteropes;

		public static string FreeRoamSteropesDescription;

		public static string MiningSteropes;

		public static string MiningSteropesDescription;

		public static string SteropesNearRandomStation;

		public static string SteropesNearRandomStationDescription;

		public static string SteropesNearDoomedOutpost;

		public static string SteropesNearDoomedOutpostDescription;

		public static string MiningArges;

		public static string MiningArgesDescription;

		public static string FreeRoamArges;

		public static string FreeRoamArgesDescription;

		public static string PressForInventory;

		public static string PressForLights;

		public static string RotationControls;

		public static string HoldToStabilize;

		public static string UseResourceCanister;

		public static string AssignOnCryo;

		public static string DragItemToHands;

		public static string WelderTooltip;

		public static string PressForNavigation;

		public static string AvailableQuests;

		public static string QuestActivated;

		public static string QuestUpdated;

		public static string QuestCompleted;

		public static string QuestFailed;

		public static string Objectives;

		public static string QuestLog;

		public static string SystemNothing;

		public static string SystemUnstable;

		public static string SystemShipCalled;

		public static string SystemShipInRange;

		public static string SystemShipArrive;

		public static string SystemServerRestart;

		public static string NoNakamaConnection;

		public static string NoServerConnection;

		public static string AccountNotFound;

		public static string AccountAlreadyExists;

		public static string InvalidEmail;

		public static string InvalidUsername;

		public static string ConsentToDataStorage;

		public static Dictionary<Enum, string> Enums;

		public static Dictionary<string, string> CanvasManagerLocalization;

		public static Dictionary<string, string> PanelsLocalization;

		public static Dictionary<string, string> EnvironmentPanelLocalization;

		public static Dictionary<MachineryPartType, string> MachineryPartsDescriptions;

		public static Dictionary<GenericItemSubType, string> GenericItemsDescriptions;

		public static Dictionary<ItemType, string> ItemsDescriptions;

		public static Dictionary<ItemCategory, string> ItemCategoryNames;

		static Localization()
		{
			LocalizationFiles = new Dictionary<int, string>
			{
				{ 1, "Data/localization_Serbian" },
				{ 2, "Data/localization_ChineseSimplified" },
				{ 3, "Data/localization_French" },
				{ 4, "Data/localization_Italian" },
				{ 5, "Data/localization_Portuguese" },
				{ 6, "Data/localization_Russian" },
				{ 7, "Data/localization_Spanish" },
				{ 8, "Data/localization_Turkish" },
				{ 9, "Data/localization_Czech" },
				{ 10, "Data/localization_Danish" },
				{ 11, "Data/localization_Dutch" },
				{ 12, "Data/localization_Finnish" },
				{ 13, "Data/localization_German" },
				{ 14, "Data/localization_Greek" },
				{ 15, "Data/localization_Hungarian" },
				{ 16, "Data/localization_Japanese" },
				{ 17, "Data/localization_Norwegian" },
				{ 18, "Data/localization_Polish" },
				{ 19, "Data/localization_PortugueseBrazilian" },
				{ 20, "Data/localization_Romanian" },
				{ 21, "Data/localization_Slovak" },
				{ 22, "Data/localization_Slovenian" },
				{ 23, "Data/localization_Swedish" },
				{ 24, "Data/localization_Ukrainian" }
			};
			Preload01 = "<color=#BEC070>Basics:</color> Cryopod serves as your re-spawn point.";
			Preload02 = "<color=#BEC070>Basics:</color> Your character stays in game after you log out. Logout from a cryopod to keep your character safe.";
			Preload03 = "<color=#BEC070>Basics:</color> Always equip the suit first then the jetpack and then the helmet.";
			Preload04 = "<color=#BEC070>Basics:</color> Never leave your station without a loaded jetpack.";
			Preload05 = "<color=#BEC070>Basics:</color> Welder can be used to repair any damage to ships and stations.";
			Preload06 = "<color=#BEC070>Basics:</color> Pressure difference causes decompression, knocking you down or sucking you into space.";
			Preload07 = "<color=#BEC070>Basics:</color> You can suffocate in your suit if you run out of oxygen.";
			Preload08 = "<color=#BEC070>Basics:</color> Staying inside a debris field will cause damage to your stations and ships.";
			Preload09 = "<color=#BEC070>Basics:</color> Damage can cause different hazards like fire, breach and system malfunctions.";
			Preload10 = "<color=#BEC070>Basics:</color> Fire needs oxygen. Decompressing a room will put it out.";
			Preload11 = "<color=#BEC070>Basics:</color> Use parts to enhance your ship/station's systems.";
			Preload12 = "<color=#BEC070>Basics:</color> Solar panel base output depends on distance from the sun.";
			Preload13 = "<color=#BEC070>Basics:</color> Fusion Reactor requires a supply of Helium-3 to work.";
			Preload14 = "<color=#BEC070>Basics:</color> Servomotors reduce power consumption of any system they are attached to.";
			Preload15 = "<color=#BEC070>Basics:</color> Resource injectors reduce resource consumption of any system they are attached to.";
			Preload16 = "<color=#BEC070>Basics:</color> Catalysts greatly improve the power output of reactors and solar panels.";
			Preload17 = "<color=#BEC070>Basics:</color> Core Containment increases maximum capacitor storage.";
			Preload18 = "<color=#BEC070>Basics:</color> Salvage from derelicts can be refined into crafting components.";
			Preload19 = "<color=#BEC070>Basics:</color> When piloting press [R] to change radar range.";
			Preload20 = "<color=#f0f0f0>Zero-G:</color> press [J] to activate jetpack.";
			Preload21 = "<color=#f0f0f0>Zero-G:</color> use [Q] and [E] to rotate.";
			Preload22 = "<color=#f0f0f0>Zero-G:</color> hold [SHIFT] to stabilize rotation and grab onto nearby walls to avoid decompression.";
			Preload23 = "<color=#f0f0f0>Zero-G:</color> watch your acceleration and lateral speed indicators and mind the inertia.";
			Preload24 = "<color=#f0f0f0>Zero-G:</color> when docking, use [R] to cycle docking ports of your current vessel.";
			Preload25 = "<color=#f0f0f0>Zero-G:</color> when docking, use [UP/DOWN] to cycle targets, [LEFT/RIGHT] to change target ports.";
			Preload26 = "<color=#f0f0f0>Zero-G:</color> collisions are deadly, avoid bumping into other space objects.";
			Preload27 = "<color=#f0f0f0>Zero-G:</color> moving in the vacuum of space is impossible without a jetpack.";
			Preload28 = "<color=#D0815B>Base-Building:</color> Command Module adds a security interface to your station.";
			Preload29 = "<color=#D0815B>Base-Building:</color> Security interface lets you control who can access your base.";
			Preload30 = "<color=#D0815B>Base-Building:</color> Cargo Bay refinery can be used to refine a large quantity of resources at once.";
			Preload31 = "<color=#D0815B>Base-Building:</color> Adding a Fabricator to your station will let you craft items.";
			Preload32 = "<color=#D0815B>Base-Building:</color> Power Supply Module (PSM) provides a massive power production boost to your station.";
			Preload33 = "<color=#D0815B>Base-Building:</color> Airlocks let you avoid decompression when going from station into space.";
			Preload34 = "<color=#D0815B>Base-Building:</color> Airlock requires a working Life Support System.";
			Preload35 = "<color=#D0815B>Base-Building:</color> Refinery and Fabricator drain a lot of power. Ensure your power supply is adequate before activating them.";
			Preload36 = "<color=#D05D5B>Combat:</color> Turrets are deadly but have limited range. Use cover and grenades when fighting them.";
			Preload37 = "<color=#D05D5B>Combat:</color> EMP grenades will disable a Turret and let you pick it up.";
			Preload38 = "<color=#D05D5B>Combat:</color> Hacking tools can be used to open locked doors and reset authorized security panels.";
			Preload39 = "<color=#D05D5B>Combat:</color> Deploying a Turret in a station without a security interface will make it hostile.";
			Preload40 = "<color=#D05D5B>Combat:</color> Only 'authorized personnel' can pass through locked doors and access system panels.";
			Preload41 = "<color=#74A1CA>Navigation:</color> Upgrading Warp Drive with a better Singularity Cell Detonator will unlock faster warp.";
			Preload42 = "<color=#74A1CA>Navigation:</color> Creating a custom orbit allows you to warp to any point in space.";
			Preload43 = "<color=#74A1CA>Navigation:</color> Moving your station to a higher orbit will hide it from other players.";
			Preload44 = "<color=#74A1CA>Navigation:</color> Warp Drive drains power directly from the capacitor. Ensure you have enough power stored before attempting warp.";
			Preload45 = "<color=#74A1CA>Navigation:</color> Propulsion includes RCS for short distance travel, Engine for quick bursts of acceleration and Warp Drive for crossing vast interplanetary distances.";
			Preload46 = "<color=#74A1CA>Navigation:</color> Explore former military zones for Command Modules, abandoned ships and propulsion upgrades.";
			Preload47 = "<color=#74A1CA>Navigation:</color> Former industrial zones often contain industrial modules, raw resources and power supply upgrades.";
			Preload48 = "<color=#74A1CA>Navigation:</color> Former civilian zones are an excellent source of medical supplies and Life Support upgrades.";
			ConnectingToMain = "Connecting to main server";
			ConnectingToGame = "Connecting to game server";
			Connect = "Connect";
			Play = "Play Online";
			PlaySP = "Single Player Game";
			Options = "Options";
			Quit = "Quit";
			Apply = "Apply";
			Disclaimer = "This is the Alpha version of the game. It represents the basic vision behind this dark world. As the player base grows, we expect Hellion to evolve beyond these initial confines and become a true space survival that we can all enjoy. One of the cornerstones of this idea is an open and honest communication between developers and community. We are looking forward to your feedback as we believe that each and every one of you can add something to this project.\n\nSo grab your space suit, put on the helmet and dive into the void with us.\nHellion awaits!\n\nDo not hesitate to report any problems, bugs or other issues you may have while playing Hellion and make sure to contact us.\n\nZero Gravity Team";
			Welcome = "Welcome to Hellion";
			Understand = "I understand";
			ReadMore = "Read more";
			LatestNews = "Latest news";
			AdvancedVideoSettings = "Advanced video settings";
			MouseSettings = "Mouse settings";
			KeyboardSettings = "Keyboard settings";
			AltKey = "AltKey";
			AmbientOcclusion = "Ambient occlusion";
			AntiAliasing = "Anti-aliasing";
			Audio = "Audio";
			AutoStabilization = "Auto-stabilization";
			Back = "Back";
			BasicVideoSettings = "Basic video settings";
			BasicAudioSettings = "Basic audio settings";
			Bloom = "Bloom";
			Cancel = "Cancel";
			Character = "Character";
			ChromaticAberration = "Chromatic aberration";
			Community = "Community";
			Confirm = "Confirm";
			Controls = "Controls";
			CreateCharacter = "Create character";
			CurrentServer = "Current server";
			PleaseSelectServer = "Please select server";
			Default = "Default";
			EADisclaimer = "Early Access Build";
			EnterCustomBoxName = "Enter custom box name";
			EnterPassword = "Enter password";
			Exit = "Exit";
			EyeAdaptation = "Eye adaptation";
			F1ForHelp = "Press F1 for help";
			Favorites = "Favorites";
			FullScreen = "Fullscreen";
			Full = "Full";
			GameSettings = "Game settings";
			GeneralSettings = "General settings";
			Glossary = "Glossary";
			HeadBobStrength = "Head bob strength";
			HideTips = "Hide tips";
			HideTutorial = "Hide tutorial";
			Interact = "Interact";
			Key = "Key";
			Loading = "Loading";
			Logout = "Logout";
			MainMenu = "Main menu";
			MasterVolume = "Master volume";
			MotionBlur = "Motion blur";
			Official = "Official";
			Ping = "Ping";
			Players = "Players";
			PreAplha = "Pre-Alpha";
			Quality = "Quality";
			Refresh = "Refresh";
			Resolution = "Resolution";
			Respawn = "Respawn";
			Resume = "Resume";
			Save = "Save";
			Search = "Search";
			ServerBrowser = "Server browser";
			Server = "Server";
			Settings = "Settings";
			Shadows = "Shadows";
			ShipSettings = "Ship settings";
			ShowCrosshair = "Show crosshair";
			TextureQuality = "Texture quality";
			Throwing = "Throwing";
			Use = "Use";
			Username = "Username";
			VSync = "VSync";
			Video = "Video";
			VoiceVolume = "Voice volume";
			LanguageSettings = "Language settings";
			ChooseLanguage = "Choose language";
			ReportServer = "Report server";
			ServerSettings = "Server settings";
			PlayerSettings = "Player settings";
			GlobalSettings = "Global settings";
			Autosave = "Autosave";
			NewGame = "New game";
			NewGameDescription = "Embark on a new adventure beneath an alien sky";
			SinglePlayer = "Single player";
			SinglePlayerModeDescription = "Your own private server just for single player mode";
			DeleteSaveGame = "Delete save game";
			AreYouSureDeleteSaveGame = "Are you sure you want to delete save game?";
			Multiplayer = "Multiplayer";
			JoinDiscord = "Join discord";
			Movement = "Movement";
			Actions = "Actions";
			Ship = "Ship";
			Suit = "Suit";
			Communications = "Communications";
			QuickActions = "Quick actions";
			Male = "Male";
			Female = "Female";
			ChooseSpawnPoint = "Choose spawn point";
			ChooseStartingPoint = "Choose starting point";
			Continue = "Continue";
			FreshStart = "Fresh start";
			LatencyProblems = "Latency problems";
			Rubberbanding = "Rubberbanding";
			ServerStuck = "Server stuck on loading";
			DisconnectedFromServer = "Disconnected from the server for no reason";
			Other = "Other";
			SendReport = "Send report";
			ReportSent = "Report sent, thanks for your feedback";
			ReportFailed = "Report sending failed, please try again later";
			FreshStartConfrimTitle = "Fresh Start Confirm";
			FreshStartConfrimText = "Are you sure you want to start a new game, current progress will be lost";
			Forward = "Forward";
			Backward = "Backwards";
			Left = "Left";
			Right = "Right";
			LeanRight = "Lean Right";
			LeanLeft = "Lean Left";
			RotationClockwise = "Rotation Clockwise";
			RotationAnticlockwise = "Rotation Counter Clockwise";
			Jump = "Jump";
			Crouch = "Crouch";
			Sprint = "Sprint";
			Up = "Up";
			Down = "Down";
			Grab = "Grab";
			FreeLook = "Free Look";
			Inventory = "Inventory";
			ExitPanel = "Exit Screen";
			PrimaryMouseButton = "Primary Mouse Button";
			SecondaryMouseButton = "Secondary Mouse Button";
			ThirdMouseButton = "Third Mouse Button";
			DropThrow = "Drop / <color='#A0D3F8'>Hold To Throw</color>";
			InteractTakeInHands = "Interact / <color='#A0D3F8'>Hold To Take Item In Hands</color>";
			EquipItem = "Hold To Equip";
			ItemOptions = "Hold + Scroll For Item Options";
			Reload = "Reload";
			ChangeStance = "Change Stance";
			Stabilization = "Stabilization";
			LeftMouse = "Left Mouse Button";
			RightMouse = "Right Mouse Button";
			MiddleMouse = "Middle Mouse Button";
			ToggleVisor = "Toggle Visor";
			ToggleLights = "Toggle Lights";
			ToggleJetpack = "Toggle Jetpack";
			MatchVelocityControl = "Match velocity with target";
			MatchVelocity = "Match Target Velocity available press [M]";
			WarpDrive = "Warp Drive";
			TargetUp = "Cycle Targets Up";
			TargetDown = "Cycle Targets Down";
			FilterLeft = "Cycle Filters Left";
			FilterRight = "Cycle Filters Right";
			ChangeDockingPort = "Change Docking Port";
			LightsToggle = "Toggle Helmet Light";
			Free = "Free";
			InProgress = "In progress";
			ExitGame = "Exit Game";
			Game = "Game";
			ConnectionError = "Connection Error";
			SystemError = "System Error";
			Disabled = "Disabled";
			InvalidSystemSpesifications = "This computer has invalid system specifications. You need at least 4 gigabytes of RAM and a processor with at least a 2 GHz clock speed.";
			ConnectionToGameBroken = "Connection to the game server has been lost.";
			DeleteCharacter = "Delete character";
			DuplicatedControl = "Duplicated Control";
			DuplicateControlMessage = "This key is already in use for the {0} action, do you want to overwrite it ?";
			VersionError = "Wrong client version";
			VersionErrorMessage = "Please update your client";
			AreYouSureLogout = "Are you sure you want to logout?";
			AreYouSureExitGame = "Are you sure you want to exit?";
			AreYouSureDeleteCharacter = "Are you sure you want to delete this character?\nAll active quests progress will be lost!";
			AreYouSureRespawn = "Confirming this option will kill your character!\nAre you sure you want to respawn?";
			AreYouSureLoad = "Confirming this option will delete all current progress! \n Are you sure you want to load game?";
			AreYouSureYouWantToSave = "Settings have been updated! \n Do you want to save changes?";
			TryAgainLater = "Try again later";
			SpawnErrorTitle = "Spawn error";
			SpawnErrorMessage = "Unable to spawn on selected point, please select a different spawn point";
			InvertMouse = "Invert Mouse";
			InvertMouseWhileDriving = "Invert Mouse While Driving";
			Sensitivity = "Sensitivity";
			ResetControls = "Reset Controls";
			ResetControlsMessage = "This action will reset controls to default values.";
			ClientVersion = "Version: {0}";
			Yes = "Yes";
			No = "No";
			Selected = "Selected";
			DistressSignal = "Distress Signals";
			ArrivalTime = "Time to Arrival";
			EngineToggle = "Engine ON/OFF";
			EngineThrustUp = "Engine Thrust Up";
			EngineThrustDown = "Engine Thrust Down";
			InventoryFull = "Full";
			Melee = "Melee";
			Chat = "Chat";
			HoldToLoot = "Hold [{0}] to loot";
			PressToInteract = "'{0}' to interact";
			HoldToEquip = "Hold [{0}] to equip";
			PressToWarp = "Press '{0}' To Activate";
			Talk = "Voice Activation";
			Active = "Active";
			Standby = "Standby";
			Malfunction = "Malfunction";
			Missing = "Missing";
			Ready = "Ready";
			Radio = "Radio";
			Scanning = "Scanning";
			All = "All";
			SaveGameSettings = "Are you sure you want to save game settings?";
			SaveQualitySettings = "Changing quality settings will take some time, please confirm changes?";
			UnnamedVessel = "Unnamed vessel";
			Reverse = "Reverse";
			Quick1 = "Piloting / Primary";
			Quick2 = "Navigation / Secondary";
			Quick3 = "Docking / Grenades";
			Quick4 = "Lights / Consumables";
			Name = "Name";
			InfoScreen = "Info screen";
			Scan = "Scan";
			ZoomOut = "Zoom out";
			MyShip = "My ship";
			HomeStation = "Home station";
			AddCustomOrbit = "Add custom orbit";
			RemoveOrbit = "Remove orbit";
			SelectedMapObject = "Selected map object";
			WarpTo = "Warp to";
			CustomOrbit = "Custom orbit";
			FtlManeuver = "FTL maneuver";
			Warp1 = "Warp 1";
			Warp2 = "Warp 2";
			Warp3 = "Warp 3";
			CellsSelected = "Cells selected";
			WarpDistance = "Warp distance";
			PowerConsumption = "Power consumption";
			ManeuverStatus = "Maneuver status";
			ManeuverTimeAdjustment = "Maneuver time adjustment";
			ActivationTime = "Time to Activation";
			InitializeNavigation = "Initialize";
			AuthorizedVessels = "Authorized vessels";
			UnstableOrbit = "Unstable Orbit";
			Inclination = "Inclination";
			ArgumentOfPeriapsis = "Argument of periapsis";
			LongitudeOfAscendingNode = "Longitude of ascending node";
			Periapsis = "Periapsis";
			Apoapsis = "Apoapsis";
			PositionOnOrbit = "Position on orbit";
			OrbitalPeriod = "Orbital period";
			Stage = "Stage";
			WarpSettings = "Warp settings";
			SelectManeuver = "Please select maneuver";
			SignalAmplification = "Signal amplification";
			Signature = "Signature";
			Register = "Register";
			Unregister = "Unregister";
			SetAsPoint = "Set spawn point";
			InviteFriend = "Invite friend";
			Authorized = "Authorized";
			Locked = "Locked";
			Unlocked = "Unlocked";
			Registered = "Registered";
			CryoChamber = "Cryo chamber";
			InvitePending = "Invite pending";
			InviteSent = "Invite sent";
			RegisterToAccess = "Register to access cryopod controls";
			SpawnPointNotSet = "Spawn point not set";
			SpawnPointSet = "Spawn point set";
			SelectFriend = "Select a friend to invite";
			ActionRequired = "Action required";
			AreYouSureCryo = "Are you sure you want to designate this chamber as your spawn point?";
			DangerCryo = "Your previous spawn will be replaced!";
			EnvironmentalMonitor = "Environmental monitor";
			UnbreathableAtmosphere = "Unbreathable atmosphere";
			Gravity = "Gravity";
			Bar = "Bar";
			AirQuality = "Air quality";
			AirFiltering = "Air filtering";
			PressureRegulation = "Pressure regulation";
			RePressurize = "Pressurize";
			Depressurize = "Depressurize";
			Pressure = "Pressure";
			InnerDoor = "Inner door";
			OuterDoor = "Outer door";
			AirTank = "Air tank";
			WarningArilock = "Opening both doors risks violent decompression";
			Raw = "Raw";
			Refined = "Refined";
			Refine = "Refine";
			Crafting = "Crafting";
			ReifineAmount = "Refine Amount";
			VentAmount = "Vent Amount";
			TransferArmount = "Transfer Amount";
			ActiveSystems = "Active systems";
			NothingConnectedToSlot = "Nothing connected to slot";
			NoOtherCargoAvailable = "No other cargo available";
			NoRafineryAvailable = "No refinery available";
			Propellant = "Fuel";
			Unload = "Unload";
			Vent = "Vent";
			VentDescription = "Drag item to box to vent it";
			Cargo = "Cargo";
			Refining = "Refining";
			Slot = "Slot";
			CargoHeading = "Cargo interaction panel";
			EnergyConsumption = "Energy consumption";
			ProcessingTime = "Processing time";
			Empty = "Empty";
			CraftingTime = "Crafting time";
			CancelCrafting = "Cancel crafting";
			CancelCraftingDescription = "Are you sure you want to cancel current crafting process?";
			CancelCraftingWarning = "You will lose all resources used in current process!";
			AccelerationHigh = "Speed too high";
			AccelerationLow = "Speed too low";
			CourseImpossible = "Course impossible";
			FTLCapacity = "Capacitor: Insufficient power";
			FTLCellFuel = "FTL Drive: Insufficient fuel in cells";
			FTLManeuverIndex = "FTL Drive: Maneuver not selected";
			FTLOffline = "FTL Drive: offline";
			FTLReady = "FTL Drive: ready";
			FTLMalfunction = "FTL Drive: malfunction";
			ToManyDockedVessels = "To many docked vessels";
			ManeuverEnds = "Maneuver ends in";
			ManeuverInterrupted = "Maneuver interrupted";
			ConfirmManeuver = "Confirm maneuver execution";
			ManeuverInitiated = "Maneuver initiated";
			AlignShip = "Please align your ship with maneuver direction";
			PowerOutput = "Power output";
			SystemParts = "System parts";
			Optimal = "Optimal";
			FusionReactor = "Fusion reactor";
			DeuteriumTank = "Helium-3 tank";
			SolarPanels = "Solar panels";
			Capacitor = "Capacitor";
			Consumption = "Consumption";
			PowerSupplyScreen = "Power supply screen";
			LifeSupportPanelLabel = "Life support panel";
			LifeSupportSystem = "Life support system";
			NoAirGenerator = "No air generator available";
			NoAirFilter = "No air filter available";
			AirGenerator = "Air generator";
			OxygenTank = "Oxygen tank";
			NitrogenTank = "Nitrogen tank";
			AirFilter = "Air filter";
			TurnOn = "Turn on";
			TurnOff = "Turn off";
			Airlock = "Airlock";
			SecurityTerminal = "Security terminal";
			Claim = "Claim";
			AddCrewMember = "Add crew member";
			ShipCrew = "Ship crew";
			AuthorizedPersonnelList = "Authorized personnel list";
			CommandingOfficer = "Commanding officer";
			Crew = "Crew";
			Resign = "Resign";
			EnterCustomShipName = "Enter custom ship name";
			AreYouSureResign = "Are you sure you wan to resign, crew member will become commanding officer!";
			Promote = "Promote";
			Remove = "Remove";
			AreYouSurePromote = "By promoting this crew member you will resign you commanding officer position!";
			RefuelingStation = "Refueling station";
			RCS = "RCS";
			Engine = "Engine";
			RechargeStation = "Power Recharge station";
			LowFuel = "Low fuel";
			NoFuel = "No fuel";
			Distance = "Distance";
			ControlChangeDockingPort = "Press {0} to change port";
			Module = "Module";
			AvailbaleDockingPorts = "Available docking ports";
			RCSFuelLevel = "RCS Fuel level";
			ModulesInRange = "Modules in range";
			AvailableModules = "Available modules";
			TargetedModule = "Targeted module";
			AvailablePorts = "Available ports";
			DirectionalSpeed = "Directional Speed";
			Power = "Power";
			Fuel = "Fuel";
			Oxygen = "Oxygen";
			Capacity = "Capacity";
			ExtPressure = "Ext. pressure";
			Jetpack = "Jetpack";
			SuitPower = "Suit power";
			JetpackOffline = "Jetpack RCS offline";
			OxygenLow = "Oxygen low";
			PressToOpenJournal = "Press [O] to activate journal screen";
			PressToToggleRcs = "Press [J] to toggle RCS";
			PressToToggleFlashlight = "Press [L] to toggle flashlight";
			PressToToggleTargeting = "Press [{0}] to toggle targeting";
			OpenJournalForMoreDetails = "Press [O] to open Journal Screen for more details";
			NewQuestAvailable = "New Quest Available, press [O] to open journal";
			HelmetRadar = "Helmet Radar Toggle";
			HelmetOnSpeed = "DIR.";
			HelmetOffSpeed = "LAT.";
			CurrentVessel = "Current vessel";
			Selection = "Selection";
			Offline = "Offline";
			Online = "Online";
			Cooldown = "Cooldown";
			Powerup = "Powerup";
			None = "None";
			Air = "Air";
			Ice = "Ice";
			Regolith = "Regolith";
			DryIce = "Dry ice";
			Nitrates = "Nitrates";
			Hydrogen = "Hydrogen";
			Helium3 = "Helium-3";
			Nitro = "Nitro";
			Nitrogen = "Nitrogen";
			CarbonFibers = "Carbon fibers";
			Alloys = "Alloys";
			Circuits = "Circuits";
			Reserved = "Reserved";
			Tut_Undefined = "Undefined";
			Tut_1AdditionalInfo = "Use [F] to interact with objects.";
			Tut_1 = "Your cryosleep is over now. Welcome to Hellion!";
			Tut_2 = "Pick up the suit, it will be your INVENTORY.\nPress [TAB] to access it.";
			Tut_3 = "Equip Helmet and Jetpack.\nPress [H] to raise/lower Helmet Visor and save suit oxygen.";
			Tut_4 = "Use [W,A,S,D] and Mouse to move in zero gravity.\nPress [SPACE] to move up.\nPress [LCTRL] to move down.\nHold [LSHIFT] to grab.\nPress [Q] and [E] to roll.";
			Tut_5 = "Make sure your suit is equipped and lower your visor before exiting the station.";
			Tut_6 = "Manually override outer door by pulling the lever.\nHold [LSHIFT] to grab onto nearby wall and avoid decompression.";
			Tut_7 = "Access the module's Docking Panel to begin docking procedure.";
			CanisterIsEmpty = "Canister is empty";
			ResourcesAreAlreadyFull = "Resources are already full";
			CODNone = "You have died";
			CODPressure = "You have died from decompression";
			CODFrost = "You froze to death";
			CODHeat = "You got fried";
			CODImpact = "You have died from impact";
			CODShot = "You have been shot";
			CODSuffocate = "You suffocated";
			CODSuicide = "You killed yourself";
			CODShipwrecked = "Your ship crashed";
			CODShredded = "You've been shredded";
			CODExplosion = "You've died in explosion";
			CODSpaceExposure = "You've died due to space exposure";
			CODVesselDacay = "Your ship has been destroyed due to structural decay";
			CODVesselGrenadeExplosion = "Your ship has been destroyed by hand grenade explosion";
			CODVesselProximityExplosion = "Your ship has been destroyed by nearby vessel explosion";
			CODVesselSmallDebrisHit = "Your ship has been destroyed by debris field";
			CODVesselLargeDebrisHit = "Your ship has been destroyed by large debris fragment";
			CODVesselSelfDestruct = "Your ship self destructed";
			PressAnyKeyToContinue = "(Press any key to continue)";
			WeaponModKey = "Switch firing mode";
			System = "System";
			AutomatedDistress = "Automated distress";
			RescueShipWillArriveIn = "Rescue Ship will arrive in:";
			RescueShipEnRoute = "Rescue Ship En Route. Time to arrival:";
			AnotherShipInRange = "Unable to call Rescue Ship. Another Ship in range.";
			RescueShipArrived = "Rescue Ship Arrived!";
			AltairRifle = "B-45 Compound -Mod.06";
			MilitaryAssaultRifle = "V22 Broadsword - 8.20";
			MilitarySniperRifle = "R15 Lance - 12.5/Helix";
			MilitaryHandGun01 = "S22 Rapier - 8.15";
			MilitaryHandGun02 = "H115 Glaive - 8.15";
			AltairRifleAmmo = "Mod.06-S";
			MilitaryAssaultRifleAmmo = "8.20-S";
			MilitarySniperRifleAmmo = "12.5/Helix";
			MilitaryHandGunAmmo01 = "8.15-S";
			AltairPressurisedSuit = "AC Mk9 - Pressure Suit";
			AltairEVASuit = "AC Proteus-h - EVA suit";
			AltairPressurisedHelmet = "AC Mk9 - Helmet";
			AltairEVAHelmet = "AC Proteus-h - Helmet";
			AltairPressurisedJetpack = "AC Mk9 - Jetpack";
			AltairEVAJetpack = "AC Proteus-h - Jetpack";
			MachineryPart = "Machinery part";
			AltairHandDrill = "AC - G5p Drilling System";
			AltairHandDrillBattery = "AC - Battery Pack";
			AltairHandDrillCanister = "AltCorp Raw Resource Canister";
			AltairResourceContainer = "AltCorp Small Resource Container";
			AltairRefinedCanister = "AltCorp Refined Resource Canister";
			AltairCrowbar = "Crowbar";
			AltairGlowStick = "Glow stick";
			AltairMedpackSmall = "Small stimpack";
			AltairMedpackBig = "Advanced stimpack";
			AltairDisposableHackingTool = "Disposable Hacking Tool";
			AltairHandheldAsteroidScanningTool = "AC - ASAT Scanner";
			LogItem = "Log file";
			GenericItem = "Generic item";
			APGrenade = "Flail AP Grenade";
			EMPGrenade = "Swordbreaker EMP Grenade";
			PortableTurret = "SDS 'Trident' AP sentry-gun";
			RepairTool = "Welding Tool";
			SoeSuit = "Sons of Earth Mk9 - Pressure Suit";
			SoeHelmet = "Sons of Earth Mk9 - Helmet";
			SoeJetpack = "Sons of Earth Mk9 - Jetpack";
			AegisAssaultRifle = "Aegis assault rifle";
			AltairRifleDescription = "A compact AltCorp carbine that combines good stopping power and decent rate of fire with exceptional accuracy.\n\nSingle: Dmg - 20, RPM - 150\nAuto: Dmg - 18, RPM - 300\n";
			MilitaryAssaultRifleDescription = "SDS assault rifle designed to provide reliable firepower in any situation.\n\nAuto: Dmg - 21, RPM - 600\n";
			MilitarySniperRifleDescription = "Magnetic version of the old anti-materiel rifle, the SDS ‘Lance’ is capable of delivering lethal force at any range with pinpoint accuracy.\n\nSingle: Dmg - 75, RPM - 36\n";
			MilitaryHandGun01Description = "Small and compact, SDS ‘Rapier’ provides lethal firepower ideal for close quarters, making it perfect for peacekeeping duties.\n\nSingle: Dmg - 16, RPM - 180\nAuto: Dmg - 14, RPM - 480\n";
			MilitaryHandGun02Description = "Heaviest hitter of all conventional weapons, SDS ‘Glaive’ is the most powerful sidearm available to Expedition personnel.\n\nSingle: Dmg - 28, RPM - 110\n";
			AltairRifleAmmoDescription = "6mm rifle ammo\n\n";
			MilitaryAssaultRifleAmmoDescription = "8mm rifle ammo\n\n";
			MilitarySniperRifleAmmoDescription = "12mm sniper ammo\n\n";
			MilitaryHandGunAmmo01Description = "8mm pistol ammo\n\n";
			AltairPressurisedSuitDescription = "Standard Altair Corporation multi-purpose pressure suit.\n\nBallistic Protection - 4\n";
			AltairEVASuitDescription = "Reinforced pressure suit, designed for prolonged EVA sessions and modified for use in Hellion system.\n";
			AltairPressurisedHelmetDescription = "Battery powered HUD and communications interface for the AltCorp Mk9 Pressure suit.\n";
			AltairEVAHelmetDescription = "Battery powered HUD and communications interface for the 'Proteus-h' EVA suit.\n";
			AltairPressurisedJetpackDescription = "Oxygen supply and zero-g maneuvering system designed for the AltCorp Mk9 Pressure suit.\n";
			AltairEVAJetpackDescription = "Oxygen supply and zero-g maneuvering system designed for the 'Proteus-h' EVA suit.\n";
			MachineryPartDescription = "Standard machinery part";
			AltairHandDrillDescription = "AltCorp 5th generation portable Drilling System designed for zero-g mining operations.\n\n";
			AltairHandDrillBatteryDescription = "Standard AltCorp battery pack.\n";
			AltairHandDrillCanisterDescription = "Standard AltCorp raw resource container. Compatible with the G5p Drilling System.\n";
			AltairResourceContainerDescription = "Small canister for emergency Jetpack refueling.\n";
			AltairRefinedCanisterDescription = "Standard AltCorp refined resource container.\n";
			AltairCrowbarDescription = "A simple and effective tool. Made out of high quality alloy steel.";
			AltairGlowStickDescription = "A self-contained, temporary light source";
			AltairMedpackSmallDescription = "Standard medical stimpack\n\nRecovery - 10HP/s\nDuration - 3s";
			AltairMedpackBigDescription = "Military grade stimpack\n\nRecovery - 15HP/s\nDuration - 4s";
			AltairDisposableHackingToolDescription = "Overrides access restrictions.\n\nA makeshift hacking device built to overload security systems. Crude but effective.\n";
			AltairHandheldAsteroidScanningToolDescription = "Asteroid scanning device.\n\nAltCorp Asteroid Survey Assist Tool. Used to determine asteroid composition.\n";
			LogItemDescription = "Portable data storage device";
			GenericItemDescription = "Nothing to see here, move along...";
			APGrenadeDescription = "Standard issue SDS anti-personnel frag grenade.\n\nFuse time - 5s\nBlast Radius - 6m\nDmg - 103\n";
			EMPGrenadeDescription = "SDS electromagnetic pulse grenade designed to disable high-end weapon systems without affecting personnel.\n\nFuse time - 5s\nBlast Radius - 10m\n";
			PortableTurretDescription = "Automated sentry.\n\nFully automated SDS anti-personnel weapon that works as part of the station’s security system.\n";
			SoeSuitDescription = "Mk9 Pressure Suit worn by Sons of Earth and customised for combat. Offers added ammo slots over storage capacity.\n\nBallistic Protection - 4\n";
			SoeHelmetDescription = "Mk9 helmet worn by Sons of Earth. Heavily modified and adorned with their logos and symbols.\n";
			SoeJetpackDescription = "Mk9 Jetpack worn by Sons of Earth. Features improved oxygen and fuel capacity compared to the standard version.\n";
			AegisAssaultRifleDescription = "Custom version of the V-22 'Broadsword' assault rifle. Reserved exclusively for members of the AEGIS task force.\n\nAuto: Dmg - 25, RPM - 500\n";
			Flag = "AltCorp Flag";
			BasketBall = "Basketball";
			BookHolder = "Book holder";
			Hoop = "Hoop";
			LavaLamp = "Lava lamp";
			PlantRing = "Verdant Aureole";
			PlantZikaLeaf = "Zika Leaf";
			PlantCanister = "Sapphire Light";
			Poster = "Poster";
			PosterBethyr = "Big Blue poster";
			PosterBurner = "Live at Burner poster";
			PosterEverest = "Everest station poster";
			PosterHellion = "Welcome to Hellion Poster";
			PosterTurret = "S.D.S. Trident poster";
			PosterCrewQuarters = "Stellar Resorts poster";
			PosterSonsOfEarth = "Sons of Earth poster";
			TeslaBall = "Tesla ball";
			Picture = "Picture";
			AltCorp_Cup = "AltCorp cup";
			CoffeeMachine = "Coffee machine";
			BrokenArmature = "Broken armature";
			ShatteredPlating = "Shattered plating";
			FriedElectronics = "Fried electronics";
			DamagedTransmiter = "Damaged transmitter";
			RupturedInsulation = "Ruptured insulation";
			BurnedPDU = "Burned PDU";
			DiamondCore = "Detachable G5p Drill Bit";
			FlagDescription = "Traditional flag of the Altair Corporation's Engineering Corps";
			BasketBallDescription = "For some quality zero-g slam dunks";
			BookHolderDescription = "A rather disturbing book holder. Just looking at it makes you feel uneasy";
			HoopDescription = "Basketball Hoop\nNear indestructible and zero-g friendly";
			LavaLampDescription = "Lava lamp\nA perfect night light solution for your quarters";
			PlantRingDescription = "Decorative plant ring.\n Sealed against vacuum exposure";
			PlantZikaLeafDescription = "A rare strain of the bio-phosphorescent Zika Leaf plant.\nSealed against vacuum exposure";
			PlantCanisterDescription = "Decorative bio-phosphorescent plant.\nSealed against vacuum exposure.";
			PosterDescription = "Promotional poster";
			TeslaBallDescription = "A small decorative lamp with a built in power source.";
			PictureDescription = "A genuine work of art";
			AltCorp_CupDescription = "Nothing like a warm cup of that special blend.";
			CoffeeMachineDescription = "Do not use in zero gravity conditions";
			ScrapDescription = "Recyclable. Otherwise useless.";
			DiamondCoreDescription = "A replaceable diamond core drill bit specifically designed for the G5p Drilling System\n";
			Fuse = "Fuse";
			ServoMotor = "Servo motor";
			SolarPanel = "Solar panel";
			ExternalAirVent = "External air vent";
			AirProcessingController = "Air processing controller";
			CarbonFilters = "Carbon filters";
			AirFilterUnit = "Air filter unit";
			PressureRegulator = "Pressure regulator";
			RadarSignalAmplifier = "Radar signal amplifier";
			CoreContainmentFieldGenerator = "Core containment field generator";
			ResourceInjector = "Resource injector";
			ThermonuclearCatalyst = "Catalyst";
			ExternalDeuteriumExhaust = "External Helium-3 exhaust";
			PowerCollector = "Power collector";
			PowerDiffuser = "Power diffuser";
			GrapheneNanotubes = "Graphene nanotubes";
			PowerDisipator = "Power dissipator";
			NaniteCore = "Civilian nanite core";
			EmShieldGenerator = "EM shield generator";
			HighEnergyLaser = "High energy laser";
			RcsThrusters = "RCS Thrusters";
			SingularityCellDetonator = "Singularity cell detonator";
			WarpFieldGenerator = "Warp field generator";
			HighEnergyConverter = "High energy converter";
			SingularityContainmentField = "Singularity containment field";
			WarpInductor = "Warp inductor";
			WarpCell = "Warp cell";
			EMFieldController = "EM Field controller";
			MilitaryNaniteCore = "Military nanite core";
			ThermonuclearCatalystDescription = "Increases system power output";
			ResourceInjectorDescription = "Reduces system resource consumption";
			CoreContainmentFieldGeneratorDescription = "Increases capacitor's maximum storage";
			EMFieldControllerDescription = "Reduces power consumption";
			ServoMotorDescription = "Reduces system power consumption";
			AirProcessingControllerDescription = "O2 CONSUMPTION of AIR FILTER\nPOWERUP TIME of AIR FILTER";
			CarbonFiltersDescription = "Eliminates O2 consumption";
			AirFilterUnitDescription = "MAX AIR FILTERING of LS NODE";
			PlasmaAcceleratorDescription = "Increases maximum thrust";
			HighEnergyLaserDescription = "ACCELERATION SPEED of MAIN ENGINE";
			SingularityCellDetonatorDescription = "Expands warp options";
			WarpCellDescription = "Standardized warp core cell container, compatible with all Expedition systems";
			PressureRegulatorDescription = "MAX AIR OUTPUT of LS NODE";
			NaniteCoreDescription = "Enhances vessel armor\nCompatible with all Nanite Diffuser Systems.\n\nMultipurpose civilian nanite core designed for prolonged exposure in low risk areas.\nExceptional durability at the cost of protection.\n";
			MilitaryNaniteCoreDescription = "Enhances vessel armor\nCompatible with all Nanite Diffuser Systems.\n\nMilitary grade nanite core designed for search and rescue operations in high risk areas.\nImproved protection at the cost of durability.\n";
			EngineStatus = "Engine status";
			ENG = "ENG";
			FTL = "FTL";
			Contacts = "Contacts";
			RadarRange = "Radar range";
			Matched = "Velocity matched with";
			ClusterOfVessels = "Cluster of vessels";
			Available = "Available";
			ETA = "ETA";
			DrivingTips = "Driving tips";
			Stabilize = "Stabilize";
			PilotingNotActive = "Piloting not active";
			CloseDoor = "Close door";
			OpenDoor = "Open door";
			AreYouSureAirlock = "Are you sure you want to open door by force?";
			DangerAirlock = "It's not safe to open door, there is difference in pressure!";
			Stop = "Stop";
			RemoveOutfit = "Remove outfit";
			AirOutput = "Air output";
			ChangeShipName = "Change ship name";
			Temperature = "Temperature";
			VesselStatus = "Hull integrity";
			GravityFail = "Gravity fail";
			Defective = "Defective";
			Breach = "Breach";
			Fire = "Fire";
			FireHazard = "Fire hazard";
			GravityMalfunction = "Gravity Malfunction";
			Failure = "Failure";
			Hull = "Hull";
			RepairPointMessageRoom = "{0} detected (location: {1})";
			RepairPointMessageVesselRoom = "{0} detected (vessel: {1}, location: {2})";
			Light = "Light";
			EmergencyLight = "Emergency light";
			Fabricator = "Fabricator";
			Refinery = "Refinery";
			Systems = "Systems";
			Health = "Health";
			FuelLevels = "Fuel levels";
			FireExtinguisherDescription = "Device used to extinguish or control small fires";
			FireExtingusher = "Fire extinguisher";
			NoItemAttachedToCargo = "No item attached to cargo";
			NoRefineryAvailable = "No refinery available";
			AmountToTransfer = "Amount to transfer";
			AllVessels = "All vessels";
			AttachPoint = "Attach point";
			AirlockPressure = "Airlock pressure";
			DoorControl = "Door control";
			DistressCallActive = "Distress call active";
			Disconnected = "Disconnected";
			UnavailableFromInGameMenu = "Changeable only from main menu";
			CheckRcsUtilityAccess = "Check rcs utility access";
			SelfDestruct = "Self destruct";
			Activate = "Activate";
			ConnectingToInvite = "Connecting to invite";
			ShutDown = "Shut down";
			Deploy = "Deploy";
			Retract = "Retract";
			AreYouSureSelfDestruct = "Are you sure you want to activate self destruction?";
			ChangeShipEmblem = "Change ship emblem";
			RefiningTime = "Refining time";
			InDebrisField = "In debris field";
			CapacitorsTotal = "Capacitors total";
			Tier = "Tier";
			Output = "Output";
			ResourcesConsumption = "Resources consumption";
			PowerCapacity = "Power capacity";
			PowerUpTime = "Power up time";
			CoolDownTime = "Cool down time";
			InsertPartToImprove = "Insert part to improve";
			NoLifeSupportConnected = "No life support system connected";
			NoPowerSupplyConnected = "No power supply system connected";
			TotalOutput = "Total output";
			TotalConsumption = "Total consumption";
			TotalCapacity = "Total capacity";
			UnauthorizedAccess = "Unauthorized access";
			Attached = "Attached";
			TransferResources = "Transfer resources";
			BaseConsumption = "Base consumption";
			Canister = "Canister";
			Generator = "Generator";
			RequiredResources = "Required resources";
			Jettison = "Jettison";
			InsertItem = "Insert item";
			From = "From";
			To = "To";
			Craft = "Craft";
			Danger = "Danger!";
			ChooseAnItemToCraft = "Choose an item to craft";
			RemoveItem = "Remove item";
			ChooseAnItem = "Choose an item";
			NotEnoughResources = "Not enough resources";
			NoPower = "No power";
			RecyclingOutput = "Recycling output";
			VolumetricLighting = "Volumetric lighting";
			NoAirTankAvailable = "No air tank available";
			UnableToDepressurize = "Unable to depressurize";
			UnableToPressurize = "Unable to pressurize";
			AirCapacity = "Air capacity";
			ConnectedLifeSupportSystems = "Connected life support systems";
			ConnectedPowerSupplySystems = "Connected power supply systems";
			ConnectedVessels = "Connected vessels";
			SunExposure = "Sun exposure";
			ConnectedCargos = "Connected cargos";
			VesselSystems = "Vessel systems";
			NoFabricatorAvailable = "No fabricator available";
			TransferFrom = "Transfer from";
			TransferTo = "Transfer to";
			NoSunExposure = "No sun exposure";
			ModuleVolume = "Module volume";
			FilteringRate = "Filtering rate";
			DragResourcesForCrafting = "Drag resources for crafting";
			JoinHellionOnDiscord = "Join hellion \non discord";
			AirLockcontrols = "Airlock controls";
			LifeSupportInfo = "Maintains breathable conditions on ships and stations by generating air to increase room pressure and by filtering existing air to improve air quality. It consists of Air Generator sub-system, Air Tank and Air Filter sub-system.";
			AirGeneratorDescription = "Air generator consumes <color=#ACAE15>power</color>, <color=#0DC2CC>oxygen</color> and <color=#28C066>nitrogen</color> to generate air for air tank.";
			AirFilterDescription = "Air filter consumes <color=#ACAE15>power</color>, <color=#0DC2CC>oxygen</color> or carbon filter to replenish air quality.";
			AirTankDescription = "Air tank serves as <color=#0DC2CC>air</color> storage for pressurizing and depressurizing rooms.";
			VolumeDescription = "Maximum air volume of room";
			PressurizeDescription = "Pressurize from air tank";
			DepressurizeDescription = "Depressurize to air tank";
			VentActionDescription = "Vents air to outer space";
			PowerSupplySystem = "Power supply system";
			PowerSupplyInfo = "System is in charge of generating power for ships and stations. Its main components are Solar panels, Fusion Reactor and a Capacitor.";
			CapacitorDescription = "Capacitor acts as a battery and stores any excess <color=#ACAE15>power</color> that your ship/station is not using and allows operation of systems with extreme <color=#EA4141>power requirements</color>.";
			SolarPanelDescription = "Solar Panels utilize sunlight to generate <color=#ACAE15>power</color>. Their efficiency depends on the distance from the <color=#FF9C5C>sun</color>.";
			FusionReactorDescription = "Fusion Reactor consumes <color=#88498B>Helium-3</color> to generate power.";
			CurrentVesselConsumtion = "Current vessel consumption";
			ConnectedVesselDescription = "Docked vessel list shows all docked ships and modules along with their current <color=#EA4141>power consumption</color> values. All vessels have a base or minimum <color=#EA4141>power requirement</color> before any of their systems can be activated.";
			ToggleBaseConsumption = "Toggle base consumption";
			Ammo = "Ammo";
			Suits = "Suits";
			Rifles = "Rifles";
			Handguns = "Handguns";
			Grenades = "Grenades";
			Helmets = "Helmets";
			ToggleGravity = "Toggle artificial gravity";
			AirTankNotConnected = "Air tank not connected";
			ExitCryo = "Exit cryopod";
			EnterCryo = "Enter cryopod";
			RecyclerSlot = "Recycler slot";
			ResearchSlot = "Research slot";
			Piloting = "Pilot Ship";
			ServerRestartIn = "Server restart in";
			EngineNotAvailable = "Engine not available";
			CargoFull = "Cargo full";
			SaveGame = "Quick save";
			LoadGame = "Quick load";
			Console = "Console";
			Items = "Items";
			Modules = "Modules";
			CommandList = "Command list";
			Medpack = "Medpack";
			SmallItems = "Small Items";
			MediumItems = "Medium Items";
			LargeItems = "Large Items";
			BasketballHoop = "Basketball Hoop";
			AmbienceVolume = "Ambience volume";
			Consumable = "Consumable";
			Hands = "Hands";
			Helmet = "Helmet";
			Outfit = "Outfit";
			Primary = "Primary";
			Secondary = "Secondary";
			Utility = "Utility";
			Tool = "Tool";
			ResearchRequired = "Research required";
			AllTiersUnlocked = "All tiers unlocked";
			FurtherResearchRequired = "Further research required to unlock all tiers";
			Radiation = "Radiation";
			SystemFailiure = "System failure";
			Researching = "Researching";
			Recycling = "Recycling";
			Weapons = "Weapons";
			Tools = "Tools";
			Parts = "Parts";
			Medical = "Medical";
			General = "General";
			Magazines = "Magazines";
			Containers = "Containers";
			CellConsumption = "Cell consumption";
			ZeroGravityMovement = "Zero gravity movement tips";
			Rotation = "Rotation";
			SlotFor = "Slot for";
			MultipleItems = "Multiple items";
			CargoAttachPoint = "Cargo attach point for canister, jetpack, welding tool or fire extinguisher";
			PowerRechargeStation = "Power recharge station for rifle, battery or jetpack";
			DockingPanel = "Docking panel controls";
			CryoPanel = "Cryo chamber panel controls";
			CargoPanel = "Cargo panel controls";
			LifeSupportPanel = "Life support panel controls";
			PowerSupplyPanel = "Power supply panel controls";
			AirlockPanel = "Airlock panel controls";
			NavigationPanel = "Navigation panel";
			DockingPortController = "Docking port controller";
			MessageTerminal = "Message terminal";
			HackingDescription = "Use hacking tool to open door";
			PullToUndock = "Pull to undock";
			CapacitorTitle = Capacitor;
			PowerOutputTitle = PowerOutput;
			PowerConsumptionTitle = PowerConsumption;
			ChangeRateTitle = Capacitor;
			AirTankTitle = AirTank;
			ServoMotorTitle = ServoMotor;
			ResourceInjectorTitle = ResourceInjector;
			CarbonFilterTitle = CarbonFilters;
			CatalystTitle = ThermonuclearCatalyst;
			CapacitorTooltip = CapacitorDescription;
			SunExposureTitle = SunExposure;
			ChangeRateOxygenTitle = Oxygen;
			ChangeRateNitorgenTitle = Nitrogen;
			HeliumChangeRateTitle = Helium3;
			SunExposureRateTitle = SunExposure;
			PowerOutputTooltip = "Current output of power";
			PowerConsumptionTooltip = "Current consumption of power";
			ChangeRateTooltip = "Charge rate";
			AirTankTooltip = AirTankDescription;
			VesselsOutputTooltip = "Connected vessels";
			SelectedVesselTooltip = "Selected vessel";
			ServoMotorTooltip = "Reduces power consumption";
			ResourceInjectorTooltip = "Reduces oxygen, nitrogen and helium-3 consumption";
			CarbonFilterTooltip = "Reduces oxygen consumption to 0";
			CatalystTooltip = "Increases power output";
			SunExposureTooltip = "It affects output based on distance from the sun.";
			ManeuverTimeTitle = "Maneuver time";
			ManeuverTimeTooltip = "Adjust maneuver time";
			WarpPowerTitle = "Power for warp";
			WarpPowerTooltip = "Store enough power for warp drive";
			WarpCellsTitle = "Warp cells";
			WarpCellsTooltip = "Get enough fuel in warp cells for warp drive";
			DockedVesselsTitle = "Docked vessels";
			DockedVesselsTooltip = "Do not exceed maximum number of docked vessels in order to warp";
			ScanButtonTitle = "Scan action";
			ScanButtonTooltip = "Scan objects around your vessel";
			VesselPowerTitle = "Vessel power";
			VesselPowerTooltip = "Turn on vessel power";
			RentYourOwnServer = "Rent your own server";
			VesselPowerOffline = "Vessel power offline";
			DoorOpenWarningTitle = "Warning door open";
			DoorOpenWarningTooltip = "Can't pressurize while outer door is open";
			Warning = "Warning";
			ChangeRateOxygenTooltip = "Oxygen consumption rate";
			ChangeRateNitrogenTooltip = "Nitrogen consumption rate";
			HeliumChangeRateTootlip = "Helium-3 consumption rate";
			SunExposureRateTootlip = "Sun exposure efficiency";
			InGameDescription = "Playing on server";
			OrbitingNear = "Orbiting near";
			WarpingNear = "Warping near";
			FloatingFreelyNear = "Floating freely near";
			TeleportingFromDiscord = "Teleporting from Discord";
			GravityInfluenceRadius = "Gravity Influence Radius";
			Radius = "Radius";
			MultipleObjects = "Multiple objects";
			ObjectsInGroup = "Objects in group";
			DefaultMapAsteroidDescription = "Visit Asteroid to mine resources.";
			DefauldMapCelestialDescription = "Add Custom Orbit to visit this planet.";
			CustomOrbitDescription = "Use MANIPULATORS to edit the orbit, or enter COORDINATES manually.";
			AltCorp_Shuttle_SARA = "Arges MkII";
			AltCorp_Shuttle_CECA = "Steropes";
			AltCorp_CorridorModule = "Corridor I";
			AltCorp_CorridorIntersectionModule = "Corridor T";
			AltCorp_Corridor45TurnModule = "Corridor L";
			AltCorp_Corridor45TurnRightModule = "Corridor L";
			AltCorp_CorridorVertical = "Corridor S";
			ALtCorp_PowerSupply_Module = "Power Supply Module";
			AltCorp_LifeSupportModule = "Life Support Module";
			AltCorp_Cargo_Module = "Cargo Module";
			AltCorp_Command_Module = "Command Module";
			AltCorp_StartingModule = "Hibernation Module";
			AltCorp_AirLock = "Airlock Module";
			AltCorp_DockableContainer = "Industrial Container";
			AltCorp_CrewQuarters_Module = "Crew Quarters Module";
			AltCorp_SolarPowerModule = "Solar Power Module";
			AltCorp_FabricatorModule = "Fabricator Module";
			SmallOutpost = "Small outpost";
			MediumOutpost = "Medium outpost";
			LargeOutpost = "Large outpost";
			SmallStation = "Small station";
			MediumStation = "Medium station";
			LargeStation = "Large station";
			AlreadyEquipped = "Already equipped";
			EquipSuitFirst = "Equip suit first";
			Error = "Error";
			UnableToStartSPGame = "Unable to start single player game.";
			Hellion_SpExeIsMissing = "File 'HELLION_SP.EXE' is missing. Game installation might be incomplete/corrupted or your AV software might have quarantined it.";
			RemoveOutfitTooltip = "You must remove helmet and jetpack first";
			Load = "Load";
			LootAll = "Loot all";
			Journal = "Journal";
			Blueprints = "Blueprints";
			NoSuitEquipped = "No suit equipped";
			NoLogAvailable = "No log available";
			ToggleTracking = "ToggleTracking";
			Tracking = "Tracking";
			ProximityLoot = "Proximity loot";
			Drop = "Drop";
			Loot = "Loot";
			TakeAll = "Take all";
			ShowContainerSlots = "Show container slots";
			NoMagazine = "No magazine";
			FireMode = "Fire mode";
			AutoFireMode = "Auto";
			SingleFireMode = "Single";
			FireType = "Fire type";
			ArmorPiercingFireType = "Armor piercing";
			FragmentationFireType = "Fragmentation";
			IncediaryFireType = "Incediary";
			NormalFireType = "Normal";
			PleaseAssignAllControls = "Please assign all controls";
			DataPrivacySettings = "Data privacy settings";
			Edit = "Edit";
			ServerInfo = "Server info";
			ExternalBrowserPage = "This will open an external browser page";
			Refill = "Refill";
			Roll = "Roll";
			HideTipsFromMenu = "You can hide tips from settings menu";
			ChangeTargetPort = "Change target port";
			ChangeTarget = "Change target";
			ToggleEngine = "Toggle engine";
			MatchTargetsVelocity = "Match target velocity";
			CollisionWarning = "Collision warning";
			OffSpeedAssistant = "Toggle off speed assistance";
			ChangeRadarRange = "Change radar range";
			OffTarget = "Off target";
			NoTargetModulesInRange = "Unauthorized access to target modules or out of range";
			CreateCharacterLore = "The industrialists behind the Expedition project, the Altair Corporation was one of the largest groups involved in the colonization of Hellion second only to UN Government. It was responsible for the production of majority of resources as well as orbital construction from shipyards and refineries to large habitats.\n\nLike the company itself, their equipment is exceptionally robust and flexible. Their ships are among the most versatile in Hellion and can usually fit any role from mining and exploration to military applications. If you want something built safe and reliable, you can\ufffdt go wrong with Alt-Corp!";
			RcsCancelManeuver = "Using RCS thrusters will cancel warp maneuver!";
			QuestTerminalHint = "Check quest terminal for new available quests";
			ToBeContinued = "To be continued";
			WrongSavegameVersion = "Wrong savegame version";
			Battery = "Battery";
			BatteryMissing = "Battery missing";
			OutOfRange = "Out of range";
			NotScannable = "Not scannable object";
			ScanningRange = "Scanning range";
			Low = "Low";
			Medium = "Medium";
			High = "High";
			Unknown = "Unknown";
			Armor = "Armor";
			UsesLeft = "Uses left";
			UnknownObject = "Unknown object";
			Equipment = "Equipment";
			ResourcesLabel = "Resources";
			Networking = "Networking";
			Sent = "Sent";
			Received = "Received";
			Reset = "Reset";
			TowingDisabled = "Towing disabled";
			NoDockedVessels = "No docked vessels";
			WarpSignature = "Warp signature";
			WarpSignatureDescription = "Warp signature detected. Further investigation required.";
			UnidentifiedObject = "Unidentified object";
			UnidentifiedObjectDescription = "Unidentified object detected. Further investigation required.";
			NoPart = "No part";
			PossibleContacts = "Possible contacts";
			RadarSignature = "Radar signature";
			SpawnMiningDescription = "Start with a ship and basic equipment close to a random ore rich asteroid";
			SpawnFreeRoamDescription = "Start with a ship and basic equipment on a random stable orbit";
			SpawnPremadeStationDescription = "Start with a ship and basic equipment close to a random pre-made station";
			SpawnDoomedDescription = "Start with a ship and basic equipment on a collision orbit close to a Doomed outpost";
			FreeRoamSteropes = "New free roam Steropes";
			FreeRoamSteropesDescription = SpawnFreeRoamDescription;
			MiningSteropes = "New mining Steropes";
			MiningSteropesDescription = SpawnMiningDescription;
			SteropesNearRandomStation = "New Steropes near random station";
			SteropesNearRandomStationDescription = SpawnPremadeStationDescription;
			SteropesNearDoomedOutpost = "New Steropes near doomed outpost";
			SteropesNearDoomedOutpostDescription = SpawnDoomedDescription;
			MiningArges = "New mining Arges";
			MiningArgesDescription = SpawnMiningDescription;
			FreeRoamArges = "New free roam Arges";
			FreeRoamArgesDescription = SpawnFreeRoamDescription;
			PressForInventory = "Press [TAB] to access inventory";
			PressForLights = "Press [L] to toggle lights";
			RotationControls = "In zero-g use [Q] and [E] to rotate";
			HoldToStabilize = "Hold [SHIFT] to stabilize rotation and grab onto nearby walls";
			UseResourceCanister = "Place Resource Canister into hands and press [LMB] to refill your Jetpack with Oxygen";
			AssignOnCryo = "Assign on cryopod to make it your default spawn point";
			DragItemToHands = "Press [TAB] and drag item to hands slot in order to use it";
			WelderTooltip = "Welder can be used to fix both internal and external hull damage";
			PressForNavigation = "Press [2] to activate navigation screen";
			AvailableQuests = "Available quests";
			QuestActivated = "Quest activated";
			QuestUpdated = "Quest updated";
			QuestCompleted = "Quest completed";
			QuestFailed = "Quest failed";
			Objectives = "Objectives";
			QuestLog = "Quest log";
			SystemNothing = "I have to say nothing";
			SystemUnstable = "Outpost - {0}, {1}, unstable orbit, collision in {2}";
			SystemShipCalled = "Ship is already called.";
			SystemShipInRange = "Ship is already in range.";
			SystemShipArrive = "Ship will arrive in {0}";
			SystemServerRestart = "Server will restart in {0} {1}";
			NoNakamaConnection = "Encountered error when trying to connect to server. Check your internet connection.";
			NoServerConnection = "Server refused connection. Please try again later.";
			AccountNotFound = "Account with this email or username doesn't exist.";
			AccountAlreadyExists = "Account with this email or username already exists.";
			InvalidEmail = "Email is invalid. Should be written as example@mail.com";
			InvalidUsername = "Username is invalid. Valid characters are letters (a-z), numbers (0-9), full stops (.), and underscores (_).";
			ConsentToDataStorage = "You need to give consent to storing data before you can create an account";
			defaultValues = GetJsonString();
			Initialize();
			ControlsRebinder.Initialize();
		}

		private static void Initialize()
		{
			TutorialText = new Dictionary<int, string>
			{
				{ 0, Tut_Undefined },
				{ 1, Tut_1 },
				{ 2, Tut_2 },
				{ 3, Tut_3 },
				{ 4, Tut_4 },
				{ 5, Tut_5 },
				{ 6, Tut_6 },
				{ 7, Tut_7 }
			};

			PreloadText = new[]
			{
				Preload01, Preload02, Preload03, Preload04, Preload05, Preload06, Preload07, Preload08, Preload09, Preload10,
				Preload11, Preload12, Preload13, Preload14, Preload15, Preload16, Preload17, Preload18, Preload19, Preload20,
				Preload21, Preload22, Preload23, Preload24, Preload25, Preload26, Preload27, Preload28, Preload29, Preload30,
				Preload31, Preload32, Preload33, Preload34, Preload35, Preload36, Preload37, Preload38, Preload39, Preload40,
				Preload41, Preload42, Preload43, Preload44, Preload45, Preload46, Preload47, Preload48
			};

			SystemChatName = new[] { AutomatedDistress, System, System, System, System, System };

			GenericItemsDescriptions = new Dictionary<GenericItemSubType, string>
			{
				{ GenericItemSubType.None, None },
				{ GenericItemSubType.Flag, FlagDescription },
				{ GenericItemSubType.BasketBall, BasketBallDescription },
				{ GenericItemSubType.BookHolder, BookHolderDescription },
				{ GenericItemSubType.Hoop, HoopDescription },
				{ GenericItemSubType.LavaLamp, LavaLampDescription },
				{ GenericItemSubType.PlantRing, PlantRingDescription },
				{ GenericItemSubType.PlantZikaLeaf, PlantZikaLeafDescription },
				{ GenericItemSubType.PlantCanister, PlantCanisterDescription },
				{ GenericItemSubType.PosterBethyr, PosterDescription },
				{ GenericItemSubType.PosterBurner, PosterDescription },
				{ GenericItemSubType.PosterEverest, PosterDescription },
				{ GenericItemSubType.PosterHellion, PosterDescription },
				{ GenericItemSubType.PosterTurret, PosterDescription },
				{ GenericItemSubType.PosterCrewQuarters, PosterDescription },
				{ GenericItemSubType.PosterSonsOfEarth, PosterDescription },
				{ GenericItemSubType.TeslaBall, TeslaBallDescription },
				{ GenericItemSubType.Picture, PictureDescription },
				{ GenericItemSubType.AltCorp_Cup, AltCorp_CupDescription },
				{ GenericItemSubType.CoffeeMachine, CoffeeMachineDescription },
				{ GenericItemSubType.BrokenArmature, ScrapDescription },
				{ GenericItemSubType.ShatteredPlating, ScrapDescription },
				{ GenericItemSubType.FriedElectronics, ScrapDescription },
				{ GenericItemSubType.DamagedTransmiter, ScrapDescription },
				{ GenericItemSubType.RupturedInsulation, ScrapDescription },
				{ GenericItemSubType.BurnedPDU, ScrapDescription },
				{ GenericItemSubType.DiamondCoreDrillBit, DiamondCoreDescription }
			};

			ItemCategoryNames = new Dictionary<ItemCategory, string>
			{
				{ ItemCategory.Containers, Containers },
				{ ItemCategory.General, General },
				{ ItemCategory.Magazines, Magazines },
				{ ItemCategory.Medical, Medical },
				{ ItemCategory.Parts, Parts },
				{ ItemCategory.Suits, Suits },
				{ ItemCategory.Tools, Tools },
				{ ItemCategory.Utility, Utility },
				{ ItemCategory.Weapons, Weapons }
			};

			MachineryPartsDescriptions = new Dictionary<MachineryPartType, string>
			{
				{ MachineryPartType.ThermonuclearCatalyst, ThermonuclearCatalystDescription },
				{ MachineryPartType.ResourceInjector, ResourceInjectorDescription },
				{ MachineryPartType.CoreContainmentFieldGenerator, CoreContainmentFieldGeneratorDescription },
				{ MachineryPartType.EMFieldController, EMFieldControllerDescription },
				{ MachineryPartType.ServoMotor, ServoMotorDescription },
				{ MachineryPartType.AirProcessingController, AirProcessingControllerDescription },
				{ MachineryPartType.CarbonFilters, CarbonFiltersDescription },
				{ MachineryPartType.AirFilterUnit, AirFilterUnitDescription },
				{ MachineryPartType.PressureRegulator, PressureRegulatorDescription },
				{ MachineryPartType.NaniteCore, NaniteCoreDescription },
				{ MachineryPartType.HighEnergyLaser, PlasmaAcceleratorDescription },
				{ MachineryPartType.SingularityCellDetonator, SingularityCellDetonatorDescription },
				{ MachineryPartType.WarpCell, WarpCellDescription },
				{ MachineryPartType.MillitaryNaniteCore, MilitaryNaniteCoreDescription }
			};

			ItemsDescriptions = new Dictionary<ItemType, string>
			{
				{ ItemType.None, None },
				{ ItemType.AltairRifle, AltairRifleDescription },
				{ ItemType.MilitaryAssaultRifle, MilitaryAssaultRifleDescription },
				{ ItemType.MilitarySniperRifle, MilitarySniperRifleDescription },
				{ ItemType.MilitaryHandGun01, MilitaryHandGun01Description },
				{ ItemType.MilitaryHandGun02, MilitaryHandGun02Description },
				{ ItemType.AltairRifleAmmo, AltairRifleAmmoDescription },
				{ ItemType.MilitaryAssaultRifleAmmo, MilitaryAssaultRifleAmmoDescription },
				{ ItemType.MilitarySniperRifleAmmo, MilitarySniperRifleAmmoDescription },
				{ ItemType.MilitaryHandGunAmmo01, MilitaryHandGunAmmo01Description },
				{ ItemType.AltairPressurisedSuit, AltairPressurisedSuitDescription },
				{ ItemType.AltairEVASuit, AltairEVASuitDescription },
				{ ItemType.AltairPressurisedHelmet, AltairPressurisedHelmetDescription },
				{ ItemType.AltairEVAHelmet, AltairEVAHelmetDescription },
				{ ItemType.AltairPressurisedJetpack, AltairPressurisedJetpackDescription },
				{ ItemType.AltairEVAJetpack, AltairEVAJetpackDescription },
				{ ItemType.MachineryPart, MachineryPartDescription },
				{ ItemType.AltairHandDrill, AltairHandDrillDescription },
				{ ItemType.AltairHandDrillBattery, AltairHandDrillBatteryDescription },
				{ ItemType.AltairHandDrillCanister, AltairHandDrillCanisterDescription },
				{ ItemType.AltairResourceContainer, AltairResourceContainerDescription },
				{ ItemType.AltairRefinedCanister, AltairRefinedCanisterDescription },
				{ ItemType.AltairCrowbar, AltairCrowbarDescription },
				{ ItemType.AltairGlowStick, AltairGlowStickDescription },
				{ ItemType.AltairMedpackSmall, AltairMedpackSmallDescription },
				{ ItemType.AltairMedpackBig, AltairMedpackBigDescription },
				{ ItemType.AltairDisposableHackingTool, AltairDisposableHackingToolDescription },
				{ ItemType.AltairHandheldAsteroidScanningTool, AltairHandheldAsteroidScanningToolDescription },
				{ ItemType.LogItem, LogItemDescription },
				{ ItemType.GenericItem, GenericItemDescription },
				{ ItemType.APGrenade, APGrenadeDescription },
				{ ItemType.EMPGrenade, EMPGrenadeDescription },
				{ ItemType.PortableTurret, PortableTurretDescription },
				{ ItemType.Welder, RepairTool },
				{ ItemType.FireExtinguisher, FireExtinguisherDescription },
				{ ItemType.SoePressurisedSuit, SoeSuitDescription },
				{ ItemType.SoePressurisedJetpack, SoeJetpackDescription },
				{ ItemType.SoePressurisedHelmet, SoeHelmetDescription },
				{ ItemType.AegisAssaultRifle, AegisAssaultRifleDescription }
			};

			SystemChat = new[] { SystemNothing, SystemUnstable, SystemShipCalled, SystemShipInRange, SystemShipArrive, SystemServerRestart };

			Enums = new Dictionary<Enum, string>
			{
				{ UI.Settings.SettingsType.All, All },
				{ UI.Settings.SettingsType.Audio, Audio },
				{ UI.Settings.SettingsType.Controls, Controls },
				{ UI.Settings.SettingsType.Video, Video },
				{ UI.Settings.SettingsType.Game, Game },
				{ Gender.Male, Male.ToUpper() },
				{ Gender.Female, Female.ToUpper() },
				{ SpawnPointState.Authorized, Authorized },
				{ SpawnPointState.Locked, Locked },
				{ SpawnPointState.Unlocked, Unlocked },
				{ HurtType.None, CODNone },
				{ HurtType.Pressure, CODPressure },
				{ HurtType.Frost, CODFrost },
				{ HurtType.Heat, CODHeat },
				{ HurtType.Impact, CODImpact },
				{ HurtType.Shot, CODShot },
				{ HurtType.Suffocate, CODSuffocate },
				{ HurtType.Suicide, CODSuicide },
				{ HurtType.Shipwreck, CODShipwrecked },
				{ HurtType.Shred, CODShredded },
				{ HurtType.Explosion, CODExplosion },
				{ HurtType.SpaceExposure, CODSpaceExposure },
				{ VesselDamageType.None, CODNone },
				{ VesselDamageType.Decay, CODVesselDacay },
				{ VesselDamageType.Collision, CODShipwrecked },
				{ VesselDamageType.LargeDebrisHit, CODVesselLargeDebrisHit },
				{ VesselDamageType.SmallDebrisHit, CODVesselSmallDebrisHit },
				{ VesselDamageType.GrenadeExplosion, CODVesselGrenadeExplosion },
				{ VesselDamageType.NearbyVesselExplosion, CODVesselProximityExplosion },
				{ VesselDamageType.SelfDestruct, CODVesselSelfDestruct },
				{ SystemStatus.Offline, Offline },
				{ SystemStatus.Online, Online },
				{ SystemStatus.Cooldown, Cooldown },
				{ SystemStatus.Powerup, Powerup },
				{ SystemStatus.None, None },
				{ ResourceType.Air, Air },
				{ ResourceType.Ice, Ice },
				{ ResourceType.Regolith, Regolith },
				{ ResourceType.DryIce, DryIce },
				{ ResourceType.NitrateMinerals, Nitrates },
				{ ResourceType.Hydrogen, Hydrogen },
				{ ResourceType.Oxygen, Oxygen },
				{ ResourceType.Helium3, Helium3 },
				{ ResourceType.Nitro, Nitro },
				{ ResourceType.Nitrogen, Nitrogen },
				{ ResourceType.CarbonFibers, CarbonFibers },
				{ ResourceType.Alloys, Alloys },
				{ ResourceType.Circuits, Circuits },
				{ ResourceType.Reserved, Reserved },
				{ ItemType.None, None },
				{ ItemType.AltairRifle, AltairRifle },
				{ ItemType.MilitaryAssaultRifle, MilitaryAssaultRifle },
				{ ItemType.MilitarySniperRifle, MilitarySniperRifle },
				{ ItemType.MilitaryHandGun01, MilitaryHandGun01 },
				{ ItemType.MilitaryHandGun02, MilitaryHandGun02 },
				{ ItemType.AltairRifleAmmo, AltairRifleAmmo },
				{ ItemType.MilitaryAssaultRifleAmmo, MilitaryAssaultRifleAmmo },
				{ ItemType.MilitarySniperRifleAmmo, MilitarySniperRifleAmmo },
				{ ItemType.MilitaryHandGunAmmo01, MilitaryHandGunAmmo01 },
				{ ItemType.AltairPressurisedSuit, AltairPressurisedSuit },
				{ ItemType.AltairEVASuit, AltairEVASuit },
				{ ItemType.AltairPressurisedHelmet, AltairPressurisedHelmet },
				{ ItemType.AltairEVAHelmet, AltairEVAHelmet },
				{ ItemType.AltairPressurisedJetpack, AltairPressurisedJetpack },
				{ ItemType.AltairEVAJetpack, AltairEVAJetpack },
				{ ItemType.MachineryPart, MachineryPart },
				{ ItemType.AltairHandDrill, AltairHandDrill },
				{ ItemType.AltairHandDrillBattery, AltairHandDrillBattery },
				{ ItemType.AltairHandDrillCanister, AltairHandDrillCanister },
				{ ItemType.AltairResourceContainer, AltairResourceContainer },
				{ ItemType.AltairRefinedCanister, AltairRefinedCanister },
				{ ItemType.AltairCrowbar, AltairCrowbar },
				{ ItemType.AltairGlowStick, AltairGlowStick },
				{ ItemType.AltairMedpackSmall, AltairMedpackSmall },
				{ ItemType.AltairMedpackBig, AltairMedpackBig },
				{ ItemType.AltairDisposableHackingTool, AltairDisposableHackingTool },
				{ ItemType.AltairHandheldAsteroidScanningTool, AltairHandheldAsteroidScanningTool },
				{ ItemType.LogItem, LogItem },
				{ ItemType.GenericItem, GenericItem },
				{ ItemType.APGrenade, APGrenade },
				{ ItemType.EMPGrenade, EMPGrenade },
				{ ItemType.PortableTurret, PortableTurret },
				{ ItemType.FireExtinguisher, FireExtingusher },
				{ ItemType.Welder, RepairTool },
				{ ItemType.SoePressurisedSuit, SoeSuit },
				{ ItemType.SoePressurisedJetpack, SoeJetpack },
				{ ItemType.SoePressurisedHelmet, SoeHelmet },
				{ ItemType.AegisAssaultRifle, AegisAssaultRifle },
				{ MachineryPartType.None, None },
				{ MachineryPartType.Fuse, Fuse },
				{ MachineryPartType.ServoMotor, ServoMotor },
				{ MachineryPartType.SolarPanel, SolarPanel },
				{ MachineryPartType.ExternalAirVent, ExternalAirVent },
				{ MachineryPartType.AirProcessingController, AirProcessingController },
				{ MachineryPartType.CarbonFilters, CarbonFilters },
				{ MachineryPartType.AirFilterUnit, AirFilterUnit },
				{ MachineryPartType.PressureRegulator, PressureRegulator },
				{ MachineryPartType.RadarSignalAmplifier, RadarSignalAmplifier },
				{ MachineryPartType.CoreContainmentFieldGenerator, CoreContainmentFieldGenerator },
				{ MachineryPartType.ResourceInjector, ResourceInjector },
				{ MachineryPartType.ThermonuclearCatalyst, ThermonuclearCatalyst },
				{ MachineryPartType.ExternalDeuteriumExhaust, ExternalDeuteriumExhaust },
				{ MachineryPartType.PowerCollector, PowerCollector },
				{ MachineryPartType.PowerDiffuser, PowerDiffuser },
				{ MachineryPartType.GrapheneNanotubes, GrapheneNanotubes },
				{ MachineryPartType.PowerDisipator, PowerDisipator },
				{ MachineryPartType.NaniteCore, NaniteCore },
				{ MachineryPartType.MillitaryNaniteCore, MilitaryNaniteCore },
				{ MachineryPartType.EmShieldGenerator, EmShieldGenerator },
				{ MachineryPartType.HighEnergyLaser, HighEnergyLaser },
				{ MachineryPartType.RcsThrusters, RcsThrusters },
				{ MachineryPartType.SingularityCellDetonator, SingularityCellDetonator },
				{ MachineryPartType.WarpFieldGenerator, WarpFieldGenerator },
				{ MachineryPartType.HighEnergyConverter, HighEnergyConverter },
				{ MachineryPartType.SingularityContainmentField, SingularityContainmentField },
				{ MachineryPartType.WarpInductor, WarpInductor },
				{ MachineryPartType.WarpCell, WarpCell },
				{ MachineryPartType.EMFieldController, EMFieldController },
				{ GenericItemSubType.None, None },
				{ GenericItemSubType.Flag, Flag },
				{ GenericItemSubType.BasketBall, BasketBall },
				{ GenericItemSubType.BookHolder, BookHolder },
				{ GenericItemSubType.Hoop, Hoop },
				{ GenericItemSubType.LavaLamp, LavaLamp },
				{ GenericItemSubType.PlantRing, PlantRing },
				{ GenericItemSubType.PlantZikaLeaf, PlantZikaLeaf },
				{ GenericItemSubType.PlantCanister, PlantCanister },
				{ GenericItemSubType.PosterBethyr, PosterBethyr },
				{ GenericItemSubType.PosterBurner, PosterBurner },
				{ GenericItemSubType.PosterEverest, PosterEverest },
				{ GenericItemSubType.PosterHellion, PosterHellion },
				{ GenericItemSubType.PosterTurret, PosterTurret },
				{ GenericItemSubType.PosterCrewQuarters, PosterCrewQuarters },
				{ GenericItemSubType.PosterSonsOfEarth, PosterSonsOfEarth },
				{ GenericItemSubType.TeslaBall, TeslaBall },
				{ GenericItemSubType.Picture, Picture },
				{ GenericItemSubType.AltCorp_Cup, AltCorp_Cup },
				{ GenericItemSubType.CoffeeMachine, CoffeeMachine },
				{ GenericItemSubType.BrokenArmature, BrokenArmature },
				{ GenericItemSubType.ShatteredPlating, ShatteredPlating },
				{ GenericItemSubType.FriedElectronics, FriedElectronics },
				{ GenericItemSubType.DamagedTransmiter, DamagedTransmiter },
				{ GenericItemSubType.RupturedInsulation, RupturedInsulation },
				{ GenericItemSubType.BurnedPDU, BurnedPDU },
				{ GenericItemSubType.DiamondCoreDrillBit, DiamondCore },
				{ CanvasManager.LoadingScreenType.None, None },
				{ CanvasManager.LoadingScreenType.Loading, Loading.ToUpper() },
				{ CanvasManager.LoadingScreenType.ConnectingToMain, ConnectingToMain.ToUpper() },
				{ CanvasManager.LoadingScreenType.ConnectingToGame, ConnectingToGame.ToUpper() },
				{ SpawnSetupType.Continue, Continue.ToUpper() },
				{ SpawnSetupType.FreeRoamArges, FreeRoamArges },
				{ SpawnSetupType.FreeRoamSteropes, FreeRoamSteropes },
				{ SpawnSetupType.MiningArges, MiningArges },
				{ SpawnSetupType.MiningSteropes, MiningSteropes },
				{ SpawnSetupType.SteropesNearRandomStation, SteropesNearRandomStation },
				{ SpawnSetupType.SteropesNearDoomedOutpost, SteropesNearDoomedOutpost },
				{ RepairPointDamageType.Breach, Breach },
				{ RepairPointDamageType.Fire, Fire },
				{ RepairPointDamageType.System, System },
				{ RepairPointDamageType.Gravity, GravityMalfunction },
				{ GeneratorType.Power, FusionReactor },
				{ GeneratorType.Air, AirGenerator },
				{ GeneratorType.AirScrubber, AirFilter },
				{ GeneratorType.Capacitor, Capacitor },
				{ GeneratorType.Solar, SolarPanel },
				{ SubSystemType.Light, Light },
				{ SubSystemType.EmergencyLight, EmergencyLight },
				{ SubSystemType.RCS, RCS },
				{ SubSystemType.FTL, FTL },
				{ SubSystemType.Engine, Engine },
				{ SubSystemType.Refinery, Refinery },
				{ SubSystemType.Fabricator, Fabricator },
				{ MachineryPartSlotScope.Output, Output },
				{ MachineryPartSlotScope.PowerOutput, PowerOutput },
				{ MachineryPartSlotScope.ResourcesConsumption, ResourcesConsumption },
				{ MachineryPartSlotScope.PowerConsumption, PowerConsumption },
				{ MachineryPartSlotScope.Capacity, Capacity },
				{ MachineryPartSlotScope.PowerCapacity, PowerCapacity },
				{ MachineryPartSlotScope.PowerUpTime, PowerUpTime },
				{ MachineryPartSlotScope.CoolDownTime, CoolDownTime },
				{ DistributionSystemType.Air, Air },
				{ DistributionSystemType.Helium3, Helium3 },
				{ DistributionSystemType.Hydrogen, Hydrogen },
				{ DistributionSystemType.Nitrogen, Nitrogen },
				{ DistributionSystemType.Oxygen, Oxygen },
				{ DistributionSystemType.Power, Power },
				{ DistributionSystemType.RCS, RCS },
				{ CargoCompartmentType.AirGeneratorNitrogen, AirGenerator + " " + Nitrogen },
				{ CargoCompartmentType.AirGeneratorOxygen, AirGenerator + " " + Oxygen },
				{ CargoCompartmentType.AirTank, AirTank },
				{ CargoCompartmentType.Canister, Canister },
				{ CargoCompartmentType.CargoBayResources, Cargo },
				{ CargoCompartmentType.CraftingResources, Crafting },
				{ CargoCompartmentType.Engine, Engine },
				{ CargoCompartmentType.JetpackOxygen, Jetpack + " " + Oxygen },
				{ CargoCompartmentType.JetpackPropellant, Jetpack + " " + Propellant },
				{ CargoCompartmentType.PowerGenerator, Power + " " + Generator },
				{ CargoCompartmentType.RawResources, Raw },
				{ CargoCompartmentType.RCS, RCS },
				{ CargoCompartmentType.RefinedCanister, Refined + " " + Canister },
				{ SceneTriggerType.CargoPanel, CargoPanel },
				{ SceneTriggerType.CryoPodPanel, CryoPanel },
				{ SceneTriggerType.LifeSupportPanel, LifeSupportPanel },
				{ SceneTriggerType.PowerSupplyPanel, PowerSupplyPanel },
				{ SceneTriggerType.SecurityScreen, SecurityTerminal },
				{ SceneTriggerType.DockingPanel, DockingPanel },
				{ SceneTriggerType.AirlockPanel, AirlockPanel },
				{ SceneTriggerType.NavigationPanel, NavigationPanel },
				{ SceneTriggerType.DockingPortController, DockingPortController },
				{ SceneTriggerType.MessageTerminal, MessageTerminal },
				{ StandardInteractionTip.Ammo, Ammo },
				{ StandardInteractionTip.Grenades, Grenades },
				{ StandardInteractionTip.Suits, Suits },
				{ StandardInteractionTip.Handguns, Handguns },
				{ StandardInteractionTip.Jetpacks, Jetpack },
				{ StandardInteractionTip.Helmets, Helmets },
				{ StandardInteractionTip.Rifles, Rifles },
				{ StandardInteractionTip.Gravity, ToggleGravity },
				{ StandardInteractionTip.ExitCryo, ExitCryo },
				{ StandardInteractionTip.EnterCryo, EnterCryo },
				{ StandardInteractionTip.Recycler, RecyclerSlot },
				{ StandardInteractionTip.ResearchTable, ResearchSlot },
				{ StandardInteractionTip.Piloting, Piloting },
				{ StandardInteractionTip.Turret, PortableTurret },
				{ StandardInteractionTip.Medpack, Medpack },
				{ StandardInteractionTip.Small, SmallItems },
				{ StandardInteractionTip.Medium, MediumItems },
				{ StandardInteractionTip.Large, LargeItems },
				{ StandardInteractionTip.FireExtinguisher, FireExtingusher },
				{ StandardInteractionTip.BasketballHoop, BasketballHoop },
				{ StandardInteractionTip.Poster, Poster },
				{ StandardInteractionTip.HackingDescription, HackingDescription },
				{ StandardInteractionTip.Undock, PullToUndock },
				{ StandardInteractionTip.Docking, DockingPanel },
				{ ManeuverCourse.FeasibilityErrorType.Acceleration_High, AccelerationHigh },
				{ ManeuverCourse.FeasibilityErrorType.Acceleration_Low, AccelerationLow },
				{ ManeuverCourse.FeasibilityErrorType.Course_Impossible, CourseImpossible },
				{ ManeuverCourse.FeasibilityErrorType.FTL_Capacity, FTLCapacity },
				{ ManeuverCourse.FeasibilityErrorType.FTL_CellFuel, FTLCellFuel },
				{ ManeuverCourse.FeasibilityErrorType.FTL_ManeuverIndex, FTLManeuverIndex },
				{ ManeuverCourse.FeasibilityErrorType.FTL_Online, FTLOffline },
				{ ManeuverCourse.FeasibilityErrorType.ToManyDockedVessels, ToManyDockedVessels },
				{ GameScenes.SceneID.AltCorp_Shuttle_SARA, AltCorp_Shuttle_SARA },
				{ GameScenes.SceneID.AltCorp_Shuttle_CECA, AltCorp_Shuttle_CECA },
				{ GameScenes.SceneID.AltCorp_CorridorModule, AltCorp_CorridorModule },
				{ GameScenes.SceneID.AltCorp_CorridorIntersectionModule, AltCorp_CorridorIntersectionModule },
				{ GameScenes.SceneID.AltCorp_Corridor45TurnModule, AltCorp_Corridor45TurnModule },
				{ GameScenes.SceneID.AltCorp_Corridor45TurnRightModule, AltCorp_Corridor45TurnRightModule },
				{ GameScenes.SceneID.AltCorp_CorridorVertical, AltCorp_CorridorVertical },
				{ GameScenes.SceneID.ALtCorp_PowerSupply_Module, ALtCorp_PowerSupply_Module },
				{ GameScenes.SceneID.AltCorp_LifeSupportModule, AltCorp_LifeSupportModule },
				{ GameScenes.SceneID.AltCorp_Cargo_Module, AltCorp_Cargo_Module },
				{ GameScenes.SceneID.AltCorp_Command_Module, AltCorp_Command_Module },
				{ GameScenes.SceneID.AltCorp_StartingModule, AltCorp_StartingModule },
				{ GameScenes.SceneID.AltCorp_AirLock, AltCorp_AirLock },
				{ GameScenes.SceneID.AltCorp_DockableContainer, AltCorp_DockableContainer },
				{ GameScenes.SceneID.AltCorp_CrewQuarters_Module, AltCorp_CrewQuarters_Module },
				{ GameScenes.SceneID.AltCorp_SolarPowerModule, AltCorp_SolarPowerModule },
				{ GameScenes.SceneID.AltCorp_FabricatorModule, AltCorp_FabricatorModule },
				{ WeaponMod.FireMode.Auto, AutoFireMode },
				{ WeaponMod.FireMode.Single, SingleFireMode },
				{ InventorySlot.Group.Ammo, Ammo },
				{ InventorySlot.Group.Consumable, Consumable },
				{ InventorySlot.Group.Hands, Hands },
				{ InventorySlot.Group.Helmet, Helmet },
				{ InventorySlot.Group.Jetpack, Jetpack },
				{ InventorySlot.Group.Outfit, Outfit },
				{ InventorySlot.Group.Primary, Primary },
				{ InventorySlot.Group.Secondary, Secondary },
				{ InventorySlot.Group.Tool, Tool },
				{ InventorySlot.Group.Utility, Utility }
			};

			CanvasManagerLocalization = new Dictionary<string, string>
			{
				{ "SinglePlayerText", SinglePlayer.ToUpper() },
				{ "MultiplayerText", Multiplayer.ToUpper() },
				{ "NameText", Name.ToUpper() },
				{ "JoinDiscordText", JoinDiscord.ToUpper() },
				{ "ActionsText", Actions.ToUpper() },
				{ "MovementText", Movement.ToUpper() },
				{ "ShipText", Ship.ToUpper() },
				{ "SuitText", Suit.ToUpper() },
				{ "CommunicationsText", Communications.ToUpper() },
				{ "QuickActionsText", QuickActions.ToUpper() },
				{ "AdvancedVideoSettingsText", AdvancedVideoSettings.ToUpper() },
				{ "ApplyText", Apply.ToUpper() },
				{ "AltKeyText", AltKey },
				{ "AmbientOcclusionText", AmbientOcclusion.ToUpper() },
				{ "AntiAliasingText", AntiAliasing.ToUpper() },
				{ "AudioText", Audio.ToUpper() },
				{ "AutoStabilizationText", AutoStabilization.ToUpper() },
				{ "BackText", Back.ToUpper() },
				{ "BasicVideoSettingsText", BasicVideoSettings.ToUpper() },
				{ "BasicAudioSettingsText", BasicAudioSettings.ToUpper() },
				{ "BloomText", Bloom.ToUpper() },
				{ "CancelText", Cancel.ToUpper() },
				{ "CharacterText", Character.ToUpper() },
				{ "ChooseStartingPointText", ChooseStartingPoint.ToUpper() },
				{ "ChromaticAberrationText", ChromaticAberration.ToUpper() },
				{ "CommunityText", Community.ToUpper() },
				{ "ConfirmText", Confirm.ToUpper() },
				{ "ControlsText", Controls.ToUpper() },
				{ "CreateCharacterText", CreateCharacter.ToUpper() },
				{ "CurrentServerText", CurrentServer.ToUpper() },
				{ "DefaultText", Default.ToUpper() },
				{ "DisclaimerText", Disclaimer },
				{ "EADisclaimerText", EADisclaimer },
				{ "EnterCustomBoxNameText", EnterCustomBoxName.ToUpper() },
				{ "EnterPasswordText", EnterPassword.ToUpper() },
				{ "ExitText", Exit.ToUpper() },
				{ "EyeAdaptationText", EyeAdaptation.ToUpper() },
				{ "F1ForHelpText", F1ForHelp.ToUpper() },
				{ "FavoritesText", Favorites.ToUpper() },
				{ "FreshStartText", FreshStart },
				{ "FullscreenText", FullScreen.ToUpper() },
				{ "FullText", Full },
				{ "GameSettingsText", GameSettings.ToUpper() },
				{ "GeneralSettingsText", GeneralSettings.ToUpper() },
				{ "GlossaryText", Glossary.ToUpper() },
				{ "HeadBobStrengthText", HeadBobStrength.ToUpper() },
				{ "HideTipsText", HideTips.ToUpper() },
				{ "HideTutorialText", HideTutorial.ToUpper() },
				{ "InteractText", Interact },
				{ "KeyText", Key },
				{ "LoadingText", Loading.ToUpper() },
				{ "LogoutText", Logout.ToUpper() },
				{ "MainMenuText", MainMenu.ToUpper() },
				{ "MasterVolumeText", MasterVolume.ToUpper() },
				{ "MotionBlurText", MotionBlur.ToUpper() },
				{ "OfficialText", Official.ToUpper() },
				{ "OptionsText", Options.ToUpper() },
				{ "PingText", Ping.ToUpper() },
				{ "PlayText", Play.ToUpper() },
				{ "PlaySPText", PlaySP.ToUpper() },
				{ "PlayersText", Players.ToUpper() },
				{ "PreAplhaText", PreAplha },
				{ "QualityText", Quality.ToUpper() },
				{ "QuitText", Quit.ToUpper() },
				{ "RefreshText", Refresh.ToUpper() },
				{ "ResolutionText", Resolution.ToUpper() },
				{ "RespawnText", Respawn.ToUpper() },
				{ "ResumeText", Resume.ToUpper() },
				{ "SaveText", Save.ToUpper() },
				{ "SearchText", Search.ToUpper() },
				{ "ServerBrowserText", ServerBrowser.ToUpper() },
				{ "ServerText", Server.ToUpper() },
				{ "SettingsText", Settings.ToUpper() },
				{ "ShadowsText", Shadows.ToUpper() },
				{ "ShipSettingsText", ShipSettings.ToUpper() },
				{ "ShowCrosshairText", ShowCrosshair.ToUpper() },
				{ "TextureQualityText", TextureQuality.ToUpper() },
				{ "ThrowingText", Throwing.ToUpper() },
				{ "MouseSettingsText", MouseSettings.ToUpper() },
				{ "SensitivityText", Sensitivity.ToUpper() },
				{ "InvertMouseWhileDrivingText", InvertMouseWhileDriving.ToUpper() },
				{ "InvertMouseText", InvertMouse.ToUpper() },
				{ "UnderstandText", Understand.ToUpper() },
				{ "UseText", Use },
				{ "UsernameText", Username },
				{ "VSyncText", VSync },
				{ "VideoText", Video.ToUpper() },
				{ "VoiceVolumeText", VoiceVolume.ToUpper() },
				{ "WelcomeText", Welcome.ToUpper() },
				{ "PressAnyKeyText", PressAnyKeyToContinue.ToUpper() },
				{ "KeyboardSettingsText", KeyboardSettings.ToUpper() },
				{ "LanguageSettingsText", LanguageSettings.ToUpper() },
				{ "ChooseLanguageText", ChooseLanguage.ToUpper() },
				{ "ReportServerText", ReportServer.ToUpper() },
				{ "OtherText", Other.ToUpper() },
				{ "SendReportText", SendReport.ToUpper() },
				{ "DeleteCharacterText", DeleteCharacter.ToUpper() },
				{ "ReportText", ReportServer.ToUpper() },
				{ "ConnectText", Connect.ToUpper() },
				{ "ReadMoreText", ReadMore.ToUpper() },
				{ "LatestNewsText", LatestNews.ToUpper() },
				{ "ServerSettingsText", ServerSettings.ToUpper() },
				{ "PlayerSettingsText", PlayerSettings.ToUpper() },
				{ "GlobalSettingsText", GlobalSettings.ToUpper() },
				{ "DisconectedText", Disconnected.ToUpper() },
				{ "ConnectionErrorText", ConnectionError.ToUpper() },
				{ "SystemErrorText", SystemError.ToUpper() },
				{ "UnavailableFromInGameMenuText", UnavailableFromInGameMenu.ToUpper() },
				{ "ConnectingToInviteText", ConnectingToInvite.ToUpper() },
				{ "RecyclingOutputText", RecyclingOutput.ToUpper() },
				{ "VolumetricLightingText", VolumetricLighting.ToUpper() },
				{ "JoinHellionOnDiscordText", JoinHellionOnDiscord.ToUpper() },
				{ "ServerRestartInText", ServerRestartIn.ToUpper() },
				{ "RentYourOwnServerText", RentYourOwnServer.ToUpper() },
				{ "SaveGameText", SaveGame.ToUpper() },
				{ "LoadGameText", LoadGame.ToUpper() },
				{ "ConsoleText", Console.ToUpper() },
				{ "ItemsText", Items.ToUpper() },
				{ "ModulesText", Modules.ToUpper() },
				{ "CommandListText", CommandList.ToUpper() },
				{ "LoadText", Load.ToUpper() },
				{ "DataPrivacySettingsText", DataPrivacySettings.ToUpper() },
				{ "EditText", Edit.ToUpper() },
				{ "ServerInfoText", ServerInfo.ToUpper() },
				{ "AmbienceVolumeText", AmbienceVolume.ToUpper() },
				{ "ExternalBrowserPageText", ExternalBrowserPage.ToUpper() },
				{ "RollText", Roll.ToUpper() },
				{ "HideTipsFromMenuText", HideTipsFromMenu.ToUpper() },
				{ "CreateCharacterLoreText", CreateCharacterLore },
				{ "NetworkingText", Networking.ToUpper() },
				{ "SentText", Sent.ToUpper() },
				{ "ReceivedText", Received.ToUpper() },
				{ "ResetText", Reset.ToUpper() },
				{ "RCSFuelText", RCS.ToUpper() + " " + Fuel.ToUpper() },
				{ "OxygenText", Oxygen.ToUpper() },
				{ "NoJetpackText", Jetpack.ToUpper() + " " + Missing.ToUpper() },
				{ "WarningText", Warning.ToUpper() },
				{ "PressureText", Pressure.ToUpper() },
				{ "SuitPowerText", SuitPower.ToUpper() },
				{ "StabilizationText", Stabilization.ToUpper() },
				{ "LateralText", HelmetOffSpeed.ToUpper() },
				{ "DirectionalText", HelmetOnSpeed.ToUpper() },
				{ "JetpackOfflineText", JetpackOffline.ToUpper() },
				{ "OxygenLowText", OxygenLow.ToUpper() },
				{ "SelectionText", Selection.ToUpper() },
				{ "ZeroGravityMovementText", ZeroGravityMovement.ToUpper() },
				{ "UpText", Up.ToUpper() },
				{ "DownText", Down.ToUpper() },
				{ "GrabStabilizeText", Grab.ToUpper() + " / " + Stabilization.ToUpper() },
				{ "BatteryMissingText", BatteryMissing.ToUpper() },
				{ "LootAllText", LootAll.ToUpper() },
				{ "InventoryText", Inventory.ToUpper() },
				{ "JournalText", Journal.ToUpper() },
				{ "BlueprintsText", Blueprints.ToUpper() },
				{ "WeaponsText", Weapons.ToUpper() },
				{ "MagazinesText", Magazines.ToUpper() },
				{ "ToolsText", Tools.ToUpper() },
				{ "UtilityText", Utility.ToUpper() },
				{ "SuitsText", Suits.ToUpper() },
				{ "MediacalText", Medical.ToUpper() },
				{ "PartsText", Parts.ToUpper() },
				{ "ContainersText", Containers.ToUpper() },
				{ "ToggleTrackingText", ToggleTracking.ToUpper() },
				{ "NoSuitEquippedText", NoSuitEquipped.ToUpper() },
				{ "DropText", Drop.ToUpper() },
				{ "RemoveOutfitText", RemoveOutfit.ToUpper() },
				{ "ObjectivesText", Objectives.ToUpper() },
				{ "QuestLogText", QuestLog.ToUpper() },
				{ "NoLogAvailableText", NoLogAvailable.ToUpper() },
				{ "ShowContainerSlotsText", ShowContainerSlots.ToUpper() },
				{ "QuestTerminalHintText", QuestTerminalHint.ToUpper() },
				{ "ToBeContinuedText", ToBeContinued.ToUpper() },
				{ "EquipmentText", Equipment.ToUpper() },
				{ "ResourcesText", ResourcesLabel.ToUpper() }
			};

			PanelsLocalization = new Dictionary<string, string>
			{
				{ "ConfirmText", Confirm.ToUpper() },
				{ "CancelText", Cancel.ToUpper() },
				{ "TurnOffText", TurnOff.ToUpper() },
				{ "TurnOnText", TurnOn.ToUpper() },
				{ "OutputText", Output.ToUpper() },
				{ "BackText", Back.ToUpper() },
				{ "ZeroGravityMovementText", ZeroGravityMovement.ToUpper() },
				{ "RotationText", Rotation.ToUpper() },
				{ "UpText", Up.ToUpper() },
				{ "DownText", Down.ToUpper() },
				{ "GrabStabilizeText", Grab.ToUpper() + " / " + Stabilization.ToUpper() },
				{ "InfoScreenText", InfoScreen.ToUpper() },
				{ "ExitPanelText", ExitPanel.ToUpper() },
				{ "PowerSupplyPanelText", PowerSupplyScreen.ToUpper() },
				{ "TotalOutputText", TotalOutput.ToUpper() },
				{ "PowerCapacityText", PowerCapacity.ToUpper() },
				{ "TotalConsumptionText", TotalConsumption.ToUpper() },
				{ "NoPowerSupplyConnectedText", NoPowerSupplyConnected.ToUpper() },
				{ "UnauthorizedAccessText", UnauthorizedAccess.ToUpper() },
				{ "PowerOutputText", PowerOutput.ToUpper() },
				{ "DeuteriumTankText", DeuteriumTank.ToUpper() },
				{ "SystemPartsText", SystemParts.ToUpper() },
				{ "SolarPanelsText", SolarPanels.ToUpper() },
				{ "CapacitorText", Capacitor.ToUpper() },
				{ "CapacityText", Capacity.ToUpper() },
				{ "ConsumptionText", Consumption.ToUpper() },
				{ "CapacitorsTotalText", CapacitorsTotal.ToUpper() },
				{ "ModuleOutputText", Module.ToUpper() + " " + Output.ToUpper() },
				{ "BaseConsumptionText", BaseConsumption.ToUpper() },
				{ "NoSunExposureText", NoSunExposure.ToUpper() },
				{ "PowerSupplySystemText", PowerSupplySystem.ToUpper() },
				{ "PowerSupplyInfoText", PowerSupplyInfo },
				{ "CapacitorDescriptionText", CapacitorDescription },
				{ "SolarPanelDescriptionText", SolarPanelDescription },
				{ "FusionReactorDescriptionText", FusionReactorDescription },
				{ "CurrentVesselConsumtionText", CurrentVesselConsumtion },
				{ "ConnectedVesselDescriptionText", ConnectedVesselDescription },
				{ "ToggleBaseConsumptionText", ToggleBaseConsumption },
				{ "LifeSupportPanelText", LifeSupportPanelLabel.ToUpper() },
				{ "LifeSupportSystemText", LifeSupportSystem.ToUpper() },
				{ "TotalCapacityText", TotalCapacity.ToUpper() },
				{ "AirTankText", AirTank.ToUpper() },
				{ "NoLifeSupportConnectedText", NoLifeSupportConnected.ToUpper() },
				{ "NoAirGeneratorsText", NoAirGenerator.ToUpper() },
				{ "NoAirFiltersText", NoAirFilter.ToUpper() },
				{ "AirGeneratorText", AirGenerator.ToUpper() },
				{ "OxygenTankText", OxygenTank.ToUpper() },
				{ "NitrogenTankText", NitrogenTank.ToUpper() },
				{ "AirFilterText", AirFilter.ToUpper() },
				{ "PressureText", Pressure.ToUpper() },
				{ "AirQualityText", AirQuality.ToUpper() },
				{ "AirFilteringText", AirFiltering.ToUpper() },
				{ "PressureRegulationText", PressureRegulation.ToUpper() },
				{ "AirlockText", Airlock.ToUpper() },
				{ "AirOutputText", AirOutput.ToUpper() },
				{ "AirCapacityText", AirCapacity.ToUpper() },
				{ "ConnectedLifeSupportSystemsText", ConnectedLifeSupportSystems.ToUpper() },
				{ "ConnectedPowerSupplySystemsText", ConnectedPowerSupplySystems.ToUpper() },
				{ "ConnectedVesselsText", ConnectedVessels.ToUpper() },
				{ "SunExposureText", SunExposure.ToUpper() },
				{ "ModuleVolumeText", ModuleVolume.ToUpper() },
				{ "FilteringRateText", FilteringRate.ToUpper() },
				{ "LifeSupportInfoText", LifeSupportInfo },
				{ "AirGeneratorDescriptionText", AirGeneratorDescription },
				{ "AirFilterDescriptionText", AirFilterDescription },
				{ "AirTankDescriptionText", AirTankDescription },
				{ "AirTankNotConnectedText", AirTankNotConnected.ToUpper() },
				{ "ConnectedCargosText", ConnectedCargos.ToUpper() },
				{ "VesselSystemsText", VesselSystems.ToUpper() },
				{ "FabricatorText", Fabricator.ToUpper() },
				{ "RefineryText", Refinery.ToUpper() },
				{ "AttachPointText", AttachPoint.ToUpper() },
				{ "CargoPanelText", CargoHeading.ToUpper() },
				{ "RawText", Raw.ToUpper() },
				{ "RefinedText", Refined.ToUpper() },
				{ "CraftingText", Crafting.ToUpper() },
				{ "CargoText", Cargo.ToUpper() },
				{ "RefiningText", Refining.ToUpper() },
				{ "SlotText", Slot.ToUpper() },
				{ "ActiveSystemsText", ActiveSystems.ToUpper() },
				{ "NoSlotConnectionText", NothingConnectedToSlot.ToUpper() },
				{ "OxygenText", Oxygen.ToUpper() },
				{ "PropellantText", Propellant.ToUpper() },
				{ "NoRafineryText", NoRafineryAvailable.ToUpper() },
				{ "EnergyConsumptionText", EnergyConsumption.ToUpper() },
				{ "ProcessingTimeText", ProcessingTime.ToUpper() },
				{ "RefineText", Refine.ToUpper() },
				{ "NoOtherCargoAttachedText", NoOtherCargoAvailable.ToUpper() },
				{ "VentText", Vent.ToUpper() },
				{ "VentDescriptionText", VentDescription.ToUpper() },
				{ "UnloadText", Unload.ToUpper() },
				{ "NoItemAttachedToCargoText", NoItemAttachedToCargo.ToUpper() },
				{ "NoRefineryAvailableText", NoRefineryAvailable.ToUpper() },
				{ "AmountToTransferText", AmountToTransfer.ToUpper() },
				{ "PowerConsumptionText", PowerConsumption.ToUpper() },
				{ "RefiningTimeText", RefiningTime.ToUpper() },
				{ "TransferResourcesText", TransferResources.ToUpper() },
				{ "ChooseAnItemToCraftText", ChooseAnItemToCraft.ToUpper() },
				{ "NoFabricatorAvailableText", NoFabricatorAvailable.ToUpper() },
				{ "TransferFromText", TransferFrom.ToUpper() },
				{ "TransferToText", TransferTo.ToUpper() },
				{ "DragResourcesForCraftingText", DragResourcesForCrafting.ToUpper() },
				{ "RefillText", Refill.ToUpper() },
				{ "CraftText", Craft.ToUpper() },
				{ "CraftingTimeText", CraftingTime.ToUpper() },
				{ "CancelCraftingText", CancelCrafting.ToUpper() },
				{ "CancelingCraftingText", CancelCraftingDescription.ToUpper() },
				{ "CancelingCraftingWarningText", CancelCraftingWarning.ToUpper() },
				{ "RegisterText", Register.ToUpper() },
				{ "SetAsPointText", SetAsPoint.ToUpper() },
				{ "InvitePlayerText", InviteFriend.ToUpper() },
				{ "UnregisterText", Unregister.ToUpper() },
				{ "CryoChamberText", CryoChamber.ToUpper() },
				{ "SelectFriendText", SelectFriend.ToUpper() },
				{ "ActionRequiredText", ActionRequired.ToUpper() },
				{ "AreYouSureCryoText", AreYouSureCryo.ToUpper() },
				{ "DangerCryoText", DangerCryo.ToUpper() },
				{ "SecurityTermninalText", SecurityTerminal.ToUpper() },
				{ "ClaimText", Claim.ToUpper() },
				{ "AddCrewMemberText", AddCrewMember.ToUpper() },
				{ "ResignText", Resign.ToUpper() },
				{ "AuthPersonnelListText", AuthorizedPersonnelList.ToUpper() },
				{ "CommandingOfficerText", CommandingOfficer.ToUpper() },
				{ "CrewText", Crew.ToUpper() },
				{ "ChangeShipNameText", ChangeShipName.ToUpper() },
				{ "CustomShipNameText", EnterCustomShipName.ToUpper() },
				{ "ShipCrewText", ShipCrew.ToUpper() },
				{ "AreYouSureResignText", AreYouSureResign.ToUpper() },
				{ "PromoteText", Promote.ToUpper() },
				{ "RemoveText", Remove.ToUpper() },
				{ "AreYouSurePromoteText", AreYouSurePromote.ToUpper() },
				{ "AreYouSureSelfDestructText", AreYouSureSelfDestruct.ToUpper() },
				{ "SelfDestructActiveText", SelfDestruct.ToUpper() + " " + Active.ToUpper() },
				{ "ChangeShipEmblemText", ChangeShipEmblem.ToUpper() },
				{ "AirlockControlText", AirLockcontrols.ToUpper() },
				{ "PressurizeText", RePressurize.ToUpper() },
				{ "DepressurizeText", Depressurize.ToUpper() },
				{ "InnerDoorText", InnerDoor.ToUpper() },
				{ "OuterDoorText", OuterDoor.ToUpper() },
				{ "WarningAirlockText", WarningArilock },
				{ "AreYouSureAirlockText", AreYouSureAirlock.ToUpper() },
				{ "DangerAirlockText", DangerAirlock.ToUpper() },
				{ "StopText", Stop.ToUpper() },
				{ "AirlockPressureText", AirlockPressure.ToUpper() },
				{ "DoorControlText", DoorControl.ToUpper() },
				{ "BarText", Bar.ToUpper() },
				{ "NoAirTankAvailableText", NoAirTankAvailable.ToUpper() },
				{ "VolumeDescriptionText", VolumeDescription.ToUpper() },
				{ "PressurizeDescriptionText", PressurizeDescription.ToUpper() },
				{ "DepressurizeDescriptionText", DepressurizeDescription.ToUpper() },
				{ "VentActionDescriptionText", VentActionDescription.ToUpper() },
				{ "HoldToStabilizeText", HoldToStabilize.ToUpper() },
				{ "ObjectsInClusterText", ObjectsInGroup.ToUpper() },
				{ "ObjectClusterText", MultipleObjects.ToUpper() },
				{ "AddCustomOrbitText", AddCustomOrbit.ToUpper() },
				{ "RemoveCustomOrbitText", RemoveOrbit.ToUpper() },
				{ "WarpToObjectText", WarpTo.ToUpper() },
				{ "ManeuverInitiatedText", ManeuverInitiated.ToUpper() },
				{ "PleaseAlignText", AlignShip.ToUpper() },
				{ "ZoomOutText", ZoomOut.ToUpper() },
				{ "HomeStationText", HomeStation.ToUpper() },
				{ "MyShipText", MyShip.ToUpper() },
				{ "ScanText", Scan.ToUpper() },
				{ "SignalAmplificationText", SignalAmplification.ToUpper() },
				{ "AutorizedVesselsText", AuthorizedVessels.ToUpper() },
				{ "DistressSignalsText", DistressSignal.ToUpper() },
				{ "FTLManeuverText", FtlManeuver.ToUpper() },
				{ "CellsSelectedText", CellsSelected.ToUpper() },
				{ "WarpDistanceText", WarpDistance.ToUpper() },
				{ "ManeuverStatusText", ManeuverStatus.ToUpper() },
				{ "ManeuverTimeAdjustmentText", ManeuverTimeAdjustment.ToUpper() },
				{ "ActivationTimeText", ActivationTime.ToUpper() },
				{ "ArrivalTimeText", ArrivalTime.ToUpper() },
				{ "InitializeText", InitializeNavigation.ToUpper() },
				{ "ClusterText", MultipleObjects.ToUpper() },
				{ "RcsCancelManeuverText", RcsCancelManeuver },
				{ "UnstableOrbitText", UnstableOrbit.ToUpper() },
				{ "ArgumentOfPeriapsisText", ArgumentOfPeriapsis.ToUpper() },
				{ "LongitudeOfAscendingNodeText", LongitudeOfAscendingNode.ToUpper() },
				{ "InclinationText", Inclination.ToUpper() },
				{ "PeriapsisText", Periapsis.ToUpper() },
				{ "ApoapsisText", Apoapsis.ToUpper() },
				{ "PositionOnOrbitText", PositionOnOrbit.ToUpper() },
				{ "OrbitalPeriodText", OrbitalPeriod.ToUpper() },
				{ "StageText", Stage.ToUpper() },
				{ "WarpSettingsText", WarpSettings.ToUpper() },
				{ "PleaseSelectManeuverText", SelectManeuver.ToUpper() },
				{ "RadiationText", Radiation.ToUpper() },
				{ "CellConsumptionText", CellConsumption.ToUpper() },
				{ "SignatureText", Signature.ToUpper() },
				{ "ModuleText", Module.ToUpper() },
				{ "AvilableDockingPortsText", AvailbaleDockingPorts.ToUpper() },
				{ "SelectedText", Selected.ToUpper() },
				{ "RCSFuelLevelText", RCSFuelLevel.ToUpper() },
				{ "ModulesInRangeText", ModulesInRange.ToUpper() },
				{ "AvailableModulesText", AvailableModules.ToUpper() },
				{ "TargetedModuleText", TargetedModule.ToUpper() },
				{ "AvailablePortsText", AvailablePorts.ToUpper() },
				{ "DistanceText", Distance.ToUpper() },
				{ "DirectionalSpeedText", DirectionalSpeed.ToUpper() },
				{ "ResourceInjectorText", ResourceInjector.ToUpper() + " :" },
				{ "ResourceInjectorMissingText", ResourceInjector.ToUpper() + " " + Missing.ToUpper() },
				{ "CheckRcsUtilityAccessText", CheckRcsUtilityAccess.ToUpper() },
				{ "ChangeDockingPortText", ChangeDockingPort.ToUpper() },
				{ "ChangeTargetText", ChangeTarget.ToUpper() },
				{ "ChangeTargetPortText", ChangeTargetPort.ToUpper() },
				{ "NoTargetModulesInRangeText", NoTargetModulesInRange.ToUpper() },
				{ "EngineStatusText", EngineStatus.ToUpper() },
				{ "FuelText", Fuel.ToUpper() },
				{ "ENGText", ENG.ToUpper() },
				{ "RCSText", RCS.ToUpper() },
				{ "FTLText", FTL.ToUpper() },
				{ "HealthText", Health.ToUpper() },
				{ "ContactsText", Contacts.ToUpper() },
				{ "RadarRangeText", RadarRange.ToUpper() },
				{ "MatchedText", Matched.ToUpper() },
				{ "AvailableText", Available.ToUpper() },
				{ "EtaText", ETA.ToUpper() },
				{ "SystemsText", Systems.ToUpper() },
				{ "EngineText", Engine.ToUpper() },
				{ "FuelLevelsText", FuelLevels.ToUpper() },
				{ "MatchVelocityText", MatchVelocity.ToUpper() },
				{ "ToggleEngineText", ToggleEngine.ToUpper() },
				{ "MatchTargetsVelocityText", MatchTargetsVelocity.ToUpper() },
				{ "CollisionImminentText", CollisionWarning.ToUpper() },
				{ "OffSpeedAssistantText", OffSpeedAssistant.ToUpper() },
				{ "ChangeRadarRangeText", ChangeRadarRange.ToUpper() },
				{ "OffTargetText", OffTarget.ToUpper() },
				{ "WarningText", Warning.ToUpper() },
				{ "DrivingTipsText", DrivingTips.ToUpper() },
				{ "RollText", Roll.ToUpper() },
				{ "StabilizeText", Stabilize.ToUpper() },
				{ "HideTipsFromMenuText", HideTipsFromMenu.ToUpper() }
			};

			EnvironmentPanelLocalization = new Dictionary<string, string>
			{
				{ "EnvironmentalMonitorText", EnvironmentalMonitor.ToUpper() },
				{ "GravityText", Gravity.ToUpper() },
				{ "PressureText", Pressure.ToUpper() },
				{ "BarText", Bar.ToUpper() },
				{ "AirQualityText", AirQuality.ToUpper() },
				{ "TemperatureText", Temperature.ToUpper() },
				{ "UnbreathableDangerText", UnbreathableAtmosphere.ToUpper() },
				{ "VesselStatusText", VesselStatus.ToUpper() },
				{ "GravityFailText", GravityFail.ToUpper() },
				{ "FireText", FireHazard.ToUpper() },
				{ "BreachText", Breach.ToUpper() },
				{ "DistressCallActiveText", DistressCallActive.ToUpper() },
				{ "InDebrisFieldText", InDebrisField.ToUpper() },
				{ "SelfDestructActiveText", SelfDestruct.ToUpper() + " " + Active.ToUpper() },
				{ "SystemFailiureText", SystemFailiure.ToUpper() },
				{ "WarningText", Warning.ToUpper() }
			};
		}

		public static void RevertToDefault()
		{
			try
			{
				ImportFromString(defaultValues);
			}
			catch (Exception ex)
			{
				Dbg.Error("Localization revert failed", ex.Message);
			}
		}

		public static void ImportFromFile(string fileName)
		{
			try
			{
				ImportFromString(File.ReadAllText(fileName));
			}
			catch (Exception ex)
			{
				Dbg.Error("Localization import failed", ex.Message);
			}
		}

		public static void ImportFromString(string jsonObject)
		{
			try
			{
				Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonObject);
				ImportFromDictionary(dict);
			}
			catch (Exception ex)
			{
				Dbg.Error("Localization import failed", ex.Message);
			}
		}

		private static void ImportFromDictionary(Dictionary<string, string> dict)
		{
			try
			{
				FieldInfo[] fields = typeof(Localization).GetFields(BindingFlags.Static | BindingFlags.Public);
				string value;
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.FieldType == typeof(string) && dict.TryGetValue(fieldInfo.Name, out value))
					{
						fieldInfo.SetValue(null, value);
					}
				}
				ScriptableObject[] array = Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)) as ScriptableObject[];
				foreach (ScriptableObject scriptableObject in array)
				{
					FieldInfo[] fields2 = scriptableObject.GetType().GetFields();
					foreach (FieldInfo fieldInfo2 in fields2)
					{
						if (fieldInfo2.FieldType == typeof(string) && dict.TryGetValue("SO_" + scriptableObject.GetType().Name + "." + scriptableObject.name + "." + fieldInfo2.Name, out value))
						{
							fieldInfo2.SetValue(scriptableObject, value);
						}
					}
				}
				Initialize();
				ControlsRebinder.Initialize();
			}
			catch (Exception ex)
			{
				Dbg.Error("Localization import failed", ex.Message);
			}
		}

		public static void SaveToFile(string fileName)
		{
			try
			{
				File.WriteAllText(fileName, GetJsonString());
			}
			catch (Exception ex)
			{
				Dbg.Error("Localization save failed", ex.Message);
			}
		}

		private static string GetJsonString()
		{
			try
			{
				// Gets all static or public fields in this class and puts them in a dictionary of keys.
				Dictionary<string, string> localisationKeys = new Dictionary<string, string>();
				FieldInfo[] fields = typeof(Localization).GetFields(BindingFlags.Static | BindingFlags.Public);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.FieldType == typeof(string))
					{
						localisationKeys[fieldInfo.Name] = (string)fieldInfo.GetValue(null);
					}
				}

				// Gets all fields on scriptable objects in the resources folder and adds them as keys.
				ScriptableObject[] scriptableObjects = Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)) as ScriptableObject[];
				Debug.Assert(scriptableObjects != null, nameof(scriptableObjects) + " != null");
				foreach (ScriptableObject scriptableObject in scriptableObjects)
				{
					FieldInfo[] fields2 = scriptableObject.GetType().GetFields();
					foreach (FieldInfo fieldInfo2 in fields2)
					{
						if (fieldInfo2.CustomAttributes.FirstOrDefault((CustomAttributeData m) => m.AttributeType == typeof(LocalizeField)) != null && fieldInfo2.FieldType == typeof(string) && (string)fieldInfo2.GetValue(scriptableObject) != string.Empty)
						{
							localisationKeys["SO_" + scriptableObject.GetType().Name + "." + scriptableObject.name + "." + fieldInfo2.Name] = (string)fieldInfo2.GetValue(scriptableObject);
						}
					}
				}
				return JsonConvert.SerializeObject(localisationKeys, Formatting.Indented);
			}
			catch (Exception ex)
			{
				Dbg.Error("Localization to string failed", ex.Message);
			}
			return null;
		}

		public static string GetLocalizedField(string fieldName, bool useDefault = false)
		{
			try
			{
				return typeof(Localization).GetField(fieldName).GetValue(null).ToString();
			}
			catch
			{
				return (!useDefault) ? null : fieldName;
			}
		}
	}
}
