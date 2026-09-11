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
  var app=new App();app.Store=new Store(Path.Combine(root,"data"));app.Store.Config.games.Clear();app.Store.Config.onboardingComplete=true;app.Tracker=new Tracker(app.Store);app.ShutdownMode=ShutdownMode.OnMainWindowClose;L.Set("en");var w=new Dashboard(app){ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.Manual,Left=-20000,Top=0};app.Dashboard=w;int step=0;string[] languages={"zh-CN","zh-TW","en","ja"};
  var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};timer.Tick+=(o,e)=>{try{
   if(step<8){int phase=step%2;int i=step/2;step++;if(phase==0){L.Set(languages[i]);w.Dispatcher.BeginInvoke(new Action(()=>w.EditGame(null,linkPath)));}else{var dialog=app.Windows.OfType<Window>().First(x=>x!=w);Find(dialog).OfType<Expander>().Single().IsExpanded=true;dialog.UpdateLayout();Save(dialog,Path.Combine(root,languages[i]+"-add.png"));if(i==3)Click(dialog,L.T("save"));else Click(dialog,L.T("cancel"));}return;}
   if(step==8){step++;Assert(app.Store.Config.games.Count==1,"UI save creates game");var g=app.Store.Config.games[0];Assert(g.executable==exe&&g.arguments=="--sample \"two words\""&&g.workingDirectory==root,"LNK values preserved");Assert(g.processNames.Single()=="ActualGame","shortcut name not used as process name");w.Dispatcher.BeginInvoke(new Action(()=>w.EditGame(g,null)));return;}
   if(step==9){step++;var dialog=app.Windows.OfType<Window>().First(x=>x!=w);Find(dialog).OfType<TextBox>().First().Text="Renamed game";Click(dialog,L.T("save"));return;}
   Assert(new Store(app.Store.Root).Config.games.Single().name=="Renamed game","edited name persisted");Assert(typeof(Observed).GetProperty("Path")!=null,"process picker supports WPF property binding");timer.Stop();File.WriteAllText(Path.Combine(root,"result.txt"),"PASS: unrelated LNK name; actual target/arguments/workdir; add and edit persisted; four-language expanded forms; process picker binding.");w.Close();
  }catch(Exception ex){timer.Stop();File.WriteAllText(Path.Combine(root,"error.txt"),ex.ToString());Environment.Exit(1);}};timer.Start();app.Run(w);app.Tracker.Dispose();
 }
}
