## MCP2515 구매 경로
- https://www.smart3dkr.com/product/MCP2315
- https://vctec.co.kr/product/can-%EB%B2%84%EC%8A%A4-%EC%BB%A8%ED%8A%B8%EB%A1%A4%EB%9F%AC-%EB%AA%A8%EB%93%88-spi-mcp2515-can-bus-controller-module-spi-mcp2515/10537/
- https://parts-parts.co.kr/product/pp-a233-mcp2515-can-%ED%86%B5%EC%8B%A0-%EB%AA%A8%EB%93%88-can-bus-module/207/

## MCP 2515 라즈베리파이 연결
- https://memories.tistory.com/129
(위와 같은 모듈)

<details>
  <summary>
    라즈베리파이-MCP2515 핀맵 ( 라즈베리파이 버전 확인 필요 )
  </summary>

  ![img](https://img1.daumcdn.net/thumb/R1280x0/?scode=mtistory2&fname=https%3A%2F%2Fblog.kakaocdn.net%2Fdn%2FcnPQRU%2Fbtq2ggaGiUc%2FDCk9gK3ZKfuUhc4znoY7S1%2Fimg.jpg)
</details>

## python mcp2515 라이브러리
https://docs.circuitpython.org/projects/mcp2515/en/latest/

(Simple test)https://docs.circuitpython.org/projects/mcp2515/en/latest/examples.html

<details>
  <summary>adafruit_mcp2515 예제</summary>
  
  ### examples/mcp2515_simpletest.py
  

  ### python code
```python
# SPDX-FileCopyrightText: Copyright (c) 2020 Bryan Siepert for Adafruit Industries
#
# SPDX-License-Identifier: MIT
from time import sleep
import board
import busio
from digitalio import DigitalInOut
from adafruit_mcp2515.canio import Message, RemoteTransmissionRequest
from adafruit_mcp2515 import MCP2515 as CAN


cs = DigitalInOut(board.D5)
cs.switch_to_output()
spi = busio.SPI(board.SCK, board.MOSI, board.MISO)

can_bus = CAN(
   spi, cs, loopback=True, silent=True
)  # use loopback to test without another device
while True:
   with can_bus.listen(timeout=1.0) as listener:

       message = Message(id=0x1234ABCD, data=b"adafruit", extended=True)
       send_success = can_bus.send(message)
       print("Send success:", send_success)
       message_count = listener.in_waiting()
       print(message_count, "messages available")
       for _i in range(message_count):
           msg = listener.receive()
           print("Message from ", hex(msg.id))
           if isinstance(msg, Message):
               print("message data:", msg.data)
           if isinstance(msg, RemoteTransmissionRequest):
               print("RTR length:", msg.length)
   sleep(1)
```
### 해결해야 할 것들
```python
message = Message(id=0x1234ABCD, data=b"adafruit", extended=True)
#여기서 id와 data를 뭘 써야 할까?
```
</details>



# 우리의 라즈베리파이는?
- 현재 소유 중: 라즈베리파이4 B 4GB
- 8GB가 필요할 수도..
<details>
  <summary>
    Raspberry Pi 4 GPIO Pinout
  </summary>
  https://linuxhint.com/gpio-pinout-raspberry-pi/

  ![img](https://linuxhint.com/wp-content/uploads/2022/02/image6-34.png)
</details>


# ELM-327을 활용하는 방법도 있다

- https://python-obd.readthedocs.io/en/latest/