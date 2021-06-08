using System.Collections.Generic;
using System.Linq;
using BepInEx;
using xiaoye97;
using UnityEngine;
using HarmonyLib;

namespace PlanetMiner
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("me.yzanh.DSP.PlanetMiner", "PlanetMinerMod", "1.0")]
    public class PlanetMiner : BaseUnityPlugin
    {
        public Sprite icon;
        public static bool isRun = false;
        private const int uesEnergy = 20000000;
        private const int waterSpeed = 100;
        private static long frame = 0;
        private static uint seed = 100000;

        void Awake()
        {
            LDBTool.PreAddDataAction += addPlanetMinerLanguage;
            LDBTool.PostAddDataAction += MakePlanetMiner;
            Harmony.CreateAndPatchAll(typeof(PlanetMiner), (string)null);
        }

        void MakePlanetMiner()
        {
            int newModelID = 195;
            var ori_station = LDB.items.Select(2104);
            var planetMiner = ori_station.Copy();
            var planetMinerRecipe = ori_station.maincraft.Copy();
            
            // create new model with new color
            var oriModel = LDB.models.Select(planetMiner.ModelIndex);
            var newModel = oriModel.Copy();
            newModel.Preload();  // important
            newModel.ID = newModelID;
            newModel.prefabDesc.modelIndex = newModelID;
            
            List<Material> temp = new List<Material>() { Instantiate(newModel.prefabDesc.materials[0]) };
            temp[0].color = new Color(153/255f, 52 / 255f, 104 / 255f);
            Material[] mats = temp.ToArray();
            
            newModel.prefabDesc.lodMaterials[0] = mats;
            newModel.Name = newModel.ID.ToString();
            newModel.name = newModel.ID.ToString();
            LDBTool.PostAddProto(ProtoType.Model, newModel);
            LDB.models.modelArray[newModel.ID] = newModel;

            // recipe
            planetMinerRecipe.ID = 310;
            planetMinerRecipe.Name = "星球矿机";
            planetMinerRecipe.name = "星球矿机".Translate();
            planetMinerRecipe.GridIndex = 2509;
            planetMinerRecipe.Items = new int[] { 2104, 2301, 6006 };
            planetMinerRecipe.ItemCounts = new int[] { 1, 10, 40 };
            planetMinerRecipe.Results = new int[] { 7005 };
            planetMinerRecipe.ResultCounts = new int[] { 1 };
            planetMinerRecipe.preTech = LDB.techs.Select(1507);


            // planetMiner Object
            planetMiner.ID = 7005;
            planetMiner.Name = "星球矿机";
            planetMiner.ModelIndex = newModel.ID;
            planetMiner.Preload(planetMiner.ID);
            planetMiner.Description = "星球矿机描述";
            planetMiner.description = "星球矿机描述".Translate();
            planetMiner.BuildIndex = 605;
            planetMiner.GridIndex = 2211;
            planetMiner.handcraft = planetMinerRecipe;
            planetMiner.handcrafts = new List<RecipeProto>() { planetMinerRecipe };
            planetMiner.recipes = new List<RecipeProto>() { planetMinerRecipe };
            planetMiner.makes = new List<RecipeProto>() { };
            planetMiner.prefabDesc.stationMaxItemCount = 20000;
            planetMiner.prefabDesc.stationMaxItemKinds = 1;
            planetMiner.prefabDesc.modelIndex = newModel.ID;

            // icon
            Traverse.Create(planetMiner).Field("_iconSprite").SetValue(ori_station.iconSprite);
            Traverse.Create(planetMinerRecipe).Field("_iconSprite").SetValue(ori_station.iconSprite);


            // load
            LDBTool.PostAddProto(ProtoType.Recipe, planetMinerRecipe);
            LDBTool.PostAddProto(ProtoType.Item, planetMiner);

            LDBTool.SetBuildBar(6, 5, planetMiner.ID);
        }

        // language (string)
        void addPlanetMinerLanguage()
        {
            StringProto nameString = new StringProto();
            nameString.ID = 9511;
            nameString.Name = "星球矿机";
            nameString.name = "星球矿机";
            nameString.ZHCN = "星球矿机";
            nameString.ENUS = "Planet Miner";
            nameString.FRFR = "Planet Miner";

            StringProto descString = new StringProto();
            descString.ID = 9512;
            descString.Name = "星球矿机描述";
            descString.name = "星球矿机描述";
            descString.ZHCN = "一个可以挖全球矿物的星际物流运输站.";
            descString.ENUS = "A Planet Miner.";
            descString.FRFR = "A Planet Miner.";

            LDBTool.PreAddProto(ProtoType.String, nameString);
            LDBTool.PreAddProto(ProtoType.String, descString);
        }



        private void Update() => ++PlanetMiner.frame;

        private void Init() => PlanetMiner.isRun = true;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), "GameTickLabResearchMode")]
        private static void Miner(FactorySystem __instance)
        {
            GameHistoryData history = GameMain.history;
            float miningSpeedScale = history.miningSpeedScale;
            if ((double)miningSpeedScale <= 0.0)
                return;
            int num1 = (int)(120.0 / (double)miningSpeedScale);
            if (num1 <= 0)
                num1 = 1;
            if ((ulong)PlanetMiner.frame % (ulong)num1 > 0UL)
                return;
            VeinData[] veinPool = __instance.factory.veinPool;
            Dictionary<int, List<int>> veins = new Dictionary<int, List<int>>();
            if (__instance.minerPool[0].seed == 0U)
            {
                System.Random random = new System.Random();
                __instance.minerPool[0].seed = (uint)(__instance.planet.id * 100000 + random.Next(1, 9999));
            }
            else
                PlanetMiner.seed = __instance.minerPool[0].seed;
            for (int index = 0; index < veinPool.Length; ++index)
            {
                VeinData veinData = veinPool[index];
                if (veinData.amount > 0 && veinData.productId > 0)
                    PlanetMiner.AddVeinData(veins, veinData.productId, index);
            }
            int[] numArray = (int[])null;
            float miningCostRate = history.miningCostRate;
            PlanetTransport transport = __instance.planet.factory.transport;
            FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[__instance.factory.index];
            bool flag = false;
            if (factoryProductionStat != null)
            {
                numArray = factoryProductionStat.productRegister;
                flag = true;
            }
            for (int index1 = 1; index1 < transport.stationCursor; ++index1)
            {
                StationComponent stationComponent = transport.stationPool[index1];
                if (stationComponent != null && stationComponent.storage != null)
                {
                    if (stationComponent.storage.Length > 1) continue;
                    StationStore stationStore1 = stationComponent.storage[0];

                    // add fuel
                    if (stationStore1.localLogic == ELogisticStorage.Demand && stationStore1.max > stationStore1.count)
                    {
                        
                        if (veins.ContainsKey(stationStore1.itemId))
                        {
                            if (stationComponent.energy >= uesEnergy)
                            {
                                int index3 = veins[stationStore1.itemId].First<int>();
                                if (veinPool[index3].type == EVeinType.Oil)
                                {
                                    float num2 = 0.0f;
                                    foreach (int index4 in veins[stationStore1.itemId])
                                    {
                                        if (veinPool.Length > index4 && veinPool[index4].productId > 0)
                                            num2 += (float)veinPool[index4].amount / 6000f;
                                    }
                                    stationComponent.storage[0].count += (int)num2;
                                    if (flag)
                                        numArray[stationStore1.itemId] += (int)num2;
                                    stationComponent.energy -= uesEnergy;
                                }
                                else
                                {
                                    int num2 = 0;
                                    foreach (int index4 in veins[stationStore1.itemId])
                                    {
                                        if (PlanetMiner.GetMine(veinPool, index4, miningCostRate, __instance.planet.factory))
                                            ++num2;
                                    }
                                    stationComponent.storage[0].count += num2;
                                    if (flag)
                                        numArray[stationStore1.itemId] += num2;
                                    stationComponent.energy -= uesEnergy;
                                }
                            }
                        }
                        else if (stationStore1.itemId == __instance.planet.waterItemId)
                        {
                            stationComponent.storage[0].count += waterSpeed;
                            if (flag)
                                numArray[stationStore1.itemId] += waterSpeed;
                            stationComponent.energy -= uesEnergy;
                        }
                    }

                }
            }
        }

        private static void AddVeinData(Dictionary<int, List<int>> veins, int item, int index)
        {
            if (!veins.ContainsKey(item))
                veins.Add(item, new List<int>());
            veins[item].Add(index);
        }

        public static bool GetMine(VeinData[] veinDatas, int index, float miningRate, PlanetFactory factory)
        {
            if (veinDatas.Length <= index || veinDatas[index].productId <= 0)
                return false;
            if (veinDatas[index].amount > 0)
            {
                bool flag = true;
                if ((double)miningRate < 0.999989986419678)
                {
                    PlanetMiner.seed = (uint)((ulong)(PlanetMiner.seed % 2147483646U + 1U) * 48271UL % (ulong)int.MaxValue) - 1U;
                    flag = (double)PlanetMiner.seed / 2147483646.0 < (double)miningRate;
                }
                if (flag)
                {
                    --veinDatas[index].amount;
                    --factory.planet.veinAmounts[(int)veinDatas[index].type];
                    if (veinDatas[index].amount <= 0)
                    {
                        --factory.planet.veinGroups[(int)veinDatas[index].groupIndex].count;
                        factory.RemoveVeinWithComponents(index);
                    }
                }
                return true;
            }
            --factory.planet.veinGroups[(int)veinDatas[index].groupIndex].count;
            factory.RemoveVeinWithComponents(index);
            return false;
        }
    }
}