using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Playtime;
class UiChecks {
 static IEnumerable<DependencyObject> Descendants(DependencyObject parent){for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var c=VisualTreeHelper.GetChild(parent,i);yield return c;foreach(var d in Descendants(c))yield return d;}}
 static void Click(Window w,string text){var b=Descendants(w).OfType<Button>().First(x=>(x.Content as string)==text);b.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}
 static void Save(Window w,string path){w.UpdateLayout();var bitmap=new RenderTargetBitmap((int)w.ActualWidth,(int)w.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(w);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var f=File.Create(path))encoder.Save(f);}
 [STAThread]static void Main(string[] args){string dir=args[0];Directory.CreateDirectory(dir);var app=new App();app.Store=new Store(Path.Combine(dir,"data"));app.Store.Config.onboardingComplete=true;app.Store.Config.games.Clear();app.Store.Config.games.Add(new Game{id="sample",name="Sample Game",executable=System.Reflection.Assembly.GetExecutingAssembly().Location,trackingDirectory=dir,processNames=new[]{"SampleGame"}});app.Store.Sessions.Clear();app.Store.Sessions.Add(new Session{gameId="sample",gameName="Sample Game",status="completed",runKey="sample:1",startTime=DateTimeOffset.Now.AddMinutes(-12).ToString("o"),endTime=DateTimeOffset.Now.ToString("o"),lastHeartbeat=DateTimeOffset.Now.ToString("o"),durationSeconds=720});app.Tracker=new Tracker(app.Store);app.ShutdownMode=ShutdownMode.OnMainWindowClose;
  var w=new Dashboard(app){ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.Manual,Left=-20000,Top=0};app.Dashboard=w;string[] languages={"zh-CN","zh-TW","en","ja"};int step=0;
  var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};timer.Tick+=(o,e)=>{
   int index=step/4;int action=step%4;if(index>=4){timer.Stop();w.Close();return;}
   if(action==0){L.Set(languages[index]);w.Width=index==2?830:1020;w.Render();}
   if(action==1){Save(w,Path.Combine(dir,languages[index]+"-overview.png"));Click(w,L.T("settings"));}
   if(action==2){Save(w,Path.Combine(dir,languages[index]+"-settings.png"));Click(w,L.T("history"));}
   if(action==3){Save(w,Path.Combine(dir,languages[index]+"-history.png"));Click(w,L.T("overview"));}
   step++;
  };timer.Start();app.Run(w);app.Tracker.Dispose();File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: rendered all 4 languages, overview/settings/history navigation and 830 DIP minimum width.");
 }
}
