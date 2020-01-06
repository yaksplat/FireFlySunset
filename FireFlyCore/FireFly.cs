using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Security.Cryptography;

namespace FireFlyCore
{
    public class FireFly
    {
        public string Sex { get; set; }
        public int FlashCycleLength { get; set; }

        public bool CompletedInitialWait { get; set; }

        public int LocalStep { get; set; }
        public int MateID { get; set; }

        public ushort interval { get; set; }



        public FlashInstruction CurrentInstruction { get; set; }

        public event EventHandler Flash;
        public int InitialDelay { get; set; }
        public int InitialDelaySteps { get; set; }
        public int PostFlashSteps { get; set; }




        public List<ushort> InstructionSet { get; set; }
        public List<int> TranslatedInstructionSet { get; set; }
        public List<int> TranslatedDelayInstructionSet { get; set; }
        public List<ushort> DelayInstructionSet { get; set; }
        public List<FlashInstruction> FlashInstructions { get; set; }



        public Species species { get; set; }
        public int ID { get; set; }

        public FireFly(Species s, string sex, int flashInterval,int MaxDelay)
        {
            LocalStep = 0;
            species = s;
            Sex = sex;
            interval = Convert.ToUInt16(flashInterval);
            InitialDelay = GetDelay(MaxDelay);
            GenerateFlashSequence();
            GenerateInstructionSet(interval);
            TranslateInstructionsForMCP3017();
            CompletedInitialWait = false;
            //     this.timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(500));
        }

        public void reset()
        {
            InitialDelay = GetDelay(InitialDelay);
            GenerateFlashSequence();
            GenerateInstructionSet(interval);
            TranslateInstructionsForMCP3017();
            CompletedInitialWait = false;
        }

        public void GenerateFlashSequence()
        {
            FlashInstructions = new List<FlashInstruction>();
            Flash f = (from c in species.Flashes where c.sex == Sex select c).FirstOrDefault();
            foreach (Taper t in f.Tapers)
            {
                FlashInstruction fi = new FlashInstruction();
                fi.Duration = t.Duration;
                fi.EndIntensity = t.EndIntensity;
                fi.StartIntensity = t.StartIntensity;
                fi.TaperDirection = t.TaperDirection;
                FlashInstructions.Add(fi);
            }
        }

        public void TranslateInstructionsForMCP3017()
        {
            TranslatedDelayInstructionSet = new List<int>();
            TranslatedInstructionSet = new List<int>();

            foreach (uint delay in DelayInstructionSet)
            {
                TranslatedDelayInstructionSet.Add(0);
            }

            foreach (uint inst in InstructionSet)
            {
                if (inst == Swarm.Light_OFF)
                {
                    TranslatedInstructionSet.Add(0);
                }
                else
                {
                    TranslatedInstructionSet.Add(1);
                }
            }
        }

        public void GenerateInstructionSet(ushort interval)
        {
            ushort stepCount, IntensityInterval, currentIntensity;
            DelayInstructionSet = new List<ushort>();
            InstructionSet = new List<ushort>();
            int DelaySteps = InitialDelay / interval;

            for (int i = 0; i < DelaySteps; i++)
            {
                DelayInstructionSet.Add(Swarm.Light_OFF);
            }


            foreach (FlashInstruction f in FlashInstructions)
            {
                switch (f.TaperDirection)
                {
                    case Taper.TaperType.UP:
                        currentIntensity = f.StartIntensity;
                        stepCount = Convert.ToUInt16(f.Duration / interval);
                        IntensityInterval = Convert.ToUInt16((f.EndIntensity - f.StartIntensity) / stepCount);
                        for (int j = 0; j < stepCount; j++)
                        {
                            InstructionSet.Add(ConvertIntensity(currentIntensity));
                            currentIntensity += IntensityInterval;
                        }
                        break;
                    case Taper.TaperType.DOWN:
                        currentIntensity = f.StartIntensity;
                        stepCount = Convert.ToUInt16(f.Duration / interval);
                        IntensityInterval = Convert.ToUInt16((f.StartIntensity - f.EndIntensity) / stepCount);
                        for (int j = 0; j < stepCount; j++)
                        {
                            InstructionSet.Add(ConvertIntensity(currentIntensity));
                            currentIntensity -= IntensityInterval;
                        }
                        break;
                    case Taper.TaperType.NONE:
                        break;
                    case Taper.TaperType.FLAT:
                        for (int j = 0; j < f.Duration / interval; j++)
                        {
                            InstructionSet.Add(ConvertIntensity(f.StartIntensity));
                        }
                        break;
                    default:
                        break;
                }
            }




        }

        private int GetDelay(int maxDelay)
        {
            return GetRandom(maxDelay);
        }

        public int GetRandom(int max)
        {
            uint rand = CryptographicBuffer.GenerateRandomNumber();
            uint high = uint.MaxValue;
            double randD = Convert.ToDouble(rand);
            double HighD = Convert.ToDouble(high);
            double fraction = randD / HighD;
            double result = max * fraction;
            int retval = Convert.ToInt32(result);
            return retval;
        }

        private ushort ConvertIntensity(ushort intensity)
        {
            //Assumption
            //0 is off
            //4096 is full on
            //this can be inverted later
            return Convert.ToUInt16(Swarm.Light_OFF - Math.Ceiling(Convert.ToDecimal(Swarm.Light_OFF * intensity / 100)));
        }

        public ushort GetCurrentLevel(int iterationNumber, bool Translated)
        {
            ushort retval;
            int loopStep;

                if (DelayInstructionSet.Count == 0)
                    CompletedInitialWait = true;


            if (!Translated)
            {


                if (!CompletedInitialWait)
                {
                    retval = DelayInstructionSet[iterationNumber - 1];
                    if (iterationNumber == DelayInstructionSet.Count())
                        CompletedInitialWait = true;
                }
                else
                {
                    loopStep = GetLoopStep(iterationNumber);
                    retval = InstructionSet[loopStep];
                }
            }
            else
            {
                if (!CompletedInitialWait)
                {
                    retval = Convert.ToUInt16(TranslatedDelayInstructionSet[iterationNumber - 1]);
                    if (iterationNumber == TranslatedDelayInstructionSet.Count())
                        CompletedInitialWait = true;
                }
                else
                {
                    loopStep = GetLoopStep(iterationNumber);
                    retval = Convert.ToUInt16(TranslatedInstructionSet[loopStep]);
                }

            }
            //  LocalStep++;
            return retval;
        }

        private int GetLoopStep(int iterationNumber)
        {
            int retval = 0;
            retval = (iterationNumber - DelayInstructionSet.Count()) % InstructionSet.Count();
            return retval;
        }




        #region Unused

        public int CurrentBrightness { get; set; }

        private bool m_lighton;
        public bool LightOn
        {
            get { return m_lighton; }
            set
            {
                m_lighton = value;
                Flash?.Invoke(this, EventArgs.Empty);
            }
        }
        public void ProcessInstructions()
        {
            foreach (FlashInstruction f in FlashInstructions)
            {
                CurrentInstruction = f;
                switch (f.TaperDirection)
                {
                    case Taper.TaperType.UP:

                        break;
                    case Taper.TaperType.DOWN:
                        break;
                    case Taper.TaperType.NONE:
                        break;
                    case Taper.TaperType.FLAT:
                        //   LightOn
                        break;
                    default:
                        break;
                }
            }

        }

        public void CalculateFlashCycleLength()
        {
            foreach (FlashInstruction f in FlashInstructions)
            {
                FlashCycleLength += f.Duration;
            }
        }

        public void Beginflash()
        {
            //timer = new Timer();
            //timer.Interval = TimeSpan.FromMilliseconds(500);
            //timer.Tick += Timer_Tick;

            //if (pin != null)
            //{
            //    timer.Start();
            //}

        }
        protected virtual void StartFlash(EventArgs e)
        {
            Flash?.Invoke(this, e);
        }
        #endregion

    }
}
