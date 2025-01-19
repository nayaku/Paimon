from funasr import AutoModel

chunk_size = [0, 10, 5] #[0, 10, 5] 600ms, [0, 8, 4] 480ms
encoder_chunk_look_back = 4 #number of chunks to lookback for encoder self-attention
decoder_chunk_look_back = 1 #number of encoder chunks to lookback for decoder cross-attention

model = AutoModel(model="paraformer-zh-streaming")

import soundfile
import os

wav_file = os.path.join(model.model_path, r"C:\Users\ZMK\AppData\LocalLow\DefaultCompany\Paimon\new.wav")
speech, sample_rate = soundfile.read(wav_file)
chunk_stride = chunk_size[1] * 960 # 600ms

cache = {}
total_chunk_num = int(len((speech)-1)/chunk_stride+1)
for i in range(total_chunk_num):
    speech_chunk = speech[i*chunk_stride:(i+1)*chunk_stride]
    is_final = i == total_chunk_num - 1
    res = model.generate(input=speech_chunk, cache=cache, is_final=is_final, chunk_size=chunk_size, encoder_chunk_look_back=encoder_chunk_look_back, decoder_chunk_look_back=decoder_chunk_look_back,hotword="派蒙 神里绫华 琴 旅行者 丽莎 芭芭拉 凯亚 迪卢克 雷泽 安柏 温迪 香菱 北斗 行秋 魈 凝光 可莉 钟离 菲谢尔 班尼特 达达利亚 诺艾尔 七七 重云 甘雨 阿贝多 迪奥娜 莫娜 刻晴 砂糖 辛焱 罗莎莉亚 胡桃 枫原万叶 烟绯 宵宫 托马 优菈 雷电将军 早柚 珊瑚宫心海 五郎 九条裟罗 荒泷一斗 八重神子 鹿野院平藏 夜兰 绮良良 埃洛伊 申鹤 云堇 久岐忍 神里绫人 柯莱 多莉 提纳里 妮露 赛诺 坎蒂丝 纳西妲 莱依拉 流浪者 珐露珊 瑶瑶 艾尔海森 迪希雅 米卡 卡维 白术 琳妮特 林尼 菲米尼 莱欧斯利 那维莱特 夏洛蒂 芙宁娜 夏沃蕾 娜维娅")
    print(res)