namespace WaterValve;

public class ValveService
{
    public int Threshold { get; set; } = 40; 
    public bool IsOpen { get; private set; } = false;

    public void ProcessHumidity(int humidity)
    {
        if (humidity < Threshold)
        {
            IsOpen = true;
            Console.WriteLine($"Humidity {humidity}% < {Threshold}% → Valve ON (watering)");
        }
        else
        {
            IsOpen = false;
            Console.WriteLine($"Humidity {humidity}% >= {Threshold}% → Valve OFF");
        }
    }
}