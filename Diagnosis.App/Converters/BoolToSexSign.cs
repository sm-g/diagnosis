
namespace Diagnosis.App.Converters
{
    public sealed class BoolToSexSign : NullableBooleanConverter<string>
    {
        public BoolToSexSign() :
            base("♂", "♀", "") { }
    }
}
