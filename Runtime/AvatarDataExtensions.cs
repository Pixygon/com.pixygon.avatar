using Pixygon.Saving;

namespace Pixygon.Avatar {
    /// <summary>
    /// Bridges the persisted <see cref="AvatarData"/> (com.pixygon.saving — the int-ID Mii spec that
    /// stays tied to each game's save) to the engine-portable <see cref="AvatarSpec"/> the builder
    /// consumes. The field→slot mapping lives here so neither the save type nor the core has to know
    /// about the other. (Biology slots — tail/claws/gills… — come from the race-mode, not AvatarData,
    /// until AvatarData grows biology fields.)
    /// </summary>
    public static class AvatarDataExtensions {
        public static AvatarSpec ToSpec(this AvatarData d) {
            var s = new AvatarSpec { BodyHeight = d._bodyheight };
            s.Set(AvatarSlot.Body,           d._bodyID);
            s.Set(AvatarSlot.SkinTone,       d._skintoneID);
            s.Set(AvatarSlot.Eyes,           d._eyeID);
            s.Set(AvatarSlot.Hair,           d._hairID);
            s.Set(AvatarSlot.HairColor,      d._haircolorID);
            s.Set(AvatarSlot.Shirt,          d._shirtID);
            s.Set(AvatarSlot.Pants,          d._pantsID);
            s.Set(AvatarSlot.Shoes,          d._shoesID);
            s.Set(AvatarSlot.Jacket,         d._jacketID);
            s.Set(AvatarSlot.Headgear,       d._headgearID);
            s.Set(AvatarSlot.Socks,          d._socksID);
            s.Set(AvatarSlot.Gloves,         d._glovesID);
            s.Set(AvatarSlot.AccessoryHead,  d._accesoryIDHead);
            s.Set(AvatarSlot.AccessoryBody,  d._accesoryIDBody);
            s.Set(AvatarSlot.AccessoryLapel, d._accesoryIDLapel);
            s.Set(AvatarSlot.Offhand,        d._offhandEquipmentID);
            return s;
        }

        /// <summary>Write a spec's clothing/cosmetic slots back into an AvatarData (for persistence).</summary>
        public static void ApplyTo(this AvatarSpec s, AvatarData d) {
            d._bodyheight        = s.BodyHeight;
            d._bodyID            = s.Get(AvatarSlot.Body);
            d._skintoneID        = s.Get(AvatarSlot.SkinTone);
            d._eyeID             = s.Get(AvatarSlot.Eyes);
            d._hairID            = s.Get(AvatarSlot.Hair);
            d._haircolorID       = s.Get(AvatarSlot.HairColor);
            d._shirtID           = s.Get(AvatarSlot.Shirt);
            d._pantsID           = s.Get(AvatarSlot.Pants);
            d._shoesID           = s.Get(AvatarSlot.Shoes);
            d._jacketID          = s.Get(AvatarSlot.Jacket);
            d._headgearID        = s.Get(AvatarSlot.Headgear);
            d._socksID           = s.Get(AvatarSlot.Socks);
            d._glovesID          = s.Get(AvatarSlot.Gloves);
            d._accesoryIDHead    = s.Get(AvatarSlot.AccessoryHead);
            d._accesoryIDBody    = s.Get(AvatarSlot.AccessoryBody);
            d._accesoryIDLapel   = s.Get(AvatarSlot.AccessoryLapel);
            d._offhandEquipmentID = s.Get(AvatarSlot.Offhand);
        }
    }
}
