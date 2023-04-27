import srt
import openai
import use_openai
import json
chatbot = use_openai.ChatGPT()

def get_final_text(srtfile):
    '''
    读取srt文件，将其分成7至8个部分并为每个部分起一个标题, 并返回每部分的索引范围
    '''
    chatbot.clear_memory()
    srt_file = open(srtfile)
    srt_content = srt.parse(srt_file)
    
    # content = srt_file.readlines()
    content = ''
    for sub in srt_content:
        content += f'{sub.index}: {sub.content}\n'

    example = '例子为：\n\
    1. 肝脏组织结构及血管、管道的组成\n\
    索引范围：1-7\n\
    2. 汇管区和其中的管道结构\n\
    索引范围：9-13\n\
    3. 脂肪变性和细胞坏死的特征\n\
    索引范围：15-19'
    ins = '请以上述例子的格式, 根据以下文本内容将其分成7至8个部分, 并为每个部分起一个标题, 并返回每部分的索引范围, 且索引范围为一个连续的范围, 各部分的索引范围要求不重叠, 内容为'
 
    input = f"{example}. {ins}: {content}"
    re = chatbot.chat(input)
    print(re)

    input = '用json的形式重新返回上述内容, 索引范围命名为index_range, index_range中的值用\'-\'隔开, 标题命名为title'

    re = chatbot.chat(input)
    try:
        part_list = json.loads(re)
        print(part_list)
    except:
        re = chatbot.chat(f'重新{input}')
        part_list = json.loads(re)
        print(part_list)

    # input = '请为所有文本内容起一个标题'
    # title = chatbot.chat(input)
    # print(title)
    return part_list

# for _ in range(10):
#     get_final_text('../1-4-2/1-4-2_肝细胞坏死__-_40x.srt')
#     chatbot.clear_memory()