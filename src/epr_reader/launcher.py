import atexit
import subprocess

"""
用于启动一个子进程，并在当前进程退出时退出子进程
by ChatGPT
"""
class Launcher:
    def __init__(self, command):
        # 子进程的命令
        self.command = command
        # 初始化时还没有启动子进程，所以process为None
        self.process = None

    def run(self):
        # 使用subprocess模块启动子进程，保存子进程对象到self.process中
        # 创建进程时指定creationflags参数，以隐藏子进程窗口
        self.process = subprocess.Popen(self.command, creationflags=subprocess.CREATE_NO_WINDOW)
        # 注册cleanup方法到atexit模块中，确保Python进程退出时，该方法会被自动调用
        atexit.register(self.cleanup)

    def kill(self):
        # 如果子进程已经启动，则终结子进程并等待其退出
        if self.process:
            self.process.terminate()
            self.process.wait()

if __name__ == '__main__':
    # 创建Launcher对象
    launcher = Launcher('Server.exe')
    # 启动子进程
    launcher.run()