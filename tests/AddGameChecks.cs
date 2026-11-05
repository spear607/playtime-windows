using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Playtime;
class AddGameChecks {
 static IEnumerable<DependencyObject> Find(DependencyObject parent){for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);yield return child;foreach(var x in Find(child))yield return x;}}
 static void Click(Window w,string text){Find(w).OfType<Button>().First(b=>(b.Content as string)==text).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}
 static void Save(Window w,string file){w.UpdateLayout();var bmp=new RenderTargetBitmap((int)w.ActualWidth,(int)w.ActualHeight,96,96,PixelFormats.Pbgra32);bmp.Render(w);var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(bmp));using(var f=File.Create(file))enc.Save(f);}
 static void Assert(bool value,string message){if(!value)throw new Exception(message);}
 [STAThread]static void Main(string[] args){string root=args[0];Directory.CreateDirectory(root);string exe=Path.Combine(root,"ActualGame.exe");File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location,exe,true);string linkPath=Path.Combine(root,"Unrelated shortcut name.lnk");dynamic shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));dynamic link=shell.CreateShortcut(linkPath);link.TargetPath=exe;link.Arguments="--sample \"two words\"";link.WorkingDirectory=root;link.Save();Marshal.FinalReleaseComObject(link);Marshal.FinalReleaseComObject(shell);
  string personal=Path.Combine(root,"personal");string common=Path.Combine(root,"common");Directory.CreateDirectory(personal);Directory.CreateDirectory(common);File.Copy(linkPath,Path.Combine(common,"Public Game.lnk"));File.Copy(linkPath,Path.Combine(personal,"Personal Game.lnk"));var desktopLinks=Dashboard.DesktopShortcuts(personal,common);Assert(desktopLinks.Count==2&&desktopLinks.Any(x=>x.Label=="Public Game")&&desktopLinks.Any(x=>x.Label=="Personal Game"),"personal and public desktop shortcuts are merged");
   Assert(Dashboard.IsLocalExecutablePath(exe)&&!Dashboard.IsLocalExecutablePath(@"\\server\share\game.exe")&&!Dashboard.IsLocalExecutablePath(Path.Combine(root,"malicious.cmd"))&&!Dashboard.IsLocalExecutablePath(@"C:relative.exe")&&!Dashboard.IsLocalExecutablePath("relative.exe"),"only local EXE paths are probed or launched");
   Assert(Dashboard.DisplayLaunchValue("one\r\ntwo\u202e").Contains("\\u000D\\u000A")&&Dashboard.DisplayLaunchValue("one\u202e").Contains("\\u202E"),"launch confirmation escapes control and bidi characters");
   string custom=Path.Combine(root,"private data");Assert(App.AutostartCommand(custom).EndsWith(" --data-dir \""+custom+"\"")&&!App.AutostartCommand(Path.Combine(App.Base,"data")).Contains("--data-dir"),"autostart preserves a custom data directory");
   Assert(App.AutostartCommand(@"C:\").EndsWith(" --data-dir \"C:\\\\\""),"autostart quotes a drive root with trailing slash");
  var app=new App();app.Store=new Store(Path.Combine(root,"data"));app.Store.Config.games.Clear();app.Store.Config.onboardingComplete=true;app.Tracker=new Tracker(app.Store);app.ShutdownMode=ShutdownMode.OnMainWindowClose;L.Set("en");var w=new Dashboard(app){ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.Manual,Left=-20000,Top=0};app.Dashboard=w;int step=0;bool? confirmResult=null;string[] languages={"zh-CN","zh-TW","en","ja"};
  var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};timer.Tick+=(o,e)=>{try{
   if(step<8){int phase=step%2;int i=step/2;step++;if(phase==0){L.Set(languages[i]);w.Dispatcher.BeginInvoke(new Action(()=>w.EditGame(null,linkPath)));}else{var dialog=app.Windows.OfType<Window>().First(x=>x!=w);Find(dialog).OfType<Expander>().Single().IsExpanded=true;dialog.UpdateLayout();Save(dialog,Path.Combine(root,languages[i]+"-add.png"));if(i==3)Click(dialog,L.T("save"));else Click(dialog,L.T("cancel"));}return;}
   if(step==8){step++;Assert(app.Store.Config.games.Count==1,"UI save creates game");var g=app.Store.Config.games[0];Assert(g.executable==exe&&g.arguments=="--sample \"two words\""&&g.workingDirectory==root,"LNK values preserved");Assert(g.processNames.Single()=="ActualGame","shortcut name not used as process name");w.Dispatcher.BeginInvoke(new Action(()=>w.EditGame(g,null)));return;}
   if(step==9){step++;var dialog=app.Windows.OfType<Window>().First(x=>x!=w);Find(dialog).OfType<TextBox>().First().Text="Renamed game";Click(dialog,L.T("save"));return;}
   if(step==10){step++;var game=app.Store.Config.games.Single();w.Dispatcher.BeginInvoke(new Action(()=>{confirmResult=(bool)typeof(Dashboard).GetMethod("ConfirmLaunch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(w,new object[]{game,game.workingDirectory});}));return;}
    if(step==11){step++;var dialog=app.Windows.OfType<Window>().First(x=>x!=w);Assert(Find(dialog).OfType<TextBox>().Any(x=>x.Text.Contains(exe)&&x.IsReadOnly),"launch confirmation shows full read-only path");Click(dialog,L.T("cancel"));return;}
    Assert(confirmResult==false,"canceling launch confirmation does not approve execution");Assert(new Store(app.Store.Root).Config.games.Single().name=="Renamed game","edited name persisted");var observed=new Observed{Path=@"C:\Games\Sample.exe",Title="Sample Window"};Assert(observed.Display.Contains("Sample Window")&&observed.Display.Contains("Sample.exe"),"window picker displays title and path");timer.Stop();File.WriteAllText(Path.Combine(root,"result.txt"),"PASS: unrelated LNK name; actual target/arguments/workdir; personal and public desktop shortcuts merged; visible-window display binding; add and edit persisted; launch confirmation cancellation; four-language expanded forms.");w.Close();
  }catch(Exception ex){timer.Stop();File.WriteAllText(Path.Combine(root,"error.txt"),ex.ToString());Environment.Exit(1);}};timer.Start();app.Run(w);app.Tracker.Dispose();
 }
}
