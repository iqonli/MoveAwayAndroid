@echo off
echo by Alone, 2024.11.23 QQ:34067513
echo 本脚本用于移走Android自动生成在OTG设备（如U盘）内的文件/文件夹，你可以修改脚本来控制操作的文件夹和目标文件夹。
echo 请确认本脚本在OTG设备的根目录下运行，
pause

move .\.android_secure .\$MoveAwayAndroid
move .\.DataStorage .\$MoveAwayAndroid
move .\.gs_file .\$MoveAwayAndroid
move .\.gs_fs0 .\$MoveAwayAndroid
move .\.nomedia .\$MoveAwayAndroid
move .\.recycle .\$MoveAwayAndroid
move .\.UTSystemConfig .\$MoveAwayAndroid
move .\Alarms .\$MoveAwayAndroid
move .\Android .\$MoveAwayAndroid
move .\Audiobooks .\$MoveAwayAndroid
move .\DCIM .\$MoveAwayAndroid
move .\Documents .\$MoveAwayAndroid
move .\Download .\$MoveAwayAndroid
move .\ktcp_video .\$MoveAwayAndroid
move .\LOST.DIR .\$MoveAwayAndroid
move .\Movies .\$MoveAwayAndroid
move .\msc .\$MoveAwayAndroid
move .\Music .\$MoveAwayAndroid
move .\Notifications .\$MoveAwayAndroid
move .\Pictures .\$MoveAwayAndroid
move .\Podcasts .\$MoveAwayAndroid
move .\Recordings .\$MoveAwayAndroid
move .\Ringtones .\$MoveAwayAndroid
move .\Sounds .\$MoveAwayAndroid
move .\tad .\$MoveAwayAndroid
move .\tencent .\$MoveAwayAndroid

echo 已经移动成功。如果你想删除$MoveAwayAndroid文件夹，
pause

echo 即将列出$MoveAwayAndroid文件夹目录，请自己检查目录内文件，和文件夹大小是否正常，如果过大说明里面可能有你自己的文件，而非全是Android自动生成的文件。
pause

tree .\$MoveAwayAndroid /f

echo 检查无误，确认删除，
pause

rd /s .\$MoveAwayAndroid
