import os
import platform
import subprocess
import urllib.request
from grpc_tools import protoc
from setuptools import setup, find_packages


def compile_proto(path: str):
    protoc.main([
        './src/protobuf/' + path,
        '-I', './src/protobuf',
        '--python_out', './src/python_epr/binding',
        '--grpc_python_out', './src/python_epr/binding'
    ])


print("Compiling Protobuf...")
compile_proto('*.proto')
compile_proto('additional_infos/*.proto')
compile_proto('data_types/*.proto')
compile_proto('frame_states/*.proto')
compile_proto('grpc/*.proto')

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
    name="PythonEpr",
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
