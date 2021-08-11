using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using xiaoye97;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace HelloMod
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("me.yzanh.DSP.HelloMod", "HelloMod", "1.0")]
    public class HelloMod : BaseUnityPlugin
    {
        void Start()
        {
            LDBTool.EditDataAction += Edit;
        }
        
        // Edit Ptroto. Name is arbitrary.
        void Edit(Proto proto)
        {
            if (proto is RecipeProto && proto.ID == 67)
            {
                var recipe = proto as RecipeProto;
                recipe.Items[1] = 1113;
            }
        }
    }
}
