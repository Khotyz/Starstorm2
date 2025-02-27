﻿using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class HuntersSigil : ItemBase
    {
        private const string token = "SS2_ITEM_HUNTERSSIGIL_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("HuntersSigil");
        public static GameObject effect;

        [ConfigurableField(ConfigDesc = "Base amount of extra armor added.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseArmor = 15;

        [ConfigurableField(ConfigDesc = "Amount of extra armor added per stack.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackArmor = 10;

        [ConfigurableField(ConfigDesc = "Base amount of extra crit added. (100 = 100%)")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float baseCrit = 25;

        [ConfigurableField(ConfigDesc = "Amount of extra crit added per stack. (100 = 100%)")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float stackCrit = 20;

        [ConfigurableField(ConfigDesc = "Base time the buff lingers for after moving, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float baseLinger = 1.5f;

        [ConfigurableField(ConfigDesc = "Amount of extra lingering time added per stack, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 5)]
        public static float stackLinger = 0.75f;

        public override void Initialize()
        {
            base.Initialize();
            effect = SS2Assets.LoadAsset<GameObject>("SigilEffect");
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.HuntersSigil;
            private bool sigilActive = false;

            public void FixedUpdate()
            {
                if (!NetworkServer.active) return;
                if (body.notMovingStopwatch > 1f)
                {
                    if (!sigilActive)
                    {
                        EffectManager.SimpleEffect(effect, body.aimOrigin + new Vector3(0, 0f), Quaternion.identity, true);
                        sigilActive = true;
                    }
                    body.AddTimedBuff(SS2Content.Buffs.BuffSigil, baseLinger + stackLinger * (stack - 1f));
                }
                else
                    sigilActive = false;
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffSigil))
                {
                    //the base amounts are added by the buff itself in case the buff is gained from another source such as Aetherium's Accursed Potion
                    args.armorAdd += stackArmor * (stack - 1);
                    args.critAdd += stackCrit * (stack - 1);
                }
            }
            public void OnDestroy()
            {
                body.ClearTimedBuffs(SS2Content.Buffs.BuffSigil);
            }
        }
    }
}
