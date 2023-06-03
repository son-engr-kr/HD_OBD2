## I2C통신과 SPI통신 차이
 I2C통신은 한 사람이 데이터를 보낼 때 다른 한 사람은 받고만 있어야 하는 무전기와 같은 통신 방식이고 SPI통신은 한 사람이 데이터를 보내면서 동시에 데이터를 받을 수 있는 전화와 같은 방식

## SPI통신
-SPI통신은 소프트웨어적으로 슬레이브를 선택하는 것이 아니라 하드웨어 적으로 슬레이브를 따로 연결하는 방식
![img](https://blog.kakaocdn.net/dn/dwQdc1/btrJ1YcDmsX/5Zq1wO9WAXEju6fQplTmE1/img.png)
-SS(Slave Select) 슬레이브 선택 핀
-SCLK(Serial Clock)
-MOSI(Master Output Slave Input) 마스터로 부터 슬레이브로 데이터 전달
-MISO(Master Input Slave Output)슬레이브에서 마스터로 데이터 전달
-SPI 통신에서 특정 슬레이브를 선택하는 것은 특정 슬레이브에 연결되어 있는 SS핀에 Low를 넣어주는 것
-반대로 슬레이브를 선택하지 않은 것은 HIGH 넣어줌

::중요한것 :: 
녹색 슬레이브만 사용한다고 녹색 슬레이브에만 LOW넣어주는 것이 아닌 다른 푸른,보라 슬레이브에 High를 같이 넣어줘야 정상 작동함.

## 아두이노 SPI통신 Library
-SPI.begin() : SPI 통신 초기화

-SPI.end() : SPI 통신 종료

-SPI.setBitOrder(bitOrder) : 전송순서 결정 bitOrder은 LSBFIRST,MSBFIRST 중 하나의 값 가짐 기본은 MSBFIRSTR로 설정

-SPI.setClockDivider(rate) : 클록의 분주비율 설정 

분주란 어느 주파수를 정수비로 그 보다 낮은 주파수로 만드는 것. 분주를 통해 원하는 낮은 주파수를 얻을 수 있고 기존의 빠른 주파수보다 안정도와 정확도를 높일 수 있음.
AVR 기반의 보드에서 분주비율 2,4,8,16,32,64,128 중 하나의 값으로 할 수 있음.
rate에는 분주비율 들어가야함 2,4,8과 같은 숫자를 적는 것이아닌 SPI_CLOCK_DIV2, SPI_CLOCK_DIV4처럼 끝의 숫자만 바꿈
기본적으로 아두이노는 16MHz 클록 사용하므로 SPI통신에서 4MHz가 기본 주파수에 해당.

-SPI.setDateMode(mode) : 전송모드 선택함수
마스터와 슬레이브 사이에 동일한 모드를 사용
https://blog.naver.com/darknisia/220673747042 모드부분 참조

-SPI.transer(data) : 통신에서 데이터를 보내고 받는 함수.

## MCP2515<->아두이노 우노 연결 (SPI 통신)

|CAN 모듈|아두이노|용도|
|:---:|:---:|:---:|
|VCC|5V|전원(+)|
|GND|GND|전원(-)|
|INT|2번핀|CAN 패킷 수신 시 인터럽트 발생|
|CS|9번핀|Chip Select(SPI 채널 선택)|
|SI(MOSI)|11번핀|Master(Arduino) -> Slave 신호|
|SO(MISO)|12번핀|Slave -> Master 신호|
|SCK|13번핀|SPI Clock|

아두이노 핀번호 앞 '~'은 PWM을 지원한다는 것.<br>
PWM(Pulse Width Modulation) : 펄스 폭 변조를 말하는 것이다. 쉽게 말해, 디지털 신호를 아날로그 신호처럼 흉내내는 것 -> 디지털 신호의 0과 1의 비율(duty cycle)을 조절해 아날로그 신호처럼 중간값을 나타낼수 있는 것처럼 보여짐(e.g. 전구를 0.01초씩 빠르게 켯다 끄면 사람의 눈에는 전구가 그냥 켜졌을때의 밝기50%로 전구가 켜진걸로 보임)

## MCP2515<-> OBD2 단자 연결
- obd2 단자의 6번이 MCP2515의 high부분에 연결
- obd2 단자의 14번이 MCP2515의 low부분에 연결


# Adafruit MCP2515

https://reference.arduino.cc/reference/en/libraries/adafruit-mcp2515/


# Adafruit MCP2313_CAN_Sender 예제

<details>
  <summary>Adafruit MCP2313_CAN_Sender 예제</summary>
  
  ### examples/mcp2515_simpletest.py
  

  ### c++ code
```cpp
#include <Adafruit_MCP2515.h>

#ifdef ESP8266
   #define CS_PIN    2
#elif defined(ESP32) && !defined(ARDUINO_ADAFRUIT_FEATHER_ESP32S2) && !defined(ARDUINO_ADAFRUIT_FEATHER_ESP32S3)
   #define CS_PIN    14
#elif defined(TEENSYDUINO)
   #define CS_PIN    8
#elif defined(ARDUINO_STM32_FEATHER)
   #define CS_PIN    PC5
#elif defined(ARDUINO_NRF52832_FEATHER)  /* BSP 0.6.5 and higher! */
   #define CS_PIN    27
#elif defined(ARDUINO_MAX32620FTHR) || defined(ARDUINO_MAX32630FTHR)
   #define CS_PIN    P3_2
#elif defined(ARDUINO_ADAFRUIT_FEATHER_RP2040)
   #define CS_PIN    7
#elif defined(ARDUINO_ADAFRUIT_FEATHER_RP2040_CAN)
   #define CS_PIN    PIN_CAN_CS
#elif defined(ARDUINO_RASPBERRY_PI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO_W) // PiCowbell CAN Bus
   #define CS_PIN    20
#else
    // Anything else, defaults!
   #define CS_PIN    9 //우리가 사용하는 우노는 CS_PIN 9로 수정!!
#endif

// Set CAN bus baud rate
#define CAN_BAUDRATE (250000) //캔 보드레이트도 115200으로 통일 해야 되지 않나? 통일하면 연결 오류됨.

Adafruit_MCP2515 mcp(CS_PIN);

void setup() {
  Serial.begin(115200);
  while(!Serial) delay(10);

  Serial.println("MCP2515 Sender test!");

  if (!mcp.begin(CAN_BAUDRATE)) {
    Serial.println("Error initializing MCP2515.");
    while(1) delay(10);
  }
  Serial.println("MCP2515 chip found");
}

void loop() {
  // send packet: id is 11 bits, packet can contain up to 8 bytes of data
  Serial.print("Sending packet ... ");

  mcp.beginPacket(0x12);
  mcp.write('h');
  mcp.write('e');
  mcp.write('l');
  mcp.write('l');
  mcp.write('o');
  mcp.endPacket();

  Serial.println("done");

  delay(1000);

  // send extended packet: id is 29 bits, packet can contain up to 8 bytes of data
  Serial.print("Sending extended packet ... ");

  mcp.beginExtendedPacket(0xabcdef);
  mcp.write('w');
  mcp.write('o');
  mcp.write('r');
  mcp.write('l');
  mcp.write('d');
  mcp.endPacket();

  Serial.println("done");

  delay(1000);
}

```
### 주의사항
```cpp
-전처리문에서 우리가 사용하는 아두이노 우노는 CS-PIN이 9번이므로 5에서 9로 수정해야됨
-아두이노에서 can통신할때 baud rate 250000를 쓰는 이유 : 
표준 Baud Rate 값: 250000은 CAN 통신의 표준 Baud Rate 값 중 하나로 정의되어 있습니다. CAN 프로토콜 표준에서는 250000 Baud를 사용
-rtr(Remote transmission request) : 원격 전송 요청

```
</details>



