using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms=System.Windows.Forms;

namespace Playtime {
 public class App : Application {
  public Store Store;public Tracker Tracker;public Dashboard Dashboard;
  Forms.NotifyIcon tray;Mutex mutex;EventWaitHandle activate;RegisteredWaitHandle registered;bool exiting;
  public static string Base=AppDomain.CurrentDomain.BaseDirectory;
  [STAThread] public static void Main(string[] args){
   try{var app=new App();app.RunApp(args);}catch(Exception ex){MessageBox.Show(ex.Message,"Playtime",MessageBoxButton.OK,MessageBoxImage.Error);}
  }
  void RunApp(string[] args){
   ShutdownMode=ShutdownMode.OnExplicitShutdown;
   string dataRoot=Path.Combine(Base,"data");int ix=Array.IndexOf(args,"--data-dir");if(ix>=0&&ix+1<args.Length)dataRoot=Path.GetFullPath(args[ix+1]);
   dataRoot=Path.GetFullPath(dataRoot);if(dataRoot.Length>Path.GetPathRoot(dataRoot).Length)dataRoot=dataRoot.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
   string token;using(var sha=System.Security.Cryptography.SHA256.Create())token=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dataRoot.ToLowerInvariant()))).Replace("-","").Substring(0,24);
   bool first;mutex=new Mutex(true,"Local\\Playtime-"+token,out first);
   if(!first){if(Array.IndexOf(args,"--background")<0)try{using(var ev=EventWaitHandle.OpenExisting("Local\\Playtime-Show-"+token))ev.Set();}catch{}mutex.Dispose();return;}
   activate=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\Playtime-Show-"+token);
   Store=new Store(dataRoot);L.Set(Store.Config.language);Tracker=new Tracker(Store);Tracker.Start();
   DispatcherUnhandledException+=(o,e)=>{Store.Log(e.Exception);e.Handled=true;MessageBox.Show(L.T("error")+"\n"+e.Exception.Message,L.T("title"));};
   tray=new Forms.NotifyIcon();tray.Icon=System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);tray.Visible=true;tray.DoubleClick+=(o,e)=>ShowDashboard();RefreshTray();
   registered=ThreadPool.RegisterWaitForSingleObject(activate,(o,t)=>Dispatcher.BeginInvoke(new Action(ShowDashboard)),null,-1,false);
   SessionEnding+=(o,e)=>Quit();Exit+=(o,e)=>Cleanup();
   if(Array.IndexOf(args,"--background")<0)ShowDashboard();
   Run();
  }
  public void ShowDashboard(){if(!Dispatcher.CheckAccess()){Dispatcher.BeginInvoke(new Action(ShowDashboard));return;}if(Dashboard==null){Dashboard=new Dashboard(this);Dashboard.Closed+=(o,e)=>{Dashboard=null;GC.Collect();};Dashboard.Show();}if(Dashboard.WindowState==WindowState.Minimized)Dashboard.WindowState=WindowState.Normal;Dashboard.Activate();}
  public void RefreshTray(){if(tray==null)return;tray.Text=L.T("title")+" · "+L.T(Tracker.Paused?"paused":"tracking");var old=tray.ContextMenuStrip;var menu=new Forms.ContextMenuStrip();menu.Items.Add(L.T("open"),null,(o,e)=>ShowDashboard());menu.Items.Add(L.T(Tracker.Paused?"resume":"pause"),null,(o,e)=>{Tracker.SetPaused(!Tracker.Paused);RefreshTray();if(Dashboard!=null)Dashboard.Render();});menu.Items.Add(new Forms.ToolStripSeparator());menu.Items.Add(L.T("exit"),null,(o,e)=>Quit());tray.ContextMenuStrip=menu;if(old!=null)old.Dispose();}
  public void Quit(){if(exiting)return;exiting=true;Cleanup();Shutdown();}
  void Cleanup(){if(tray!=null){tray.Visible=false;tray.Dispose();tray=null;}if(registered!=null){registered.Unregister(null);registered=null;}if(Tracker!=null){try{Tracker.Dispose();}catch(Exception ex){Store.Log(ex);}Tracker=null;}if(activate!=null){activate.Dispose();activate=null;}if(mutex!=null){try{mutex.ReleaseMutex();}catch{}mutex.Dispose();mutex=null;}}
  public static bool Autostart {get{using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")){return key!=null&&string.Equals(key.GetValue("PlaytimeNext") as string,"\""+Path.Combine(Base,"Playtime.exe")+"\" --background",StringComparison.OrdinalIgnoreCase);}}}
  public static void SetAutostart(bool enabled){using(var key=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")){if(enabled)key.SetValue("PlaytimeNext","\""+Path.Combine(Base,"Playtime.exe")+"\" --background");else key.DeleteValue("PlaytimeNext",false);}}
 }
}
