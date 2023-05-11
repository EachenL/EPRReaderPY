import urllib3
import requests
import json
import os
import launcher

def read_epr(file_path: str, read_header_only=False):
	launcher.Launcher(launcher.Command).run()
	register_url = 'http://localhost:6082/Reader/ReadEpr'
	header = {
		"Content-Type": "application/json"
	}
	post = {
		"filePath": file_path,
		"readHeaderOnly": read_header_only
	}
	re = requests.post(url=register_url, json=post, headers=header)
	data = json.loads(re.content)
	# print(data)
	return data


# read_epr(r'E:\病理视频教学\吴泽病理教学视频\1532677\1532677.epr', False)