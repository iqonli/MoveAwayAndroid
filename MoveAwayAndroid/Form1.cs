/*    
MoveAwayAndroid
Copyright (C) 2025 IQ Online Studio

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
USA
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoveAwayAndroid
{
	public partial class Form1 : Form
	{
		public const long ONE_MB_BYTES = 1024 * 1024; // 1048576字节
		public const string TARGET_FOLDER = "$MoveAwayAndroid"; // 目标文件夹名（和原BAT一致）

		public Form1()
		{
			InitializeComponent();
		}

		// ===================== 核心工具方法开始（无修改） =====================
		/// 
		/// <returns>文件夹名列表</returns>
		private List<string> ReadFolderCfg()
		{
			List<string> folderNames = new List<string>();
			// cfg文件路径：程序运行目录下的MoveAwayAndroid.cfg
			string cfgPath = Path.Combine(Application.StartupPath, "MoveAwayAndroid.cfg");

			// 判断cfg文件是否存在
			if (!File.Exists(cfgPath))
			{
				using (File.Create(cfgPath)) { } // 自动创建空文件，using释放文件流
				MessageBox.Show($"未找到配置文件，已自动创建：{cfgPath}{Environment.NewLine}请在文件内一行一个填写文件夹名", "提示", MessageBoxButtons.OK);
				return folderNames;
			}

			// 逐行读取cfg文件
			string[] allLines = File.ReadAllLines(cfgPath, System.Text.Encoding.UTF8);
			foreach (string line in allLines)
			{
				// 过滤空行、仅空格的行
				string folderName = line.Trim();
				if (!string.IsNullOrEmpty(folderName))
				{
					folderNames.Add(folderName);
				}
			}
			return folderNames;
		}

		/// 
		/// <param name="folderPath">文件夹完整路径</param>
		/// <param name="fileCount">统计出的文件总数（输出参数）</param>
		/// <param name="totalSize">统计出的总大小（字节）</param>
		public void CalculateFolderInfo(string folderPath, out long fileCount, out long totalSize)
		{
			// 初始化统计结果
			fileCount = 0;
			totalSize = 0;

			// 判断文件夹是否存在
			if (!Directory.Exists(folderPath))
			{
				return; // 不存在则返回0
			}

			try
			{
				// 1. 统计当前文件夹的文件
				FileInfo[] fileInfos = new DirectoryInfo(folderPath).GetFiles();
				foreach (FileInfo fi in fileInfos)
				{
					fileCount++;
					totalSize += fi.Length;
				}

				// 2. 递归统计子文件夹的文件
				DirectoryInfo[] dirInfos = new DirectoryInfo(folderPath).GetDirectories();
				foreach (DirectoryInfo di in dirInfos)
				{
					long subFileCount;
					long subTotalSize;
					CalculateFolderInfo(di.FullName, out subFileCount, out subTotalSize);
					fileCount += subFileCount;
					totalSize += subTotalSize;
				}
			}
			catch (Exception ex)
			{
				// 捕获权限不足/文件被占用等异常，仅忽略该文件夹
				return;
			}
		}

		/// 
		/// <param name="bytes">原始字节数</param>
		/// <returns>带单位的大小字符串（如1.23KB、45.67MB）</returns>
		public string ConvertBytesToSize(long bytes)
		{
			// 1024进制单位
			string[] units = { "B", "KB", "MB", "GB", "TB" };
			double size = bytes;
			int unitIndex = 0;

			// 循环转换单位，直到大小小于1024或到最大单位
			while (size >= 1024 && unitIndex < units.Length - 1)
			{
				size /= 1024;
				unitIndex++;
			}

			// 保留2位小数，拼接单位
			return $"{size.ToString("0.00")} {units[unitIndex]}";
		}

		/// 
		private void OverwriteMoveDirectory(string sourceDir, string targetDir)
		{
			// 目标存在则先删除（递归删除文件夹及所有内容）
			if (Directory.Exists(targetDir))
			{
				Directory.Delete(targetDir, true);
			}
			Directory.Move(sourceDir, targetDir);
		}

		/// 
		private void OverwriteMoveFile(string sourceFile, string targetFile)
		{
			// 目标存在则先删除
			if (File.Exists(targetFile))
			{
				File.Delete(targetFile);
			}
			File.Move(sourceFile, targetFile);
		}
		// ===================== 核心工具方法结束 =====================

		/// 
		private void InitDataGridView()
		{
			dataGridView1.Rows.Clear();
			List<string> folderNames = ReadFolderCfg();
			if (folderNames.Count == 0) return;

			foreach (string folderName in folderNames)
			{
				string itemPath = Path.Combine(Application.StartupPath, folderName);
				// 过滤不存在的文件/文件夹，不显示在表格
				if (!Directory.Exists(itemPath) && !File.Exists(itemPath))
				{
					continue;
				}

				long fileCount;
				long totalSizeBytes;
				CalculateFolderInfo(itemPath, out fileCount, out totalSizeBytes);
				string sizeStr = ConvertBytesToSize(totalSizeBytes);

				bool isAutoSelect = totalSizeBytes < ONE_MB_BYTES;
				int rowIndex = dataGridView1.Rows.Add(isAutoSelect, folderName, fileCount, sizeStr);
				DataGridViewRow row = dataGridView1.Rows[rowIndex];

				// ≥1MB标红加粗
				if (totalSizeBytes >= ONE_MB_BYTES)
				{
					row.Cells["Column4"].Style.ForeColor = Color.Red;
					row.Cells["Column4"].Style.Font = new Font(dataGridView1.Font, FontStyle.Bold);
				}
			}
			button1.Text = "全选";
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			InitDataGridView();
			dataGridView1.CellContentClick += dataGridView1_CellContentClick;
			// 新增：绑定文件夹名点击事件（打开资源管理器并选中目标）
			dataGridView1.CellClick += dataGridView1_CellClick;
		}

		// 全选/全不选（无修改）
		private void button1_Click(object sender, EventArgs e)
		{
			if (dataGridView1.Rows.Count == 0)
			{
				MessageBox.Show("表格无数据，无需选择！", "提示", MessageBoxButtons.OK);
				return;
			}

			// 判断是否已全选
			bool isAllSelected = true;
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				if (!Convert.ToBoolean(row.Cells["Column1"].Value))
				{
					isAllSelected = false;
					break;
				}
			}

			// 全选/全不选切换
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				row.Cells["Column1"].Value = !isAllSelected;
			}
			button1.Text = isAllSelected ? "全选" : "全不选";
		}

		// 反选（无修改）
		private void button2_Click(object sender, EventArgs e)
		{
			if (dataGridView1.Rows.Count == 0)
			{
				MessageBox.Show("表格无数据，无需反选！", "提示", MessageBoxButtons.OK);
				return;
			}

			// 所有行勾选状态取反
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				bool current = Convert.ToBoolean(row.Cells["Column1"].Value);
				row.Cells["Column1"].Value = !current;
			}

			// 重新判断是否全选，更新button1文本
			bool isAllSelected = true;
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				if (!Convert.ToBoolean(row.Cells["Column1"].Value))
				{
					isAllSelected = false;
					break;
				}
			}
			button1.Text = isAllSelected ? "全不选" : "全选";
		}

		// 多地址超时下载cfg+刷新表格（无修改）
		private async void button3_Click(object sender, EventArgs e)
		{
			// 定义3个下载地址，按优先级排序
			List<string> downloadUrls = new List<string>()
			{
				"https://v6.gh-proxy.org/https://raw.githubusercontent.com/iqonli/MoveAwayAndroid/main/MoveAwayAndroid.cfg",
				"https://gh-proxy.org/https://raw.githubusercontent.com/iqonli/MoveAwayAndroid/main/MoveAwayAndroid.cfg",
				"https://raw.githubusercontent.com/iqonli/MoveAwayAndroid/main/MoveAwayAndroid.cfg"
			};
			string localCfgPath = Path.Combine(Application.StartupPath, "MoveAwayAndroid.cfg");
			bool downloadSuccess = false;

			// 禁用按钮，避免重复点击
			button3.Enabled = false;
			button3.Text = "下载中...";

			try
			{
				// 遍历地址依次下载
				for (int i = 0; i < downloadUrls.Count; i++)
				{
					string currentUrl = downloadUrls[i];
					try
					{
						// 初始化HttpClient，设置超时2秒
						using (HttpClient client = new HttpClient())
						{
							client.Timeout = TimeSpan.FromSeconds(2);
							// 下载文件内容（字节数组，兼容所有编码）
							byte[] cfgContent = await client.GetByteArrayAsync(currentUrl);
							// 覆盖写入本地Cfg文件
							File.WriteAllBytes(localCfgPath, cfgContent);
							downloadSuccess = true;
							MessageBox.Show($"第{i + 1}个地址下载成功！{Environment.NewLine}{currentUrl}", "成功", MessageBoxButtons.OK);
							break; // 下载成功，跳出循环
						}
					}
					catch (Exception ex)
					{
						// 单个地址失败，提示并继续下一个
						string tip = $"第{i + 1}个地址下载失败：{ex.Message}{Environment.NewLine}正在尝试下一个地址...";
						if (i == downloadUrls.Count - 1)
						{
							tip = $"最后一个地址下载失败：{ex.Message}{Environment.NewLine}所有地址均尝试完毕！";
						}
						MessageBox.Show(tip, "提示", MessageBoxButtons.OK);
					}
				}

				// 下载成功则刷新表格，失败则提示
				if (downloadSuccess)
				{
					InitDataGridView();
					MessageBox.Show("cfg文件已更新，表格数据已重新加载！", "完成", MessageBoxButtons.OK);
				}
				else
				{
					MessageBox.Show("所有地址均下载失败，请检查网络或地址有效性！", "失败", MessageBoxButtons.OK);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"下载异常：{ex.Message}", "错误", MessageBoxButtons.OK);
			}
			finally
			{
				// 恢复按钮状态
				button3.Enabled = true;
				button3.Text = "下载配置";
			}
		}

		// 覆盖移动选中项到$MoveAwayAndroid（无修改）
		private void button4_Click(object sender, EventArgs e)
		{
			// 1. 筛选选中的文件夹/文件
			List<string> selectedItems = new List<string>();
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				if (Convert.ToBoolean(row.Cells["Column1"].Value))
				{
					selectedItems.Add(row.Cells["Column2"].Value.ToString());
				}
			}
			if (selectedItems.Count == 0)
			{
				MessageBox.Show("请先勾选需要移动的文件夹/文件！", "提示", MessageBoxButtons.OK);
				return;
			}

			// 2. 确认移动操作
			if (MessageBox.Show($"将覆盖移动{selectedItems.Count}个项到{TARGET_FOLDER}文件夹，是否继续？", "确认", MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return;
			}

			// 3. 初始化目标文件夹（不存在则创建）
			string targetRoot = Path.Combine(Application.StartupPath, TARGET_FOLDER);
			if (!Directory.Exists(targetRoot))
			{
				Directory.CreateDirectory(targetRoot);
			}

			// 4. 遍历选中项，执行覆盖移动，统计结果
			int success = 0, fail = 0;
			List<string> failMsg = new List<string>();
			foreach (string itemName in selectedItems)
			{
				string sourcePath = Path.Combine(Application.StartupPath, itemName);
				string targetPath = Path.Combine(targetRoot, itemName);
				try
				{
					// 跳过不存在的项
					if (!Directory.Exists(sourcePath) && !File.Exists(sourcePath))
					{
						continue;
					}
					// 覆盖移动：区分文件夹/文件
					if (Directory.Exists(sourcePath))
					{
						OverwriteMoveDirectory(sourcePath, targetPath);
					}
					else
					{
						OverwriteMoveFile(sourcePath, targetPath);
					}
					success++;
				}
				catch (Exception ex)
				{
					fail++;
					failMsg.Add($"{itemName}：{ex.Message}");
				}
			}

			// 5. 提示结果，刷新表格
			string result = $"移动完成！{Environment.NewLine}成功：{success}个 | 失败：{fail}个";
			if (fail > 0) result += $"{Environment.NewLine}失败详情：{Environment.NewLine}{string.Join(Environment.NewLine, failMsg)}";
			MessageBox.Show(result, "结果", MessageBoxButtons.OK);
			InitDataGridView(); // 刷新表格（移动后项消失，数据更新）
		}

		// 删除选中项 + 确认删除$MoveAwayAndroid（替换为C#原生MessageBox，无声音）（无修改）
		private void button5_Click(object sender, EventArgs e)
		{
			#region 第一步：删除表格中选中的文件夹/文件（永久删除）
			List<string> selectedItems = new List<string>();
			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				if (Convert.ToBoolean(row.Cells["Column1"].Value))
				{
					selectedItems.Add(row.Cells["Column2"].Value.ToString());
				}
			}

			// 严格确认：永久删除，不可恢复（无声音）
			DialogResult dr = MessageBox.Show($"删除警告{Environment.NewLine}将直接删除{selectedItems.Count}个项，不可恢复。是否继续？", "危险操作", MessageBoxButtons.YesNo, MessageBoxIcon.None);
			if (dr != DialogResult.Yes) return;

			// 执行直接删除，统计结果
			int success = 0, fail = 0;
			List<string> failMsg = new List<string>();
			foreach (string itemName in selectedItems)
			{
				string itemPath = Path.Combine(Application.StartupPath, itemName);
				try
				{
					if (Directory.Exists(itemPath))
					{
						Directory.Delete(itemPath, true); // 递归删除文件夹
					}
					else if (File.Exists(itemPath))
					{
						File.Delete(itemPath); // 删除文件
					}
					success++;
				}
				catch (Exception ex)
				{
					fail++;
					failMsg.Add($"{itemName}：{ex.Message}");
				}
			}

			// 提示第一步删除结果，刷新表格
			string firstResult = $"勾选项删除完成！{Environment.NewLine}成功：{success}个 | 失败：{fail}个";
			if (fail > 0) firstResult += $"{Environment.NewLine}失败详情：{Environment.NewLine}{string.Join(Environment.NewLine, failMsg)}";
			MessageBox.Show(firstResult, "勾选项删除结果", MessageBoxButtons.OK);
			InitDataGridView();
			#endregion

			#region 第二步：C#原生MessageBox确认删除$MoveAwayAndroid（无声音）
			string targetRoot = Path.Combine(Application.StartupPath, TARGET_FOLDER);
			if (!Directory.Exists(targetRoot))
			{
				MessageBox.Show($"{TARGET_FOLDER}文件夹不存在！", "提示", MessageBoxButtons.OK);
				return;
			}

			// 计算$MoveAwayAndroid的总大小和文件数
			long dirFileCount, dirTotalSize;
			CalculateFolderInfo(targetRoot, out dirFileCount, out dirTotalSize);
			string dirSizeStr = ConvertBytesToSize(dirTotalSize);

			// 核心替换：C#原生MessageBox，YesNo按钮+None图标（无声音）
			string msgText = $"是否删除{TARGET_FOLDER}文件夹？{Environment.NewLine}大小：{dirSizeStr}{Environment.NewLine}文件数：{dirFileCount}个{Environment.NewLine}删除后不可恢复！";
			DialogResult result = MessageBox.Show(msgText, "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.None);

			// 判断返回值（Yes/No），逻辑和原WinAPI一致
			if (result == DialogResult.Yes)
			{
				try
				{
					Directory.Delete(targetRoot, true);
					MessageBox.Show($"{TARGET_FOLDER}文件夹已成功删除！", "成功", MessageBoxButtons.OK);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"删除{TARGET_FOLDER}失败：{ex.Message}", "失败", MessageBoxButtons.OK);
				}
			}
			else
			{
				MessageBox.Show($"已取消删除{TARGET_FOLDER}文件夹！", "提示", MessageBoxButtons.OK);
			}
			#endregion
		}

		// Checkbox列点击事件（无修改）
		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// 1. 只处理Checkbox列（Column1）的点击，点击其他列直接返回，避免无效检查
			if (e.ColumnIndex != dataGridView1.Columns["Column1"].Index || e.RowIndex < 0)
			{
				return;
			}

			// 2. 定义全选状态标记
			bool isAllSelected = true;

			// 3. 表格无数据时，直接设为未全选状态
			if (dataGridView1.Rows.Count == 0)
			{
				isAllSelected = false;
			}
			else
			{
				// 4. 遍历所有行，检查Checkbox是否全选（处理单元格值为null的情况，避免空引用异常）
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					// 单元格值可能为DBNull，先判断是否为null，再转换为bool
					bool isChecked = row.Cells["Column1"].Value != null && Convert.ToBoolean(row.Cells["Column1"].Value);
					if (!isChecked)
					{
						isAllSelected = false;
						break; // 只要有一个未选中，直接跳出循环，无需继续检查
					}
				}
			}

			// 5. 同步更新button1的文本，和原有逻辑完全一致
			button1.Text = isAllSelected ? "全选" : "全不选";
		}

		// ===================== 新增：文件夹名点击事件 =====================
		/// 
		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			// 1. 仅处理文件夹名列（Column2）的点击，排除标题行和其他列
			if (e.ColumnIndex != dataGridView1.Columns["Column2"].Index || e.RowIndex < 0)
			{
				return;
			}

			// 2. 获取当前点击行的文件夹名（避免空值异常）
			string folderName = dataGridView1.Rows[e.RowIndex].Cells["Column2"].Value?.ToString();
			if (string.IsNullOrEmpty(folderName))
			{
				MessageBox.Show("文件夹名为空，无法定位目标！", "提示", MessageBoxButtons.OK);
				return;
			}

			// 3. 拼接目标文件/文件夹的完整路径（程序运行目录=OTG根目录）
			string targetPath = Path.Combine(Application.StartupPath, folderName);

			// 4. 判断目标是否存在，避免打开无效路径
			if (!Directory.Exists(targetPath) && !File.Exists(targetPath))
			{
				MessageBox.Show($"目标不存在：{targetPath}", "提示", MessageBoxButtons.OK);
				return;
			}

			// 5. 打开资源管理器并选中目标（/select 参数用于精准选中）
			try
			{
				Process.Start(new ProcessStartInfo()
				{
					FileName = "explorer.exe",
					Arguments = $"/select,\"{targetPath}\"", // 双引号包裹路径，兼容含空格的文件名
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				// 捕获异常（如资源管理器无法启动），提示用户
				MessageBox.Show($"打开资源管理器失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// 链接标签点击事件（无修改）
		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				// 2. 定义要打开的GitHub地址
				string githubUrl = "https://github.com/iqonli/MoveAwayAndroid";
				// 3. 启动系统默认浏览器打开地址（跨.NET版本兼容）
				Process.Start(new ProcessStartInfo(githubUrl)
				{
					UseShellExecute = true // 关键：确保调用默认浏览器
				});
				// 4. 核心新增：还原链接颜色为 RGB(0,0,255) 纯蓝色
				linkLabel1.LinkColor = Color.FromArgb(0, 0, 255);
			}
			catch (Exception ex)
			{
				// 打开失败时弹窗提示，避免程序崩溃
				MessageBox.Show($"打开链接失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}