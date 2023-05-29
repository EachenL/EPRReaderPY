## Server
Server由ASP.NET编写，可以在本地开启一个HTTP服务器，供RPC调用

Server引用了EasyPathology的一些程序包，我上传到了私人nuget服务器，需要添加以下的源

```
http://52.185.191.181:13288/v3/index.json
```
如果在执行setup.py过程中出现呢错误 请根据提示手动操作

源随时可能变动，注意关注。

## 已开放的Routers
可使用SwaggerUI查看所有Routers，Debug模式运行后访问`http://localhost:6082/swagger/index.html`即可

## 安装
在项目目录下执行
```
python setup.py install
```
以开发形式执行

```
python setup.py develop
```