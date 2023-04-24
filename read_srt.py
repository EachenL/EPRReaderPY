import srt
import openai
import use_openai
srt_file = open('1-4-2_肝细胞坏死__-_40x.srt')
srt_content = srt.parse(srt_file)
chatbot = use_openai.ChatGPT()
# content = srt_file.readlines()
content = ''
for sub in srt_content:
    content += f'{sub.index}: {sub.content}\n'
    # content.append(sub.content)
    # print(sub.content)

# content = ','.join(content)
ins = '文本内容由索引加内容构成，请根据文本内容将以下文本分成7到8段的概述, 并返回每段的索引范围'
# completion = openai.ChatCompletion.create(
#     model="gpt-3.5-turbo",
#     messages=[
#         {"role": "user", "content": f"{ins}: {content}"},
#     ]
# )

# print(completion.choices[0].message.content)
input = f"{ins}: {content}"
re = chatbot.chat(input)
print(re)
# input = '请以json的格式重新返回'
input = '请将学术的方式重新表述'

re = chatbot.chat(input)
print(re)


