using IdGen;
using System.ComponentModel;
using System.Globalization;

namespace Rikarin;

/// <summary>
///     ID class
/// </summary>
[TypeConverter(typeof(IDTypeConverter))]
public readonly record struct ID(ulong Id) : IComparable<ID> {
    const long TimeMask = (1L << 41) - 1;
    static readonly IdGenerator idGen;
    static readonly DateTime Epoch = new(2021, 11, 29, 0, 0, 0, DateTimeKind.Utc);

    public DateTimeOffset CreatedAt => FromId(Id);

    static ID() {
        var structure = new IdStructure(41, 10, 12);
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(Epoch));

        idGen = new(0, options);
    }

    public int CompareTo(ID other) => Id.CompareTo(other.Id);

    public override string ToString() => Id.ToString();
    public override int GetHashCode() => Id.GetHashCode();

    public static ID NewId() => new((ulong)idGen.CreateId());

    public static bool TryParse(string? input, out ID id) {
        if (ulong.TryParse(input, out var num)) {
            id = new(num);
            return true;
        }

        id = default!;
        return false;
    }

    public static ID Parse(string id) => new(ulong.Parse(id));

    public static ID FromDateTime(DateTime dateTime) {
        var idTicks = (dateTime - Epoch).Ticks / TimeSpan.TicksPerMillisecond;

        return new((ulong)(idTicks & TimeMask) << 22);
    }

    public static bool operator <(ID id, DateTime dateTime) => id.Id < FromDateTime(dateTime).Id;

    public static bool operator >(ID id, DateTime dateTime) => id.Id > FromDateTime(dateTime).Id;

    static DateTimeOffset FromId(ulong id) => idGen.FromId((long)id).DateTimeOffset;

    internal class IDTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);


        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            var val = value as string;
            return TryParse(val, out var result) ? result : base.ConvertFrom(context, culture, value);
        }
    }
}
