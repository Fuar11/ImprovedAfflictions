using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using Il2CppTLD.Stats;
using Random = UnityEngine.Random;
using System.Text.Json;
using Il2CppSystem.Data;
using ImprovedAfflictions.Utils;
using static Il2Cpp.Panel_Affliction;
using ImprovedAfflictions.Pain;
using Il2CppSystem.Xml.Schema;
using ImprovedAfflictions.FoodPoisoning;
using ImprovedAfflictions.Component;
using Il2CppSystem.Security;
using Unity.VisualScripting;
using Il2CppTLD.IntBackedUnit;

namespace ImprovedAfflictions
{
    internal class UIPatches
    {

        [HarmonyPatch(typeof(Panel_FirstAid), nameof(Panel_FirstAid.Initialize))]

        public class AddBloodDrugLevelLabel : MonoBehaviour
        {

            public static void Postfix(Panel_FirstAid __instance)
            {
                //make calories section smaller
                GameObject statusBars = __instance.gameObject.transform.GetChild(2).gameObject;
                GameObject caloriesSection = statusBars.transform.GetChild(12).gameObject;
                GameObject caloriesBg = caloriesSection.transform.GetChild(3).gameObject;

                Vector3 scale = caloriesBg.transform.localScale;
                scale.x -= 0.2f;
                caloriesBg.transform.localScale = scale;

                GameObject bloodDrugLevelSection = Instantiate(caloriesSection, caloriesSection.transform.parent);
                bloodDrugLevelSection.name = "Blood Drug Level";
                
                /**UISprite pillIcon = bloodDrugLevelSection.transform.GetChild(0).GetComponent<UISprite>();
                pillIcon.mSpriteName = "ico_units_pill";
                pillIcon.gameObject.active = true; **/

                Vector3 pos = caloriesSection.transform.position;
                pos.x -= 0.33f;
                bloodDrugLevelSection.transform.position = pos;

                /**
                GameObject conditionBarSection = __instance.gameObject.transform.GetChild(3).gameObject;
                GameObject conditionBarSpawner = conditionBarSection.transform.GetChild(1).gameObject;

                GenericStatusBarSpawner gsbs = conditionBarSpawner.AddComponent<GenericStatusBarSpawner>();
                gsbs.m_EmptySpriteName = "ico_units_pill";
                gsbs.m_MainSpriteName = "ico_units_pill";
                gsbs.m_StatusBarType = StatusBar.StatusBarType.Condition;
                gsbs.m_Prefab = conditionBarSpawner.transform.GetChild(0).gameObject;

                gsbs.enabled = true;
                gsbs.Awake(); **/
            }

        }

        [HarmonyPatch(typeof(Panel_FirstAid), nameof(Panel_FirstAid.RefreshStatusLabels))]

        public class UpdateBloodDrugLevelOnUI
        {

            public static void Postfix(Panel_FirstAid __instance)
            {
                //PainManager ac = GameObject.Find("SCRIPT_ConditionSystems").GetComponent<PainManager>();
                GameObject statusBars = __instance.gameObject.transform.GetChild(2).gameObject;
                GameObject bloodDrugLevel = statusBars.transform.GetChild(13).gameObject;


                if (bloodDrugLevel)
                {
                    bloodDrugLevel.transform.GetChild(1).GetComponent<UILabel>().text = "BLOOD DRUG LEVEL";
                   // bloodDrugLevel.transform.GetChild(2).GetComponent<UILabel>().text = ac.GetPainkillerLevelPercent();
                }
            }

        }
    }
}
