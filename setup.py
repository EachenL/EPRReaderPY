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

try:
    urllib.request.urlretrieve(sdk_url, sdk_path)
except Exception as e:
    print(e)
    
# network doesn't work
# # 安装.NET SDK
# if current_os == "Windows":
#     subprocess.run(["powershell.exe", sdk_path, "--install-dir", os.getcwd(), "--no-path"])
# else:
#     subprocess.run(["chmod", "+x", sdk_path])
#     subprocess.run(["bash", sdk_path, "--install-dir", os.getcwd(), "--no-path"])

# 发布ASP.NET项目
print("Publishing Server...")
# nuget sources Add -Name "MyServer" -Source \\myserver\packages
try:
    subprocess.run([
        "nuget",
        "source",
        "Add",
        "-Name",
        "\"EasyPathology\"",
        "-Source",
        "http://52.185.191.181:13288/v3/index.json"
    ])
    # nuget sources Disable -Name "MyServer"
    subprocess.run([
        "nuget",
        "sources",
        "Disable",
        "-Name",
        "\"nuget.org\""
    ])
    # nuget sources Enable -Name "nuget.org"
    subprocess.run([
        "nuget",
        "sources",
        "Enable",
        "-Name",
        "\"EasyPathology\""
    ])
    
except:
    raise Exception("lack of nuget")
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
    name="EprReaderPY",
    version="24.0.0",
    author="EachenL && Dear.Va",
    packages=find_packages(where="src"),
    package_dir={"epr_reader": "src/epr_reader", "Server": "src/Server"},
    package_data={"Server": ["bin/Release/net7.0/linux-x64/*"]},
    url="https://github.com/EachenL/EPRReaderPY",
    install_requires=[
        'dataclasses',
        # 你的依赖项
    ],
    # 其他元数据
)