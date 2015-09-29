﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using SharpDX;

namespace RivenBuddy
{
    internal class Queuer
    {
        public static List<string> Queue = new List<string>();
        public static InventorySlot tiamat;

        public static void DoQueue(Obj_AI_Base target)
        {
            if (Queue.Any() && Queue[0] != null)
            {
                DoTask(Queue[0], target);
            }
        }

        public static void Remove(string s)
        {
            if (Queue.Any() && Queue[0] == s)
            {
                Queue.RemoveAt(0);
            }
        }

        private static void DoTask(string s, Obj_AI_Base target)
        {
            if (Orbwalker.IsAutoAttacking || SpellEvents.FastQ) return;

            var spellSlot = SpellSlot.Unknown;
            var isR2 = false;
            switch (s)
            {
                case "AA":
                    Orbwalker.ResetAutoAttack();
                    return;
                case "Q":
                    spellSlot = SpellSlot.Q;
                    break;
                case "W":
                    spellSlot = SpellSlot.W;
                    break;
                case "E":
                    spellSlot = SpellSlot.E;
                    break;
                case "R1":
                    spellSlot = SpellSlot.R;
                    break;
                case "R2":
                    spellSlot = SpellSlot.R;
                    isR2 = true;
                    break;
                case "H":
                    CastTiamat();
                    return;
                case "FL":
                    CastFlash(target != null ? target.Position : Game.CursorPos);
                    break;
            }

            if ((spellSlot == SpellSlot.Unknown || !Player.GetSpell(spellSlot).IsReady))
            {
                if (SpellEvents.LastCast.ContainsKey(s) && SpellEvents.LastCast[s] + 500 < Environment.TickCount)
                {
                    Queue.RemoveAt(0);
                }
                return;
            }

            switch (spellSlot)
            {
                case SpellSlot.Q:
                    if (target == null) break;
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q, target.ServerPosition);
                    break;
                case SpellSlot.W:
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                    break;
                case SpellSlot.E:
                    if (target == null) break;
                    Player.CastSpell(SpellSlot.E, target.Position);
                    break;
                case SpellSlot.R:
                    if (isR2 && target.IsValidTarget())
                    {
                        var r2 = new Spell.Skillshot(spellSlot, 900, SkillShotType.Cone, 250, 1200, (int) (45*0.5));
                        var pred = r2.GetPrediction(target);
                        if (pred.UnitPosition.Distance(ObjectManager.Player) < 900) r2.Cast(pred.CastPosition);
                    }
                    else
                    {
                        SpellManager.Spells[SpellSlot.R].Cast();
                    }
                    break;
            }
        }

        public static void CastFlash(Vector3 position)
        {
            var flash = Player.Spells.FirstOrDefault(a => a.SData.Name == "summonerflash");
            if (flash != null && position.IsValid() && flash.IsReady)
            {
                Player.CastSpell(flash.Slot, position);
                Remove("FL");
            }
            else
            {
                Remove("FL");
            }
        }

        public static void CastTiamat()
        {
            if (tiamat != null && tiamat.CanUseItem())
            {
                tiamat.Cast();
            }
            else
            {
                Remove("H");
            }
        }
    }
}