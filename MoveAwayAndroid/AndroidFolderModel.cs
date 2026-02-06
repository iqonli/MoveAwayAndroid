using System;

namespace MoveAwayAndroid
{
	/// <summary>
	/// 安卓文件夹信息实体类（对应表格每一行）
	/// </summary>
	public class AndroidFolderModel
	{
		// 选择状态（对应CheckBox列）
		public bool IsSelected { get; set; }
		// 文件夹名称（对应文件夹列）
		public string FolderName { get; set; }
		// 递归统计的文件总数（对应文件数列）
		public long FileCount { get; set; }
		// 文件夹总大小（原始字节数，用于单位转换，对应大小列）
		public long TotalSizeBytes { get; set; }
	}
}