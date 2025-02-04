机器人基础通信工具

https://docs.go-cqhttp.org/guide/config.html

基于python的框架

https://docs.nonebot.dev/guide/basic-configuration.html

aiohttp

https://docs.aiohttp.org/en/stable/client_quickstart.html#aiohttp-client-websockets



搞了快一天，终于弄出一个基本demo了。

首先要下载go-cqhttp，然后第一次运行会生成配置文件，这里需要配置：

- uin：qq账号
- password：密码

最关键的是servers：

```yml
servers:
  # 添加方式，同一连接方式可添加多个，具体配置说明请查看文档
  #- http: # http 通信
  #- ws:   # 正向 Websocket
  #- ws-reverse: # 反向 Websocket
  #- pprof: #性能分析服务器
  # 正向WS设置
    # HTTP 通信设置
  - http:
      # 服务端监听地址
      host: 127.0.0.1
      # 服务端监听端口
      port: 5700
      # 反向HTTP超时时间, 单位秒
      # 最小值为5，小于5将会忽略本项设置
      timeout: 5
      middlewares:
        <<: *default # 引用默认中间件
      # 反向HTTP POST地址列表
      post:
      #- url: '' # 地址
      #  secret: ''           # 密钥
        - url: 127.0.0.1:5701 # 地址
      #  secret: ''          # 密钥

  - ws:
      # 正向WS服务器监听地址
      host: 127.0.0.1
      # 正向WS服务器监听端口
      port: 6700
      middlewares:
        <<: *default # 引用默认中间件
```

之前我一直没配置http，导致post请求一直失败！

然后需要配置过滤文件

```yml
  # 事件过滤器文件目录
  filter: 'myfilter.json'
```

过滤文件这里我按照官方文档设置了只接受群组的消息

```python
{
    ".or": [
        {
            "message_type": "private"
        },
        {
            "message_type": "group",
            "group_id": {
                ".in": [
                    12345678,
                    542423773
                ]
            },
            "anonymous": {
                ".eq": null
            }
        }
    ]
}

```



然后安装aiohttp

这一段代码是接收服务端的消息

```python
async def main():
    async with aiohttp.ClientSession() as session:
        async with session.ws_connect("ws://127.0.0.1:6700") as ws_connection:
            while True:
                received_data = await ws_connection.receive_json()
                print(received_data)
                break
        # async with session.get('http://httpbin.org/get') as resp:
        # wait session.post('ws://127.0.0.1:6700/send_group_msg', json=reply)

await main()
```

```python
{'_post_method': 2, 'meta_event_type': 'lifecycle', 'post_type': 'meta_event', 'self_id': 323566263, 'sub_type': 'connect', 'time': 1637737377}
```



这一段是机器人给QQ群发消息

```python
import async
import aiohttp

async def send_group_msg(group_id, msg):
    msg = {'group_id': group_id, 'message': msg}
    async with aiohttp.ClientSession() as session:
        await session.post("ws://127.0.0.1:5700/send_group_msg", json=msg)
        # async with session.post("ws://127.0.0.1:5700/send_group_msg", json=reply) as response:
        #    data = await response.text(encoding="utf-8")
        # 

await send_group_msg(542423773, '确实')
```

```
{"data":{"message_id":788732479},"retcode":0,"status":"ok"}
 <ClientResponse(ws://127.0.0.1:5700/send_group_msg) [200 OK]>
<CIMultiDictProxy('Content-Type': 'application/json; charset=utf-8', 'Date': 'Wed, 24 Nov 2021 07:12:07 GMT', 'Content-Length': '60')>
```

### 确实机器人

当你在群里聊天时，如果说了**确实**这两个字，机器人也会自动附和一个确实。

```python
import aiohttp
import asyncio

async def send_group_msg(group_id, msg):
    msg = {'group_id': group_id, 'message': msg}
    async with aiohttp.ClientSession() as session:
        await session.post("ws://127.0.0.1:5700/send_group_msg", json=msg)

async def main():
    async with aiohttp.ClientSession() as session:
        async with session.ws_connect("ws://127.0.0.1:6700") as ws_connection:
            while True:
                event = await ws_connection.receive_json()
                if event['post_type'] == 'message' and event['message_type'] == 'group':
                    if '确实' in event['message']:
                        await send_group_msg(event['group_id'], '确实')
                    	print(event)
                        continue

# await main()
if __name__ == '__main__':
    # asyncio.run(main)
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())
```

简化版



你的这段代码似乎有些混淆了。`session.post` 是用于发送 HTTP POST 请求的，而不是用来发送 WebSocket 消息的。如果你要使用 aiohttp 库发送 WebSocket 消息，你需要使用 `session.ws_connect` 来创建一个 WebSocket 连接。

```python
import json
import asyncio
import aiohttp

async def send_group_msg():
    async with aiohttp.ClientSession() as session:
        async with session.ws_connect("ws://127.0.0.1:8090/send_group_msg") as ws:
            while True:
                event = await ws.receive_json()
                print(event)
                if 'post_type' not in event:
                    continue

                if event['post_type'] == 'message' and event['message_type'] == 'group':
                    data = {
                        "action": "send_group_msg",
                        "params": {'group_id': event['group_id'], 'message': '确实'}
                    }
                    if '确实' in event['message']:
                        await ws.send_str(json.dumps(data))

async def main():
    await send_group_msg()

# await main()
if __name__ == '__main__':
    asyncio.run(main())
    # loop = asyncio.get_event_loop()
    # loop.run_until_complete(main())
```



event 示例

```json
{
    'post_type': 'message', 
    'message_type': 'group', 
    'time': 1690355174, 
    'self_id': 323566263, 
    'sub_type': 'normal', 
    'anonymous': None, 
    'message_seq': 8336, 
    'raw_message': '这个确实是这样的', 
    'message_id': 1392889488, 
    'font': 0, 
    'group_id': 542423773, 
    'message': '这个确实是这样的', 
    'sender': {'age': 0, 'area': '', 'card': '213', 'level': '', 'nickname': '梦见月球的猫', 'role': 'owner', 'sex': 'unknown', 'title': '', 'user_id': 435786117}, 
    'user_id': 435786117}
```



定时播报

```python
async def regular_broadcast():
    # 设计为每天
```

## 设计思路

每天3点提醒饮茶

### 股票系统设计思路

每天的0点-8点收盘，股价不变

输入：**股价** 查看所有股票（5支）的当前价格

输入：**买股 1 100**：购买100股股票1。

输入：**卖股 1 100**：卖出持有的100股股票1。

输入：**股票走势图**：显示近7天的股票走势。（先不实现）

输入：**资产榜** 显示股票资产和个人资产top5

使用随机漫步模拟股票的涨跌

股价初始价格为10

初始资产10000g 每日签到可以获得100g

用什么数据结构存储买了哪些股呢？

股票购买表:

- 用户ID
- 股票ID
- 股票数量：

股票购买记录

- 用户ID
- 股票ID
- 股票购买价格
- 股票购买数量
