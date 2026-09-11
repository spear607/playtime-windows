using System;
using System.Collections.Generic;
using System.Globalization;
namespace Playtime {
 public static class L {
  public static string Language="zh-CN";
  static readonly Dictionary<string,string[]> Text=new Dictionary<string,string[]> {
   {"title",new[]{"时长统计","時長統計","Playtime","プレイ時間"}},
   {"overview",new[]{"总览","總覽","Overview","概要"}}, {"history",new[]{"记录","記錄","History","履歴"}}, {"settings",new[]{"设置","設定","Settings","設定"}},
   {"total",new[]{"累计游玩","累計遊玩","Total playtime","合計プレイ時間"}}, {"runs",new[]{"游玩次数","遊玩次數","Play sessions","プレイ回数"}}, {"games",new[]{"游戏","遊戲","Games","ゲーム"}},
   {"mygames",new[]{"我的游戏","我的遊戲","My games","ゲーム一覧"}}, {"add",new[]{"添加游戏","新增遊戲","Add game","ゲームを追加"}},
   {"empty",new[]{"添加第一个游戏","新增第一個遊戲","Add your first game","最初のゲームを追加"}},
   {"drop",new[]{"拖入快捷方式或 EXE，也可以点击添加","拖入捷徑或 EXE，也可以點擊新增","Drop a shortcut or EXE, or choose Add game","ショートカットや EXE をドロップして追加"}},
   {"nohistory",new[]{"暂无记录","暫無記錄","No sessions yet","履歴はまだありません"}},
   {"help",new[]{"使用说明","使用說明","Help","使い方"}}, {"edit",new[]{"编辑","編輯","Edit","編集"}}, {"start",new[]{"启动","啟動","Play","起動"}},
   {"remove",new[]{"移除游戏","移除遊戲","Remove game","ゲームを削除"}}, {"removeConfirm",new[]{"移除此游戏？已有记录会保留。","移除此遊戲？現有記錄會保留。","Remove this game? Existing sessions will be kept.","このゲームを削除しますか？履歴は残ります。"}},
   {"save",new[]{"保存","儲存","Save","保存"}}, {"cancel",new[]{"取消","取消","Cancel","キャンセル"}},
   {"name",new[]{"游戏名称","遊戲名稱","Game name","ゲーム名"}}, {"file",new[]{"启动文件","啟動檔案","Launch file","起動ファイル"}},
   {"browse",new[]{"选择文件","選擇檔案","Browse","参照"}}, {"advanced",new[]{"高级设置","進階設定","Advanced","詳細設定"}},
   {"args",new[]{"启动参数","啟動參數","Arguments","起動引数"}}, {"workdir",new[]{"工作目录","工作目錄","Working directory","作業フォルダー"}},
   {"process",new[]{"游戏进程名（多个用逗号分隔）","遊戲處理程序名稱（以逗號分隔）","Game processes (comma separated)","ゲームのプロセス名（カンマ区切り）"}},
   {"directory",new[]{"游戏所在目录（包含子目录）","遊戲所在目錄（包含子目錄）","Game folder (including subfolders)","ゲームのフォルダー（サブフォルダーを含む）"}},
   {"chooseRunning",new[]{"从运行中的程序选择","從執行中的程式選擇","Choose a running process","実行中のプロセスから選択"}},
   {"enabled",new[]{"自动统计此游戏","自動統計此遊戲","Track this game automatically","このゲームを自動計測"}},
   {"language",new[]{"语言","語言","Language","言語"}}, {"system",new[]{"跟随系统","跟隨系統","System default","システムに合わせる"}},
   {"autostart",new[]{"登录 Windows 时启动","登入 Windows 時啟動","Start when I sign in to Windows","Windows サインイン時に起動"}},
   {"pause",new[]{"暂停统计","暫停統計","Pause tracking","計測を一時停止"}}, {"resume",new[]{"继续统计","繼續統計","Resume tracking","計測を再開"}},
   {"tracking",new[]{"自动统计已开启","自動統計已開啟","Tracking enabled","自動計測中"}}, {"paused",new[]{"已暂停","已暫停","Paused","一時停止中"}},
   {"open",new[]{"打开统计","開啟統計","Open Playtime","開く"}}, {"exit",new[]{"退出","結束","Quit","終了"}},
   {"backup",new[]{"导出备份","匯出備份","Export backup","バックアップを書き出す"}}, {"restore",new[]{"导入备份","匯入備份","Import backup","バックアップを読み込む"}},
   {"legacy",new[]{"导入旧版数据","匯入舊版資料","Import legacy data","旧バージョンのデータを読み込む"}},
   {"datafolder",new[]{"打开数据文件夹","開啟資料夾","Open data folder","データフォルダーを開く"}},
   {"data",new[]{"数据","資料","Data","データ"}}, {"behavior",new[]{"后台运行","背景執行","Background tracking","バックグラウンド計測"}},
   {"trayInfo",new[]{"关闭窗口后继续在托盘统计；从托盘菜单退出。","關閉視窗後繼續在系統匣統計；從系統匣選單結束。","Closing the window keeps tracking in the tray. Use Quit to stop.","ウィンドウを閉じても計測を続けます。終了するにはトレイメニューを使います。"}},
   {"welcome",new[]{"自动统计已添加的游戏","自動統計已新增的遊戲","Track your added games automatically","追加したゲームを自動計測"}},
   {"welcomeBody",new[]{"以后照常打开游戏即可。关闭此窗口后，程序会留在系统托盘；退出程序会停止统计。","之後照常開啟遊戲即可。關閉此視窗後，程式會留在系統匣；結束程式會停止統計。","Launch games as usual. The app stays in the system tray when you close its window. Quitting stops tracking.","いつも通りにゲームを起動してください。ウィンドウを閉じるとトレイに常駐し、アプリを終了すると計測も停止します。"}},
   {"continue",new[]{"开始使用","開始使用","Get started","はじめる"}},
   {"completed",new[]{"已完成","已完成","Completed","完了"}}, {"running",new[]{"正在游玩","正在遊玩","Playing","プレイ中"}},
   {"interrupted",new[]{"异常中断","異常中斷","Interrupted","中断"}}, {"sleep",new[]{"睡眠暂停","睡眠暫停","Sleep break","スリープ"}},
   {"stopped",new[]{"程序退出","程式結束","Tracker closed","アプリ終了"}}, {"manual",new[]{"手动补录","手動補登","Manual entry","手動記録"}}, {"failed",new[]{"启动失败","啟動失敗","Launch failed","起動失敗"}},
   {"hours",new[]{"小时","小時","hours","時間"}}, {"minutes",new[]{"分钟","分鐘","minutes","分"}},
   {"delete",new[]{"删除记录","刪除記錄","Delete session","履歴を削除"}},
   {"deleteConfirm",new[]{"删除这条记录？文件会移到数据目录的 trash 文件夹。","刪除此記錄？檔案會移至資料目錄的 trash 資料夾。","Delete this session? Its file will be moved to the data folder's trash directory.","この履歴を削除しますか？ファイルはデータフォルダーの trash に移動します。"}},
   {"manualHint",new[]{"补录时长（分钟）","補登時長（分鐘）","Add playtime (minutes)","プレイ時間を追加（分）"}},
   {"manualNote",new[]{"此记录标记为补录，不增加启动次数。","此記錄標示為補登，不增加啟動次數。","Marked as manual; does not increase play count.","手動記録として追加し、プレイ回数には含めません。"}},
   {"recent",new[]{"最近","最近","Last played","前回"}}, {"never",new[]{"尚未游玩","尚未遊玩","Not played yet","未プレイ"}},
   {"invalid",new[]{"请检查名称、启动文件、进程名和目录。","請檢查名稱、啟動檔案、處理程序名稱與目錄。","Check the name, launch file, process names and folder.","名前、起動ファイル、プロセス名、フォルダーを確認してください。"}},
   {"duplicate",new[]{"这个游戏已添加，可在游戏卡片中编辑。","此遊戲已新增，可在遊戲卡片中編輯。","This game is already added. Edit its existing card.","このゲームは登録済みです。既存のカードから編集してください。"}},
   {"error",new[]{"操作失败","操作失敗","Operation failed","操作に失敗しました"}},
   {"imported",new[]{"导入完成，重复记录已跳过。","匯入完成，已略過重複記錄。","Import complete. Duplicate sessions were skipped.","読み込みが完了しました。重複する履歴はスキップしました。"}},
   {"exported",new[]{"备份已保存。","備份已儲存。","Backup saved.","バックアップを保存しました。"}},
   {"damaged",new[]{"部分记录无法读取，原文件已保留。请检查数据文件夹。","部分記錄無法讀取，原檔案已保留。請檢查資料夾。","Some records could not be read. Original files were kept. Check your data folder.","一部の履歴を読み込めません。元ファイルは保持されています。データフォルダーを確認してください。"}},
   {"trackingError",new[]{"统计遇到错误，已暂停。请检查数据目录中的日志。","統計發生錯誤，已暫停。請檢查資料目錄中的日誌。","Tracking stopped after an error. Check the log in your data folder.","エラーにより計測を停止しました。データフォルダーのログを確認してください。"}},
   {"notFound",new[]{"启动文件不存在，请编辑游戏路径。","啟動檔案不存在，請編輯遊戲路徑。","Launch file not found. Edit the game's path.","起動ファイルが見つかりません。パスを編集してください。"}},
   {"importFirst",new[]{"选择旧版程序所在的文件夹。","選擇舊版程式所在的資料夾。","Select the folder containing the legacy app.","旧バージョンのアプリがあるフォルダーを選択してください。"}},
   {"startPick",new[]{"先正常启动游戏，再选择实际的游戏进程。","先正常啟動遊戲，再選擇實際的遊戲處理程序。","Start the game normally, then select its actual process.","ゲームを起動してから、実際のゲームプロセスを選択してください。"}},
   {"refresh",new[]{"刷新","重新整理","Refresh","更新"}},
   {"privacy",new[]{"仅记录添加的游戏；没有遥测或联网功能。","僅記錄新增的遊戲；沒有遙測或連線功能。","Only added games are recorded. No telemetry or network features.","追加したゲームだけを記録します。テレメトリや通信機能はありません。"}}
  };
  public static void Set(string code){if(code=="system"||string.IsNullOrEmpty(code)){string c=CultureInfo.CurrentUICulture.Name;Language=c.StartsWith("ja")?"ja":c=="zh-TW"||c=="zh-HK"||c=="zh-Hant"?"zh-TW":c.StartsWith("zh")?"zh-CN":"en";}else Language=code;}
  public static string T(string key){int i=Language=="zh-TW"?1:Language=="en"?2:Language=="ja"?3:0;string[] s;return Text.TryGetValue(key,out s)?s[i]:key;}
  public static string Duration(double seconds){long s=(long)Math.Round(seconds);if(Language=="en")return s>=3600?s/3600+"h "+s%3600/60+"m":s>=60?s/60+"m "+s%60+"s":s+"s";string h=Language=="ja"?"時間":Language=="zh-TW"?"小時":"小时";return s>=3600?s/3600+" "+h+" "+s%3600/60+" 分":s>=60?s/60+" 分 "+s%60+" 秒":s+" 秒";}
  public static string Date(string raw){DateTimeOffset d;return DateTimeOffset.TryParse(raw,out d)?d.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"):T("never");}
  public static IEnumerable<string> Keys {get{return Text.Keys;}}
 }
}
