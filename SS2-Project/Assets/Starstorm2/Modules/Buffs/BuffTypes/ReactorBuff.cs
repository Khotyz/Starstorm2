﻿using Moonstorm.Components;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ReactorBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffReactor");
        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matReactorBuffOverlay");

        //To-Do: Maybe better invincibility implementation. Projectile deflection for cool points?
        public sealed class Behavior : BaseBuffBodyBehavior, RoR2.IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffReactor;
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                damageInfo.damage = 0f;
                //★ *sips
                //★ so how do you do your invincibility?
            }
        }
    }
}
