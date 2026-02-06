#include <iostream>
#include <string>
#include <vector>
#include <cstdlib>
#include <ctime>
#include <fstream>
#include <windows.h>
#include <random>
#include <algorithm>  // 修复std::find的核心：添加算法头文件
#include <cstdio>     // 辅助获取运行目录

using namespace std;

// 安卓目录列表（字典序，30个）
const vector<string> androidFolders = {
	".android_secure", ".DataStorage", ".gs_fs0", ".gs_file", ".MediaCenter",
	".nomedia", ".recycle", ".UTSystemConfig", "Alarms", "Android",
	"Audiobooks", "AudiocnKalaok_TV", "DCIM", "Documents", "Download",
	"Huawei", "HuaweiSystem", "ktcp_video", "LOST.DIR", "Movies",
	"msc", "Music", "Notifications", "Pictures", "Podcasts",
	"Recordings", "Ringtones", "Sounds", "tad", "tencent"
};

// 生成随机字符串（用于文件名，8位）
string getRandomFileName(int len = 8) {
	string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	string fileName;
	random_device rd;
	mt19937 gen(rd());
	uniform_int_distribution<> dis(0, (int)chars.size() - 1);
	for (int i = 0; i < len; i++) {
		fileName += chars[dis(gen)];
	}
	return fileName + ".bin"; // 二进制文件后缀
}

// 生成随机二进制内容（指定大小）
vector<char> getRandomBinaryContent(long long size) {
	vector<char> content(size);
	random_device rd;
	mt19937 gen(rd());
	uniform_int_distribution<> dis(0, 255); // 0-255随机二进制值
	for (long long i = 0; i < size; i++) {
		content[i] = static_cast<char>(dis(gen));
	}
	return content;
}

// 创建目录（Windows API，兼容MinGW）
bool createFolder(const string& folderPath) {
	if (CreateDirectoryA(folderPath.c_str(), NULL)) {
		cout << "创建目录成功：" << folderPath << endl;
		return true;
	}
	else {
		// 目录已存在也返回成功，避免重复创建报错
		if (GetLastError() == ERROR_ALREADY_EXISTS) {
			cout << "目录已存在：" << folderPath << endl;
			return true;
		}
		cout << "创建目录失败：" << folderPath << " 错误码：" << GetLastError() << endl;
		return false;
	}
}

// 生成随机文件大小（1B ~ 2MB）
long long getRandomFileSize() {
	random_device rd;
	mt19937 gen(rd());
	// 1B ~ 2*1024*1024B（2MB），适配MinGW的类型推导
	uniform_int_distribution<long long> dis(1, 2 * 1024 * 1024);
	return dis(gen);
}

int main() {
	setlocale(LC_ALL, "chs"); // 修复MinGW中文显示
	srand((unsigned int)time(NULL));
	cout << "===== 安卓测试目录生成器 =====" << endl;
	
	// 修复：正确获取程序运行目录（解决GetCurrentDirectoryA参数错误）
	char szPath[1024] = {0};
	GetCurrentDirectoryA(1024, szPath); // 第一个参数传缓冲区大小，第二个传缓冲区
	cout << "程序运行目录：" << szPath << endl;
	cout << "开始生成安卓目录..." << endl << endl;
	
	// 第一步：生成所有安卓目录
	for (const auto& folder : androidFolders) {
		createFolder(folder);
	}
	
	cout << endl << "开始随机生成二进制测试文件..." << endl << endl;
	
	// 第二步：随机选择5-10个目录生成文件（优化随机数生成，避免冗余）
	random_device rd;
	mt19937 gen(rd());
	int selectCount = uniform_int_distribution<>(5, 10)(gen); // 选5-10个目录
	vector<int> selectedIndexes; // 已选目录索引，避免重复
	
	// 随机选不重复的目录索引（现在std::find可正常使用）
	while (selectedIndexes.size() < selectCount) {
		int idx = uniform_int_distribution<>(0, (int)androidFolders.size() - 1)(gen);
		// 查找是否已选，无报错
		if (find(selectedIndexes.begin(), selectedIndexes.end(), idx) == selectedIndexes.end()) {
			selectedIndexes.push_back(idx);
		}
	}
	
	// 为选中的目录生成随机二进制文件
	for (int idx : selectedIndexes) {
		string folderName = androidFolders[idx];
		int fileCount = uniform_int_distribution<>(1, 5)(gen); // 每个目录1-5个文件
		cout << "【" << folderName << "】生成" << fileCount << "个二进制文件：" << endl;
		
		for (int i = 0; i < fileCount; i++) {
			string fileName = getRandomFileName();
			string filePath = folderName + "\\" + fileName;
			long long fileSize = getRandomFileSize();
			vector<char> fileContent = getRandomBinaryContent(fileSize);
			
			// 写入二进制文件（兼容MinGW文件操作）
			ofstream fout(filePath, ios::binary | ios::out);
			if (fout.is_open()) {
				fout.write(fileContent.data(), (streamsize)fileContent.size());
				fout.close();
				cout << "  生成文件：" << fileName << " 大小：" << fileSize << "B" << endl;
			}
			else {
				cout << "  生成文件失败：" << fileName << endl;
			}
		}
		cout << endl;
	}
	
	cout << "===== 生成完成！=====" << endl;
	cout << "共生成" << androidFolders.size() << "个安卓目录，"
	<< selectCount << "个目录包含测试文件，可用于测试C#程序。" << endl;
	system("pause"); // 暂停窗口，查看结果
	return 0;
}
