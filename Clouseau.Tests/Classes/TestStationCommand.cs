using System;

namespace Clouseau.Tests
{
    /// <summary>
    /// Instance only command
    /// </summary>
    public class TestStationCommand : AbstractStationCommand
    {
        public override bool IsStationScope { get { return false; } }
        public override bool IsInstanceScope { get { return true; } }
        public override bool IsBatchInstanceScope { get { return true; } }

        public override CommandResult Execute(Station station, Instance instance)
        {
            string message = string.Format("Performed Command: {0} for instance ID {1} on station {2}",
                this.Name, instance.ID, station.StationDescription);
            Console.WriteLine(message);
            CommandResultWritable result = new CommandResultWritable
            {
                Command = this,
                Station = station,
                Instance = instance,
                Success = true,
                Message = message
            };
            return result;
        }

    }

    /// <summary>
    /// Station only command
    /// </summary>
    public class TestStationCommand1 : AbstractStationCommand
    {
        public override bool IsStationScope { get { return true; } }
        public override bool IsInstanceScope { get { return false; } }
        public override bool IsBatchInstanceScope { get { return false; } }

        public override CommandResult Execute(Station station)
        {
            string message = string.Format("Performed Command: {0} for station {1}",
                this.Name, station.StationDescription);
            Console.WriteLine(message);
            CommandResultWritable result = new CommandResultWritable
            {
                Command = this,
                Station = station,
                Instance = null,
                Success = true,
                Message = message
            };
            return result;
        }

    }

}
