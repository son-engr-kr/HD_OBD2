## 구매 해야 될 품목
-MCP2515<br>
-점퍼선<br>
-SD카드<br>
-라즈베리파이 4 hdmi(마이크로)<br>
-디스플레이(나중에, 일단 포터블모니터 사용)


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

<details>
  <summary>
  RAW OBD2 frame details
  </summary>

  https://www.csselectronics.com/pages/obd2-explained-simple-intro


  ![img](https://canlogger1000.csselectronics.com/img/OBD2-frame-raw-mode-PID-ID-bytes.svg)

  - Identifier: For OBD2 messages, the identifier is standard 11-bit and used to distinguish between "request messages" (ID 7DF) and "response messages" (ID 7E8 to 7EF). Note that 7E8 will typically be where the main engine or ECU responds at.

  - Length: This simply reflects the length in number of bytes of the remaining data (03 to 06). For the Vehicle Speed example, it is 02 for the request (since only 01 and 0D follow), while for the response it is 03 as both 41, 0D and 32 follow.

  - Mode: For requests, this will be between 01-0A. For responses the 0 is replaced by 4 (i.e. 41, 42, … , 4A). There are 10 modes as described in the SAE J1979 OBD2 standard. Mode 1 shows Current Data and is e.g. used for looking at real-time vehicle speed, RPM etc. Other modes are used to e.g. show or clear stored diagnostic trouble codes and show freeze frame data.

  - PID: For each mode, a list of standard OBD2 PIDs exist - e.g. in Mode 01, PID 0D is Vehicle Speed. For the full list, check out our OBD2 PID overview. Each PID has a description and some have a specified min/max and conversion formula.

  - The formula for speed is e.g. simply A, meaning that the A data byte (which is in HEX) is converted to decimal to get the km/h converted value (i.e. 32 becomes 50 km/h above). For e.g. RPM (PID 0C), the formula is (256*A + B) / 4.

  - A, B, C, D: These are the data bytes in HEX, which need to be converted to decimal form before they are used in the PID formula calculations. Note that the last data byte (after Dh) is not used.

  ![OBD2 request/response example](https://cdn.shopify.com/s/files/1/0579/8032/1980/files/obd2-pid-request-response-7df-7e8-vehicle-speed.svg?v=1633690039)


  <details>
    <summary>
    Extended OBD2 PID request/response
    </summary>
    In some vehicles (e.g. vans and light/medium/heavy duty vehicles), you may find that the <u>raw CAN data uses extended 29-bit CAN identifiers instead of 11-bit CAN identifiers.</u>

    In this case, you will typically need to modify the OBD2 PID requests to use the <u>CAN ID 18DB33F1 instead of 7DF.</u> The data payload structure is kept identical to the examples for 11-bit CAN IDs.

    If the vehicle responds to the requests, you'll typically see responses with CAN IDs 18DAF100 to 18DAF1FF (in practice, typically 18DAF110 and 18DAF11E). The response identifier is also sometimes shown in the 'J1939 PGN' form, specifically the PGN 0xDA00 (55808), which in the J1939-71 standard is marked as 'Reserved for ISO 15765-2'.

    We provide an OBD2 DBC file for both the 11-bit and 29-bit responses, enabling simple decoding of the data in most CAN software tools.
  </details>

  ![OBD mode - SAE J1979](https://cdn.shopify.com/s/files/1/0579/8032/1980/files/obd2-10-pid-modes-diagnostic-services.svg?v=1633690039)
  
</details>

### 현대 OBD-2 PID List는 어디서?
-  https://gsw.hyundai.com (일반인 가입 가능) - 아직 PID list 찾지는 못함


<br><br>
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