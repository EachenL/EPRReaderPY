import os
import platform
import subprocess
import urllib.request

from setuptools import setup, find_packages

# 获取当前系统
current_os = platform.system()

# 下载最新版本的.NET SDK
print("Installing dotnet...")
if current_os == "Windows":
    sdk_url = "https://dot.net/v1/dotnet-install.ps1"
    sdk_path = os.path.join(os.getcwd(), "dotnet-install.ps1")
else:
    sdk_url = "https://dot.net/v1/dotnet-install.sh"
    sdk_path = os.path.join(os.getcwd(), "dotnet-install.sh")

urllib.request.urlretrieve(sdk_url, sdk_path)

# 安装.NET SDK
if current_os == "Windows":
    subprocess.run(["powershell.exe", sdk_path, "--install-dir", os.getcwd(), "--no-path"])
else:
    subprocess.run(["chmod", "+x", sdk_path])
    subprocess.run([sdk_path, "--install-dir", os.getcwd(), "--no-path"])

# 发布ASP.NET项目
print("Publishing Server...")
subprocess.run([
    "dotnet", 
    "publish", 
    "src/Server/Server.csproj", 
    "-c", 
    "Release", 
    "-r", 
    "win-x64" if current_os == "Windows" else "linux-x64"
])

# 设置包的元数据
setup(
    name="epr_reader",
    version="24.0.0",
    packages=find_packages("src"),
    package_dir={"": "src"},
    install_requires=[
        # 你的依赖项
    ],
    # 其他元数据
)