namespace NMKR.Shared.Classes
{
    public class MinMaxClass
    {
        public MinMaxClass(double minValue, double maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public long MinValueCardanoLovelace
        {
            get
            {
                return (long)(MinValue * 1000000);
            }
        }
        public long MaxValueCardanoLovelace
        {
            get
            {
                return (long)(MaxValue * 1000000);
            }
        }
    }
}
