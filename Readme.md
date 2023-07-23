## 组成
本项目由两部分组成：C#程序和Python程序。由于EPR读取的文件格式为C#的dll，因此需要使用C#编写的程序来读取EPR文件，然后将读取到的数据传输给Python程序，由Python程序进行处理。

## Server
Server由C#编写，用于读取EPR文件，然后将读取到的数据传输给Python程序。

数据传输时，使用gRPC，确保数据以二进制形式传输，减少传输时间。

## 安装
在项目目录下执行
```
python setup.py install
```
以开发形式执行

```
python setup.py develop
```