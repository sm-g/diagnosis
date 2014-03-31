
namespace Diagnosis.App.Converters
{
    public sealed class BoolToSexSign : BooleanConverter<string>
    {
        public BoolToSexSign() :
            base("♂", "♀") { }
    }
}
