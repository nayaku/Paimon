# Set the device with environment, default is cuda:0
# export SENSEVOICE_DEVICE=cuda:1
import functools
import re
import tempfile
import time
from enum import Enum

from fastapi import FastAPI, File, Form
from fastapi.responses import HTMLResponse
from funasr import AutoModel
from funasr.utils.postprocess_utils import rich_transcription_postprocess
from starlette.responses import JSONResponse
from typing_extensions import Annotated


class Language(str, Enum):
    auto = "auto"
    zh = "zh"
    en = "en"
    yue = "yue"
    ja = "ja"
    ko = "ko"
    nospeech = "nospeech"


model = AutoModel(
    model="iic/SenseVoiceSmall",
    device="cuda:0",
    # vad_model="fsmn-vad",
    # vad_kwargs={"max_single_segment_time": 30000},
    ban_emo_unk=True
)
# punc_model = AutoModel(model="ct-punc") # 符号补全
regex = r"<\|.*\|>"

app = FastAPI()


def timer(func):
    @functools.wraps(func)  # 保持被装饰函数的元信息
    async def wrapper(*args, **kwargs):
        start_time = time.time()
        response = await func(*args, **kwargs)
        # response = JSONResponse(content=response)
        end_time = time.time()
        execution_time = end_time - start_time
        response["X-Execution-Time"] = str(execution_time)
        return response

    return wrapper


@app.get("/", response_class=HTMLResponse)
async def root():
    return """
    <!DOCTYPE html>
    <html>
        <head>
            <meta charset=utf-8>
            <title>Api information</title>
        </head>
        <body>
            <a href='./docs'>Documents of API</a>
        </body>
    </html>
    """


@app.post("/api/v1/asr")
@timer
async def turn_audio_to_text(file: Annotated[bytes, File(description="wav or mp3 audios in 16KHz")],
                             lang: Annotated[Language, Form(description="language of audio content")] = "auto",
                             hotword: Annotated[str, Form(
                                 description="热词")] = "派蒙\n神里绫华\n琴\n旅行者\n丽莎\n芭芭拉\n凯亚\n迪卢克\n雷泽\n安柏\n温迪\n香菱\n北斗\n行秋\n魈\n凝光\n可莉\n钟离\n菲谢尔\n班尼特\n达达利亚\n诺艾尔\n七七\n重云\n甘雨\n阿贝多\n迪奥娜\n莫娜\n刻晴\n砂糖\n辛焱\n罗莎莉亚\n胡桃\n枫原万叶\n烟绯\n宵宫\n托马\n优菈\n雷电将军\n早柚\n珊瑚宫心海\n五郎\n九条裟罗\n荒泷一斗\n八重神子\n鹿野院平藏\n夜兰\n绮良良\n埃洛伊\n申鹤\n云堇\n久岐忍\n神里绫人\n柯莱\n多莉\n提纳里\n妮露\n赛诺\n坎蒂丝\n纳西妲\n莱依拉\n流浪者\n珐露珊\n瑶瑶\n艾尔海森\n迪希雅\n米卡\n卡维\n白术\n琳妮特\n林尼\n菲米尼\n莱欧斯利\n那维莱特\n夏洛蒂\n芙宁娜\n夏沃蕾\n娜维娅"):
    if lang == "":
        lang = "auto"

    # 读取文件到临时目录
    _, file_path = tempfile.mkstemp()
    with open(file_path, 'wb') as f:
        f.write(file)

    res = model.generate(
        input=file_path,
        language=lang,  # "zh", "en", "yue", "ja", "ko", "nospeech"
        hotword=hotword,
    )
    if len(res) == 0:
        return {}

    # res[0]['text'] = punc_model.generate(res[0]['text'])
    res[0]['raw_text'] = res[0]['text']
    res[0]['clean_text'] = re.sub(regex, "", res[0]["text"], 0, re.MULTILINE)
    res[0]['text'] = rich_transcription_postprocess(res[0]['text'])
    return res[0]
