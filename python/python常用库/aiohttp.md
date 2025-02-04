# aiohttp

https://docs.aiohttp.org/en/stable/

类似异步的requests库，理论上性能要比requests库强吧

客户端例子

```python
async def main():
    async with aiohttp.ClientSession() as session:
        async with session.get('https://www.baidu.com') as response:
            print("Status:", response.status)
            print("Content-type:", response.headers['content-type'])

            html = await response.text()
            print("Body:", html[:15], "...")
            return html

html = asyncio.run(main())
```

```
Status: 200
Content-type: text/html
Body: <html>
 ...
```

服务端例子

```python
from aiohttp import web

async def handle(request):
    name = request.match_info.get('name', "Anonymous")
    text = "Hello, " + name
    return web.Response(text=text)

app = web.Application()
app.add_routes([web.get('/', handle),
                web.get('/{name}', handle)])

if __name__ == '__main__':
    web.run_app(app)
```



