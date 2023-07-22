import json
import requests
from epr_reader.bindings.easy_pathology_record import EasyPathologyRecord


def read(file_path: str, read_header_only=False):
    register_url = 'http://127.0.0.1:6082/Reader/ReadEpr'
    header = {
        "Content-Type": "application/json"
    }
    data = {
        "filePath": file_path,
        "readHeaderOnly": read_header_only
    }
    re = requests.post(url=register_url, json=data, headers=header)
    if re.status_code != 200:
        raise Exception(f'服务器请求失败，状态码：{re.status_code}')

    data = json.loads(re.content)['data']
    data = EasyPathologyRecord(**data)
    return data


def save(epr: EasyPathologyRecord, file_path: str):
    register_url = 'http://127.0.0.1:6082/Writer/SaveEpr'
    header = {
        "Content-Type": "application/json"
    }
    data = {
        "filePath": file_path,
        "easyPathologyRecord": epr.to_json()
    }
    requests.post(url=register_url, json=data, headers=header)


if __name__ == "__main__":
    epr = read(r'G:\Source\CSharp\EasyPathology5\build\Debug\net6.0-windows\saved\乔思源\2022-5-16\1-1-2_肾水样变性_-_40x.epr')
    # print(epr)
    save(epr, r'G:\1-1-2_肾水样变性_-_40x.epr')
