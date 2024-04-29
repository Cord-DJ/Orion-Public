using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;

namespace Cord.Equipment;

public static class ItemColors {
    public static readonly ItemModification Red = new("color_red");
    public static readonly ItemModification Green = new("color_green");
    public static readonly ItemModification Blue = new("color_blue");
    public static readonly ItemModification Lime = new("color_lime");
    public static readonly ItemModification Orange = new("color_orange");
    public static readonly ItemModification GrayPurple = new("color_gray_purple");
    public static readonly ItemModification Burgundy = new("color_burgundy");
    public static readonly ItemModification DarkBlue = new("color_dark_blue");
    public static readonly ItemModification ShadyBlue = new("color_shady_blue");
    public static readonly ItemModification SkyBlue = new("color_sky_blue");
    public static readonly ItemModification DarkPink = new("color_dark_pink");
    public static readonly ItemModification Purple = new("color_purple");
    public static readonly ItemModification DarkPurple = new("color_dark_purple");
    public static readonly ItemModification CreamWhite = new("color_cream_white");
    public static readonly ItemModification Gold = new("color_gold");

    public static readonly IEnumerable<ItemModification> All = new[] {
        Red, Green, Blue, Lime, Orange, GrayPurple, Burgundy, DarkBlue, ShadyBlue, SkyBlue, DarkPink, Purple,
        DarkPurple, CreamWhite, Gold
    };
}

public static class HumanSkinColors {
    public static readonly ItemModification White = new("human_skin_white");
    public static readonly ItemModification Black = new("human_skin_black");
    public static readonly ItemModification Chocolate = new("human_skin_chocolate");
    public static readonly ItemModification Asian = new("human_skin_asian");
    public static readonly ItemModification Hispanic = new("human_skin_hispanic");

    public static readonly IEnumerable<ItemModification> All = new[] { White, Black, Chocolate, Asian, Hispanic }; 
}

public static class Genders {
    public static readonly ItemModification Male = new("male");
    public static readonly ItemModification Female = new("female");

    public static readonly IEnumerable<ItemModification> All = new[] { Male, Female };
}

[TypeConverter(typeof(ItemModificationTypeConverter))]
[JsonConverter(typeof(ItemModificationJsonConverter))]
public record ItemModification(string Value) {
    internal class ItemModificationTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object value
        ) =>
            value is string s ? new ItemModification(s) : base.ConvertFrom(context, culture, value);
    }

    class ItemModificationJsonConverter : JsonConverter<ItemModification> {
        public override void WriteJson(JsonWriter writer, ItemModification? value, JsonSerializer serializer) {
            writer.WriteValue(value?.Value);
        }

        public override ItemModification? ReadJson(
            JsonReader reader,
            Type objectType,
            ItemModification? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        ) {
            var str = reader.Value?.ToString();
            if (str == null) {
                throw new ArgumentNullException();
            }

            return new(str);
        }
    }
}
