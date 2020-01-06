namespace FireFlySunset
{
    public sealed class appSettings
    {
        public int timezone { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int minutesFromMidnight { get; set; }
        public int minutesFromSunset { get; set; }
        public int BugQuantity { get; set; }
        public int MaxInitialDelay { get; set; }
        public int stepcount { get; set; }
        public int interval { get; set; }

        public int spinCount { get; set; }

        //<add key = "timezone" value="-5"/>
        //<add key = "latitude" value="42.8212"/>
        //<add key = "longitude" value="-78.6342"/>
        //<add key = "minutesFromMidnight" value="0"/>
        //<add key = "minutesFromSunset" value="1"/>
        //<add key = "BugQuantity" value="48"/>
        //<add key = "MaxInitialDelay" value="10000"/>
        //<add key = "stepcount" value="30000"/>
    }
}
