namespace ElectricityUsageMonitor
{
    internal class SimulatedElectricityUsageProvider : IElectricityUsageProvider
    {
        private double baseUsage = 10;

        public double GetCurrentUsage()
        {
            baseUsage = baseUsage += 2;
            return baseUsage;
        }
    }
}