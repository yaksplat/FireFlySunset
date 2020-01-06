using static FireFlyCore.Taper;

namespace FireFlyCore
{
    public class FlashInstruction
    {
        public ushort StartIntensity { get; set; }
        public ushort EndIntensity { get; set; }
        public TaperType TaperDirection { get; set; }
        public ushort Duration { get; set; }
    }
}
