﻿using EntityStates;
using EntityStates.AI.Walker;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm.Components;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System;
using UnityEngine;


namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Fear : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffFear");

        public override void Initialize()
        {
            Hook();
        }
        // Make this shit not a hook
        // Note: keep the hook until kevin comes back.
        // Hook wins!!
        // Fuck kevin, lol
        internal void Hook()
        {
            IL.EntityStates.AI.Walker.Combat.UpdateAI += (il) =>
            {
                ILCursor curs = new ILCursor(il);
                //go to where movement type is checked (applying movement vector)
                curs.GotoNext(x => x.MatchCall<Vector3>("Cross"));
                curs.Index += 2;
                curs.Emit(OpCodes.Ldarg_0);
                curs.Emit(OpCodes.Ldfld, typeof(EntityState).GetFieldCached("outer"));
                curs.Emit(OpCodes.Ldloc_1);
                curs.EmitDelegate<Func<EntityStateMachine, AISkillDriver.MovementType, AISkillDriver.MovementType>>((ESM, MoveType) =>
                {
                    if (ESM.GetComponent<CharacterMaster>().GetBody().HasBuff(SS2Content.Buffs.BuffFear))
                    {
                        return AISkillDriver.MovementType.FleeMoveTarget;
                    }
                    else
                        return MoveType;
                });
                curs.Emit(OpCodes.Stloc_1);
            };

            IL.EntityStates.AI.Walker.Combat.GenerateBodyInputs += (il) =>
            {
                ILCursor curs = new ILCursor(il);
                curs.GotoNext(x => x.MatchLdfld<Combat>("currentSkillMeetsActivationConditions"));
                curs.Index += 1;
                curs.Emit(OpCodes.Ldarg_0);
                curs.Emit(OpCodes.Ldfld, typeof(EntityState).GetFieldCached("outer"));
                curs.EmitDelegate<Func<bool, EntityStateMachine, bool>>((cond, ESM) =>
                {
                    if (ESM.GetComponent<CharacterMaster>().GetBody())
                        if (ESM.GetComponent<CharacterMaster>().GetBody().HasBuff(SS2Content.Buffs.BuffFear))
                            return false;
                    return cond;
                });
            };
        }

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier, IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffFear;
            private void Start()
            {
                if (body.healthComponent)
                {
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedReductionMultAdd += 0.3f;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                damageInfo.damage *= 1.5f;
            }

            private void OnDestroy()
            {
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }
        }
    }
}
