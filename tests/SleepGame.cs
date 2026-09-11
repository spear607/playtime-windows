using System;
using System.Threading;
class SleepGame {static void Main(string[] args){Thread.Sleep(args.Length>0?int.Parse(args[0]):12000);}}
