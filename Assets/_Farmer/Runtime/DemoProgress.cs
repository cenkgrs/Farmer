using System;
namespace Farmer
{
    [Flags] public enum JourneyStep { Seeds=1, Plant=2, Water=4, Harvest=8, Sale=16, Build=32, Explore=64 }
    public sealed partial class FarmModel
    {
        public JourneyStep Journey {get;private set;}
        public bool Completed(JourneyStep step)=>(Journey&step)==step;
    }
}
