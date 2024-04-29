using System.ComponentModel;
using System.Globalization;

namespace Cord;

[TypeConverter(typeof(CategoryTypeConverter))]
public readonly record struct Category {
    public static readonly Category Edm = new("edm");
    public static readonly Category Pop = new("pop");
    public static readonly Category Rock = new("rock");
    public static readonly Category Metal = new("metal");
    public static readonly Category Jazz = new("jazz");
    public static readonly Category Country = new("country");
    public static readonly Category Classics = new("classics");
    public static readonly Category Retro = new("retro");
    public static readonly Category Seventies = new("seventies");
    public static readonly Category Eighties = new("eighties");
    public static readonly Category Nineties = new("nineties");
    public static readonly Category Gaming = new("gaming");
    public static readonly Category Anime = new("anime");
    public static readonly Category HipHop = new("hiphop");
    public static readonly Category Indie = new("indie");
    public static readonly Category Techno = new("techno");
    public static readonly Category Other = new("other");

    public static readonly Category[] AvailableCategories = {
        Edm, Pop, Rock, Metal, Jazz, Country, Classics, Retro, Seventies, Eighties, Nineties, Gaming, Anime, HipHop,
        Indie, Techno, Other
    };

    readonly string value;

    Category(string value) {
        this.value = value;
    }

    public override string ToString() => value;

    internal class CategoryTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
            value is string val ? new Category(val) : base.ConvertFrom(context, culture, value);
    }
}
