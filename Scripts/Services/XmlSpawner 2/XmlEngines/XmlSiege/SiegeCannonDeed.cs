using System;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
    public class SiegeCannonDeed : Item
    {
        [Constructable]
        public SiegeCannonDeed()
            : base(0x14F0)
        {
            this.Hue = 0x488;
            this.Weight = 1.0;
            this.LootType = LootType.Blessed;
            this.Name = "a deed for a Siege Cannon";
        }

        public SiegeCannonDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            this.LootType = LootType.Blessed;
        }

        public bool ValidatePlacement(Mobile from, Point3D loc)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (!from.InRange(this.GetWorldLocation(), 1))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return false;
            }

            Map map = from.Map;

            if (map == null)
                return false;

            BaseHouse house = BaseHouse.FindHouseAt(loc, map, 20);

            if (house != null && !house.IsFriend(from))
            {
                from.SendLocalizedMessage(500269); // You cannot build that there.
                return false;
            }

            if (!map.CanFit(loc, 20))
            {
                from.SendLocalizedMessage(500269); // You cannot build that there.
                return false;
            }

            return true;
        }

        public void BeginPlace(Mobile from)
        {
            from.BeginTarget(-1, true, TargetFlags.None, new TargetCallback(Placement_OnTarget));
        }

        public void Placement_OnTarget(Mobile from, object targeted)
        {
            IPoint3D p = targeted as IPoint3D;

            if (p == null)
                return;

            Point3D loc = new Point3D(p);

            if (p is StaticTarget)
                loc.Z -= TileData.ItemTable[((StaticTarget)p).ItemID & 0x3FFF].CalcHeight; /* NOTE: OSI does not properly normalize Z positioning here.
            * A side affect is that you can only place on floors (due to the CanFit call).
            * That functionality may be desired. And so, it's included in this script.
            */
            if (this.ValidatePlacement(from, loc))
                this.EndPlace(from, loc);
        }

        public void EndPlace(Mobile from, Point3D loc)
        {
            if (from == null)
                return;

            this.Delete();
            SiegeCannon cannon = new SiegeCannon();

            cannon.Location = loc;
            cannon.Map = from.Map;
        }

        public override void OnDoubleClick(Mobile from)
        {
            this.BeginPlace(from);
        }
    }
}