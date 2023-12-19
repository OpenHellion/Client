using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OpenHellion.UI;
using UnityEditor;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity
{
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

		public static string Play;

		public static string Options;

		public static string Quit;

		public static string Apply;

		public static string Disclaimer;

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

		public static string Confirm;

		public static string Controls;

		public static string CreateCharacter;

		public static string Default;

		public static string EADisclaimer;

		public static string EnterCustomBoxName;

		public static string Exit;

		public static string EyeAdaptation;

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

		public static string MasterVolume;

		public static string MotionBlur;

		public static string Quality;

		public static string Resolution;

		public static string Respawn;

		public static string Resume;

		public static string Server;

		public static string Settings;

		public static string Shadows;

		public static string ShipSettings;

		public static string ShowCrosshair;

		public static string TextureQuality;

		public static string Throwing;

		public static string Use;

		public static string Username;

		public static string Video;

		public static string VoiceVolume;

		public static string ChooseLanguage;

		public static string ReportServer;

		public static string PlayerSettings;

		public static string GlobalSettings;

		public static string JoinDiscord;

		public static string Movement;

		public static string Actions;

		public static string Ship;

		public static string Suit;

		public static string Communications;

		public static string QuickActions;

		public static string Male;

		public static string Female;

		public static string ChooseStartingPoint;

		public static string Continue;

		public static string LatencyProblems;

		public static string Rubberbanding;

		public static string ServerStuck;

		public static string DisconnectedFromServer;

		public static string Other;

		public static string SendReport;

		public static string FreshStartConfrimTitle;

		public static string FreshStartConfrimText;

		public static string Forward;

		public static string Backward;

		public static string Left;

		public static string Right;

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

		public static string DropThrow;

		public static string InteractTakeInHands;

		public static string EquipItem;

		public static string Reload;

		public static string ChangeStance;

		public static string Stabilization;

		public static string ToggleVisor;

		public static string ToggleLights;

		public static string ToggleJetpack;

		public static string MatchVelocityControl;

		public static string MatchVelocity;

		public static string TargetUp;

		public static string TargetDown;

		public static string FilterLeft;

		public static string FilterRight;

		public static string ChangeDockingPort;
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

		public static string AreYouSureLogout;

		public static string AreYouSureExitGame;

		public static string AreYouSureDeleteCharacter;

		public static string AreYouSureRespawn;

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

		public static string Melee;

		public static string Chat;

		public static string HoldToLoot;

		public static string PressToInteract;

		public static string HoldToEquip;

		public static string Talk;

		public static string Active;

		public static string Missing;

		public static string Ready;

		public static string Radio;

		public static string Scanning;

		public static string All;

		public static string SaveGameSettings;

		public static string UnnamedVessel;

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

		public static string WarpTo;

		public static string FtlManeuver;

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

		public static string ActiveSystems;

		public static string NothingConnectedToSlot;

		public static string NoOtherCargoAvailable;

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

		public static string ToManyDockedVessels;

		public static string ManeuverInitiated;

		public static string AlignShip;

		public static string PowerOutput;

		public static string SystemParts;

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

		public static string Jetpack;

		public static string PressToToggleTargeting;

		public static string HelmetRadar;

		public static string HelmetOnSpeed;

		public static string HelmetOffSpeed;

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

		public static string DamagedTransmitter;

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

		public static string TransferResources;

		public static string BaseConsumption;

		public static string Canister;

		public static string Generator;

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

		public static string HackingDescription;

		public static string PullToUndock;

		public static string VesselPowerOffline;

		public static string Warning;

		public static string InGameDescription;

		public static string OrbitingNear;

		public static string WarpingNear;

		public static string FloatingFreelyNear;

		public static string GravityInfluenceRadius;

		public static string Radius;

		public static string ObjectCluster;

		public static string ObjectsInCluster;

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

		public static string LootAll;

		public static string Journal;

		public static string Blueprints;

		public static string NoSuitEquipped;

		public static string NoLogAvailable;

		public static string ProximityLoot;

		public static string Drop;

		public static string Loot;

		public static string ShowContainerSlots;

		public static string FireMode;

		public static string AutoFireMode;

		public static string SingleFireMode;

		public static string PleaseAssignAllControls;

		public static string ServerInfo;

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

		public static string OutOfRange;

		public static string NotScannable;

		public static string ScanningRange;

		public static string Low;

		public static string Medium;

		public static string High;

		public static string Unknown;

		public static string Armor;

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

		public static string FreeRoamSteropes;

		public static string MiningSteropes;

		public static string SteropesNearRandomStation;

		public static string SteropesNearDoomedOutpost;

		public static string MiningArges;

		public static string FreeRoamArges;

		public static string HoldToStabilize;

		public static string AvailableQuests;

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

		public static string SessionExpired;

		public static string AccountNotFound;

		public static string AccountAlreadyExists;

		public static string InvalidEmail;

		public static string InvalidUsername;

		public static string ConsentToDataStorage;

		public static Dictionary<Enum, string> Enums;

		public static Dictionary<string, string> MainMenuLocalisation;

		public static Dictionary<string, string> InGameGUILocalisation;

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
				{ 0, "Localisation/en-US" },
				{ 1, "Localisation/sr-RS" },
				{ 2, "Localisation/ch-CN" },
				{ 3, "Localisation/fr-FR" },
				{ 4, "Localisation/it-IT" },
				{ 5, "Localisation/pt-pt" },
				{ 6, "Localisation/ru-RU" },
				{ 7, "Localisation/es-ES" },
				{ 8, "Localisation/tr-TR" },
				{ 9, "Localisation/cs-CZ" },
				{ 10, "Localisation/da-DK" },
				{ 11, "Localisation/nl-NL" },
				{ 12, "Localisation/fi-FI" },
				{ 13, "Localisation/de-DE" },
				{ 14, "Localisation/el-GR" },
				{ 15, "Localisation/hu-HU" },
				{ 16, "Localisation/ja-JP" },
				{ 17, "Localisation/nb-NO" },
				{ 18, "Localisation/pl-PL" },
				{ 19, "Localisation/pt-BR" },
				{ 20, "Localisation/ro-RO" },
				{ 21, "Localisation/sk-SK" },
				{ 22, "Localisation/sl-SL" },
				{ 23, "Localisation/sv-SE" },
				{ 24, "Localisation/uk-UA" }
			};
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
				Preload01, Preload02, Preload03, Preload04, Preload05, Preload06, Preload07, Preload08, Preload09,
				Preload10,
				Preload11, Preload12, Preload13, Preload14, Preload15, Preload16, Preload17, Preload18, Preload19,
				Preload20,
				Preload21, Preload22, Preload23, Preload24, Preload25, Preload26, Preload27, Preload28, Preload29,
				Preload30,
				Preload31, Preload32, Preload33, Preload34, Preload35, Preload36, Preload37, Preload38, Preload39,
				Preload40,
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
				{ GenericItemSubType.DamagedTransmitter, ScrapDescription },
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

			SystemChat = new[]
			{
				SystemNothing, SystemUnstable, SystemShipCalled, SystemShipInRange, SystemShipArrive,
				SystemServerRestart
			};

			Enums = new Dictionary<Enum, string>
			{
				{ OpenHellion.Settings.SettingsType.All, All },
				{ OpenHellion.Settings.SettingsType.Audio, Audio },
				{ OpenHellion.Settings.SettingsType.Controls, Controls },
				{ OpenHellion.Settings.SettingsType.Video, Video },
				{ OpenHellion.Settings.SettingsType.Game, Game },
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
				{ GenericItemSubType.DamagedTransmitter, DamagedTransmitter },
				{ GenericItemSubType.RupturedInsulation, RupturedInsulation },
				{ GenericItemSubType.BurnedPDU, BurnedPDU },
				{ GenericItemSubType.DiamondCoreDrillBit, DiamondCore },
				{ GlobalGUI.LoadingScreenType.None, None },
				{ GlobalGUI.LoadingScreenType.Loading, Loading.ToUpper() },
				{ GlobalGUI.LoadingScreenType.ConnectingToMain, ConnectingToMain.ToUpper() },
				{ GlobalGUI.LoadingScreenType.ConnectingToGame, ConnectingToGame.ToUpper() },
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
				{ ManeuverCourse.FeasibilityErrorType.AccelerationHigh, AccelerationHigh },
				{ ManeuverCourse.FeasibilityErrorType.AccelerationLow, AccelerationLow },
				{ ManeuverCourse.FeasibilityErrorType.CourseImpossible, CourseImpossible },
				{ ManeuverCourse.FeasibilityErrorType.FtlCapacity, FTLCapacity },
				{ ManeuverCourse.FeasibilityErrorType.FtlCellFuel, FTLCellFuel },
				{ ManeuverCourse.FeasibilityErrorType.FtlManeuverIndex, FTLManeuverIndex },
				{ ManeuverCourse.FeasibilityErrorType.FtlOnline, FTLOffline },
				{ ManeuverCourse.FeasibilityErrorType.ToManyDockedVessels, ToManyDockedVessels },
				{ GameScenes.SceneId.AltCorp_Shuttle_SARA, AltCorp_Shuttle_SARA },
				{ GameScenes.SceneId.AltCorp_Shuttle_CECA, AltCorp_Shuttle_CECA },
				{ GameScenes.SceneId.AltCorp_CorridorModule, AltCorp_CorridorModule },
				{ GameScenes.SceneId.AltCorp_CorridorIntersectionModule, AltCorp_CorridorIntersectionModule },
				{ GameScenes.SceneId.AltCorp_Corridor45TurnModule, AltCorp_Corridor45TurnModule },
				{ GameScenes.SceneId.AltCorp_Corridor45TurnRightModule, AltCorp_Corridor45TurnRightModule },
				{ GameScenes.SceneId.AltCorp_CorridorVertical, AltCorp_CorridorVertical },
				{ GameScenes.SceneId.ALtCorp_PowerSupply_Module, ALtCorp_PowerSupply_Module },
				{ GameScenes.SceneId.AltCorp_LifeSupportModule, AltCorp_LifeSupportModule },
				{ GameScenes.SceneId.AltCorp_Cargo_Module, AltCorp_Cargo_Module },
				{ GameScenes.SceneId.AltCorp_Command_Module, AltCorp_Command_Module },
				{ GameScenes.SceneId.AltCorp_StartingModule, AltCorp_StartingModule },
				{ GameScenes.SceneId.AltCorp_AirLock, AltCorp_AirLock },
				{ GameScenes.SceneId.AltCorp_DockableContainer, AltCorp_DockableContainer },
				{ GameScenes.SceneId.AltCorp_CrewQuarters_Module, AltCorp_CrewQuarters_Module },
				{ GameScenes.SceneId.AltCorp_SolarPowerModule, AltCorp_SolarPowerModule },
				{ GameScenes.SceneId.AltCorp_FabricatorModule, AltCorp_FabricatorModule },
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

			MainMenuLocalisation = new Dictionary<string, string>
			{
				{ "NameText", Name.ToUpper() },
				{ "JoinDiscordText", JoinDiscord.ToUpper() },
				{ "BackText", Back.ToUpper() },
				{ "CancelText", Cancel.ToUpper() },
				{ "CharacterText", Character.ToUpper() },
				{ "ChooseStartingPointText", ChooseStartingPoint.ToUpper() },
				{ "ConfirmText", Confirm.ToUpper() },
				{ "CreateCharacterText", CreateCharacter.ToUpper() },
				{ "DisclaimerText", Disclaimer },
				{ "KeyText", Key },
				{ "PlayText", Play.ToUpper() },
				{ "QuitText", Quit.ToUpper() },
				{ "SettingsText", Settings.ToUpper() },
				{ "PressAnyKeyText", PressAnyKeyToContinue.ToUpper() },
				{ "ReadMoreText", ReadMore.ToUpper() },
				{ "LatestNewsText", LatestNews.ToUpper() },
				{ "DisconnectedText", Disconnected.ToUpper() },
				{ "ConnectionErrorText", ConnectionError.ToUpper() },
				{ "ConnectingToInviteText", ConnectingToInvite.ToUpper() },
			};

			InGameGUILocalisation = new Dictionary<string, string>
			{
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
				{ "BasicVideoSettingsText", BasicVideoSettings.ToUpper() },
				{ "BasicAudioSettingsText", BasicAudioSettings.ToUpper() },
				{ "BloomText", Bloom.ToUpper() },
				{ "ChromaticAberrationText", ChromaticAberration.ToUpper() },
				{ "ConfirmText", Confirm.ToUpper() },
				{ "ControlsText", Controls.ToUpper() },
				{ "DefaultText", Default.ToUpper() },
				{ "EADisclaimerText", EADisclaimer },
				{ "EnterCustomBoxNameText", EnterCustomBoxName.ToUpper() },
				{ "ExitText", Exit.ToUpper() },
				{ "EyeAdaptationText", EyeAdaptation.ToUpper() },
				{ "FullscreenText", FullScreen.ToUpper() },
				{ "GameSettingsText", GameSettings.ToUpper() },
				{ "GeneralSettingsText", GeneralSettings.ToUpper() },
				{ "GlossaryText", Glossary.ToUpper() },
				{ "HeadBobStrengthText", HeadBobStrength.ToUpper() },
				{ "HideTipsText", HideTips.ToUpper() },
				{ "HideTutorialText", HideTutorial.ToUpper() },
				{ "InteractText", Interact },
				{ "LoadingText", Loading.ToUpper() },
				{ "MasterVolumeText", MasterVolume.ToUpper() },
				{ "MotionBlurText", MotionBlur.ToUpper() },
				{ "OptionsText", Options.ToUpper() },
				{ "QualityText", Quality.ToUpper() },
				{ "ResolutionText", Resolution.ToUpper() },
				{ "RespawnText", Respawn.ToUpper() },
				{ "ResumeText", Resume.ToUpper() },
				{ "ServerText", Server.ToUpper() },
				{ "ShadowsText", Shadows.ToUpper() },
				{ "ShipSettingsText", ShipSettings.ToUpper() },
				{ "ShowCrosshairText", ShowCrosshair.ToUpper() },
				{ "TextureQualityText", TextureQuality.ToUpper() },
				{ "ThrowingText", Throwing.ToUpper() },
				{ "MouseSettingsText", MouseSettings.ToUpper() },
				{ "SensitivityText", Sensitivity.ToUpper() },
				{ "InvertMouseWhileDrivingText", InvertMouseWhileDriving.ToUpper() },
				{ "InvertMouseText", InvertMouse.ToUpper() },
				{ "UseText", Use },
				{ "UsernameText", Username },
				{ "VideoText", Video.ToUpper() },
				{ "VoiceVolumeText", VoiceVolume.ToUpper() },
				{ "PressAnyKeyText", PressAnyKeyToContinue.ToUpper() },
				{ "KeyboardSettingsText", KeyboardSettings.ToUpper() },
				{ "ChooseLanguageText", ChooseLanguage.ToUpper() },
				{ "ReportServerText", ReportServer.ToUpper() },
				{ "OtherText", Other.ToUpper() },
				{ "SendReportText", SendReport.ToUpper() },
				{ "ReportText", ReportServer.ToUpper() },
				{ "PlayerSettingsText", PlayerSettings.ToUpper() },
				{ "GlobalSettingsText", GlobalSettings.ToUpper() },
				{ "UnavailableFromInGameMenuText", UnavailableFromInGameMenu.ToUpper() },
				{ "RecyclingOutputText", RecyclingOutput.ToUpper() },
				{ "VolumetricLightingText", VolumetricLighting.ToUpper() },
				{ "ServerRestartInText", ServerRestartIn.ToUpper() },
				{ "ConsoleText", Console.ToUpper() },
				{ "ItemsText", Items.ToUpper() },
				{ "ModulesText", Modules.ToUpper() },
				{ "CommandListText", CommandList.ToUpper() },
				{ "ServerInfoText", ServerInfo.ToUpper() },
				{ "AmbienceVolumeText", AmbienceVolume.ToUpper() },
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
				{ "LateralText", HelmetOffSpeed.ToUpper() },
				{ "DirectionalText", HelmetOnSpeed.ToUpper() },
				{ "ZeroGravityMovementText", ZeroGravityMovement.ToUpper() },
				{ "UpText", Up.ToUpper() },
				{ "DownText", Down.ToUpper() },
				{ "GrabStabilizeText", Grab.ToUpper() + " / " + Stabilization.ToUpper() },
				{ "InventoryText", Inventory.ToUpper() },
				{ "JournalText", Journal.ToUpper() },
				{ "BlueprintsText", Blueprints.ToUpper() },
				{ "WeaponsText", Weapons.ToUpper() },
				{ "MagazinesText", Magazines.ToUpper() },
				{ "ToolsText", Tools.ToUpper() },
				{ "UtilityText", Utility.ToUpper() },
				{ "SuitsText", Suits.ToUpper() },
				{ "PartsText", Parts.ToUpper() },
				{ "ContainersText", Containers.ToUpper() },
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
				{ "ResourcesText", ResourcesLabel.ToUpper() },
				{ "MedicalText", Medical.ToUpper() }
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
				{ "CurrentVesselConsumptionText", CurrentVesselConsumtion },
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
				{ "ObjectsInClusterText", ObjectsInCluster.ToUpper() },
				{ "ObjectClusterText", ObjectCluster.ToUpper() },
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
				{ "ClusterText", ObjectCluster.ToUpper() },
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

		public static void ImportFromFile(string fileName)
		{
			try
			{
				ImportFromString(File.ReadAllText(fileName));
			}
			catch (Exception ex)
			{
				Debug.LogError("Localization import failed " + ex.Message);
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
				Debug.LogError("Localization import failed " + ex.Message);
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

				ScriptableObject[] array =
					Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)) as ScriptableObject[];
				foreach (ScriptableObject scriptableObject in array)
				{
					FieldInfo[] fields2 = scriptableObject.GetType().GetFields();
					foreach (FieldInfo fieldInfo2 in fields2)
					{
						if (fieldInfo2.FieldType == typeof(string) && dict.TryGetValue(
							    "SO_" + scriptableObject.GetType().Name + "." + scriptableObject.name + "." +
							    fieldInfo2.Name, out value))
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
				Debug.LogError("Localization import failed " + ex.Message);
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
				Debug.LogError("Localization import failed " + ex.Message);
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
				ScriptableObject[] scriptableObjects =
					Resources.FindObjectsOfTypeAll(typeof(ScriptableObject)) as ScriptableObject[];
				Debug.Assert(scriptableObjects != null, nameof(scriptableObjects) + " != null");
				foreach (ScriptableObject scriptableObject in scriptableObjects)
				{
					FieldInfo[] fields2 = scriptableObject.GetType().GetFields();
					foreach (FieldInfo fieldInfo2 in fields2)
					{
						if (fieldInfo2.CustomAttributes.FirstOrDefault((CustomAttributeData m) =>
							    m.AttributeType == typeof(LocalizeField)) != null &&
						    fieldInfo2.FieldType == typeof(string) &&
						    (string)fieldInfo2.GetValue(scriptableObject) != string.Empty)
						{
							localisationKeys[
								"SO_" + scriptableObject.GetType().Name + "." + scriptableObject.name + "." +
								fieldInfo2.Name] = (string)fieldInfo2.GetValue(scriptableObject);
						}
					}
				}

				return JsonConvert.SerializeObject(localisationKeys, Formatting.Indented);
			}
			catch (Exception ex)
			{
				Debug.LogError("Localization import failed " + ex.Message);
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
				return !useDefault ? null : fieldName;
			}
		}
	}
}
