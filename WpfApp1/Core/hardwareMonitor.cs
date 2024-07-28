using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Core
{
    class hardwareMonitor
    {
        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        public float Monitor(SensorType _sensorType, string _sensorName)
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware)
            {

                if (hardware.HardwareType == HardwareType.Cpu) //found cpu
                {
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == _sensorType && sensor.Name == _sensorName)
                        {
                            computer.Close();
                            if (_sensorType == SensorType.Clock) return (float)sensor.Value * 10f;
                            return (sensor.Value == null) ? 0.0f : (float)sensor.Value;
                        }
                    }
                }
                
            }


            computer.Close();
            return 0.0f;
        }
    }
}
