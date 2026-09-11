using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Playtime;
class EventProbe {
 static void Main(string[] args){string root=args[0];var s=new Store(Path.Combine(root,"admin-data"));s.Config.games.Clear();s.Config.games.Add(new Game{id="probe",name="Probe",executable=Path.Combine(root,"SleepGame.exe"),workingDirectory=root,trackingDirectory=root,processNames=new[]{"SleepGame"}});s.SaveConfig();using(var t=new Tracker(s)){t.Start();Thread.Sleep(1500);using(var p=Process.Start(new ProcessStartInfo(Path.Combine(root,"SleepGame.exe"),"6000"){UseShellExecute=false,CreateNoWindow=true})){p.WaitForExit();}Thread.Sleep(8500);File.WriteAllText(Path.Combine(root,"event-result.txt"),"EventMode="+t.EventMode+"; Sessions="+t.Snapshot().Count+"; Error="+t.Error);}}
}
