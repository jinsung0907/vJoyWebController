using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace tabletjoy
{
    class vjoyHandler : vJoy
    {
        List<uint> owned = new List<uint>();

        public void startJoy()
        {
            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                GetvJoyManufacturerString(),
                GetvJoyProductString(),
                GetvJoySerialNumberString());

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n",
                DrvVer, DllVer);
        }

        public List<VjdStat> getAllDeviceStatus()
        {
            List<VjdStat> stats = new List<VjdStat>();
            for (uint i = 1; i <= 16; i++)
            {
                VjdStat status = GetVJDStatus(i);
                stats.Add(status);
            }

            return stats;
        }

        public void grabDevice(uint id)
        {
            VjdStat status = GetVJDStatus(id);
            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!AcquireVJD(id))))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            } else
            {
                ResetVJD(id);
                owned.Add(id);
            }
        }

        public void processButtonInput(uint vid, uint btn, bool status)
        {
            // validate필요
            SetBtn(status, vid, btn);
        }
    }
}
