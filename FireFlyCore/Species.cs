using System.Collections.Generic;
using System.Diagnostics;

namespace FireFlyCore
{
    public class Species
    {

        public Species()
        {
        }
        public string Name { get; set; }
        public string ShortName { get; set; }


        public List<Flash> Flashes { get; set; }


        public void GetDetails()
        {
            Debug.WriteLine("Here's what I Do:");
            foreach (Flash item in Flashes)
            {
                item.GetDetails();
            }
        }

    }

    public class Taper
    {
        public Taper() { }

        public Taper(bool m_exists, int m_startIntensity, int m_endIntensity, double m_duration, TaperType direction)
        {




        }

        public enum TaperType
        {
            UP,
            DOWN,
            NONE,
            FLAT
        }
        public bool Repeat { get; set; }
        public int RepeatQty { get; set; }
        public double RepeatDelay { get; set; }
        public ushort StartIntensity { get; set; }
        public ushort EndIntensity { get; set; }
        public TaperType TaperDirection { get; set; }
        public ushort Duration { get; set; }

        public void GetDetails()
        {
            Debug.WriteLine($"I start at a level of {StartIntensity} for {Duration} and end with an intensity of {EndIntensity}");

        }

    }
    public class Flash
    {
        public string sex { get; set; }
        public double ResponseTime { get; set; }
        public string QorA { get; set; }
        public int quantity { get; set; }
        public bool TaperMultiple { get; set; }
        public int Intensity { get; set; }
        public double Duration { get; set; }
        public double delay { get; set; }

        public List<Taper> Tapers { get; set; }


        public void GetDetails()
        {
            foreach (Taper item in Tapers)
            {
                Debug.WriteLine($"I'm a {sex}");
                  item.GetDetails();
            }
        }
    }
}
