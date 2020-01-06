using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Security.Cryptography;

namespace FireFlyCore
{
    public class Swarm
    {
        public const ushort Light_HI = 0;
        public const ushort Light_OFF = 4096;
        

        //13 when debug on
        //higher when off
        // public const ushort interval = 13;
        //    public ushort interval { get; set; }
        //    public int CycleDelay { get; set; }
        public int interval { get; set; }
        public int spinCount { get; set; }
        public int BugQuantity { get; set; }

        public int MaxInitialDelay;
        public int StepCount;
        public byte address = 0x20;
        //  public bool SwarmInitialized { get; set; }
        public int SwarmGroupCount { get; set; }
        public List<SwarmGroup> SwarmGroupList { get; set; }
        // private List<Species> SpeciesList;
        public Species SwarmSpecies { get; set; }
        //public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
        public int MaxDelay { get; set; }
        public int ExtraSteps { get; set; }
        public int LiveQuantity { get; set; }

        IBackgroundTaskInstance t;

        public  Swarm(int quantity, IBackgroundTaskInstance taskInstance, int stepcount, int flashInterval, int spin, Species newSpecies, int SettingsInitialDelay)
        {
            MaxInitialDelay = SettingsInitialDelay;

            StepCount = stepcount;
            spinCount = spin;
            interval = flashInterval;
            SwarmSpecies = newSpecies;
            t = taskInstance;
            BackgroundTaskDeferral deferral = t.GetDeferral();
            //     interval = 18;
            //     CycleDelay = 5;
            BugQuantity = quantity;
            //  SwarmInitialized = true;
            LoadThemUp();
        }

        public void GetMaxDelay()
        {
            int maxDelay = 0;
            foreach (SwarmGroup sg  in SwarmGroupList)
            {
                foreach (FireFly f  in sg.FireFlies)
                {
                    if (f.InitialDelay > maxDelay)
                        maxDelay = f.InitialDelay;
                }
            }
            this.MaxDelay = maxDelay;
        }


        public void CalcExtraSteps()
        {



        }




        public Swarm(IBackgroundTaskInstance taskInstance, int flashInterval,int spin)
        {
            t = taskInstance;
            BackgroundTaskDeferral deferral = t.GetDeferral();
            interval = flashInterval;
            BugQuantity = 48;
            //RunCalibration();
        }

        private  void LoadThemUp()
        {
            SwarmGroupList = new List<SwarmGroup>();
          //   loadSpeciesList();
            address = 0x20;
            LoadUpSwarms(BugQuantity);
        }
        //private  void loadSpeciesList()
        //{
        //   // var file =   Package.Current.InstalledLocation.GetFolderAsync("Assets\\FireFlyData.xml");
        //    var path = System.IO.Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\FireFlyData.xml");
        //    //  var file =   WebRoot.GetFileAsync("FireFlyData.xml");
        //    XDocument doc;
        //    try
        //    {
        //        doc = XDocument.Load(path);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    var xmldoc = XDocument.Parse(doc.ToString());
        //    var SpList = (from mainRequest in xmldoc.Descendants("Species")
        //                  select new
        //                  {
        //                      Name = mainRequest.Element("Name").Value,
        //                      ShortName = mainRequest.Element("ShortName").Value,
        //                      Flashes = (from flash in mainRequest.Descendants("Flash")
        //                                 select new
        //                                 {
        //                                     sex = flash.Element("sex").Value,
        //                                     ResponseTime = flash.Element("ResponseTime").Value,
        //                                     QorA = flash.Element("QorA").Value,
        //                                     quantity = flash.Element("quantity").Value,
        //                                     TaperMultiple = flash.Element("TaperMultiple").Value,
        //                                     delay = flash.Element("delay").Value,
        //                                     Tapers = (from taper in flash.Descendants("Taper")
        //                                               select new
        //                                               {
        //                                                   Repeat = taper.Element("Repeat").Value,
        //                                                   RepeatQty = taper.Element("RepeatQty").Value,
        //                                                   RepeatDelay = taper.Element("RepeatDelay").Value,
        //                                                   StartIntensity = taper.Element("StartIntensity").Value,
        //                                                   Duration = taper.Element("Duration").Value,
        //                                                   EndIntensity = taper.Element("EndIntensity").Value,
        //                                                   TaperDirection = taper.Element("TaperDirection").Value
        //                                               })
        //                                 })
        //                  });


        //    SpeciesList = new List<Species>();

        //    foreach (var i in SpList)
        //    {
        //        //    Fireflies.FireFly f = new Fireflies.FireFly();
        //        Species s = new Species();

        //        s.Name = i.Name;
        //        s.ShortName = i.ShortName;
        //        s.Flashes = new List<Flash>();
        //        foreach (var j in i.Flashes)
        //        {
        //            Flash fl = new Flash();
        //            fl.Tapers = new List<Taper>();
        //            fl.sex = j.sex;
        //            fl.QorA = j.QorA;
        //            fl.ResponseTime = Convert.ToDouble(j.ResponseTime);
        //            //   fl.Duration = Convert.ToDouble(j.Duration);
        //            fl.delay = Convert.ToDouble(j.delay);
        //            fl.TaperMultiple = Convert.ToBoolean(j.TaperMultiple);
        //            fl.quantity = Convert.ToInt16(j.quantity);
        //            //       fl.Intensity = Convert.ToInt16(j.Intensity);

        //            foreach (var k in j.Tapers)
        //            {
        //                Taper t = new Taper();
        //                t.Repeat = Convert.ToBoolean(k.Repeat);
        //                t.RepeatQty = Convert.ToInt16(k.RepeatQty);
        //                t.RepeatDelay = Convert.ToDouble(k.RepeatDelay);
        //                t.Duration = Convert.ToUInt16(k.Duration);
        //                t.EndIntensity = Convert.ToUInt16(k.EndIntensity);
        //                t.StartIntensity = Convert.ToUInt16(k.StartIntensity);
        //                switch (k.TaperDirection)
        //                {
        //                    case "UP":
        //                        t.TaperDirection = Taper.TaperType.UP;
        //                        break;
        //                    case "DOWN":
        //                        t.TaperDirection = Taper.TaperType.DOWN;
        //                        break;
        //                    case "NONE":
        //                        t.TaperDirection = Taper.TaperType.NONE;
        //                        break;
        //                    case "FLAT":
        //                        t.TaperDirection = Taper.TaperType.FLAT;
        //                        break;
        //                    default:
        //                        t.TaperDirection = Taper.TaperType.NONE;
        //                        break;
        //                }
        //                fl.Tapers.Add(t);
        //            }
        //            s.Flashes.Add(fl);
        //        }
        //        SpeciesList.Add(s);
        //    }
        //}
        //private Species GetRandomSpecies()
        //{
        //    int rand, selectedSpecies;
        //    //if (SpeciesList.Count > 1)
        //    //{
        //    rand = (int)GetRandom(Convert.ToUInt16(SpeciesList.Count - 1));
        //    selectedSpecies = rand;
        //    //}
        //    //else
        //    //    selectedSpecies = 0;


        //    return SpeciesList[selectedSpecies];
        //}

        public ushort GetRandom(ushort max)
        {
            uint rand = CryptographicBuffer.GenerateRandomNumber();
            uint high = uint.MaxValue;
            double randD = Convert.ToDouble(rand);
            double HighD = Convert.ToDouble(high);
            double fraction = randD / HighD;
            double result = max * fraction;
            ushort retval = Convert.ToUInt16(result);
            return retval;
        }


        private async void LoadUpSwarms(int qty)
        {
            LiveQuantity = qty;
            int StandAloneCount = LiveQuantity % 16;

            SwarmGroupCount = (int)Math.Ceiling((decimal)(LiveQuantity / 16));
            int RemainingGroupCount = SwarmGroupCount;

            for (int i = 0; i < SwarmGroupCount; i++)
            {
                int FillLevel;

                if (RemainingGroupCount == 1 && StandAloneCount != 0)
                    FillLevel = StandAloneCount;
                else
                    FillLevel = 16;
               // Species s = GetRandomSpecies();
                SwarmGroup sg;
                try
                {
                 sg = new SwarmGroup(address, FillLevel, SwarmSpecies, StepCount, interval,spinCount,MaxInitialDelay);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }


                RemainingGroupCount -= 1;
                SwarmGroupList.Add(sg);
                address += 1;
            }
        }

        public async Task StartFlashing()
        {
            //generate the pattern
            bool translated = true;

            Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Start");
            foreach (SwarmGroup sg in SwarmGroupList)
            {
                await sg.GenerateBuffer(translated, StepCount);
            }
            Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " End Buffer Generation");


            for (int i = 0; i < StepCount; i++)
            {
                foreach (SwarmGroup sg in SwarmGroupList)
                {
                    await sg.SetAllLights(i);
                }
            }
            Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " End Flash");

            foreach (SwarmGroup sg in SwarmGroupList)
            {
                sg.allOff();
            }
            Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " End Off");

            //  Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Reload");
            //  SwarmGroupList = null;
            //  LoadThemUp();
            //  SwarmGroupList.Clear();
            //  LoadUpSwarms(BugQuantity);
            //   Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " End Reload");
            //foreach (SwarmGroup sg in SwarmGroupList)
            //{
            //    await sg.ResetSwarmGroup();
            //}
            //Debug.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " End Reset");



            //foreach (SwarmGroup sg in SwarmGroupList)
            //{


            //}





        }

        //public void LoadCalibrationSpecies()
        //{
        //    SpeciesList = new List<Species>();
        //    Species s = new Species();
        //    s.Name = "Calibration";

        //    Flash f = new Flash();
        //    f.sex = "M";
        //    f.ResponseTime = 0;
        //    f.quantity = 1;
        //    f.QorA = "Q";
        //    f.TaperMultiple = false;
        //    f.delay = 5;
        //    LoadUpSwarms(BugQuantity);

        //    Taper t1 = new Taper() { Repeat = false, Duration = 500, RepeatQty = 1, EndIntensity = 99, RepeatDelay = 0, StartIntensity = 99, TaperDirection = Taper.TaperType.FLAT };
        //    Taper t2 = new Taper() { Repeat = false, Duration = 2500, RepeatQty = 1, EndIntensity = 0, RepeatDelay = 0, StartIntensity = 0, TaperDirection = Taper.TaperType.FLAT };
        //    f.Tapers.Add(t1);
        //    f.Tapers.Add(t2);

        //    Flash f2 = new Flash();
        //    f.sex = "F";
        //    f.ResponseTime = 3;
        //    f.quantity = 1;
        //    f.QorA = "Q";
        //    f.TaperMultiple = false;
        //    f.delay = 5;
        //    LoadUpSwarms(BugQuantity);

        //    Taper t4 = new Taper() { Repeat = false, Duration = 1500, RepeatQty = 1, EndIntensity = 0, RepeatDelay = 0, StartIntensity = 0, TaperDirection = Taper.TaperType.FLAT };
        //    Taper t5 = new Taper() { Repeat = false, Duration = 1000, RepeatQty = 1, EndIntensity = 99, RepeatDelay = 0, StartIntensity = 99, TaperDirection = Taper.TaperType.FLAT };
        //    Taper t6 = new Taper() { Repeat = false, Duration = 500, RepeatQty = 1, EndIntensity = 0, RepeatDelay = 0, StartIntensity = 0, TaperDirection = Taper.TaperType.FLAT };

        //    f.Tapers.Add(t4);
        //    f.Tapers.Add(t5);
        //    f.Tapers.Add(t6);

        //    s.Flashes.Add(f);
        //    s.Flashes.Add(f2);

        //    SpeciesList.Add(s);

        //}

        //public void RunCalibration()
        //{
        //    SwarmGroupList = new List<SwarmGroup>();
        //    LoadCalibrationSpecies();
        //    LoadUpSwarms(BugQuantity);


        //    SwarmGroup s1 = new SwarmGroup(0x20, interval);
        //    SwarmGroup s2 = new SwarmGroup(0x21, interval);
        //    SwarmGroup s3 = new SwarmGroup(0x22, interval);


        //}
    }
}
