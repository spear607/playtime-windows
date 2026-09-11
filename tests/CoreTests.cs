using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Playtime;
class CoreTests {
 static int n;static string root;
 static void Assert(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("PASS "+message);n++;}
 static Game Game(){return new Game{id="game",name="Test Game",executable=Path.Combine(root,"game.exe"),trackingDirectory=root,processNames=new[]{"game"}};}
 static Store New(string name){var s=new Store(Path.Combine(root,name));s.Config.games.Add(Game());s.SaveConfig();return s;}
 static List<Observed> Seen(string key="1"){return new List<Observed>{new Observed{Key=key,Name="game",Path=Path.Combine(root,"game.exe"),Started=DateTimeOffset.UtcNow.AddMinutes(-1)}};}
 static void Main(string[] args){root=args[0];Directory.CreateDirectory(root);DateTimeOffset date=DateTimeOffset.Now;
  var s=New("normal");using(var t=new Tracker(s)){t.Tick(Seen(),0,date);t.Tick(Seen(),3,date.AddSeconds(3));t.Tick(new List<Observed>(),6,date.AddSeconds(6));t.Tick(new List<Observed>(),9,date.AddSeconds(9));Assert(s.Sessions.Count==1&&s.Sessions[0].status=="completed"&&s.Sessions[0].durationSeconds==3,"complete without counting absence grace");}
  s=New("segments");using(var t=new Tracker(s)){t.Tick(Seen(),0,date);t.Tick(Seen(),3,date.AddSeconds(3));t.SetPaused(true);t.Tick(Seen(),100,date.AddSeconds(100));Assert(t.Snapshot().Count==1,"pause stops accumulation");t.SetPaused(false);t.Tick(Seen(),101,date.AddSeconds(101));t.Tick(Seen(),104,date.AddSeconds(104));t.Suspend();t.Resume();t.Tick(Seen(),500,date.AddSeconds(500));t.Tick(Seen(),503,date.AddSeconds(503));var all=t.Snapshot();Assert(all.Sum(x=>x.durationSeconds)==9,"sleep and pause gaps excluded");Assert(all.Select(x=>x.runKey).Distinct().Count()==1,"same process segments count once");t.Tick(Seen("2"),506,date.AddSeconds(506));Assert(t.Snapshot().Select(x=>x.runKey).Distinct().Count()==2,"new process counted separately");}
  s=New("gap");using(var t=new Tracker(s)){t.Tick(Seen(),0,date);t.Tick(Seen(),3,date.AddSeconds(3));t.Tick(Seen(),100,date.AddSeconds(100));Assert(t.Snapshot().Sum(x=>x.durationSeconds)==3&&s.Sessions[0].status=="interrupted","unexpected long gap not counted");}
  s=New("recovery");s.SaveActive(new Session{gameId="game",gameName="Test",status="running",runKey="game:1",startTime=date.ToString("o"),lastHeartbeat=date.AddSeconds(6).ToString("o"),durationSeconds=6});s=new Store(s.Root);Assert(s.Sessions.Count==1&&s.Sessions[0].status=="interrupted"&&s.Sessions[0].durationSeconds==6,"crash recovery uses persisted heartbeat");
  string backup=Path.Combine(root,"backup.json");s.Export(backup);var dest=New("import");Assert(dest.Import(backup)==1&&dest.Import(backup)==0&&dest.Sessions.Count==1,"backup merge is idempotent");dest.Delete(dest.Sessions[0]);Assert(dest.Sessions.Count==0&&Directory.GetFiles(Path.Combine(dest.Root,"trash")).Length==1,"delete moves to trash");
  bool rejected=false;try{Store.ValidateSession(new Session{id="../outside",gameId="game",startTime=date.ToString("o")});}catch{rejected=true;}Assert(rejected,"reject unsafe imported ID");
  Assert(!Tracker.Matches(Game(),new Observed{Name="game",Path=Path.Combine(root+"other","game.exe")}),"same name outside tracking folder excluded");
  var activeBackup=New("active-export");activeBackup.SaveActive(new Session{gameId="game",gameName="Test",runKey="game:1",status="running",startTime=date.ToString("o"),lastHeartbeat=date.ToString("o"),durationSeconds=3});activeBackup.Export(Path.Combine(root,"active.json"));dest=New("active-import");dest.Import(Path.Combine(root,"active.json"));Assert(dest.Sessions.Count==1&&dest.Sessions[0].status=="interrupted","backup includes active heartbeat safely");
  Assert(activeBackup.Import(Path.Combine(root,"active.json"))==0&&activeBackup.Sessions.Count==0,"import skips session currently being tracked");
  foreach(string lang in new[]{"zh-CN","zh-TW","en","ja"}){L.Set(lang);Assert(L.Keys.All(k=>!string.IsNullOrWhiteSpace(L.T(k))),"localization complete "+lang);}
  Console.WriteLine(n+" assertions passed.");
 }
}
