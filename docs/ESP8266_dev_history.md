# 아래 내용은 테스트 후 검증되기까지 과정임

- 아두이노 메가
- ESP8266
- ESP8266 어댑터
## 아두이노 메가와 어댑터 배선
<table>
<tr>
<td>어댑터</td><td>아두이노</td>
</tr>
<tr>
<td>GND</td><td>GND</td>
</tr>
<tr>
<td>VCC</td><td>5V</td>
</tr>

<tr>
<td>TX</td><td>19(RX1)</td>
</tr>
<tr>
<td>RX</td><td>18(TX1)</td>
</tr>

</table>
- TX와 RX는 교차로 연결되어야 한다.

## ESP_Command
[ESP_Command.ino](../Arduino_project/esp_8266_tcpip_test/ESP_Command/ESP_Command.ino)

처음 esp 연결해서 커맨드 날려볼 수 있는 예제

https://blog.naver.com/ssshin22/220868021464

기본적으로 AT 커맨드만 해서 OK가 오는지만 보면 됨

## TCPTest
[TCPtest.ino](../Arduino_project/esp_8266_tcpip_test/TCPtest/TCPtest.ino)

유니티 클라이언트와 연결해서 패킷 주고 받는 예제(아직 걸음마 단계)
아직 고정 ip는 안해봄