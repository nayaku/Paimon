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
    model="paraformer-zh",
    # vad_model="fsmn-vad",
    # punc_model="ct-punc",
    # spk_model="cam++",
    device="cpu"
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
                                 description="热词")] = "派蒙 神里绫华 琴 旅行者 丽莎 芭芭拉 凯亚 迪卢克 雷泽 安柏 温迪 香菱 北斗 行秋 魈 凝光 可莉 钟离 菲谢尔 班尼特 达达利亚 诺艾尔 七七 重云 甘雨 阿贝多 迪奥娜 莫娜 刻晴 砂糖 辛焱 罗莎莉亚 胡桃 枫原万叶 烟绯 宵宫 托马 优菈 雷电将军 早柚 珊瑚宫心海 五郎 九条裟罗 荒泷一斗 八重神子 鹿野院平藏 夜兰 绮良良 埃洛伊 申鹤 云堇 久岐忍 神里绫人 柯莱 多莉 提纳里 妮露 赛诺 坎蒂丝 纳西妲 莱依拉 流浪者 珐露珊 瑶瑶 艾尔海森 迪希雅 米卡 卡维 白术 琳妮特 林尼 菲米尼 莱欧斯利 那维莱特 夏洛蒂 芙宁娜 夏沃蕾 娜维娅"):
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

    res[0]['text'] = res[0]['text'].replace(' ', '')
    # res[0]['text'] = punc_model.generate(res[0]['text'])
    # res[0]['raw_text'] = res[0]['text']
    # res[0]['clean_text'] = re.sub(regex, "", res[0]["text"], 0, re.MULTILINE)
    # res[0]['text'] = rich_transcription_postprocess(res[0]['text'])
    return res[0]
