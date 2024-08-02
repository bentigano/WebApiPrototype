using System.Diagnostics;

namespace WebApiPrototype
{

    [DebuggerDisplay("WeatherForecast {Date}: {TemperatureC}*C {TemperatureF}*F ({Summary})")]    
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}