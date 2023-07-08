# 아래 내용 의미 없음

## ESP8266
- ESP8266(또는 ESP8266EX)은 Wi-Fi 무선 통신 기능을 탑재한 저전력 마이크로컨트롤러
- ESP8266모듈을 사용 시 하나의 아두이노에 mcp2515,esp8266모듈 동시에 SPI통신 안됨
- ESP8266이 내장된 아두이노 우노(WeMos) D1 R1 또는 아두이노 우노(WeMos) D1 R2
  
## 아두이노 우노(WeMos) D1 R1, 아두이노 우노(WeMos) D1 R2

- R1, R2 차이점
- https://blog.naver.com/PostView.naver?blogId=geniusus&logNo=221196921556
- R1 vs. R2 - The Difference : 
When you take a look at the figure 1 you will see it with one blink of the eye thet the old Wemos is different against the new version R2. The old version has an ESP8266 12B or 12E model. The new one has an ESP8266 12F model on the pcb board. You see this in the difference of the Wifi antenna. Here is a Wiki link to the different ESP8266 modules. The next part for R2 are the 2x4 solder pins for the serial and I2C interface which is not on the old model R1. The techncal data/specifiactions are the same. SO as you see they look like an Arduino UNO including the stacks to mount different shields. Later I will give more information over shields which i have tested and there are a lot of them.
- https://tasmota.github.io/docs/devices/Wemos-D1-R1-%26-R2/

## 아두이노 우노 D1 R1 wemos 라이브러리 설치 방법 및 예제
- https://blog.naver.com/eduino/221201659046
- COM인식 실패시 따로 드라이버 설치해야 할 수도

## 아두이노가 할당 받은 IP 확인 방법
- https://jakpentest.tistory.com/197
- 위의 사이트 접속 후 맨 밑에 코드 부분 확인

## 아두이노 d1 r1, d1 r2 , wemos mini 차이점 확인


## eso8266은 3.3V만 지원
- 아두이노 우노를 사용시 빵판을 사용하여 1k옴 저항 3개를 사용해 전압분배로 3.3V만들어줘야함


