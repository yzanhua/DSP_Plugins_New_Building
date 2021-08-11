using System.Collections.Generic;
using System.Linq;
using BepInEx;
using xiaoye97;
using UnityEngine;


namespace HiddensModPlus
{
    [BepInDependency("com.loshen.plugin.DSPMOD1", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("me.yzanh.DSP.HiddensModPlus", "HiddensModPlus", "1.0")]
    public class HiddensModPlus : BaseUnityPlugin
    {
        private static int nextUnusedID = 200;

        // helper structs
        private static int[] itemIDs = { 8003, 8004, 8001, 8002, 8025, 8026, 8027, 8011, 8009 };
        private static int[] templateModelIDs = { 66, 66, 194, 194, 64, 63, 69, 69, 64 };
        private static Color[] colors =
        {
            new Color(194 / 255f, 3 / 255f, 252 / 255f), // purple
            Color.red, Color.green,
            new Color(194 / 255f, 3 / 255f, 252 / 255f), // purple
            Color.green, Color.green,Color.green,
            new Color(242/ 255f, 79/ 255f, 24/ 255f), // orange
            new Color(242/ 255f, 79/ 255f, 24/ 255f), // orange
        };

        private static List<ModelProto> models = new List<ModelProto>();

        void Start()
        {
            LDBTool.PostAddDataAction += addModelProto;
            LDBTool.EditDataAction += ChangeColor;
        }

        void addModelProto()
        {
            for (int i = 0; i < 8; i++)
                models.Add(addModelHelper(HiddensModPlus.templateModelIDs[i], HiddensModPlus.colors[i]));
        }
        ModelProto addModelHelper(int templateModelId, Color newColor)
        {
            int newModelID = GetNextID();
            var oriModel = LDB.models.Select(templateModelId);
            var newModel = oriModel.Copy();

            newModel.Preload();
            newModel.ID = newModelID;
            newModel.prefabDesc.modelIndex = newModelID;

            List<Material> temp = new List<Material>() { Instantiate(newModel.prefabDesc.materials[0]) };

            temp[0].color = newColor;

            Material[] mats = temp.ToArray();
            for (int i = 0; i < newModel.prefabDesc.lodMaterials.Count(); i++)
            {
                if (newModel.prefabDesc.lodMaterials[i] is null)
                    continue;
                newModel.prefabDesc.lodMaterials[i] = mats;
            }

            if (newModel.prefabDesc.materials.Count() == 1)
                newModel.prefabDesc.materials = mats;
            else
            {
                List<Material> temp2 = new List<Material>() { };
                for (int i = 0; i < newModel.prefabDesc.materials.Count(); i++)
                {
                    temp2.Add(Instantiate(newModel.prefabDesc.materials[i]));
                    temp2[i].color = newColor;
                }
                newModel.prefabDesc.materials = mats;
            }


            newModel.Name = newModel.ID.ToString();
            newModel.name = newModel.ID.ToString();

            LDBTool.PostAddProto(ProtoType.Model, newModel);
            LDB.models.modelArray[newModel.ID] = newModel;
            return newModel;
        }

        void ChangeColor(Proto proto)
        {
            if (proto is ItemProto)
            {
                var itemProto = proto as ItemProto;
                for (int i = 0; i < 8; i++)
                {
                    if (itemProto.ID != HiddensModPlus.itemIDs[i]) continue;
                    itemProto.prefabDesc.modelIndex = HiddensModPlus.models[i].ID;
                    itemProto.ModelIndex = HiddensModPlus.models[i].ID;
                }

            }
        }

        private int GetNextID()
        {
            nextUnusedID += 1;
            return nextUnusedID - 1;
        }
    }
}
