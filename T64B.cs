using System;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using GHPC.Weapons;
using UnityEngine;
using GHPC.Camera;
using GHPC.Player;
using GHPC.Vehicle;
using GHPC.Equipment;
using GHPC.Utility;
using GHPC;
using NWH.VehiclePhysics;
using Reticle;
using GHPC.State;
using System.Collections;

namespace T_64B
{
    public class T64B : MelonMod
    {
        WeaponSystemCodexScriptable gun_2a46m1;

        AmmoClipCodexScriptable clip_codex_kobra;
        AmmoType.AmmoClip clip_kobra;
        AmmoCodexScriptable ammo_codex_kobra;
        AmmoType ammo_kobra;

        AmmoClipCodexScriptable clip_codex_3bm42;
        AmmoType.AmmoClip clip_3bm42;
        AmmoCodexScriptable ammo_codex_3bm42;
        AmmoType ammo_3bm42;

        AmmoClipCodexScriptable clip_codex_3bk29;
        AmmoType.AmmoClip clip_3bk29;
        AmmoCodexScriptable ammo_codex_3bk29;
        AmmoType ammo_3bk29;

        AmmoType ammo_9m111;
        AmmoType ammo_3bm32;
        AmmoType ammo_3of26;
        AmmoType ammo_3bk14m;

        GameObject[] vic_gos;
        GameObject gameManager;
        CameraManager cameraManager;
        PlayerInput playerManager;

        GameObject atgm_reticle;


        // https://snipplr.com/view/75285/clone-from-one-object-to-another-using-reflection
        public static void ShallowCopy(System.Object dest, System.Object src)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] destFields = dest.GetType().GetFields(flags);
            FieldInfo[] srcFields = src.GetType().GetFields(flags);

            foreach (FieldInfo srcField in srcFields)
            {
                FieldInfo destField = destFields.FirstOrDefault(field => field.Name == srcField.Name);

                if (destField != null && !destField.IsLiteral)
                {
                    if (srcField.FieldType == destField.FieldType)
                        destField.SetValue(dest, srcField.GetValue(src));
                }
            }
        }
        public static void EmptyRack(GHPC.Weapons.AmmoRack rack)
        {
            MethodInfo removeVis = typeof(GHPC.Weapons.AmmoRack).GetMethod("RemoveAmmoVisualFromSlot", BindingFlags.Instance | BindingFlags.NonPublic);

            PropertyInfo stored_clips = typeof(GHPC.Weapons.AmmoRack).GetProperty("StoredClips");
            stored_clips.SetValue(rack, new List<AmmoType.AmmoClip>());

            rack.SlotIndicesByAmmoType = new Dictionary<AmmoType, List<byte>>();

            foreach (Transform transform in rack.VisualSlots)
            {
                AmmoStoredVisual vis = transform.GetComponentInChildren<AmmoStoredVisual>();

                if (vis != null && vis.AmmoType != null)
                {
                    removeVis.Invoke(rack, new object[] { transform });
                }
            }
        }
        public override void OnUpdate()
        {
            if (!playerManager) return;
            if (playerManager.CurrentPlayerWeapon == null) return;
            if (playerManager.CurrentPlayerUnit == null) return;
            if (playerManager.CurrentPlayerWeapon.Name != "125mm gun 2A46M-1") return;

            Vehicle vic = (Vehicle)playerManager.CurrentPlayerUnit;
            var gps = vic.transform.gameObject.transform.Find("---T64A_MESH---/HULL/TURRET/Main gun/---MAIN GUN SCRIPTS---/2A46/TPD-2-49 gunner's sight/GPS").transform;
            if (gps == null || gps.gameObject.activeSelf == false) return;


            FireControlSystem FCS = playerManager.CurrentPlayerWeapon.FCS;
            ParticleSystem[] particleSystem = playerManager.CurrentPlayerWeapon.Weapon.MuzzleEffects;

            FieldInfo reticleCurrentRange = typeof(ReticleMesh).GetField("curReticleRange", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo reticleTargetRange = typeof(ReticleMesh).GetField("targetReticleRange", BindingFlags.NonPublic | BindingFlags.Instance);

            if (FCS.CurrentAmmoType.Name == "9M112 КОБРА BLATGM")
            {
                gps.GetChild(0).gameObject.SetActive(false);
                gps.GetChild(2).gameObject.SetActive(true);

                particleSystem[0].transform.GetChild(0).transform.gameObject.SetActive(false);
                particleSystem[0].transform.GetChild(1).transform.gameObject.SetActive(false);
                particleSystem[0].transform.GetChild(3).transform.gameObject.SetActive(false);
            }
            else
            {
                gps.GetChild(0).gameObject.SetActive(true);
                gps.GetChild(2).gameObject.SetActive(false);

                particleSystem[0].transform.GetChild(0).transform.gameObject.SetActive(true);
                particleSystem[0].transform.GetChild(1).transform.gameObject.SetActive(true);
                particleSystem[0].transform.GetChild(3).transform.gameObject.SetActive(true);
            }
            MelonLogger.Msg("Check 1");
        }

        public override async void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName == "LOADER_INITIAL" || sceneName == "MainMenu2_Scene") return;

            vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");

            while (vic_gos.Length == 0)
            {

                vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");
                await Task.Delay(5000);
            }

            MelonLogger.Msg("Sucessfully loaded vehicle into scene.");
            if (ammo_3bm42 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "9M111 Fagot")
                    {
                        ammo_9m111 = s.AmmoType;
                    }

                    if (s.AmmoType.Name == "3BM32 APFSDS-T")
                    {
                        ammo_3bm32 = s.AmmoType;
                    }
                    if (s.AmmoType.Name == "3BK14M HEAT-FS-T")
                    {
                        ammo_3bk14m = s.AmmoType;
                    }
                    if (s.AmmoType.Name == "3OF26 HEF-FS-T")
                    {
                        ammo_3of26 = s.AmmoType;
                    }
                }

                /*
                foreach (ArmorCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(ArmorCodexScriptable)))
                {
                    if (s.ArmorType.Name == "glass textolite")
                    {
                        armor_textolite = s.ArmorType;
                    }
                }
                */

                // 2a46m-1
                gun_2a46m1 = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
                gun_2a46m1.name = "gun_2a46m-1";
                gun_2a46m1.CaliberMm = 125;
                gun_2a46m1.FriendlyName = "125mm Gun 2A46M-1";
                gun_2a46m1.Type = WeaponSystemCodexScriptable.WeaponType.LargeCannon;

                // 3bm42 
                ammo_3bm42 = new AmmoType();
                ShallowCopy(ammo_3bm42, ammo_3bm32);
                ammo_3bm42.Name = "3BM42 МАНГО APFSDS-T";
                ammo_3bm42.Caliber = 125;
                ammo_3bm42.RhaPenetration = 680;
                ammo_3bm42.MuzzleVelocity = 1700f;
                ammo_3bm42.Mass = 4.85f;

                ammo_codex_3bm42 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bm42.AmmoType = ammo_3bm42;
                ammo_codex_3bm42.name = "ammo_3bm42";

                clip_3bm42 = new AmmoType.AmmoClip();
                clip_3bm42.Capacity = 1;
                clip_3bm42.Name = "3BM42 МАНГО APFSDS-T";
                clip_3bm42.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3bm42.MinimalPattern[0] = ammo_codex_3bm42;

                clip_codex_3bm42 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bm42.name = "clip_3bm42";
                clip_codex_3bm42.ClipType = clip_3bm42;

                // Kobra AT missile
                ammo_kobra = new AmmoType();
                ShallowCopy(ammo_kobra, ammo_9m111);
                ammo_kobra.Name = "9M112 КОБРА BLATGM";
                ammo_kobra.Caliber = 125;
                ammo_kobra.RhaPenetration = 840;
                ammo_kobra.MuzzleVelocity = 350;
                ammo_kobra.Mass = 23.2f;
                ammo_kobra.ArmingDistance = 70;
                ammo_kobra.SpallMultiplier = 1.5f;
                ammo_kobra.DetonateSpallCount = 20;
                ammo_kobra.SpiralPower = 2f;
                ammo_kobra.TntEquivalentKg = 4.6f;
                ammo_kobra.TurnSpeed = 0.08f;
                ammo_kobra.SpiralAngularRate = 2100f;
                ammo_kobra.RangedFuseTime = 12.5f;
                ammo_kobra.MaximumRange = 4000;
                ammo_kobra.MaxSpallRha = 60f;
                ammo_kobra.MinSpallRha = 30f;
                ammo_kobra.CertainRicochetAngle = 3f;
                ammo_kobra.ShotVisual = ammo_9m111.ShotVisual;
                ammo_kobra.Guidance = AmmoType.GuidanceType.Saclos;

                ammo_codex_kobra = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_kobra.AmmoType = ammo_kobra;
                ammo_codex_kobra.name = "ammo_kobra";

                clip_kobra = new AmmoType.AmmoClip();
                clip_kobra.Capacity = 1;
                clip_kobra.Name = "9M112 КОБРА BLATGM";
                clip_kobra.MinimalPattern = new AmmoCodexScriptable[1];
                clip_kobra.MinimalPattern[0] = ammo_codex_kobra;

                clip_codex_kobra = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_kobra.name = "clip_kobra";
                clip_codex_kobra.ClipType = clip_kobra;

                // 3bk29

                ammo_3bk29 = new AmmoType();
                ShallowCopy(ammo_3bk29, ammo_3bk14m);
                ammo_3bk29.Name = "3BK29 БРЕЙК HEATFS";
                ammo_3bk29.Caliber = 125;
                ammo_3bk29.RhaPenetration = 710;
                ammo_3bk29.MuzzleVelocity = 915;
                ammo_3bk29.Mass = 22.0f;
                ammo_3bk29.TntEquivalentKg = 3.4f;
                ammo_3bk29.CertainRicochetAngle = 0f;
                ammo_3bk29.MaxSpallRha = 15;
                ammo_3bk29.MinSpallRha = 3;
                ammo_3bk29.SpallMultiplier = 1.35f;

                ammo_codex_3bk29 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bk29.AmmoType = ammo_3bk29;
                ammo_codex_3bk29.name = "ammo_3bk29";

                clip_3bk29 = new AmmoType.AmmoClip();
                clip_3bk29.Capacity = 1;
                clip_3bk29.Name = "3BK29 БРЕЙК HEATFS";
                clip_3bk29.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3bk29.MinimalPattern[0] = ammo_codex_3bk29;

                clip_codex_3bk29 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bk29.name = "clip_3bk29";
                clip_codex_3bk29.ClipType = clip_3bk29;

                MelonLogger.Msg("Sucessfully configured ammunition.");

                /*
                armor_superTextolite = new ArmorType();
                ShallowCopy(armor_superTextolite, armor_textolite);
                armor_superTextolite.RhaeMultiplierCe = 1.2f;
                armor_superTextolite.RhaeMultiplierKe = 0.95f;
                armor_superTextolite.Name = "super textolite";

                armor_codex_superTextolite = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                armor_codex_superTextolite.name = "super textolite";
                armor_codex_superTextolite.ArmorType = armor_superTextolite;
                */
            }

            /*
            foreach (GameObject armour in GameObject.FindGameObjectsWithTag("Penetrable"))
            {
                if (armour == null) continue;

                VariableArmor texolitePlate = armour.GetComponent<VariableArmor>();

                if (texolitePlate == null) continue;
                if (texolitePlate.Unit == null) continue;
                if (texolitePlate.Unit.FriendlyName != "T-72M1") continue;
                if (texolitePlate.Name != "glass textolite layers") continue;

                FieldInfo armorPlate = typeof(VariableArmor).GetField("_armorType", BindingFlags.NonPublic | BindingFlags.Instance);
                armorPlate.SetValue(texolitePlate, armor_codex_superTextolite);

                MelonLogger.Msg("Sucessfully configured hull armour.");
            }
            */

            foreach (GameObject vic_go in vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName == "T-64A")
                {

                    if (atgm_reticle == null)
                    {
                        // ATGM reticle
                        foreach (Vehicle vic_go2 in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                        {
                            if (vic_go2 && vic_go2.FriendlyName == "9K111")
                            {
                                WeaponsManager weapons_manager = vic_go2.GetComponent<WeaponsManager>();
                                FireControlSystem FCS = weapons_manager.Weapons[0].FCS;
                                atgm_reticle = FCS.transform.GetChild(0).GetChild(0).gameObject;
                                break;
                            }
                        }
                    }

                    MelonLogger.Msg("Success loading leaning tower of brackets.");
                    GameObject ammo_3bm42_vis = null;
                    GameObject ammo_kobra_vis = null;
                    GameObject ammo_3bk29_vis = null;
                    // generate visual models 
                    if (ammo_3bm42_vis == null)
                    {
                        ammo_3bm42_vis = GameObject.Instantiate(ammo_3bm32.VisualModel);
                        ammo_3bm42_vis.name = "3BM42 visual";
                        ammo_3bm42.VisualModel = ammo_3bm42_vis;
                        ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm42;
                        ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm42;
                    }

                    if (ammo_kobra_vis == null)
                    {
                        ammo_kobra_vis = GameObject.Instantiate(ammo_3of26.VisualModel);
                        ammo_kobra_vis.name = "kobra visual";
                        ammo_kobra.VisualModel = ammo_kobra_vis;
                        ammo_kobra.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_kobra;
                        ammo_kobra.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_kobra;
                    }

                    if (ammo_3bk29_vis == null)
                    {
                        ammo_3bk29_vis = GameObject.Instantiate(ammo_3bk14m.VisualModel);
                        ammo_3bk29_vis.name = "3bk29 visual";
                        ammo_3bk29.VisualModel = ammo_3bk29_vis;
                        ammo_3bk29.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bk29;
                        ammo_3bk29.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bk29;
                    }


                    MelonLogger.Msg("Sucessfully loaded vis models.");

                    // rename to t64b
                    vic._friendlyName = "T-64B";


                    gameManager = GameObject.Find("_APP_GHPC_");
                    cameraManager = gameManager.GetComponent<CameraManager>();
                    playerManager = gameManager.GetComponent<PlayerInput>();


                    // convert weapon system and FCS, especially big thanks to Atlas here

                    LoadoutManager loadoutManager = vic.GetComponent<LoadoutManager>();
                    WeaponsManager weaponsManager = vic.GetComponent<WeaponsManager>();
                    WeaponSystemInfo mainGunInfo = weaponsManager.Weapons[0];
                    WeaponSystem mainGun = mainGunInfo.Weapon;

                    mainGun.Feed.ReloadDuringMissileTracking = true;
                    mainGunInfo.FCS.MainOptic.slot.VibrationBlurScale = 0.0f;
                    mainGunInfo.FCS.MainOptic.slot.VibrationShakeMultiplier = 0.1f;
                    mainGunInfo.FCS.MainOptic.slot.OtherFovs = new float[] { 3.6f };

                    MethodInfo reparent_awake = typeof(Reparent).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

                    GameObject reticle = GameObject.Instantiate(atgm_reticle);
                    reticle.AddComponent<Reparent>();
                    Reparent reticle_reparent = reticle.GetComponent<Reparent>();
                    reticle_reparent.NewParent = vic_go.transform.Find("---T64A_MESH---/HULL/TURRET/Main gun/---MAIN GUN SCRIPTS---/2A46/TPD-2-49 gunner's sight/GPS");
                    reparent_awake.Invoke(reticle_reparent, new object[] { });
                    reticle.transform.rotation = new Quaternion(0, 0, 0, 0);
                    reticle.transform.localPosition = new Vector3(0, 0, 0);
                    reticle.SetActive(false);

                    mainGunInfo.Name = "125mm gun 2A46M-1";
                    FieldInfo codex = typeof(WeaponSystem).GetField("CodexEntry", BindingFlags.NonPublic | BindingFlags.Instance);
                    codex.SetValue(mainGun, gun_2a46m1);

                    loadoutManager.LoadedAmmoTypes[0] = clip_codex_3bm42;
                    loadoutManager.LoadedAmmoTypes[2] = clip_codex_kobra;
                    loadoutManager.LoadedAmmoTypes[1] = clip_codex_3bk29;

                    MelonLogger.Msg("Success converting FCS & W.S.");

                    // Modify carousel loadout

                    FieldInfo nonFixedAmmoClipCountsByRack = typeof(GHPC.Weapons.LoadoutManager).GetField("_nonFixedAmmoClipCountsByRack", BindingFlags.NonPublic | BindingFlags.Instance);
                    List<int[]> AMMO = nonFixedAmmoClipCountsByRack.GetValue(loadoutManager) as List<int[]>;
                    AMMO[0] = new int[] { 16, 6, 6 };
                    MelonLogger.Msg(AMMO[0][0]);
                    nonFixedAmmoClipCountsByRack.SetValue(loadoutManager, new List<int[]>());

                    // [0] = ap [1] = heat [2] = he/atgm
                    // carousel is 28 rnds 
                    loadoutManager.RackLoadouts[0].AmmoCounts = new int[] { 16, 6, 6 };
                    for (int i = 0; i <= 2; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadoutManager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = clip_codex_3bm42.ClipType;
                        rack.ClipTypes[1] = clip_codex_3bk29.ClipType;
                        rack.ClipTypes[2] = clip_codex_kobra.ClipType;
                        EmptyRack(rack);
                    }
                    loadoutManager.SpawnCurrentLoadout();

                    PropertyInfo roundInBreech = typeof(AmmoFeed).GetProperty("AmmoTypeInBreech");
                    roundInBreech.SetValue(mainGun.Feed, null);

                    MethodInfo refreshBreech = typeof(AmmoFeed).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                    refreshBreech.Invoke(mainGun.Feed, new object[] { });

                    MelonLogger.Msg("Configured Ammo Count.");

                    // attach guidance computer
                    GameObject guidance_computer_obj = new GameObject("guidance computer");
                    guidance_computer_obj.transform.parent = vic.transform;
                    guidance_computer_obj.AddComponent<MissileGuidanceUnit>();

                    guidance_computer_obj.AddComponent<Reparent>();
                    Reparent reparent = guidance_computer_obj.GetComponent<Reparent>();
                    reparent.NewParent = vic_go.transform.Find("---T64A_MESH---/HULL/TURRET").gameObject.transform;
                    typeof(Reparent).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(reparent, new object[] { });

                    MissileGuidanceUnit computer = guidance_computer_obj.GetComponent<MissileGuidanceUnit>();
                    computer.AimElement = mainGunInfo.FCS.AimTransform;
                    mainGun.GuidanceUnit = computer;

                    MelonLogger.Msg("Success attaching Guid. Comp.");

                    //engine upgrade 
                    VehicleController vehicleController = vic_go.GetComponent<VehicleController>();
                    vehicleController.engine.maxPower = 730;

                    // update ballistics computer
                    MethodInfo registerAllBallistics = typeof(LoadoutManager).GetMethod("RegisterAllBallistics", BindingFlags.Instance | BindingFlags.NonPublic);
                    registerAllBallistics.Invoke(loadoutManager, new object[] { });

                    MelonLogger.Msg("Sucessfully loaded mod.");
                }
            }
        }

    }

}


