using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Management;

namespace Playtime {
 public class Game {
  public string id {get;set;} public string name {get;set;}
  public string executable {get;set;} public string arguments {get;set;} public string workingDirectory {get;set;}
  public string originalShortcut {get;set;} public string trackingDirectory {get;set;} public string[] processNames {get;set;}
  public bool enabled {get;set;}
  public Game(){id=Guid.NewGuid().ToString("N");enabled=true;processNames=new string[0];}
 }
 public class Config {
  public int schemaVersion=1; public string language="system"; public bool onboardingComplete=false;
  public List<Game> games=new List<Game>();
 }
 public class Session {
  public string id=Guid.NewGuid().ToString("N"),gameId,gameName,runKey,status,startTime,endTime,lastHeartbeat;
  public double durationSeconds;
 }
 public class Backup {public int schemaVersion=1;public Config config;public List<Session> sessions=new List<Session>();}
 public class Store {
  public readonly string Root; public readonly object Gate=new object();
  readonly JavaScriptSerializer json=new JavaScriptSerializer {MaxJsonLength=67108864};
  public Config Config; public List<Session> Sessions=new List<Session>();
  public string Warning;
  public Store(string root) {
   Root=Path.GetFullPath(root);Directory.CreateDirectory(Root);Directory.CreateDirectory(Path.Combine(Root,"sessions"));Directory.CreateDirectory(Path.Combine(Root,"active"));Directory.CreateDirectory(Path.Combine(Root,"trash"));
   string path=Path.Combine(Root,"settings.json");
   Config=File.Exists(path)?json.Deserialize<Config>(File.ReadAllText(path)):new Config();ValidateConfig(Config);
   foreach(string f in Directory.GetFiles(Path.Combine(Root,"sessions"),"*.json"))try {var s=json.Deserialize<Session>(File.ReadAllText(f));ValidateSession(s);Sessions.Add(s);}catch{Warning="damaged";}
   foreach(string f in Directory.GetFiles(Path.Combine(Root,"active"),"*.json")) {
    try {var s=json.Deserialize<Session>(File.ReadAllText(f));ValidateSession(s);if(!Sessions.Any(x=>x.id==s.id)){s.status="interrupted";s.endTime=s.lastHeartbeat;WriteSession(s);Sessions.Add(s);}File.Delete(f);}catch{Warning="damaged";}
   }
  }
  public static void ValidateConfig(Config c){if(c==null||c.games==null||c.games.Count>500)throw new InvalidDataException("Invalid configuration");var ids=new HashSet<string>();foreach(var g in c.games){ValidateGame(g);if(!ids.Add(g.id))throw new InvalidDataException("Duplicate game ID");}}
  public static void ValidateGame(Game g){if(g==null||!SafeId(g.id)||string.IsNullOrWhiteSpace(g.name)||g.processNames==null||g.processNames.Length==0||g.processNames.Any(n=>string.IsNullOrWhiteSpace(n)||n.IndexOfAny(new char[]{'\\','/',':','*','?'})>=0)||string.IsNullOrWhiteSpace(g.trackingDirectory)||!Path.IsPathRooted(g.trackingDirectory))throw new InvalidDataException("Invalid game configuration");}
  static bool SafeId(string s){return !string.IsNullOrEmpty(s)&&s.Length<100&&s.All(c=>char.IsLetterOrDigit(c)||c=='_'||c=='-');}
  public static void ValidateSession(Session s){DateTimeOffset dt;if(s==null||!SafeId(s.id)||!SafeId(s.gameId)||double.IsNaN(s.durationSeconds)||double.IsInfinity(s.durationSeconds)||s.durationSeconds<0||s.durationSeconds>3155760000||!DateTimeOffset.TryParse(s.startTime,out dt))throw new InvalidDataException("Invalid session");}
  public void Atomic(string path,object data){string tmp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(tmp,json.Serialize(data),new System.Text.UTF8Encoding(false));for(int attempt=0;;attempt++){try{if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);break;}catch(IOException){if(attempt>=3)throw;Thread.Sleep(30);}}}finally{if(File.Exists(tmp))File.Delete(tmp);}}
  public void SaveConfig(){lock(Gate){ValidateConfig(Config);Atomic(Path.Combine(Root,"settings.json"),Config);}}
  public void SaveActive(Session s){lock(Gate)Atomic(Path.Combine(Root,"active",s.id+".json"),s);}
  void WriteSession(Session s){Atomic(Path.Combine(Root,"sessions",s.id+".json"),s);}
  public void Finish(Session s){lock(Gate){WriteSession(s);Sessions.RemoveAll(x=>x.id==s.id);Sessions.Add(s);string path=Path.Combine(Root,"active",s.id+".json");if(File.Exists(path))File.Delete(path);}}
  public void Delete(Session s){lock(Gate){string src=Path.Combine(Root,"sessions",s.id+".json");if(File.Exists(src))File.Move(src,Path.Combine(Root,"trash",s.id+"-"+DateTime.UtcNow.Ticks+".json"));Sessions.RemoveAll(x=>x.id==s.id);}}
  public void Export(string path){lock(Gate){var list=Sessions.ToList();foreach(string f in Directory.GetFiles(Path.Combine(Root,"active"),"*.json")){var s=json.Deserialize<Session>(File.ReadAllText(f));ValidateSession(s);if(!list.Any(x=>x.id==s.id)){s.status="interrupted";s.endTime=s.lastHeartbeat;list.Add(s);}}Atomic(path,new Backup{config=Config,sessions=list});}}
  public int Import(string path){
   var backup=json.Deserialize<Backup>(File.ReadAllText(path));if(backup==null||backup.schemaVersion!=1||backup.sessions==null)throw new InvalidDataException("Invalid backup");ValidateConfig(backup.config);foreach(var s in backup.sessions)ValidateSession(s);
   lock(Gate){Export(Path.Combine(Root,"before-import-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".json"));
    foreach(var g in backup.config.games)if(!Config.games.Any(x=>x.id==g.id))Config.games.Add(g);
    int n=0;foreach(var s in backup.sessions)if(!Sessions.Any(x=>x.id==s.id)&&!File.Exists(Path.Combine(Root,"active",s.id+".json"))){WriteSession(s);Sessions.Add(s);n++;}SaveConfig();return n;}
  }
  public int ImportLegacy(string root){
   // Legacy data is explicitly selected by the user; never load it into a public release.
   var c=json.Deserialize<Config>(File.ReadAllText(Path.Combine(root,"games.json")));
   foreach(var g in c.games){g.enabled=true;if(g.processNames==null||g.processNames.Length==0)g.processNames=new[]{Path.GetFileNameWithoutExtension(g.executable)};if(string.IsNullOrEmpty(g.trackingDirectory))g.trackingDirectory=Path.GetDirectoryName(g.executable);}ValidateConfig(c);
   var b=new Backup{config=c};foreach(string f in Directory.GetFiles(Path.Combine(root,"data","sessions"),"*.json")){var s=json.Deserialize<Session>(File.ReadAllText(f));if(s.startTime==null)continue;s.runKey="legacy-"+s.id;ValidateSession(s);b.sessions.Add(s);}
   string temp=Path.Combine(Root,"legacy-import.json");lock(Gate)Atomic(temp,b);try{return Import(temp);}finally{File.Delete(temp);}
  }
  public void Log(Exception ex){try{lock(Gate){string p=Path.Combine(Root,"errors.log");if(File.Exists(p)&&new FileInfo(p).Length>1048576)File.Move(p,p+"."+DateTime.UtcNow.Ticks);File.AppendAllText(p,DateTimeOffset.Now.ToString("o")+" "+ex+Environment.NewLine);}}catch{}}
 }
 public class Observed {public string Key {get;set;} public string Path {get;set;} public string Name {get;set;} public DateTimeOffset Started {get;set;}}
 public class Running {public Session Session;public double LastSeen,LastSaved;}
 public class Tracker : IDisposable {
  public readonly Store Store;readonly object gate=new object();readonly AutoResetEvent wake=new AutoResetEvent(false);readonly Dictionary<string,Running> active=new Dictionary<string,Running>();
  Thread thread;volatile bool stopping,paused,suspended;readonly Stopwatch clock=Stopwatch.StartNew();double lastTick=-1;
  ManagementEventWatcher startEvents,stopEvents;public bool EventMode {get;private set;}
  public string Error;public bool Paused {get{return paused;}}
  public Tracker(Store store){Store=store;}
  public void Start(){SystemEvents.PowerModeChanged+=Power;
   try {startEvents=new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");stopEvents=new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");startEvents.EventArrived+=(o,e)=>Signal();stopEvents.EventArrived+=(o,e)=>Signal();startEvents.Start();stopEvents.Start();EventMode=true;}catch{StopEvents();}
   thread=new Thread(Loop){IsBackground=true,Name="Game tracking"};thread.Start();}
  void StopEvents(){foreach(var watcher in new[]{startEvents,stopEvents})if(watcher!=null){try{watcher.Stop();}catch{}watcher.Dispose();}startEvents=null;stopEvents=null;EventMode=false;}
  void Signal(){if(!stopping)try{wake.Set();}catch(ObjectDisposedException){}}
  void Power(object sender,PowerModeChangedEventArgs e){if(e.Mode==PowerModes.Suspend)Suspend();else if(e.Mode==PowerModes.Resume)Resume();}
  public void Suspend(){lock(gate){suspended=true;FinishAll("sleep");lastTick=-1;}wake.Set();}
  public void Resume(){lock(gate){suspended=false;lastTick=-1;}wake.Set();}
  public void SetPaused(bool value){lock(gate){paused=value;if(value)FinishAll("paused");else Error=null;lastTick=-1;}wake.Set();}
  public void Changed(){wake.Set();}
  public List<Session> Snapshot(){lock(gate){lock(Store.Gate){return Store.Sessions.Concat(active.Values.Select(x=>x.Session)).Select(Clone).ToList();}}}
  static Session Clone(Session s){return new Session{id=s.id,gameId=s.gameId,gameName=s.gameName,runKey=s.runKey,status=s.status,startTime=s.startTime,endTime=s.endTime,lastHeartbeat=s.lastHeartbeat,durationSeconds=s.durationSeconds};}
  void Loop(){while(!stopping){try{List<Game> games;lock(Store.Gate)games=Store.Config.games.Where(g=>g.enabled).ToList();
    if(!paused&&!suspended&&games.Count>0)Tick(Scan(games),clock.Elapsed.TotalSeconds,DateTimeOffset.Now);else if(games.Count==0)lock(gate)FinishAll("completed");
   }catch(Exception ex){Error="trackingError";Store.Log(ex);try{lock(gate){FinishAll("interrupted");paused=true;}}catch{}}
   int interval;lock(gate)interval=EventMode&&active.Count==0?15000:3000;wake.WaitOne(interval);if(EventMode)Thread.Sleep(100);
  }}
  public void Tick(List<Observed> processes,double now,DateTimeOffset utc){lock(gate){if(paused||suspended)return;
   if(lastTick>=0&&now-lastTick>20)FinishAll("interrupted");lastTick=now;
   List<Game> games;lock(Store.Gate)games=Store.Config.games.Where(g=>g.enabled).ToList();
   foreach(string removed in active.Keys.Where(id=>!games.Any(g=>g.id==id)).ToArray())Finish(removed,"completed");
   foreach(var g in games){var matched=processes.Where(p=>Matches(g,p)).OrderBy(p=>p.Started).ToList();Running r;active.TryGetValue(g.id,out r);
    if(matched.Count>0){
     string key=g.id+":"+matched[0].Key;
     if(r!=null&&r.Session.runKey!=key){Finish(g.id,"completed");r=null;}
     if(r==null){r=new Running {Session=new Session{gameId=g.id,gameName=g.name,runKey=key,status="running",startTime=utc.ToString("o"),lastHeartbeat=utc.ToString("o")},LastSeen=now,LastSaved=now};active[g.id]=r;Store.SaveActive(r.Session);}
     else {r.Session.durationSeconds+=Math.Max(0,now-r.LastSeen);r.LastSeen=now;r.Session.lastHeartbeat=utc.ToString("o");if(now-r.LastSaved>=5){Store.SaveActive(r.Session);r.LastSaved=now;}}
    }else if(r!=null&&now-r.LastSeen>=6)Finish(g.id,"completed");
   }
  }}
  public static bool Matches(Game g,Observed p){string dir=System.IO.Path.GetFullPath(g.trackingDirectory).TrimEnd('\\')+"\\";return g.processNames.Any(n=>string.Equals(System.IO.Path.GetFileNameWithoutExtension(n),p.Name,StringComparison.OrdinalIgnoreCase))&&p.Path.StartsWith(dir,StringComparison.OrdinalIgnoreCase);}
  void Finish(string id,string state){var r=active[id];r.Session.status=state;r.Session.endTime=r.Session.lastHeartbeat;Store.Finish(r.Session);active.Remove(id);}
  void FinishAll(string state){foreach(string id in active.Keys.ToArray())Finish(id,state);}
  public void Dispose(){stopping=true;StopEvents();wake.Set();if(thread!=null)thread.Join(5000);lock(gate)FinishAll("stopped");SystemEvents.PowerModeChanged-=Power;wake.Dispose();}
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool QueryFullProcessImageName(IntPtr h,int flags,System.Text.StringBuilder path,ref int size);
  [DllImport("kernel32.dll")]static extern IntPtr OpenProcess(int access,bool inherit,int pid);
  [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr handle);
  [DllImport("kernel32.dll")]static extern IntPtr CreateToolhelp32Snapshot(uint flags,uint pid);
  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct ProcessEntry {public uint size,usage,pid;public UIntPtr heap;public uint module,threads,parent;public int priority;public uint flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string exe;}
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,EntryPoint="Process32FirstW")]static extern bool ProcessFirst(IntPtr snapshot,ref ProcessEntry entry);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,EntryPoint="Process32NextW")]static extern bool ProcessNext(IntPtr snapshot,ref ProcessEntry entry);
  public static List<Observed> Scan(List<Game> games){var names=new HashSet<string>(games.SelectMany(g=>g.processNames).Select(n=>Path.GetFileNameWithoutExtension(n)),StringComparer.OrdinalIgnoreCase);return ScanNames(names);}
  public static List<Observed> ScanNames(HashSet<string> names){var result=new List<Observed>();IntPtr snapshot=CreateToolhelp32Snapshot(2,0);if(snapshot==new IntPtr(-1))throw new System.ComponentModel.Win32Exception();int self=Process.GetCurrentProcess().Id;
   try{var entry=new ProcessEntry{size=(uint)Marshal.SizeOf(typeof(ProcessEntry))};bool found=ProcessFirst(snapshot,ref entry);while(found){try{string name=Path.GetFileNameWithoutExtension(entry.exe);
    if(entry.pid!=self&&(names==null||names.Contains(name))){IntPtr h=OpenProcess(0x1000,false,(int)entry.pid);if(h!=IntPtr.Zero){string path=null;try{var b=new System.Text.StringBuilder(32768);int size=b.Capacity;if(QueryFullProcessImageName(h,0,b,ref size))path=b.ToString();}finally{CloseHandle(h);}if(path!=null)using(var p=Process.GetProcessById((int)entry.pid)){DateTimeOffset started=p.StartTime.ToUniversalTime();result.Add(new Observed{Name=name,Path=path,Started=started,Key=entry.pid+":"+started.UtcTicks});}}}
   }catch{}found=ProcessNext(snapshot,ref entry);}}finally{CloseHandle(snapshot);}return result;}
 }
}
