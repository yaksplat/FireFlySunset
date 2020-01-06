using System;
using Windows.ApplicationModel.Background;
using FireFlyCore;
using System.Xml.Linq;
using System.Linq;
using AdafruitClassLibrary;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.Threading;
using Windows.Devices.Gpio;
using System.Collections.Generic;
using Windows.Security.Cryptography;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace FireFlySunset
{
    public sealed class StartupTask : IBackgroundTask
    {
        Swarm s;
        static appSettings settings;
        SpinWait sw;
        private List<Species> SpeciesList;
        //   Mcp23017 p;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            LoadConfigFile();
          //  bool IsItTimeToRun;

            GpioController gpioController = GpioController.GetDefault();

            GpioPin SwitchPin = gpioController.OpenPin(5);

            GpioPin CalculatingPin = gpioController.OpenPin(6);
            GpioPin OutsideHoursPin = gpioController.OpenPin(13);
            GpioPin PowerPin = gpioController.OpenPin(19);

            PowerPin.SetDriveMode(GpioPinDriveMode.Output);
            CalculatingPin.SetDriveMode(GpioPinDriveMode.Output);
            OutsideHoursPin.SetDriveMode(GpioPinDriveMode.Output);
            SwitchPin.SetDriveMode(GpioPinDriveMode.Input);

            PowerPin.Write(GpioPinValue.Low);
            CalculatingPin.Write(GpioPinValue.Low);
            OutsideHoursPin.Write(GpioPinValue.Low);
           



            PowerPin.Write(GpioPinValue.High);
            // IsItTimeToRun = ShouldIBeRunning();


            sw = new SpinWait();
            //   await CalcDelay(taskInstance);
            loadSpeciesList();
            GpioPinValue pinval;

          //  int runcount = 0;
            //  int FireFlyCount = 48;
           // int stepcount = 10;
            while (true)
            {
                pinval = SwitchPin.Read();

              //  if (ShouldIBeRunning() || pin.Read() == GpioPinValue.Low)
                if (pinval == GpioPinValue.Low || ShouldIBeRunning())
                    {
                    //Turn off the indicator pin since we're in runmode
                    OutsideHoursPin.Write(GpioPinValue.Low);
                    //if (runcount % 2 == 0)
                    //     stepcount = settings.stepcount;
                    //  else
                    //      stepcount = 10;
                    Species newSpecies = GetRandomSpecies();

                    CalculatingPin.Write(GpioPinValue.High);
                    s = new Swarm(settings.BugQuantity, taskInstance, settings.stepcount, settings.interval, settings.spinCount,newSpecies,settings.MaxInitialDelay);
                    CalculatingPin.Write(GpioPinValue.Low);

                    s.StartFlashing();
                    s = null;
                    //  runcount += 1;
                    //IsItTimeToRun = ShouldIBeRunning();
                }
                else
                {
                    //light the indicator pin that we're outside the hours
                    OutsideHoursPin.Write(GpioPinValue.High);

                }
            }
        }

        private void loadSpeciesList()
        {
            // var file =   Package.Current.InstalledLocation.GetFolderAsync("Assets\\FireFlyData.xml");
            var path = System.IO.Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\FireFlyData.xml");
            //  var file =   WebRoot.GetFileAsync("FireFlyData.xml");
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (Exception)
            {
                throw;
            }

            var xmldoc = XDocument.Parse(doc.ToString());
            var SpList = (from mainRequest in xmldoc.Descendants("Species")
                          select new
                          {
                              Name = mainRequest.Element("Name").Value,
                              ShortName = mainRequest.Element("ShortName").Value,
                              Flashes = (from flash in mainRequest.Descendants("Flash")
                                         select new
                                         {
                                             sex = flash.Element("sex").Value,
                                             ResponseTime = flash.Element("ResponseTime").Value,
                                             QorA = flash.Element("QorA").Value,
                                             quantity = flash.Element("quantity").Value,
                                             TaperMultiple = flash.Element("TaperMultiple").Value,
                                             delay = flash.Element("delay").Value,
                                             Tapers = (from taper in flash.Descendants("Taper")
                                                       select new
                                                       {
                                                           Repeat = taper.Element("Repeat").Value,
                                                           RepeatQty = taper.Element("RepeatQty").Value,
                                                           RepeatDelay = taper.Element("RepeatDelay").Value,
                                                           StartIntensity = taper.Element("StartIntensity").Value,
                                                           Duration = taper.Element("Duration").Value,
                                                           EndIntensity = taper.Element("EndIntensity").Value,
                                                           TaperDirection = taper.Element("TaperDirection").Value
                                                       })
                                         })
                          });


            SpeciesList = new List<Species>();

            foreach (var i in SpList)
            {
                //    Fireflies.FireFly f = new Fireflies.FireFly();
                Species s = new Species();

                s.Name = i.Name;
                s.ShortName = i.ShortName;
                s.Flashes = new List<Flash>();
                foreach (var j in i.Flashes)
                {
                    Flash fl = new Flash();
                    fl.Tapers = new List<Taper>();
                    fl.sex = j.sex;
                    fl.QorA = j.QorA;
                    fl.ResponseTime = Convert.ToDouble(j.ResponseTime);
                    //   fl.Duration = Convert.ToDouble(j.Duration);
                    fl.delay = Convert.ToDouble(j.delay);
                    fl.TaperMultiple = Convert.ToBoolean(j.TaperMultiple);
                    fl.quantity = Convert.ToInt16(j.quantity);
                    //       fl.Intensity = Convert.ToInt16(j.Intensity);

                    foreach (var k in j.Tapers)
                    {
                        Taper t = new Taper();
                        t.Repeat = Convert.ToBoolean(k.Repeat);
                        t.RepeatQty = Convert.ToInt16(k.RepeatQty);
                        t.RepeatDelay = Convert.ToDouble(k.RepeatDelay);
                        t.Duration = Convert.ToUInt16(k.Duration);
                        t.EndIntensity = Convert.ToUInt16(k.EndIntensity);
                        t.StartIntensity = Convert.ToUInt16(k.StartIntensity);
                        switch (k.TaperDirection)
                        {
                            case "UP":
                                t.TaperDirection = Taper.TaperType.UP;
                                break;
                            case "DOWN":
                                t.TaperDirection = Taper.TaperType.DOWN;
                                break;
                            case "NONE":
                                t.TaperDirection = Taper.TaperType.NONE;
                                break;
                            case "FLAT":
                                t.TaperDirection = Taper.TaperType.FLAT;
                                break;
                            default:
                                t.TaperDirection = Taper.TaperType.NONE;
                                break;
                        }
                        fl.Tapers.Add(t);
                    }
                    s.Flashes.Add(fl);
                }
                SpeciesList.Add(s);
            }
        }
        private Species GetRandomSpecies()
        {
            int rand, selectedSpecies;
            //if (SpeciesList.Count > 1)
            //{
            rand = (int)GetRandom(Convert.ToUInt16(SpeciesList.Count - 1));
            selectedSpecies = rand;
            //}
            //else
            //    selectedSpecies = 0;


            return SpeciesList[selectedSpecies];
        }
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

        public bool ShouldIBeRunning()
        {
            double JD = 0;
            DateTime sunrise = DateTime.Now;
            DateTime sunset = DateTime.Now;

            DateTime TimeOn = DateTime.Now;
            DateTime TimeOff = DateTime.Now;
            DateTime Midnight = DateTime.Now.AddDays(1).Date;
            
            int zone = settings.timezone; // Seattle time Zone
            double latitude = settings.latitude; // Seattle lat
            double longitude = settings.longitude; // Seattle lon 
            bool dst = IsItDST(); // Day Light Savings 

            //42.8212° N, 78.6342° W
            JD = Util.calcJD(DateTime.Today);  //OR   JD = Util.calcJD(2014, 6, 1);
            double sunRise = Util.calcSunRiseUTC(JD, latitude, longitude);
            double sunSet = Util.calcSunSetUTC(JD, latitude, longitude);
            //System.Console.WriteLine(Util.getTimeString(sunRise, zone, JD, dst));
            //System.Console.WriteLine(Util.getTimeString(sunSet, zone, JD, dst));
            //System.Console.WriteLine("");
            sunrise = Util.getDateTime(sunRise, zone, DateTime.Today, dst).Value;
            sunset = Util.getDateTime(sunSet, zone, DateTime.Today, dst).Value;

            TimeOn = sunset.AddHours(settings.minutesFromSunset);
            TimeOff = Midnight.AddHours(settings.minutesFromMidnight);

            if (DateTime.Now > TimeOn && DateTime.Now < TimeOff)
                return true;
            else
                return false;

        }
        public bool IsItDST()
        {
            DateTime thisTime = DateTime.Now;
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(thisTime);
            return isDaylight;
        }
        public void loadFakeSettings()
        {
            settings = new appSettings();
            settings.timezone = -5;
            settings.latitude = 42.8212;
            settings.longitude = -78.6342;
            settings.minutesFromMidnight = 0;
            settings.minutesFromSunset = 1;
            settings.BugQuantity = 48;
            settings.MaxInitialDelay = 10000;
            settings.stepcount = 30000;
        }
        public async void LoadConfigFile()
        {
            settings = new appSettings();
            var WebRoot = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            var file = await WebRoot.GetFileAsync("app.xml");
            XDocument doc;
            try
            {
                doc = XDocument.Load(file.Path);
            }
            catch (Exception)
            {
                throw;
            }

            var xmldoc = XDocument.Parse(doc.ToString());

            var xmlConfig = (from keys in xmldoc.Descendants("appSettings")
                             select new
                             {
                                 timezone = keys.Element("timezone").Value,
                                 latitude = keys.Element("latitude").Value,
                                 longitude = keys.Element("longitude").Value,
                                 minutesFromMidnight = keys.Element("hoursFromMidnight").Value,
                                 minutesFromSunset = keys.Element("minutesFromSunset").Value,
                                 BugQuantity = keys.Element("BugQuantity").Value,
                                 MaxInitialDelay = keys.Element("MaxInitialDelay").Value,
                                 stepcount = keys.Element("stepcount").Value,
                                 interval = keys.Element("interval").Value,
                                 spinCount = keys.Element("spinCount").Value
                             });
            foreach (var i in xmlConfig)
            {
                settings.timezone = Convert.ToInt16(i.timezone);
                settings.latitude = Convert.ToDouble(i.latitude);
                settings.longitude = Convert.ToDouble(i.longitude);
                settings.minutesFromMidnight = Convert.ToInt16(i.minutesFromMidnight);
                settings.minutesFromSunset = Convert.ToInt16(i.minutesFromSunset);
                settings.BugQuantity = Convert.ToInt16(i.BugQuantity);
                settings.MaxInitialDelay = Convert.ToInt32(i.MaxInitialDelay);
                settings.stepcount = Convert.ToInt32(i.stepcount);
                settings.interval = Convert.ToInt16(i.interval);
                settings.spinCount = Convert.ToInt16(i.spinCount);
            }

        }

        Mcp23017 p, p1, p2;

        internal async Task CalcDelay(IBackgroundTaskInstance taskInstance)
        {
            // Swarm s = new Swarm(taskInstance, settings.interval);


            await InitChips();

            p.writeGPIOAB(0xffff);
            p1.writeGPIOAB(0xffff);
            p2.writeGPIOAB(0xffff);

            int delay = 0;
            TimeSpan timeSpan;
            int totalMilliseconds = 0;

            int TestTime = 500;
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            int stepCount;
            int spincount = settings.spinCount;


            bool calculating = true;

            while (calculating)
            {
                stepCount = TestTime / settings.interval;



                ushort allOn = 0xffff;
                ushort allOff = 0x0000;

                ushort value = allOn;


                startTime = DateTime.Now;
                for (int i = 0; i < stepCount; i++)
                {
                    p.writeGPIOAB(value);
                    Spin(spincount);
                    p1.writeGPIOAB(value);
                    Spin(spincount);
                    p2.writeGPIOAB(value);
                    Spin(spincount);

                    value = (value == allOn) ? allOff : allOn;
                }
                endTime = DateTime.Now;


                timeSpan = endTime - startTime;

                totalMilliseconds = timeSpan.Seconds * 1000 + timeSpan.Milliseconds;

                if (Math.Abs(TestTime - totalMilliseconds) < 30)
                    calculating = false;
                else
                {
                    if (totalMilliseconds > TestTime)
                    {
                        spincount -= 1;
                    }
                    else
                    {
                        spincount += 1;
                    }
                }


                p.writeGPIOAB(0);
                p1.writeGPIOAB(0);
                p2.writeGPIOAB(0);
            }
            
            p = null;
            p1 = null;
            p2 = null;
            GC.Collect();
            System.Threading.Tasks.Task.Delay(500).Wait();
            settings.spinCount = spincount;
        }

        private void Spin(int times)
        {
            for (int i = 0; i < times; i++)
            {
                sw.SpinOnce();
            }
        }

        internal async Task InitChips()
        {
            p = new Mcp23017(0x20);
            p1 = new Mcp23017(0x21);
            p2 = new Mcp23017(0x22);

            await p.InitMCP23017Async();
            await p1.InitMCP23017Async();
            await p2.InitMCP23017Async();

            for (int i = 0; i < 16; i++)
            {
                p.pinMode(i, Mcp23017.Direction.OUTPUT);
                p1.pinMode(i, Mcp23017.Direction.OUTPUT);
                p2.pinMode(i, Mcp23017.Direction.OUTPUT);
            }

        }


    }
}
