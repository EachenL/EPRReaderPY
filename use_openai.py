
import os
import openai

# re = openai.Completion.create(
#     model='text-davinci-003',
#     prompt='who are you?',
#     max_tokens=100,
#     temperature=0
# )
# print(re._previous['choices'][0]['text'])
# completion = openai.ChatCompletion.create(
#     model="gpt-3.5-turbo",
#     messages=[
#         {"role": "user", "content": "who are you?"}
#     ]
# )

# print(completion.choices[0].message)
# messages = []
# while True:
#     print('You:')
#     ins = input()
#     input_message = {"role": "user", "content": ins}
#     messages.append(input_message)
#     # print('waiting for response...')
#     completion = openai.ChatCompletion.create(
#         model="gpt-3.5-turbo",
#         messages=messages,
#     )
#     response_message = completion.choices[0].message
#     print('chatbot:')
#     print(completion.choices[0].message.content)
#     messages.append(response_message)
class ChatGPT():
    def __init__(self):
        self.messages = []
        api_key = os.getenv("NSD_OPENAI_API_KEY")
        org_id = 'org-emV4mY3cz3RsLMUkKTTnGZlE'
        openai.organization = org_id
        openai.api_key = api_key
        self.response_content = ''
        
    def chat(self, input):
        # print('You:')
        # ins = input()
        input_message = {"role": "user", "content": input}
        self.messages.append(input_message)
        # print('waiting for response...')
        completion = openai.ChatCompletion.create(
            model="gpt-3.5-turbo",
            messages=self.messages,
        )
        response_message = completion.choices[0].message
        # print('chatbot:')
        # print(completion.choices[0].message.content)
        self.messages.append(response_message)
        self.response_content = completion.choices[0].message.content
        return self.response_content
    
    def clear_memory(self):
        self.messages = []